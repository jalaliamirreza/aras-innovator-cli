namespace ArasCatiaAddin.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabConnection = new System.Windows.Forms.TabPage();
            this.lblServerUrl = new System.Windows.Forms.Label();
            this.txtServerUrl = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.chkRememberPassword = new System.Windows.Forms.CheckBox();
            this.chkAutoLogin = new System.Windows.Forms.CheckBox();
            this.tabWorkspace = new System.Windows.Forms.TabPage();
            this.lblWorkspace = new System.Windows.Forms.Label();
            this.txtWorkspace = new System.Windows.Forms.TextBox();
            this.btnBrowseWorkspace = new System.Windows.Forms.Button();
            this.chkOverwrite = new System.Windows.Forms.CheckBox();
            this.tabCheckin = new System.Windows.Forms.TabPage();
            this.chkAutoGenerate = new System.Windows.Forms.CheckBox();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.lblDefaultType = new System.Windows.Forms.Label();
            this.cboDefaultType = new System.Windows.Forms.ComboBox();
            this.chkConfirmCheckin = new System.Windows.Forms.CheckBox();
            this.tabBom = new System.Windows.Forms.TabPage();
            this.chkCreateMissingParts = new System.Windows.Forms.CheckBox();
            this.chkSyncProperties = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabConnection.SuspendLayout();
            this.tabWorkspace.SuspendLayout();
            this.tabCheckin.SuspendLayout();
            this.tabBom.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabConnection);
            this.tabControl.Controls.Add(this.tabWorkspace);
            this.tabControl.Controls.Add(this.tabCheckin);
            this.tabControl.Controls.Add(this.tabBom);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(460, 290);
            this.tabControl.TabIndex = 0;
            //
            // tabConnection
            //
            this.tabConnection.Controls.Add(this.lblServerUrl);
            this.tabConnection.Controls.Add(this.txtServerUrl);
            this.tabConnection.Controls.Add(this.lblDatabase);
            this.tabConnection.Controls.Add(this.txtDatabase);
            this.tabConnection.Controls.Add(this.chkRememberPassword);
            this.tabConnection.Controls.Add(this.chkAutoLogin);
            this.tabConnection.Location = new System.Drawing.Point(4, 22);
            this.tabConnection.Name = "tabConnection";
            this.tabConnection.Padding = new System.Windows.Forms.Padding(10);
            this.tabConnection.Size = new System.Drawing.Size(452, 264);
            this.tabConnection.TabIndex = 0;
            this.tabConnection.Text = "Connection";
            this.tabConnection.UseVisualStyleBackColor = true;
            //
            // lblServerUrl
            //
            this.lblServerUrl.AutoSize = true;
            this.lblServerUrl.Location = new System.Drawing.Point(13, 20);
            this.lblServerUrl.Name = "lblServerUrl";
            this.lblServerUrl.Size = new System.Drawing.Size(63, 13);
            this.lblServerUrl.TabIndex = 0;
            this.lblServerUrl.Text = "Server URL:";
            //
            // txtServerUrl
            //
            this.txtServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerUrl.Location = new System.Drawing.Point(100, 17);
            this.txtServerUrl.Name = "txtServerUrl";
            this.txtServerUrl.Size = new System.Drawing.Size(336, 20);
            this.txtServerUrl.TabIndex = 1;
            //
            // lblDatabase
            //
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(13, 50);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(56, 13);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Database:";
            //
            // txtDatabase
            //
            this.txtDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDatabase.Location = new System.Drawing.Point(100, 47);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(336, 20);
            this.txtDatabase.TabIndex = 3;
            //
            // chkRememberPassword
            //
            this.chkRememberPassword.AutoSize = true;
            this.chkRememberPassword.Location = new System.Drawing.Point(16, 90);
            this.chkRememberPassword.Name = "chkRememberPassword";
            this.chkRememberPassword.Size = new System.Drawing.Size(126, 17);
            this.chkRememberPassword.TabIndex = 4;
            this.chkRememberPassword.Text = "Remember Password";
            this.chkRememberPassword.UseVisualStyleBackColor = true;
            //
            // chkAutoLogin
            //
            this.chkAutoLogin.AutoSize = true;
            this.chkAutoLogin.Location = new System.Drawing.Point(16, 115);
            this.chkAutoLogin.Name = "chkAutoLogin";
            this.chkAutoLogin.Size = new System.Drawing.Size(118, 17);
            this.chkAutoLogin.TabIndex = 5;
            this.chkAutoLogin.Text = "Auto-login at startup";
            this.chkAutoLogin.UseVisualStyleBackColor = true;
            //
            // tabWorkspace
            //
            this.tabWorkspace.Controls.Add(this.lblWorkspace);
            this.tabWorkspace.Controls.Add(this.txtWorkspace);
            this.tabWorkspace.Controls.Add(this.btnBrowseWorkspace);
            this.tabWorkspace.Controls.Add(this.chkOverwrite);
            this.tabWorkspace.Location = new System.Drawing.Point(4, 22);
            this.tabWorkspace.Name = "tabWorkspace";
            this.tabWorkspace.Padding = new System.Windows.Forms.Padding(10);
            this.tabWorkspace.Size = new System.Drawing.Size(452, 264);
            this.tabWorkspace.TabIndex = 1;
            this.tabWorkspace.Text = "Workspace";
            this.tabWorkspace.UseVisualStyleBackColor = true;
            //
            // lblWorkspace
            //
            this.lblWorkspace.AutoSize = true;
            this.lblWorkspace.Location = new System.Drawing.Point(13, 20);
            this.lblWorkspace.Name = "lblWorkspace";
            this.lblWorkspace.Size = new System.Drawing.Size(91, 13);
            this.lblWorkspace.TabIndex = 0;
            this.lblWorkspace.Text = "Workspace Folder:";
            //
            // txtWorkspace
            //
            this.txtWorkspace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWorkspace.Location = new System.Drawing.Point(110, 17);
            this.txtWorkspace.Name = "txtWorkspace";
            this.txtWorkspace.Size = new System.Drawing.Size(285, 20);
            this.txtWorkspace.TabIndex = 1;
            //
            // btnBrowseWorkspace
            //
            this.btnBrowseWorkspace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseWorkspace.Location = new System.Drawing.Point(401, 15);
            this.btnBrowseWorkspace.Name = "btnBrowseWorkspace";
            this.btnBrowseWorkspace.Size = new System.Drawing.Size(35, 23);
            this.btnBrowseWorkspace.TabIndex = 2;
            this.btnBrowseWorkspace.Text = "...";
            this.btnBrowseWorkspace.UseVisualStyleBackColor = true;
            this.btnBrowseWorkspace.Click += new System.EventHandler(this.btnBrowseWorkspace_Click);
            //
            // chkOverwrite
            //
            this.chkOverwrite.AutoSize = true;
            this.chkOverwrite.Location = new System.Drawing.Point(16, 55);
            this.chkOverwrite.Name = "chkOverwrite";
            this.chkOverwrite.Size = new System.Drawing.Size(136, 17);
            this.chkOverwrite.TabIndex = 3;
            this.chkOverwrite.Text = "Overwrite existing files";
            this.chkOverwrite.UseVisualStyleBackColor = true;
            //
            // tabCheckin
            //
            this.tabCheckin.Controls.Add(this.chkAutoGenerate);
            this.tabCheckin.Controls.Add(this.lblPrefix);
            this.tabCheckin.Controls.Add(this.txtPrefix);
            this.tabCheckin.Controls.Add(this.lblDefaultType);
            this.tabCheckin.Controls.Add(this.cboDefaultType);
            this.tabCheckin.Controls.Add(this.chkConfirmCheckin);
            this.tabCheckin.Location = new System.Drawing.Point(4, 22);
            this.tabCheckin.Name = "tabCheckin";
            this.tabCheckin.Padding = new System.Windows.Forms.Padding(10);
            this.tabCheckin.Size = new System.Drawing.Size(452, 264);
            this.tabCheckin.TabIndex = 2;
            this.tabCheckin.Text = "Check-In";
            this.tabCheckin.UseVisualStyleBackColor = true;
            //
            // chkAutoGenerate
            //
            this.chkAutoGenerate.AutoSize = true;
            this.chkAutoGenerate.Location = new System.Drawing.Point(16, 20);
            this.chkAutoGenerate.Name = "chkAutoGenerate";
            this.chkAutoGenerate.Size = new System.Drawing.Size(162, 17);
            this.chkAutoGenerate.TabIndex = 0;
            this.chkAutoGenerate.Text = "Auto-generate Item Numbers";
            this.chkAutoGenerate.UseVisualStyleBackColor = true;
            //
            // lblPrefix
            //
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(13, 55);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(96, 13);
            this.lblPrefix.TabIndex = 1;
            this.lblPrefix.Text = "Item Number Prefix:";
            //
            // txtPrefix
            //
            this.txtPrefix.Location = new System.Drawing.Point(115, 52);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(100, 20);
            this.txtPrefix.TabIndex = 2;
            //
            // lblDefaultType
            //
            this.lblDefaultType.AutoSize = true;
            this.lblDefaultType.Location = new System.Drawing.Point(13, 90);
            this.lblDefaultType.Name = "lblDefaultType";
            this.lblDefaultType.Size = new System.Drawing.Size(116, 13);
            this.lblDefaultType.TabIndex = 3;
            this.lblDefaultType.Text = "Default Document Type:";
            //
            // cboDefaultType
            //
            this.cboDefaultType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultType.FormattingEnabled = true;
            this.cboDefaultType.Location = new System.Drawing.Point(135, 87);
            this.cboDefaultType.Name = "cboDefaultType";
            this.cboDefaultType.Size = new System.Drawing.Size(150, 21);
            this.cboDefaultType.TabIndex = 4;
            //
            // chkConfirmCheckin
            //
            this.chkConfirmCheckin.AutoSize = true;
            this.chkConfirmCheckin.Location = new System.Drawing.Point(16, 125);
            this.chkConfirmCheckin.Name = "chkConfirmCheckin";
            this.chkConfirmCheckin.Size = new System.Drawing.Size(133, 17);
            this.chkConfirmCheckin.TabIndex = 5;
            this.chkConfirmCheckin.Text = "Confirm before check-in";
            this.chkConfirmCheckin.UseVisualStyleBackColor = true;
            //
            // tabBom
            //
            this.tabBom.Controls.Add(this.chkCreateMissingParts);
            this.tabBom.Controls.Add(this.chkSyncProperties);
            this.tabBom.Location = new System.Drawing.Point(4, 22);
            this.tabBom.Name = "tabBom";
            this.tabBom.Padding = new System.Windows.Forms.Padding(10);
            this.tabBom.Size = new System.Drawing.Size(452, 264);
            this.tabBom.TabIndex = 3;
            this.tabBom.Text = "BOM Sync";
            this.tabBom.UseVisualStyleBackColor = true;
            //
            // chkCreateMissingParts
            //
            this.chkCreateMissingParts.AutoSize = true;
            this.chkCreateMissingParts.Location = new System.Drawing.Point(16, 20);
            this.chkCreateMissingParts.Name = "chkCreateMissingParts";
            this.chkCreateMissingParts.Size = new System.Drawing.Size(150, 17);
            this.chkCreateMissingParts.TabIndex = 0;
            this.chkCreateMissingParts.Text = "Auto-create missing Parts";
            this.chkCreateMissingParts.UseVisualStyleBackColor = true;
            //
            // chkSyncProperties
            //
            this.chkSyncProperties.AutoSize = true;
            this.chkSyncProperties.Location = new System.Drawing.Point(16, 45);
            this.chkSyncProperties.Name = "chkSyncProperties";
            this.chkSyncProperties.Size = new System.Drawing.Size(134, 17);
            this.chkSyncProperties.TabIndex = 1;
            this.chkSyncProperties.Text = "Sync Part properties";
            this.chkSyncProperties.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(316, 315);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(397, 315);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // btnReset
            //
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(12, 315);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 23);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "Reset Defaults";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            //
            // SettingsForm
            //
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 351);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.tabConnection.ResumeLayout(false);
            this.tabConnection.PerformLayout();
            this.tabWorkspace.ResumeLayout(false);
            this.tabWorkspace.PerformLayout();
            this.tabCheckin.ResumeLayout(false);
            this.tabCheckin.PerformLayout();
            this.tabBom.ResumeLayout(false);
            this.tabBom.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabConnection;
        private System.Windows.Forms.Label lblServerUrl;
        private System.Windows.Forms.TextBox txtServerUrl;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.CheckBox chkRememberPassword;
        private System.Windows.Forms.CheckBox chkAutoLogin;
        private System.Windows.Forms.TabPage tabWorkspace;
        private System.Windows.Forms.Label lblWorkspace;
        private System.Windows.Forms.TextBox txtWorkspace;
        private System.Windows.Forms.Button btnBrowseWorkspace;
        private System.Windows.Forms.CheckBox chkOverwrite;
        private System.Windows.Forms.TabPage tabCheckin;
        private System.Windows.Forms.CheckBox chkAutoGenerate;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.Label lblDefaultType;
        private System.Windows.Forms.ComboBox cboDefaultType;
        private System.Windows.Forms.CheckBox chkConfirmCheckin;
        private System.Windows.Forms.TabPage tabBom;
        private System.Windows.Forms.CheckBox chkCreateMissingParts;
        private System.Windows.Forms.CheckBox chkSyncProperties;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnReset;
    }
}
