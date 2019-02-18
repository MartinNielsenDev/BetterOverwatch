using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BetterOverwatch
{
    public partial class AuthorizeForm : Form
    {
        public bool shouldLink = false;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public AuthorizeForm()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, this.Bounds);
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            if (shouldLink)
            {
                Close();
            }
            else
            {
                Application.Exit();
            }
        }
        public void MoveForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void borderPanel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.borderPanel.ClientRectangle, Color.FromArgb(255, 57, 96, 130), ButtonBorderStyle.Solid);
        }

        private async void authorizeButton_Click(object sender, EventArgs e)
        {
            Hide();
            Program.authorizeForm.authorizeButton.Enabled = false;
            Process.Start("https://eu.battle.net/oauth/authorize?response_type=code&client_id=20d78829a4e641e694d8ec7f1198dc8b&redirect_uri=http://betteroverwatch.com/api/authorize/" + (shouldLink ? "&state=" + Vars.settings.privateToken : ""));
            this.Focus();
            await Server.StartLocalAuthServer();
        }
    }
}