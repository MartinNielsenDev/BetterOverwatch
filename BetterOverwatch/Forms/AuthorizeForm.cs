using BetterOverwatch.Networking;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BetterOverwatch.Forms
{
    public partial class AuthorizeForm : Form
    {
        public bool isLinking = false;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public AuthorizeForm()
        {
            InitializeComponent();
            if (isLinking)
            {
                textLabel.Text = "You can now link your Battle.net account to your Better Overwatch\r\n\r\nYou will then be able to login to your Better Overwatch from anywhere";
            }
            else
            {
                textLabel.Text = "It appears to be your first time running Better Overwatch.\r\n\r\nTo login to an exis" +
                    "ting account or create a new account, click the authorize button below.\r\n";
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, Bounds);
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
            ControlPaint.DrawBorder(e.Graphics, borderPanel.ClientRectangle, Color.FromArgb(255, 57, 96, 130), ButtonBorderStyle.Solid);
        }
        private async void authorizeButton_Click(object sender, EventArgs e)
        {
            Hide();
            Program.authorizeForm.authorizeButton.Enabled = false;
            Process.Start("https://eu.battle.net/oauth/authorize?response_type=code&client_id=20d78829a4e641e694d8ec7f1198dc8b&redirect_uri=http://betteroverwatch.com/api/authorize/" + (isLinking ? "&state=" + Vars.settings.privateToken : string.Empty));
            Focus();
            await Server.StartLocalAuthServer();
        }
    }
}