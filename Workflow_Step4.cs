using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class Workflow_Step4
    {
        private readonly TranslationContext _ctx;

        public Workflow_Step4(TranslationContext context)
        {
            _ctx = context;
        }

        public async Task Run(Action<string> log, Action<int, int, string> onProgress, CancellationToken token)
        {
            log("=== [Step 4] xTranslator 手動実行ガイド ===");
            onProgress(0, 0, "Step 4: ユーザー操作待ち...");

            string msg = $"""
            [ESP翻訳]
            1. オプション->オプションの「特定のフォルダにEsp/Esmを出力する」に以下のパスを指定してください
               {_ctx.TranslatedEspDir}
            
            2. ウィザード->バッチプロセッサを開き、ファイル->バッチプロセッサの読み込みで
               以下のフォルダにある Batch_ESP.txt を選択後、プロセッサを実行を押下してください
               {_ctx.BatchFilesDir}

            ---------------------------------------------------------

            [MCM翻訳]
            ※ 保存先はスクリプトで自動設定済みです。オプション設定の変更は不要です。
            
            1. ウィザード->バッチプロセッサを開き、ファイル->バッチプロセッサの読み込みで
               以下のフォルダにある Batch_MCM.txt を選択後、プロセッサを実行を押下してください
               {_ctx.BatchFilesDir}
            """;

            await Task.Run(() =>
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0]!.Invoke(new Action(() =>
                    {
                        using (var dlg = new InstructionDialog(msg))
                        {
                            dlg.ShowDialog();
                        }
                    }));
                }
            }, token);

            log("Step 4 完了 (ユーザーにより翻訳完了が確認されました)");
            onProgress(100, 100, "Step 4 完了");
        }
    }
}