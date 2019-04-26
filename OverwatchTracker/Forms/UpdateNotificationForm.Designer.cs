using System.ComponentModel;
using System.Windows.Forms;

namespace BetterOverwatch.Forms
{
    partial class UpdateNotificationForm
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
            this.changeLogTextBox = new System.Windows.Forms.TextBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.installedVersionLabel = new System.Windows.Forms.Label();
            this.updateVersionLabel = new System.Windows.Forms.Label();
            this.changeLogLabel = new System.Windows.Forms.Label();
            this.updateButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.titleSubLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // changeLogTextBox
            // 
            this.changeLogTextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.changeLogTextBox.Location = new System.Drawing.Point(16, 170);
            this.changeLogTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.changeLogTextBox.Multiline = true;
            this.changeLogTextBox.Name = "changeLogTextBox";
            this.changeLogTextBox.ReadOnly = true;
            this.changeLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.changeLogTextBox.Size = new System.Drawing.Size(640, 199);
            this.changeLogTextBox.TabIndex = 0;
            this.changeLogTextBox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(101, 16);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(400, 22);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "Better Overwatch update available";
            // 
            // installedVersionLabel
            // 
            this.installedVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installedVersionLabel.Location = new System.Drawing.Point(16, 90);
            this.installedVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.installedVersionLabel.Name = "installedVersionLabel";
            this.installedVersionLabel.Size = new System.Drawing.Size(280, 18);
            this.installedVersionLabel.TabIndex = 0;
            this.installedVersionLabel.Text = "Installed version: ";
            // 
            // updateVersionLabel
            // 
            this.updateVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateVersionLabel.Location = new System.Drawing.Point(16, 112);
            this.updateVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.updateVersionLabel.Name = "updateVersionLabel";
            this.updateVersionLabel.Size = new System.Drawing.Size(280, 18);
            this.updateVersionLabel.TabIndex = 0;
            this.updateVersionLabel.Text = "Update version: ";
            // 
            // changeLogLabel
            // 
            this.changeLogLabel.AutoSize = true;
            this.changeLogLabel.Location = new System.Drawing.Point(16, 148);
            this.changeLogLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.changeLogLabel.Name = "changeLogLabel";
            this.changeLogLabel.Size = new System.Drawing.Size(80, 17);
            this.changeLogLabel.TabIndex = 4;
            this.changeLogLabel.Text = "Change log";
            // 
            // updateButton
            // 
            this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.updateButton.Location = new System.Drawing.Point(436, 382);
            this.updateButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(107, 37);
            this.updateButton.TabIndex = 0;
            this.updateButton.Text = "Update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.cancelButton.Location = new System.Drawing.Point(551, 382);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(107, 37);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // pictureBox1
            // 
            this.logoPictureBox.Image = global::BetterOverwatch.Properties.Resources.IconDownload;
            this.logoPictureBox.Location = new System.Drawing.Point(16, 16);
            this.logoPictureBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.logoPictureBox.Name = "pictureBox1";
            this.logoPictureBox.Size = new System.Drawing.Size(60, 50);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.logoPictureBox.TabIndex = 5;
            this.logoPictureBox.TabStop = false;
            // 
            // titleSubLabel
            // 
            this.titleSubLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleSubLabel.Location = new System.Drawing.Point(105, 42);
            this.titleSubLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleSubLabel.Name = "titleSubLabel";
            this.titleSubLabel.Size = new System.Drawing.Size(400, 18);
            this.titleSubLabel.TabIndex = 6;
            this.titleSubLabel.Text = "Would you like to download the newest version?";
            // 
            // UpdateNotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 431);
            this.Controls.Add(this.titleSubLabel);
            this.Controls.Add(this.logoPictureBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.changeLogLabel);
            this.Controls.Add(this.updateVersionLabel);
            this.Controls.Add(this.installedVersionLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.changeLogTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::BetterOverwatch.Properties.Resources.Idle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateNotificationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New version available";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateNotificationForm_FormClosing);
            this.Shown += new System.EventHandler(this.UpdateNotificationForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public TextBox changeLogTextBox;
        private Label titleLabel;
        public Label installedVersionLabel;
        public Label updateVersionLabel;
        private Label changeLogLabel;
        private Button updateButton;
        private Button cancelButton;
        private PictureBox logoPictureBox;
        private Label titleSubLabel;
    }
}