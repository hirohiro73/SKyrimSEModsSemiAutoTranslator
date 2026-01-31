using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class Workflow
    {
        private AppSettings _settings { get; set; }

        public Workflow(AppSettings settings) { _settings = settings; }

        public async Task Process(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var stepStopwatch = new Stopwatch();
            var timeReport = new StringBuilder();

            timeReport.AppendLine("各ステップの所要時間:");
            timeReport.AppendLine("-----------------------------");

            var context = new TranslationContext(_settings);

            try
            {
                // =========================================================
                // 初期化: フォルダのクリーンアップ
                // =========================================================
                log("環境の初期化(クリーンアップ)を開始します...");
                onProgress(0, 0, "初期化中...");

                await Task.Run(() =>
                {
                    // ワークフォルダのクリーンアップ
                    CleanupDirectory(context.WorkRootDir, log);
                    CleanupDirectory(context.FinalDestDir, log);
                }, token);

                // ワークフォルダ内の各フォルダを作成
                context.EnsureDirectories();

                log("クリーンアップ完了。");

                // =========================================================
                // Step 1 : 翻訳対象の抽出
                // =========================================================
                stepStopwatch.Restart();

                var runStep1 = new Workflow_Step1(context);
                await runStep1.Run(log, onProgress, token);

                stepStopwatch.Stop();
                RecordTime("Step 1 (抽出)", stepStopwatch.Elapsed, log, timeReport);

                // =========================================================
                // Step 2 : 翻訳ファイルのダウンロード
                // =========================================================
                stepStopwatch.Restart();

                var runStep2 = new Workflow_Step2(context);
                await runStep2.Run(log, onProgress, token);

                stepStopwatch.Stop();
                RecordTime("Step 2 (DL)", stepStopwatch.Elapsed, log, timeReport);

                // =========================================================
                // Step 3 : 展開・仕分け・マージ・バッチ生成
                // =========================================================
                stepStopwatch.Restart();

                var runStep3 = new Workflow_Step3(context);
                await runStep3.Run(log, onProgress, token);

                stepStopwatch.Stop();
                RecordTime("Step 3 (統合)", stepStopwatch.Elapsed, log, timeReport);

                // =========================================================
                // Step 4 : 手動実行ガイド
                // =========================================================
                stepStopwatch.Restart();

                var runStep4 = new Workflow_Step4(context);
                await runStep4.Run(log, onProgress, token);

                stepStopwatch.Stop();
                RecordTime("Step 4 (手動操作)", stepStopwatch.Elapsed, log, timeReport);

                // =========================================================
                // Step 5 : 成果物の配備 (Finalize)
                // =========================================================
                stepStopwatch.Restart();

                var runStep5 = new Workflow_Step5(context);
                await runStep5.Run(log, onProgress, token);

                stepStopwatch.Stop();
                RecordTime("Step 5 (配備)", stepStopwatch.Elapsed, log, timeReport);

                // =========================================================
                // 完了処理
                // =========================================================
                totalStopwatch.Stop();
                TimeSpan totalTime = totalStopwatch.Elapsed;

                // ワークフォルダのクリーンアップ
                CleanupDirectory(context.WorkRootDir, log);

                log("------------------------------------------------");
                log($"全ての工程が完了しました。総所要時間: {FormatTime(totalTime)}");

                timeReport.AppendLine("-----------------------------");
                timeReport.AppendLine($"合計時間: {FormatTime(totalTime)}");

                ShowSummaryMessageBox(timeReport.ToString());
            }
            catch (Exception)
            {
                totalStopwatch.Stop();
                throw;
            }
        }

        // フォルダ削除＆再作成ヘルパー
        private void CleanupDirectory(string path, Action<string> log)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    // 削除
                    Directory.Delete(path, true);
                    log($"  [Clean] 削除しました: {path}");
                }
                // 再作成
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                // エラーが出てもログに出して続行する（権限エラーなどで落ちないように）
                log($"  [Warn] クリーンアップ失敗 ({Path.GetFileName(path)}): {ex.Message}");
            }
        }

        private void RecordTime(string stepName, TimeSpan ts, Action<string> log, StringBuilder report)
        {
            string formatted = FormatTime(ts);
            log($"[Timer] {stepName} 完了: {formatted}");
            report.AppendLine($"{stepName.PadRight(15)} : {formatted}");
        }

        private string FormatTime(TimeSpan ts)
        {
            return ts.ToString(@"mm\:ss\.ff");
        }

        private void ShowSummaryMessageBox(string message)
        {
            if (Application.OpenForms.Count > 0)
            {
                Application.OpenForms[0]!.Invoke(new Action(() =>
                {
                    MessageBox.Show(message, "処理完了レポート", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
        }
    }
}