using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace OverwatchTracker
{
    public partial class AdminPromptForm : Form
    {
        private const int BCM_SETSHIELD = (0x1600 + 0x000C);

        public AdminPromptForm()
        {
            InitializeComponent();
        }

        private void AdminPromptForm_Load(object sender, EventArgs e)
        {
            Program.SendMessage(button1.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread restartAsAdminThread = new Thread(Elevate)
            {
                IsBackground = true
            };
            restartAsAdminThread.Start();

            while (restartAsAdminThread.ThreadState == System.Threading.ThreadState.Background)
            {
                Thread.Sleep(1);
            }

            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static void Elevate()
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };
                process.Start();
            }
            catch { }
        }
    }
}
