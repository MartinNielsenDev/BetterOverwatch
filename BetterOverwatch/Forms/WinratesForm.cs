using System;
using System.IO;
using System.Windows.Forms;

namespace BetterOverwatch.Forms
{
    public partial class WinratesForm : Form
    {
        public WinratesForm()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(Path.Combine(AppData.configPath, "stats"), textBox1.Text);
                Hide();
            }catch(Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            AppData.settings.outputStatsToTextFile = !AppData.settings.outputStatsToTextFile;
            UpdateButtonText();
            Settings.Save();
        }

        private void WinratesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void WinratesForm_Activated(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(Path.Combine(AppData.configPath, "stats")))
                {
                    textBox1.Text = File.ReadAllText(Path.Combine(AppData.configPath, "stats"));
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            UpdateButtonText();
        }

        private void UpdateButtonText()
        {
            if (AppData.settings.outputStatsToTextFile)
            {
                button2.Text = "Disable";
            }
            else
            {
                button2.Text = "Enable";
            }
        }
    }
}
