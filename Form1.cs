using System.IO;
using System.Text.Json;

namespace SKyrimSEModsSemiAutoTranslator
{
    public partial class mainWindow : Form
    {
        private string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private CancellationTokenSource? _cts;

        public mainWindow()
        {
            InitializeComponent();
            LoadSettings();

            this.tbCacheTranslationFilesDir.Validating += tbCacheTranslationFilesDir_Validating;
            this.tbFinalDestDir.Validating += tbFinalDestDir_Validating;

        }

        private void tbCacheTranslationFilesDir_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // ワークフォルダが入力されていない場合はチェックしない
            if (string.IsNullOrWhiteSpace(tbWorkDir.Text)) return;

            if (IsSubPathOf(tbWorkDir.Text, tbCacheTranslationFilesDir.Text))
            {
                MessageBox.Show(
                    "キャッシュフォルダをワークフォルダの下に置かないでください。\n(処理時に削除されてしまいます)",
                    "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tbFinalDestDir_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbWorkDir.Text)) return;

            if (IsSubPathOf(tbWorkDir.Text, tbFinalDestDir.Text))
            {
                MessageBox.Show(
                    "翻訳結果格納フォルダをワークフォルダの下に置かないでください。\n(処理時に削除されてしまいます)",
                    "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveSettings(AppSettings settings)
        {
            settings.MO2ModDir = tbMO2ModsDir.Text;
            settings.SseATPath = tbSseAtPath.Text;
            settings.SevenZipPath = tb7zPath.Text;
            settings.TranslationFileCacheDir = tbCacheTranslationFilesDir.Text;
            settings.ReferedJsonDir = tbReferedJsonDir.Text;
            settings.WorkDir = tbWorkDir.Text;
            settings.FinalDestDir = tbFinalDestDir.Text;

            // JSONに変換して保存
            // WriteIndented=true で人間が見やすいように整形
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }
        private void LoadSettings()
        {
            if (!File.Exists(settingsPath))
                return; // ファイルがなければ何もしない

            try
            {
                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings != null)
                {
                    tbMO2ModsDir.Text = settings.MO2ModDir;
                    tbSseAtPath.Text = settings.SseATPath;
                    tb7zPath.Text = settings.SevenZipPath;
                    tbCacheTranslationFilesDir.Text = settings.TranslationFileCacheDir;
                    tbReferedJsonDir.Text = settings.ReferedJsonDir;
                    tbWorkDir.Text = settings.WorkDir;
                    tbFinalDestDir.Text = settings.FinalDestDir;
                }
            }
            catch
            {
                // 読み込み失敗時は無視するかログ出す
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings(new AppSettings());
        }

        private async void onClick_btnProcessStart(object sender, EventArgs e)
        {
            var settings = new AppSettings();
            SaveSettings(settings);

            btnProcessStart.Enabled = false;
            btnCancel.Enabled = true;

            _cts = new CancellationTokenSource();

            var workflow = new Workflow(settings);
            try
            {
                await Task.Run(() => workflow.Process(AppendLog, UpdateProgress, _cts.Token));
            }
            catch (OperationCanceledException)
            {
                // 4. 中止された場合ここに来る
                AppendLog("処理がユーザーによって中止されました。");
                label_Progress.Text = "中止されました";
                pbProgress.Maximum = 0;
                pbProgress.Value = 0;
            }
            catch (Exception ex)
            {
                AppendLog($"エラー: {ex.Message}");
            }
            finally
            {
                _cts.Dispose();
                _cts = null;

                btnProcessStart.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        private void AppendLog(string message)
        {
            // 別スレッドから呼ばれた場合、メインスレッド(UI)に投げ直す (必須)
            if (this.tbLog.InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            // --- ここはメインスレッド ---

            // タイムスタンプを付けるとかっこいい (任意)
            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            // テキストボックスに追加 + 改行
            this.tbLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");

            // 常に一番下までスクロールさせる処理 (任意だが推奨)
            this.tbLog.SelectionStart = this.tbLog.Text.Length;
            this.tbLog.ScrollToCaret();
        }

        private void UpdateProgress(int current, int total, string message)
        {
            // 別スレッドから呼ばれた場合、メインスレッドに投げ直す (必須)
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, int, string>(UpdateProgress), current, total, message);
                return;
            }

            // --- 以下、安全にUIを操作できるエリア ---
            label_Progress.Text = message;

            pbProgress.Maximum = total;
            pbProgress.Value = current;
        }

        private void onClick_btnMO2ModsDir(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var fbd = new FolderBrowserDialog())
            {
                // オプション設定
                fbd.Description = "MO2のmodsフォルダを選択してください";
                fbd.ShowNewFolderButton = false; // 「新しいフォルダ作成」ボタンを表示するか

                // もしテキストボックスに既にパスが入っていれば、そこを初期位置にする
                if (Directory.Exists(tbMO2ModsDir.Text))
                {
                    fbd.SelectedPath = tbMO2ModsDir.Text;
                }

                // ダイアログを表示し、「OK」が押されたら処理する
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたパスをテキストボックスに入れる
                    tbMO2ModsDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void onClick_btnSseAtPath(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var ofd = new OpenFileDialog())
            {
                // === オプション設定 ===
                ofd.Title = "SSE-AT.exe を選択してください";

                // フィルタ設定：ユーザーが見つけやすいように .exe だけ表示させます
                // 書式: "表示名|拡張子パターン|表示名2|拡張子パターン2..."
                ofd.Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*";

                // ダイアログを閉じた時に、カレントディレクトリを元に戻す（重要）
                ofd.RestoreDirectory = true;

                // 初期位置の設定（もしテキストボックスに既にパスが入っていれば）
                if (File.Exists(tbSseAtPath.Text))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(tbSseAtPath.Text); // フォルダ部分だけ抽出
                    ofd.FileName = Path.GetFileName(tbSseAtPath.Text);              // ファイル名部分
                }

                // === ダイアログ表示 & OK判定 ===
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたフルパスをテキストボックスに入れる
                    tbSseAtPath.Text = ofd.FileName;
                }
            }
        }

        private void onClick_btn7zPath(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var ofd = new OpenFileDialog())
            {
                // === オプション設定 ===
                ofd.Title = "7z.exe を選択してください";

                // フィルタ設定：ユーザーが見つけやすいように .exe だけ表示させます
                // 書式: "表示名|拡張子パターン|表示名2|拡張子パターン2..."
                ofd.Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*";

                // ダイアログを閉じた時に、カレントディレクトリを元に戻す（重要）
                ofd.RestoreDirectory = true;

                // 初期位置の設定（もしテキストボックスに既にパスが入っていれば）
                if (File.Exists(tb7zPath.Text))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(btn7zPath.Text); // フォルダ部分だけ抽出
                    ofd.FileName = Path.GetFileName(tb7zPath.Text);              // ファイル名部分
                }

                // === ダイアログ表示 & OK判定 ===
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたフルパスをテキストボックスに入れる
                    tb7zPath.Text = ofd.FileName;
                }
            }

        }

