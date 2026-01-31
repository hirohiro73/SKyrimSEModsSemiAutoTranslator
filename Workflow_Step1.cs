using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class Workflow_Step1
    {
        private readonly TranslationContext _ctx;

        public Workflow_Step1(TranslationContext context)
        {
            _ctx = context;
        }

        public async Task Run(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            log("=== [Step 1] 翻訳対象の抽出を開始します ===");

            // ---------------------------------------------------------
            //  Part A: Plugin Files
            // ---------------------------------------------------------
            log("プラグインファイルをスキャン中...");
            onProgress(0, 0, "Step 1: プラグインファイルをスキャン中...");

            var pluginFiles = Directory.GetFiles(_ctx.Mo2ModDir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".esp", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".esm", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".esl", StringComparison.OrdinalIgnoreCase));

            int scanCount = 0;
            int copyCount = 0;
            int skipCc = 0;
            int skipDone = 0;
            int skipNoText = 0;

            await Task.Run(() =>
            {
                int totalFiles = pluginFiles.Count();
                foreach (var file in pluginFiles)
                {
                    token.ThrowIfCancellationRequested();
                    scanCount++;

                    // UI更新: ファイル名は出さず、カウントと固定メッセージのみ
                    if (scanCount % 10 == 0)
                        onProgress(scanCount, totalFiles, $"Step 1: プラグインスキャン中 ({scanCount}/{totalFiles})...");

                    string fileName = Path.GetFileName(file);

                    // [除外1] Creation Club
                    if (fileName.StartsWith("cc", StringComparison.OrdinalIgnoreCase) &&
                        fileName.Contains("sse", StringComparison.OrdinalIgnoreCase))
                    {
                        skipCc++;
                        continue;
                    }

                    // [除外2] 翻訳済みチェック
                    string existingJsonPath = Path.Combine(_ctx.CacheDir, "SKSE", "Plugins", "DynamicStringDistributor", fileName, "SSE-AT_output.json");
                    if (File.Exists(existingJsonPath))
                    {
                        log($"[Skip] 翻訳済み: {fileName}");
                        skipDone++;
                        continue;
                    }

                    // コピー実行
                    string destPath = Path.Combine(_ctx.TargetEspDir, fileName);
                    File.Copy(file, destPath, true);

                    // meta.ini
                    string metaSrc = Path.Combine(Path.GetDirectoryName(file)!, "meta.ini");
                    if (File.Exists(metaSrc))
                        File.Copy(metaSrc, Path.Combine(_ctx.TargetEspDir, fileName + "_meta.ini"), true);

                    // [除外3] 文字列チェック
                    if (!HasTranslatableStrings(file))
                    {
                        log($"[Skip] 文字列なし: {fileName}");
                        if (File.Exists(destPath)) File.Delete(destPath);
                        string metaDest = Path.Combine(_ctx.TargetEspDir, fileName + "_meta.ini");
                        if (File.Exists(metaDest)) File.Delete(metaDest);
                        skipNoText++;
                    }
                    else
                    {
                        log($"[Target] 対象追加: {fileName}");
                        copyCount++;
                    }
                }
            }, token);

            log("----------");
            log($"[Step 1 Result - ESP]");
            log($"  - 総スキャン数: {scanCount}");
            log($"  - 除外(CC/済/空): {skipCc}/{skipDone}/{skipNoText}");
            log($"  => 翻訳対象: {copyCount} 件");
            log("----------");

            // ---------------------------------------------------------
            //  Part B: MCM Text Files
            // ---------------------------------------------------------
            log("MCMテキストファイルを抽出中...");
            onProgress(0, 0, "Step 1: MCMテキストを抽出中...");

            int mcmCount = 0;

            var transDirs = Directory.GetDirectories(_ctx.Mo2ModDir, "translations", SearchOption.AllDirectories)
                    .Where(d => d.EndsWith(@"interface\translations", StringComparison.OrdinalIgnoreCase))
                    .ToList();

            await Task.Run(() =>
            {
                int totalDirs = transDirs.Count;
                int currentDir = 0;

                foreach (var dir in transDirs)
                {
                    token.ThrowIfCancellationRequested();
                    currentDir++;
                    onProgress(currentDir, totalDirs, $"Step 1: MCM抽出中 ({currentDir}/{totalDirs})...");

                    var engFiles = Directory.GetFiles(dir, "*_english.txt");
                    foreach (var engPath in engFiles)
                    {
                        string baseName = Path.GetFileName(engPath).Replace("_english.txt", "", StringComparison.OrdinalIgnoreCase);
                        string jpPath = Path.Combine(dir, baseName + "_japanese.txt");
                        string destPath = Path.Combine(_ctx.TargetMcmDir, baseName + "_japanese.txt");

                        bool shouldCopy = false;

                        if (!File.Exists(jpPath))
                        {
                            log($"[MCM] {baseName}: 英語版のみ -> 対象追加");
                            File.Copy(engPath, destPath, true);
                            shouldCopy = true;
                        }
                        else
                        {
                            string content = File.ReadAllText(jpPath);
                            if (!Regex.IsMatch(content, @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]"))
                            {
                                log($"[MCM] {baseName}: 日本語版(未翻訳) -> 対象追加");
                                File.Copy(jpPath, destPath, true);
                                shouldCopy = true;
                            }
                        }
                        if (shouldCopy) mcmCount++;
                    }
                }
            }, token);

            log($"  => 翻訳対象MCMファイル数: {mcmCount}");
            log("Step 1 完了");
            onProgress(100, 100, "Step 1 完了");
        }

        private bool HasTranslatableStrings(string espPath)
        {
            string tempDir = _ctx.TempInternalDir;
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
            else
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(tempDir);
                    foreach (FileInfo file in di.GetFiles()) file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                }
                catch { }
            }

            string fileName = Path.GetFileName(espPath);
            RunProcessHelper.RunProcess(_ctx.SseAtExePath, $"esp2dsd -o \"{tempDir}\" \"{espPath}\" \"{espPath}\"");

            string checkPath = Path.Combine(tempDir, "SKSE", "Plugins", "DynamicStringDistributor", fileName, "0.json");

            if (File.Exists(checkPath))
            {
                string jsonContent = File.ReadAllText(checkPath).Trim();
                if (jsonContent.Length > 2 && jsonContent != "[]") return true;
            }
            return false;
        }
    }
}