using System.ComponentModel;
using System.Windows.Forms;

namespace BetterOverwatch.Forms
{
    partial class AuthenticationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.borderPanel = new System.Windows.Forms.Panel();
            this.authorizePanel = new System.Windows.Forms.Panel();
            this.textLabel = new System.Windows.Forms.Label();
            this.authenticationButton = new System.Windows.Forms.Button();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.borderPanel.SuspendLayout();
            this.authorizePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.Transparent;
            this.headerPanel.Controls.Add(this.closeButton);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Font = new System.Drawing.Font("Century", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerPanel.Location = new System.Drawing.Point(1, 1);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(498, 30);
            this.headerPanel.TabIndex = 28;
            this.headerPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveForm_MouseDown);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.ForeColor = System.Drawing.Color.White;
            this.closeButton.Location = new System.Drawing.Point(463, -7);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(35, 42);
            this.closeButton.TabIndex = 999;
            this.closeButton.Text = "x";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.BackgroundImage = global::BetterOverwatch.Properties.Resources.Icon_Small;
            this.logoPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.logoPictureBox.Location = new System.Drawing.Point(1, 31);
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.Size = new System.Drawing.Size(498, 80);
            this.logoPictureBox.TabIndex = 27;
            this.logoPictureBox.TabStop = false;
            this.logoPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveForm_MouseDown);
            // 
            // borderPanel
            // 
            this.borderPanel.BackColor = System.Drawing.Color.Transparent;
            this.borderPanel.Controls.Add(this.authorizePanel);
            this.borderPanel.Controls.Add(this.logoPictureBox);
            this.borderPanel.Controls.Add(this.headerPanel);
            this.borderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.borderPanel.Location = new System.Drawing.Point(0, 0);
            this.borderPanel.Name = "borderPanel";
            this.borderPanel.Padding = new System.Windows.Forms.Padding(1);
            this.borderPanel.Size = new System.Drawing.Size(500, 451);
            this.borderPanel.TabIndex = 27;
            this.borderPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.borderPanel_Paint);
            this.borderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveForm_MouseDown);
            // 
            // authorizePanel
            // 
            this.authorizePanel.Controls.Add(this.textLabel);
            this.authorizePanel.Controls.Add(this.authenticationButton);
            this.authorizePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authorizePanel.Location = new System.Drawing.Point(1, 111);
            this.authorizePanel.Name = "authorizePanel";
            this.authorizePanel.Size = new System.Drawing.Size(498, 339);
            this.authorizePanel.TabIndex = 35;
            this.authorizePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveForm_MouseDown);
            // 
            // textLabel
            // 
            this.textLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.textLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textLabel.ForeColor = System.Drawing.Color.White;
            this.textLabel.Location = new System.Drawing.Point(2, 175);
            this.textLabel.Name = "textLabel";
            this.textLabel.Size = new System.Drawing.Size(496, 116);
            this.textLabel.TabIndex = 32;
            this.textLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.textLabel.Text = "It appears to be your first time running Better Overwatch.\r\n\r\nTo login to an existing account or create a new account, click the authenticate button above.\r\n";
            this.textLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveForm_MouseDown);
            // 
            // authorizeButton
            // 
            this.authenticationButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.authenticationButton.BackColor = System.Drawing.Color.ForestGreen;
            this.authenticationButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.authenticationButton.FlatAppearance.BorderSize = 0;
            this.authenticationButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.authenticationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.authenticationButton.ForeColor = System.Drawing.SystemColors.Window;
            this.authenticationButton.Location = new System.Drawing.Point(176, 76);
            this.authenticationButton.Name = "authorizeButton";
            this.authenticationButton.Size = new System.Drawing.Size(147, 44);
            this.authenticationButton.TabIndex = 0;
            this.authenticationButton.Text = "Authenticate";
            this.authenticationButton.UseVisualStyleBackColor = false;
            this.authenticationButton.Click += new System.EventHandler(this.authenticateButton_Click);
            // 
            // AuthorizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(54)))), ((int)(((byte)(73)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(500, 451);
            this.Controls.Add(this.borderPanel);
            this.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AuthorizeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SignUp";
            this.TopMost = true;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveForm_MouseDown);
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.borderPanel.ResumeLayout(false);
            this.authorizePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel headerPanel;
        private Panel borderPanel;
        private Panel authorizePanel;
        private Button closeButton;
        private PictureBox logoPictureBox;
        public Label textLabel;
        public Button authenticationButton;
    }
}