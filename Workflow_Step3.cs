using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class Workflow_Step3
    {
        private readonly TranslationContext _ctx;

        public Workflow_Step3(TranslationContext context)
        {
            _ctx = context;
        }

        public async Task Run(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            log("=== [Step 3] 展開・仕分け・マージ・バッチ生成 ===");
            onProgress(0, 0, "Step 3: 開始...");
            _ctx.EnsureDirectories();

            // ---------------------------------------------------------
            // Phase 1: 翻訳ファイルの展開と仕分け
            // ---------------------------------------------------------
            await ExtractAndSortAsync(log, onProgress, token);

            // ---------------------------------------------------------
            // Phase 2: XMLのマージ
            // ---------------------------------------------------------
            log("翻訳XMLを解析・マージ中...");
            onProgress(0, 0, "Step 3: マージXMLを作成中...");

            // ESP用マッチング (厳密一致)
            var targetEspFiles = Directory.GetFiles(_ctx.TargetEspDir, "*.es?", SearchOption.TopDirectoryOnly);
            var availableEspXmls = Directory.GetFiles(_ctx.SortedEspDir, "*.xml");

            var espXmlPairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var matchedXmls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var espPath in targetEspFiles)
            {
                string espName = Path.GetFileNameWithoutExtension(espPath);
                string expectedXmlName = $"{espName}_english_japanese.xml";

                var match = availableEspXmls.FirstOrDefault(xml =>
                {
                    string xmlName = Path.GetFileName(xml);
                    return xmlName.Equals(expectedXmlName, StringComparison.OrdinalIgnoreCase);
                });

                if (match != null)
                {
                    espXmlPairs[espPath] = match;
                    matchedXmls.Add(match);
                }
            }

            var xmlsToMergeEsp = availableEspXmls.Where(x => !matchedXmls.Contains(x)).ToArray();
            await CreateMergedXmlAsync(xmlsToMergeEsp, _ctx.MergedEspXmlPath, token);
            log($"[Merge-ESP] 厳密マッチ: {matchedXmls.Count}件, マージ対象(Fallback): {xmlsToMergeEsp.Length}件");

            // MCM用全マージ
            var availableMcmXmls = Directory.GetFiles(_ctx.SortedMcmDir, "*.xml");
            await CreateMergedXmlAsync(availableMcmXmls, _ctx.MergedMcmXmlPath, token);
            log($"[Merge-MCM] マージ対象: {availableMcmXmls.Length}件");

            // ---------------------------------------------------------
            // Phase 3: バッチファイル生成
            // ---------------------------------------------------------
            log("xTranslator用バッチファイルを生成中...");
            onProgress(0, 0, "Step 3: xTranslator用バッチを生成中...");

            // A. ESPバッチ (分割なし)
            await GenerateEspBatchAsync(targetEspFiles, espXmlPairs, _ctx.MergedEspXmlPath, log, token);

            // B. MCMバッチ (分割なし) - 事前コピー処理を追加
            // MCMは上書き翻訳になるため、先に翻訳先フォルダへコピーしておく
            log("MCMファイルを翻訳先フォルダへ事前コピー中...");
            var sourceMcmFiles = Directory.GetFiles(_ctx.TargetMcmDir, "*.txt");
            var batchTargetMcmFiles = new List<string>();

            foreach (var src in sourceMcmFiles)
            {
                string fileName = Path.GetFileName(src);
                string dest = Path.Combine(_ctx.TranslatedTxtDir, fileName);
                File.Copy(src, dest, true);
                batchTargetMcmFiles.Add(dest); // バッチ処理対象はコピー後のファイル
            }
            log($"  -> {batchTargetMcmFiles.Count} 件コピー完了");

            await GenerateMcmBatchAsync(batchTargetMcmFiles, _ctx.MergedMcmXmlPath, log, token);

            onProgress(100, 100, "Step 3 完了");
            log("Step 3 全工程完了。");
        }

        // --- Helper Methods ---

        private async Task ExtractAndSortAsync(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            var archives = Directory.GetFiles(_ctx.DownloadDir)
                .Where(f => Regex.IsMatch(f, @"\.(zip|7z|rar|xml)$", RegexOptions.IgnoreCase)).ToList();

            int total = archives.Count;
            int current = 0;

            foreach (var archive in archives)
            {
                token.ThrowIfCancellationRequested();
                current++;

                onProgress(current, total, $"Step 3: アーカイブ展開・仕分け中 ({current}/{total})...");

                string name = Path.GetFileName(archive);
                string tempDir = Path.Combine(_ctx.WorkRootDir, $"_Temp_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);

                try
                {
                    if (archive.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        File.Copy(archive, Path.Combine(tempDir, name));
                    else
                        await Task.Run(() => RunProcessHelper.RunProcess(_ctx.SevenZipPath, $"x \"{archive}\" -o\"{tempDir}\" -y"));

                    log($"[Extract] 展開完了: {name}");

                    foreach (var file in Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories))
                    {
                        string fname = Path.GetFileName(file).ToLower();
                        string? destDir = null;

                        if (fname.EndsWith(".xml"))
                        {
                            if (fname.Contains("_mcm_")) destDir = _ctx.SortedMcmDir;
                            else if (fname.Contains("_pex_")) destDir = _ctx.SortedPexDir;
                            else destDir = _ctx.SortedEspDir;
                        }
                        else if (fname.EndsWith(".txt") && fname.Contains("japanese"))
                        {
                            destDir = _ctx.SortedTxtDir;
                        }

                        if (destDir != null)
                            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
                    }
                }
                catch (Exception ex) { log($"[Warn] 展開エラー {name}: {ex.Message}"); }
                finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
            }
        }

        private async Task CreateMergedXmlAsync(string[] xmlFiles, string outputPath, CancellationToken token)
        {
            if (xmlFiles.Length == 0) return;
            using (var sw = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                await sw.WriteLineAsync("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                await sw.WriteLineAsync("<SSTXMLRessources><Params><Addon>Merged</Addon><Source>english</Source><Dest>japanese</Dest><Version>2</Version></Params><Content>");
                foreach (var xml in xmlFiles)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        string c = await File.ReadAllTextAsync(xml, token);
                        c = Regex.Replace(c, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
                        var m = Regex.Match(c, @"(?s)<Content>(.*)</Content>");
                        if (m.Success) await sw.WriteLineAsync(m.Groups[1].Value);
                    }
                    catch { }
                }
                await sw.WriteLineAsync("</Content></SSTXMLRessources>");
            }
        }

        private async Task GenerateEspBatchAsync(IEnumerable<string> files, Dictionary<string, string> pairs, string mergedXml, Action<string> log, CancellationToken token)
        {
            var sb = new StringBuilder();
            var list = files.ToList();
            int count = 0;

            foreach (var espPath in list)
            {
                token.ThrowIfCancellationRequested();
                string espName = Path.GetFileName(espPath);

                sb.AppendLine("StartRule\nLangSource=english\nLangDest=japanese\nusedatadir=0");
                sb.AppendLine($"command=loadfile:{espPath}");

                if (pairs.ContainsKey(espPath))
                {
                    log($"  [Batch] {espName} -> {Path.GetFileName(pairs[espPath])} (Strict)");
                    sb.AppendLine($"command=importxml:0:2:{pairs[espPath]}");
                }
                else if (File.Exists(mergedXml))
                {
                    sb.AppendLine($"command=importxml:0:2:{mergedXml}");
                }
                sb.AppendLine("command=finalize\ncommand=SaveFile\ncommand=CloseFile\nEndRule\n");
                count++;
            }

            if (sb.Length > 0)
            {
                string p = Path.Combine(_ctx.BatchFilesDir, "Batch_ESP.txt");
                await File.WriteAllTextAsync(p, sb.ToString(), token);
                log($"ESP Batch生成完了: {Path.GetFileName(p)} ({count} files)");
            }
        }

        private async Task GenerateMcmBatchAsync(IEnumerable<string> files, string mergedXml, Action<string> log, CancellationToken token)
        {
            if (!File.Exists(mergedXml)) return;

            var sb = new StringBuilder();
            var list = files.ToList();
            int count = 0;

            foreach (var txtPath in list)
            {
                token.ThrowIfCancellationRequested();
                sb.AppendLine("StartRule\nLangSource=english\nLangDest=japanese\nusedatadir=0");
                sb.AppendLine($"command=loadfile:{txtPath}");
                sb.AppendLine($"command=importxml:0:2:{mergedXml}");
                sb.AppendLine("command=finalize\ncommand=SaveFile\ncommand=CloseFile\nEndRule\n");
                count++;
            }

            if (sb.Length > 0)
            {
                string p = Path.Combine(_ctx.BatchFilesDir, "Batch_MCM.txt");
                await File.WriteAllTextAsync(p, sb.ToString(), token);
                log($"MCM Batch生成完了: {Path.GetFileName(p)} ({count} files)");
            }
        }
    }
}