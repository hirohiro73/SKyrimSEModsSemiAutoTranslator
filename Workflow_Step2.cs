using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class Workflow_Step2
    {
        private readonly TranslationContext _ctx;
        private static readonly HttpClient _httpClient = new HttpClient();
        private const int RequestWaitMs = 1000;

        static Workflow_Step2()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public Workflow_Step2(TranslationContext context)
        {
            _ctx = context;
        }

        public async Task Run(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            log("=== [Step 2] 翻訳ファイルのダウンロード (Version Match) ===");
            onProgress(0, 0, "Step 2: 開始...");

            string downloadDir = _ctx.DownloadDir;
            string cacheDir = _ctx.CacheDir;
            string targetEspDir = _ctx.TargetEspDir;

            var metaFiles = Directory.GetFiles(targetEspDir, "*_meta.ini");
            int total = metaFiles.Length;
            int current = 0;
            int downloadCount = 0;
            int cacheUsedCount = 0;

            var processedIds = new HashSet<string>();

            foreach (var metaPath in metaFiles)
            {
                token.ThrowIfCancellationRequested();
                current++;

                string espName = Path.GetFileName(metaPath).Replace("_meta.ini", "");
                string content = File.ReadAllText(metaPath);
                var modIdMatch = Regex.Match(content, @"modid=(\d+)");
                var verMatch = Regex.Match(content, @"version=(.+)");

                if (!modIdMatch.Success || modIdMatch.Groups[1].Value == "0")
                {
                    log($"[Skip] {espName}: Mod IDなし");
                    continue;
                }

                string modId = modIdMatch.Groups[1].Value;
                string modVerStr = verMatch.Success ? verMatch.Groups[1].Value.Trim() : "1.0";

                onProgress(current, total, $"確認中: {espName} (ID:{modId} v{modVerStr})");

                if (processedIds.Contains(modId)) continue;

                try
                {
                    await Task.Delay(RequestWaitMs, token);

                    string dbUrl = $"https://skyrimspecialedition.2game.info/detail.php?id={modId}";
                    string html = await _httpClient.GetStringAsync(dbUrl, token);

                    var candidates = ParseCandidatesFromHtml(html, modId);

                    if (candidates.Count == 0)
                    {
                        log($"[NotFound] {espName}: 翻訳ファイルが見つかりません");
                        processedIds.Add(modId);
                        continue;
                    }

                    var bestMatch = SelectBestVersion(modVerStr, candidates, log, espName);

                    if (bestMatch == null)
                    {
                        log($"[Error] {espName}: 適切なバージョンの選択に失敗しました");
                        processedIds.Add(modId);
                        continue;
                    }

                    string fileBaseName = $"{espName}_{modId}_{SanitizeFileName(bestMatch.VersionStr)}";
                    string[] existingCache = Directory.GetFiles(cacheDir, $"{fileBaseName}.*");

                    if (existingCache.Length > 0)
                    {
                        string cacheFile = existingCache[0];
                        string destPath = Path.Combine(downloadDir, Path.GetFileName(cacheFile));
                        File.Copy(cacheFile, destPath, true);
                        log($"[Cache] {espName}: {Path.GetFileName(cacheFile)} (ID:{bestMatch.FileId})");
                        cacheUsedCount++;
                    }
                    else
                    {
                        log($"[DL] {espName}: v{bestMatch.VersionStr} (FileID:{bestMatch.FileId}) をダウンロード中...");

                        var req = new HttpRequestMessage(HttpMethod.Get, bestMatch.Url);
                        req.Headers.Referrer = new Uri(dbUrl);
                        var res = await _httpClient.SendAsync(req, token);
                        res.EnsureSuccessStatusCode();
                        byte[] data = await res.Content.ReadAsByteArrayAsync(token);

                        if (data.Length > 0)
                        {
                            string ext = DetectExtension(data);
                            string saveName = $"{fileBaseName}{ext}";
                            string cachePath = Path.Combine(cacheDir, saveName);
                            string dlPath = Path.Combine(downloadDir, saveName);

                            await File.WriteAllBytesAsync(cachePath, data, token);
                            File.Copy(cachePath, dlPath, true);
                            downloadCount++;
                        }
                    }
                    processedIds.Add(modId);
                }
                catch (Exception ex)
                {
                    log($"[Error] {espName}: {ex.Message}");
                    processedIds.Add(modId);
                }
            }

            log("----------");
            log($"[Step 2 Result]");
            log($"  - 新規ダウンロード: {downloadCount}");
            log($"  - キャッシュ使用:   {cacheUsedCount}");
            onProgress(100, 100, "Step 2 完了");
        }

        // =====================================================================
        //  Helper Methods
        // =====================================================================

        private List<DownloadCandidate> ParseCandidatesFromHtml(string html, string modId)
        {
            var list = new List<DownloadCandidate>();
            string pattern = $@"<a\s+([^>]*?)href=[""'][^""']*?jp_download\.php\?file_id=(\d+)(?:&|&amp;)id={modId}[""']([^>]*?)>(.*?)</a>";

            var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match m in matches)
            {
                // Group 1: hrefより前の属性
                // Group 2: file_id
                // Group 3: hrefより後の属性
                // Group 4: リンクテキスト

                int fileId = int.Parse(m.Groups[2].Value);

                // 前後の属性を結合して検索対象にする
                string attributes = m.Groups[1].Value + " " + m.Groups[3].Value;
                string linkText = m.Groups[4].Value.Trim();

                string versionStr = "";
                string titleValue = "";

                // title属性の抽出
                var titleMatch = Regex.Match(attributes, @"title=[""'](.*?)[""']", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                {
                    titleValue = titleMatch.Groups[1].Value;

                    // "Version:" の後ろを取得
                    // 例: "Song of the Green... Version:VIGILANT Patch 0.2"
                    var verInTitle = Regex.Match(titleValue, @"Version:(.*)", RegexOptions.IgnoreCase);
                    if (verInTitle.Success)
                    {
                        // "VIGILANT Patch 0.2" -> "0.2" を後でExtractVersionStringで抽出する
                        // ここでは記述全体を取る
                        versionStr = ExtractVersionString(verInTitle.Groups[1].Value.Trim());
                    }
                }

                // Fallback: Titleから取れなければリンクテキストから
                if (string.IsNullOrWhiteSpace(versionStr))
                {
                    versionStr = ExtractVersionString(linkText);
                }

                // Fallback: それでもなければ Title全体から抽出
                if (string.IsNullOrWhiteSpace(versionStr) && !string.IsNullOrWhiteSpace(titleValue))
                {
                    versionStr = ExtractVersionString(titleValue);
                }

                list.Add(new DownloadCandidate
                {
                    FileId = fileId,
                    RawText = !string.IsNullOrEmpty(titleValue) ? titleValue : linkText,
                    VersionStr = versionStr,
                    Url = $"https://skyrimspecialedition.2game.info/jp_download.php?file_id={fileId}&id={modId}"
                });
            }
            return list;
        }

        private DownloadCandidate? SelectBestVersion(string modVerStr, List<DownloadCandidate> candidates, Action<string> log, string espName)
        {
            var modVer = ParseVersion(modVerStr);
            var parsedCandidates = candidates.Select(c => new
            {
                Data = c,
                Ver = ParseVersion(c.VersionStr)
            }).ToList();

            foreach (var cand in parsedCandidates)
            {
                if (IsVersionMatch(modVer, cand.Ver))
                {
                    log($"  -> Version Match: Mod[{modVerStr}] == Trans[{cand.Data.VersionStr}]");
                    return cand.Data;
                }
            }

            var bestFallback = parsedCandidates.OrderByDescending(c => c.Ver).FirstOrDefault();

            if (bestFallback != null)
            {
                log($"  -> Fallback (Highest): Mod[{modVerStr}] != Trans[{bestFallback.Data.VersionStr}]");
                return bestFallback.Data;
            }

            return null;
        }

        private bool IsVersionMatch(Version modVer, Version transVer)
        {
            if (transVer.Major != -1 && transVer.Major != modVer.Major) return false;
            if (transVer.Minor != -1 && transVer.Minor != modVer.Minor) return false;
            if (transVer.Build != -1 && transVer.Build != modVer.Build) return false;
            if (transVer.Revision != -1 && transVer.Revision != modVer.Revision) return false;
            return true;
        }

        private Version ParseVersion(string verStr)
        {
            var match = Regex.Match(verStr, @"(\d+(\.\d+)*)");
            if (match.Success)
            {
                string cleanVer = match.Groups[1].Value;
                if (Version.TryParse(cleanVer, out var v)) return v;
            }
            return new Version(0, 0);
        }

        private string ExtractVersionString(string text)
        {
            // 数字とドットの連続抽出
            var match = Regex.Match(text, @"\d+(\.\d+)+");
            if (match.Success) return match.Value;

            // "v1.5" や "v 0.2" のようなパターン
            match = Regex.Match(text, @"v\s?(\d+(\.\d+)*)", RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups[1].Value;

            // 単独の数字 (例: "Patch 2")
            // ただし誤爆しやすいので慎重に。ここでは単純な数字も許可する
            match = Regex.Match(text, @"\d+");
            if (match.Success) return match.Value;

            return text;
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "0.0";
            return Regex.Replace(name, @"[\\/:*?""<>|]", "_");
        }

        private string DetectExtension(byte[] data)
        {
            if (data.Length < 4) return ".xml";
            if (data[0] == 0x50 && data[1] == 0x4B) return ".zip";
            if (data[0] == 0x37 && data[1] == 0x7A) return ".7z";
            if (data[0] == 0x52 && data[1] == 0x61) return ".rar";
            return ".xml";
        }

        private class DownloadCandidate
        {
            public int FileId { get; set; }
            public string Url { get; set; } = "";
            public string RawText { get; set; } = "";
            public string VersionStr { get; set; } = "";
        }
    }
}