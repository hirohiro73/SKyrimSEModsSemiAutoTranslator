namespace SKyrimSEModsSemiAutoTranslator
{
    partial class mainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            tbMO2ModsDir = new TextBox();
            tbSseAtPath = new TextBox();
            tb7zPath = new TextBox();
            tbCacheTranslationFilesDir = new TextBox();
            tbReferedJsonDir = new TextBox();
            tbWorkDir = new TextBox();
            btnMO2ModsDir = new Button();
            btnSseAtPath = new Button();
            btn7zPath = new Button();
            btnCacheTranslationFilesDir = new Button();
            btnReferedJsonDir = new Button();
            btnWorkDir = new Button();
            label7 = new Label();
            tbFinalDestDir = new TextBox();
            btnFinalDestDir = new Button();
            groupBox1 = new GroupBox();
            pbProgress = new ProgressBar();
            tbLog = new TextBox();
            btnProcessStart = new Button();
            label8 = new Label();
            label_Progress = new Label();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(58, 53);
            label1.Name = "label1";
            label1.Size = new Size(152, 15);
            label1.TabIndex = 0;
            label1.Text = "MO2 Mod インストールフォルダ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(90, 83);
            label2.Name = "label2";
            label2.Size = new Size(120, 15);
            label2.TabIndex = 1;
            label2.Text = "SSE-AT.exe ファイルパス";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(76, 113);
            label3.Name = "label3";
            label3.Size = new Size(134, 15);
            label3.TabIndex = 2;
            label3.Text = "7-Zip (7z.exe) ファイルパス";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(64, 145);
            label4.Name = "label4";
            label4.Size = new Size(146, 15);
            label4.TabIndex = 3;
            label4.Text = "翻訳ファイル キャッシュフォルダ";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(91, 206);
            label5.Name = "label5";
            label5.Size = new Size(119, 15);
            label5.TabIndex = 4;
            label5.Text = "翻訳済み DSD 用 json";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(142, 176);
            label6.Name = "label6";
            label6.Size = new Size(68, 15);
            label6.TabIndex = 5;
            label6.Text = "ワークフォルダ";
            // 
            // tbMO2ModsDir
            // 
            tbMO2ModsDir.Location = new Point(232, 50);
            tbMO2ModsDir.Name = "tbMO2ModsDir";
            tbMO2ModsDir.Size = new Size(378, 23);
            tbMO2ModsDir.TabIndex = 6;
            // 
            // tbSseAtPath
            // 
            tbSseAtPath.Location = new Point(232, 80);
            tbSseAtPath.Name = "tbSseAtPath";
            tbSseAtPath.Size = new Size(378, 23);
            tbSseAtPath.TabIndex = 7;
            // 
            // tb7zPath
            // 
            tb7zPath.Location = new Point(232, 110);
            tb7zPath.Name = "tb7zPath";
            tb7zPath.Size = new Size(378, 23);
            tb7zPath.TabIndex = 8;
            // 
            // tbCacheTranslationFilesDir
            // 
            tbCacheTranslationFilesDir.Location = new Point(232, 142);
            tbCacheTranslationFilesDir.Name = "tbCacheTranslationFilesDir";
            tbCacheTranslationFilesDir.Size = new Size(378, 23);
            tbCacheTranslationFilesDir.TabIndex = 9;
            // 
            // tbReferedJsonDir
            // 
            tbReferedJsonDir.Location = new Point(232, 203);
            tbReferedJsonDir.Name = "tbReferedJsonDir";
            tbReferedJsonDir.Size = new Size(378, 23);
            tbReferedJsonDir.TabIndex = 10;
            // 
            // tbWorkDir
            // 
            tbWorkDir.Location = new Point(232, 173);
            tbWorkDir.Name = "tbWorkDir";
            tbWorkDir.Size = new Size(378, 23);
            tbWorkDir.TabIndex = 11;
            // 
            // btnMO2ModsDir
            // 
            btnMO2ModsDir.Location = new Point(618, 50);
            btnMO2ModsDir.Name = "btnMO2ModsDir";
            btnMO2ModsDir.Size = new Size(40, 23);
            btnMO2ModsDir.TabIndex = 12;
            btnMO2ModsDir.Text = "...";
            btnMO2ModsDir.UseVisualStyleBackColor = true;
            btnMO2ModsDir.Click += onClick_btnMO2ModsDir;
            // 
            // btnSseAtPath
            // 
            btnSseAtPath.Location = new Point(618, 80);
            btnSseAtPath.Name = "btnSseAtPath";
            btnSseAtPath.Size = new Size(40, 23);
            btnSseAtPath.TabIndex = 13;
            btnSseAtPath.Text = "...";
            btnSseAtPath.UseVisualStyleBackColor = true;
            btnSseAtPath.Click += onClick_btnSseAtPath;
            // 
            // btn7zPath
            // 
            btn7zPath.Location = new Point(618, 109);
            btn7zPath.Name = "btn7zPath";
            btn7zPath.Size = new Size(40, 23);
            btn7zPath.TabIndex = 14;
            btn7zPath.Text = "...";
            btn7zPath.UseVisualStyleBackColor = true;
            btn7zPath.Click += onClick_btn7zPath;
            // 
            // btnCacheTranslationFilesDir
            // 
            btnCacheTranslationFilesDir.Location = new Point(618, 142);
            btnCacheTranslationFilesDir.Name = "btnCacheTranslationFilesDir";
            btnCacheTranslationFilesDir.Size = new Size(40, 23);
            btnCacheTranslationFilesDir.TabIndex = 15;
            btnCacheTranslationFilesDir.Text = "...";
            btnCacheTranslationFilesDir.UseVisualStyleBackColor = true;
            btnCacheTranslationFilesDir.Click += onClick_btnCacheTranslationFilesDir;
            // 
            // btnReferedJsonDir
            // 
            btnReferedJsonDir.Location = new Point(618, 202);
            btnReferedJsonDir.Name = "btnReferedJsonDir";
            btnReferedJsonDir.Size = new Size(40, 23);
            btnReferedJsonDir.TabIndex = 16;
            btnReferedJsonDir.Text = "...";
            btnReferedJsonDir.UseVisualStyleBackColor = true;
            btnReferedJsonDir.Click += onClick_btnReferedJsonDir;
            // 
            // btnWorkDir
            // 
            btnWorkDir.Location = new Point(618, 173);
            btnWorkDir.Name = "btnWorkDir";
            btnWorkDir.Size = new Size(40, 23);
            btnWorkDir.TabIndex = 17;
            btnWorkDir.Text = "...";
            btnWorkDir.UseVisualStyleBackColor = true;
            btnWorkDir.Click += onClick_btnWorkDir;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(96, 261);
            label7.Name = "label7";
            label7.Size = new Size(114, 15);
            label7.TabIndex = 18;
            label7.Text = "翻訳結果格納フォルダ";
            // 
            // tbFinalDestDir
            // 
            tbFinalDestDir.Location = new Point(232, 258);
            tbFinalDestDir.Name = "tbFinalDestDir";
            tbFinalDestDir.Size = new Size(378, 23);
            tbFinalDestDir.TabIndex = 19;
            // 
            // btnFinalDestDir
            // 
            btnFinalDestDir.Location = new Point(618, 258);
            btnFinalDestDir.Name = "btnFinalDestDir";
            btnFinalDestDir.Size = new Size(40, 23);
            btnFinalDestDir.TabIndex = 20;
            btnFinalDestDir.Text = "...";
            btnFinalDestDir.UseVisualStyleBackColor = true;
            btnFinalDestDir.Click += onClick_btnFinalDestDir;
            // 
            // groupBox1
            // 
            groupBox1.Location = new Point(35, 289);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(621, 2);
            groupBox1.TabIndex = 21;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // pbProgress
            // 
            pbProgress.Location = new Point(232, 311);
            pbProgress.Name = "pbProgress";
            pbProgress.Size = new Size(426, 23);
            pbProgress.TabIndex = 22;
            // 
            // tbLog
            // 
            tbLog.Location = new Point(35, 351);
            tbLog.Multiline = true;
            tbLog.Name = "tbLog";
            tbLog.ReadOnly = true;
            tbLog.ScrollBars = ScrollBars.Vertical;
            tbLog.Size = new Size(623, 240);
            tbLog.TabIndex = 23;
            // 
            // btnProcessStart
            // 
            btnProcessStart.Location = new Point(559, 597);
            btnProcessStart.Name = "btnProcessStart";
            btnProcessStart.Size = new Size(97, 32);
            btnProcessStart.TabIndex = 24;
            btnProcessStart.Text = "処理開始";
            btnProcessStart.UseVisualStyleBackColor = true;
            btnProcessStart.Click += onClick_btnProcessStart;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(91, 221);
            label8.Name = "label8";
            label8.Size = new Size(58, 15);
            label8.TabIndex = 25;
            label8.Text = "(オプション)";
            // 
            // label_Progress
            // 
            label_Progress.Location = new Point(35, 315);
            label_Progress.Name = "label_Progress";
            label_Progress.Size = new Size(175, 19);
            label_Progress.TabIndex = 26;
            label_Progress.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(456, 597);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(97, 32);
            btnCancel.TabIndex = 27;
            btnCancel.Text = "中止";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += onClick_btnCancel;
            // 
            // mainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(697, 640);
            Controls.Add(btnCancel);
            Controls.Add(label_Progress);
            Controls.Add(label8);
            Controls.Add(btnProcessStart);
            Controls.Add(tbLog);
            Controls.Add(pbProgress);
            Controls.Add(groupBox1);
            Controls.Add(btnFinalDestDir);
            Controls.Add(tbFinalDestDir);
            Controls.Add(label7);
            Controls.Add(btnWorkDir);
            Controls.Add(btnReferedJsonDir);
            Controls.Add(btnCacheTranslationFilesDir);
            Controls.Add(btn7zPath);
            Controls.Add(btnSseAtPath);
            Controls.Add(btnMO2ModsDir);
            Controls.Add(tbWorkDir);
            Controls.Add(tbReferedJsonDir);
            Controls.Add(tbCacheTranslationFilesDir);
            Controls.Add(tb7zPath);
            Controls.Add(tbSseAtPath);
            Controls.Add(tbMO2ModsDir);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "mainWindow";
            Text = "Skyrim SE Mods Semi-Auto Translator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox tbMO2ModsDir;
        private TextBox tbSseAtPath;
        private TextBox tb7zPath;
        private TextBox tbCacheTranslationFilesDir;
        private TextBox tbReferedJsonDir;
        private TextBox tbWorkDir;
        private Button btnMO2ModsDir;
        private Button btnSseAtPath;
        private Button btn7zPath;
        private Button btnCacheTranslationFilesDir;
        private Button btnReferedJsonDir;
        private Button btnWorkDir;
        private Label label7;
        private TextBox tbFinalDestDir;
        private Button btnFinalDestDir;
        private GroupBox groupBox1;
        private ProgressBar pbProgress;
        private TextBox tbLog;
        private Button btnProcessStart;
        private Label label8;
        private Label label_Progress;
        private Button btnCancel;
    }
}
