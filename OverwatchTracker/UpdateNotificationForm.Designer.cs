namespace OverwatchTracker
{
    partial class UpdateNotificationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.titleSubLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // changeLogTextBox
            // 
            this.changeLogTextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.changeLogTextBox.Location = new System.Drawing.Point(12, 138);
            this.changeLogTextBox.Multiline = true;
            this.changeLogTextBox.Name = "changeLogTextBox";
            this.changeLogTextBox.ReadOnly = true;
            this.changeLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.changeLogTextBox.Size = new System.Drawing.Size(481, 177);
            this.changeLogTextBox.TabIndex = 0;
            this.changeLogTextBox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(76, 13);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(300, 18);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "New Tracker update is available!";
            // 
            // installedVersionLabel
            // 
            this.installedVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installedVersionLabel.Location = new System.Drawing.Point(12, 73);
            this.installedVersionLabel.Name = "installedVersionLabel";
            this.installedVersionLabel.Size = new System.Drawing.Size(210, 15);
            this.installedVersionLabel.TabIndex = 0;
            this.installedVersionLabel.Text = "Installed version: ";
            // 
            // updateVersionLabel
            // 
            this.updateVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateVersionLabel.Location = new System.Drawing.Point(12, 91);
            this.updateVersionLabel.Name = "updateVersionLabel";
            this.updateVersionLabel.Size = new System.Drawing.Size(210, 15);
            this.updateVersionLabel.TabIndex = 0;
            this.updateVersionLabel.Text = "Update version: ";
            // 
            // changeLogLabel
            // 
            this.changeLogLabel.AutoSize = true;
            this.changeLogLabel.Location = new System.Drawing.Point(12, 122);
            this.changeLogLabel.Name = "changeLogLabel";
            this.changeLogLabel.Size = new System.Drawing.Size(61, 13);
            this.changeLogLabel.TabIndex = 4;
            this.changeLogLabel.Text = "Change log";
            // 
            // updateButton
            // 
            this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.updateButton.Location = new System.Drawing.Point(327, 324);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(80, 30);
            this.updateButton.TabIndex = 2;
            this.updateButton.Text = "Update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.cancelButton.Location = new System.Drawing.Point(413, 324);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(80, 30);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::OverwatchTracker.Properties.Resources.IconDownload;
            this.pictureBox1.Location = new System.Drawing.Point(12, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(60, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // titleSubLabel
            // 
            this.titleSubLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleSubLabel.Location = new System.Drawing.Point(79, 34);
            this.titleSubLabel.Name = "titleSubLabel";
            this.titleSubLabel.Size = new System.Drawing.Size(300, 15);
            this.titleSubLabel.TabIndex = 6;
            this.titleSubLabel.Text = "Overwatch Tracker  v";
            // 
            // UpdateNotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 364);
            this.Controls.Add(this.titleSubLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.changeLogLabel);
            this.Controls.Add(this.updateVersionLabel);
            this.Controls.Add(this.installedVersionLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.changeLogTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::OverwatchTracker.Properties.Resources.Idle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateNotificationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Update Available";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateNotificationForm_FormClosing);
            this.Load += new System.EventHandler(this.UpdateNoficationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox changeLogTextBox;
        private System.Windows.Forms.Label titleLabel;
        public System.Windows.Forms.Label installedVersionLabel;
        public System.Windows.Forms.Label updateVersionLabel;
        private System.Windows.Forms.Label changeLogLabel;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.Label titleSubLabel;
    }
}