using System;
using System.Drawing;
using System.Windows.Forms;

namespace SKyrimSEModsSemiAutoTranslator
{
    public class InstructionDialog : Form
    {
        // = null!; を追加して "初期化漏れ警告" (CS8618) を抑制
        private TextBox txtMessage = null!;
        private Button btnOk = null!;

        public InstructionDialog(string message)
        {
            InitializeComponent();
            txtMessage.Text = message;
            txtMessage.SelectionLength = 0;
        }

        private void InitializeComponent()
        {
            this.Text = "xTranslator 手動実行手順";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            txtMessage = new TextBox();
            txtMessage.Multiline = true;
            txtMessage.ReadOnly = true;
            txtMessage.ScrollBars = ScrollBars.Vertical;
            txtMessage.Dock = DockStyle.Top;
            txtMessage.Height = 400;
            txtMessage.Font = new Font("Consolas", 10);
            txtMessage.BackColor = Color.White;

            btnOk = new Button();
            btnOk.Text = "翻訳完了";
            btnOk.Size = new Size(150, 40);
            btnOk.Location = new Point((this.ClientSize.Width - btnOk.Width) / 2, 415);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Cursor = Cursors.Hand;

            this.Controls.Add(txtMessage);
            this.Controls.Add(btnOk);

            this.AcceptButton = btnOk;
        }
    }
}