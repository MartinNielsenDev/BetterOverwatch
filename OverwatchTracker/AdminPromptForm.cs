using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace OverwatchTracker
{
    public partial class AdminPromptForm : Form
    {
        [DllImport("user32.dll")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);
        private const int BCM_SETSHIELD = (0x1600 + 0x000C);

        public AdminPromptForm()
        {
            InitializeComponent();
        }

        private void AdminPromptForm_Load(object sender, EventArgs e)
        {
            SendMessage(button1.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
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
                Console.WriteLine(restartAsAdminThread.ThreadState);
                Thread.Sleep(100);
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
