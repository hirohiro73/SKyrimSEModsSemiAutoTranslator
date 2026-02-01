using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class Workflow_Step5
    {
        private readonly TranslationContext _ctx;

        public Workflow_Step5(TranslationContext context)
        {
            _ctx = context;
        }

        public async Task Run(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            log("=== [Step 5] esp2dsd変換 & MCM配備 (Finalize) ===");
            onProgress(0, 0, "Step 5: 処理開始...");

            // 1. 出力先設定の確認
            if (string.IsNullOrWhiteSpace(_ctx.FinalDestDir))
            {
                log("[Skip] 最終出力フォルダ(FinalDestDir)が設定されていません。スキップします。");
                return;
            }

            // フォルダ準備
            string finalDest = _ctx.FinalDestDir;
            string finalMcmDir = Path.Combine(finalDest, "interface", "translations");

            if (!Directory.Exists(finalDest)) Directory.CreateDirectory(finalDest);
            if (!Directory.Exists(finalMcmDir)) Directory.CreateDirectory(finalMcmDir);

            int countEsp = 0;
            int countJson = 0;
            int countMcm = 0;

            await Task.Run(() =>
            {
                // ---------------------------------------------------------
                // A. ESP -> esp2dsd (JSON生成)
                // ---------------------------------------------------------
                if (Directory.Exists(_ctx.TranslatedEspDir))
                {
                    var translatedEsps = Directory.GetFiles(_ctx.TranslatedEspDir, "*.es?", SearchOption.TopDirectoryOnly);
                    int total = translatedEsps.Length;
                    int current = 0;

                    foreach (var translatedPath in translatedEsps)
                    {
                        token.ThrowIfCancellationRequested();
                        current++;
                        string fileName = Path.GetFileName(translatedPath);
                        string originalPath = Path.Combine(_ctx.TargetEspDir, fileName);

                        if (!File.Exists(originalPath))
                        {
                            log($"[Skip] 原文ESPが見つかりません: {fileName}");
                            continue;
                        }

                        onProgress(current, total, $"Step 5: esp2dsd変換中 ({fileName})...");
                        string args = $"esp2dsd -o \"{finalDest}\" \"{translatedPath}\" \"{originalPath}\"";

                        try
                        {
                            RunProcessHelper.RunProcess(_ctx.SseAtExePath, args);
                            // esp2dsdは拡張子ありのフォルダ名を作る (例: Mod.esp/0.json)
                            string dsdDir = Path.Combine(finalDest, "SKSE", "Plugins", "DynamicStringDistributor", fileName);

                            if (Directory.Exists(dsdDir) && Directory.GetFiles(dsdDir, "*.json").Length > 0)
                            {
                                log($"  [ESP->JSON] {fileName} : 成功");
                                countEsp++;
                                countJson += Directory.GetFiles(dsdDir, "*.json").Length;
                            }
                            else
                            {
                                log($"  [Warn] JSONが生成されませんでした: {fileName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            log($"  [Error] esp2dsd実行エラー ({fileName}): {ex.Message}");
                        }
                    }
                }

                // ---------------------------------------------------------
                // B. MCMテキスト (*.txt) のコピー (FinalDestへ)
                // ---------------------------------------------------------
                onProgress(0, 0, "Step 5: MCMファイルを配備中...");

                // 1. xTranslatorで翻訳したテキスト (TranslatedTxtDir) をコピー
                if (Directory.Exists(_ctx.TranslatedTxtDir))
                {
                    var translatedTxts = Directory.GetFiles(_ctx.TranslatedTxtDir, "*.txt", SearchOption.TopDirectoryOnly);
                    foreach (var file in translatedTxts)
                    {
                        token.ThrowIfCancellationRequested();
                        string fileName = Path.GetFileName(file);
                        string destPath = Path.Combine(finalMcmDir, fileName);

                        try
                        {
                            File.Copy(file, destPath, true);
                            log($"  [MCM Copy (Translated)] {fileName}");
                            countMcm++;
                        }
                        catch (Exception ex)
                        {
                            log($"  [Error] MCMコピー失敗 (Translated): {ex.Message}");
                        }
                    }
                }

                // 2. アーカイブ由来の翻訳済みテキスト (SortedTxtDir) をコピー
                if (Directory.Exists(_ctx.SortedTxtDir))
                {
                    var sortedTxts = Directory.GetFiles(_ctx.SortedTxtDir, "*.txt", SearchOption.TopDirectoryOnly);
                    foreach (var file in sortedTxts)
                    {
                        token.ThrowIfCancellationRequested();
                        string fileName = Path.GetFileName(file);
                        string destPath = Path.Combine(finalMcmDir, fileName);

                        try
                        {
                            File.Copy(file, destPath, true);
                            log($"  [MCM Copy (Archive)] {fileName}");
                            countMcm++;
                        }
                        catch (Exception ex)
                        {
                            log($"  [Error] MCMコピー失敗 (Archive): {ex.Message}");
                        }
                    }
                }


                if (countMcm == 0 && countEsp > 0)
                {
                    // ESPはあるがMCMがないだけなら正常かもしれないのでWarnレベルは下げておく
                    // log("[Info] MCMテキストの配備はありませんでした。");
                }

            }, token);

            log("----------");
            log($"[Step 5 Result]");
            log($"  - 出力先: {finalDest}");
            log($"  - 処理したESP: {countEsp} 件 (JSON: {countJson} 個)");
            log($"  - MCMテキスト: {countMcm} 件");

            onProgress(100, 100, "Step 5 完了");
        }
    }
}