        private void onClick_btnCacheTranslationFilesDir(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var fbd = new FolderBrowserDialog())
            {
                // オプション設定
                fbd.Description = "翻訳ファイル キャッシュ フォルダを選択してください";
                fbd.ShowNewFolderButton = true; // 「新しいフォルダ作成」ボタンを表示するか

                // もしテキストボックスに既にパスが入っていれば、そこを初期位置にする
                if (Directory.Exists(tbCacheTranslationFilesDir.Text))
                {
                    fbd.SelectedPath = tbCacheTranslationFilesDir.Text;
                }

                // ダイアログを表示し、「OK」が押されたら処理する
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたパスをテキストボックスに入れる
                    tbCacheTranslationFilesDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void onClick_btnWorkDir(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var fbd = new FolderBrowserDialog())
            {
                // オプション設定
                fbd.Description = "ワーク フォルダを選択してください";
                fbd.ShowNewFolderButton = true; // 「新しいフォルダ作成」ボタンを表示するか

                // もしテキストボックスに既にパスが入っていれば、そこを初期位置にする
                if (Directory.Exists(tbWorkDir.Text))
                {
                    fbd.SelectedPath = tbWorkDir.Text;
                }

                // ダイアログを表示し、「OK」が押されたら処理する
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたパスをテキストボックスに入れる
                    tbWorkDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void onClick_btnReferedJsonDir(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var fbd = new FolderBrowserDialog())
            {
                // オプション設定
                fbd.Description = "翻訳済み json フォルダを選択してください";
                fbd.ShowNewFolderButton = false; // 「新しいフォルダ作成」ボタンを表示するか

                // もしテキストボックスに既にパスが入っていれば、そこを初期位置にする
                if (Directory.Exists(tbReferedJsonDir.Text))
                {
                    fbd.SelectedPath = tbReferedJsonDir.Text;
                }

                // ダイアログを表示し、「OK」が押されたら処理する
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたパスをテキストボックスに入れる
                    tbReferedJsonDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void onClick_btnFinalDestDir(object sender, EventArgs e)
        {
            // ダイアログのインスタンス作成
            using (var fbd = new FolderBrowserDialog())
            {
                // オプション設定
                fbd.Description = "翻訳結果格納フォルダを選択してください";
                fbd.ShowNewFolderButton = true; // 「新しいフォルダ作成」ボタンを表示するか

                // もしテキストボックスに既にパスが入っていれば、そこを初期位置にする
                if (Directory.Exists(tbFinalDestDir.Text))
                {
                    fbd.SelectedPath = tbFinalDestDir.Text;
                }

                // ダイアログを表示し、「OK」が押されたら処理する
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 選ばれたパスをテキストボックスに入れる
                    tbFinalDestDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void onClick_btnCancel(object sender, EventArgs e)
        {
            // キャンセルを指令する
            if (_cts != null)
            {
                _cts.Cancel();
                btnCancel.Enabled = false;
                AppendLog("中止処理中...");
            }
        }
        // ---------------------------------------------------------
        //  Helper: ディレクトリの包含関係チェック
        // ---------------------------------------------------------
        private bool IsSubPathOf(string baseDir, string checkDir)
        {
            if (string.IsNullOrWhiteSpace(baseDir) || string.IsNullOrWhiteSpace(checkDir))
            {
                return false;
            }

            try
            {
                // フルパスに正規化し、末尾の区切り文字を削除して統一
                string fullBase = Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string fullCheck = Path.GetFullPath(checkDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // 1. 全く同じフォルダの場合 -> NG
                if (string.Equals(fullBase, fullCheck, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // 2. checkDir が baseDir のサブフォルダか判定
                // 文字列として StartsWith かつ、その直後が区切り文字であることを確認
                // (例: C:\Work と C:\Worker は区別する)
                if (fullCheck.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                {
                    // ベースパスの長さの位置にある文字がセパレータならサブフォルダ
                    char separator = fullCheck[fullBase.Length];
                    if (separator == Path.DirectorySeparatorChar || separator == Path.AltDirectorySeparatorChar)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // パスとして不正な文字列などの場合は一旦無視（実行時にエラーになる）
                return false;
            }

            return false;
        }

        // 共通の警告表示メソッド
        private bool ValidateDirectories()
        {
            string workDir = tbWorkDir.Text;

            // キャッシュフォルダのチェック
            if (IsSubPathOf(workDir, tbCacheTranslationFilesDir.Text))
            {
                MessageBox.Show(
                    "「翻訳ファイル キャッシュフォルダ」が「ワークフォルダ」の中、または同じ場所に設定されています。\n" +
                    "ワークフォルダは処理開始時に初期化(削除)されるため、別の場所を指定してください。",
                    "設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 最終出力フォルダのチェック
            if (IsSubPathOf(workDir, tbFinalDestDir.Text))
            {
                MessageBox.Show(
                    "「翻訳結果格納フォルダ」が「ワークフォルダ」の中、または同じ場所に設定されています。\n" +
                    "ワークフォルダは処理開始時に初期化(削除)されるため、別の場所を指定してください。",
                    "設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
