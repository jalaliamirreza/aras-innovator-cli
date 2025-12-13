namespace ArasCatiaAddin.Forms
{
    partial class BomSyncForm
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tvBom = new System.Windows.Forms.TreeView();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.lblPartNumber = new System.Windows.Forms.Label();
            this.txtPartNumber = new System.Windows.Forms.TextBox();
            this.lblPartName = new System.Windows.Forms.Label();
            this.txtPartName = new System.Windows.Forms.TextBox();
            this.lblNomenclature = new System.Windows.Forms.Label();
            this.txtNomenclature = new System.Windows.Forms.TextBox();
            this.lblRevision = new System.Windows.Forms.Label();
            this.txtRevision = new System.Windows.Forms.TextBox();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblFileType = new System.Windows.Forms.Label();
            this.txtFileType = new System.Windows.Forms.TextBox();
            this.lblStatusLabel = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.btnSync = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer
            //
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 12);
            this.splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.Controls.Add(this.chkSelectAll);
            this.splitContainer.Panel1.Controls.Add(this.tvBom);
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.panelDetails);
            this.splitContainer.Size = new System.Drawing.Size(860, 350);
            this.splitContainer.SplitterDistance = 400;
            this.splitContainer.TabIndex = 0;
            //
            // chkSelectAll
            //
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Checked = true;
            this.chkSelectAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectAll.Location = new System.Drawing.Point(3, 3);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(70, 17);
            this.chkSelectAll.TabIndex = 0;
            this.chkSelectAll.Text = "Select All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            //
            // tvBom
            //
            this.tvBom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tvBom.CheckBoxes = true;
            this.tvBom.Location = new System.Drawing.Point(3, 26);
            this.tvBom.Name = "tvBom";
            this.tvBom.Size = new System.Drawing.Size(394, 321);
            this.tvBom.TabIndex = 1;
            this.tvBom.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvBom_AfterSelect);
            //
            // panelDetails
            //
            this.panelDetails.Controls.Add(this.lblPartNumber);
            this.panelDetails.Controls.Add(this.txtPartNumber);
            this.panelDetails.Controls.Add(this.lblPartName);
            this.panelDetails.Controls.Add(this.txtPartName);
            this.panelDetails.Controls.Add(this.lblNomenclature);
            this.panelDetails.Controls.Add(this.txtNomenclature);
            this.panelDetails.Controls.Add(this.lblRevision);
            this.panelDetails.Controls.Add(this.txtRevision);
            this.panelDetails.Controls.Add(this.lblFilePath);
            this.panelDetails.Controls.Add(this.txtFilePath);
            this.panelDetails.Controls.Add(this.lblFileType);
            this.panelDetails.Controls.Add(this.txtFileType);
            this.panelDetails.Controls.Add(this.lblStatusLabel);
            this.panelDetails.Controls.Add(this.txtStatus);
            this.panelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetails.Location = new System.Drawing.Point(0, 0);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(456, 350);
            this.panelDetails.TabIndex = 0;
            //
            // lblPartNumber
            //
            this.lblPartNumber.AutoSize = true;
            this.lblPartNumber.Location = new System.Drawing.Point(10, 15);
            this.lblPartNumber.Name = "lblPartNumber";
            this.lblPartNumber.Size = new System.Drawing.Size(70, 13);
            this.lblPartNumber.TabIndex = 0;
            this.lblPartNumber.Text = "Part Number:";
            //
            // txtPartNumber
            //
            this.txtPartNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPartNumber.Location = new System.Drawing.Point(100, 12);
            this.txtPartNumber.Name = "txtPartNumber";
            this.txtPartNumber.ReadOnly = true;
            this.txtPartNumber.Size = new System.Drawing.Size(346, 20);
            this.txtPartNumber.TabIndex = 1;
            //
            // lblPartName
            //
            this.lblPartName.AutoSize = true;
            this.lblPartName.Location = new System.Drawing.Point(10, 45);
            this.lblPartName.Name = "lblPartName";
            this.lblPartName.Size = new System.Drawing.Size(38, 13);
            this.lblPartName.TabIndex = 2;
            this.lblPartName.Text = "Name:";
            //
            // txtPartName
            //
            this.txtPartName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPartName.Location = new System.Drawing.Point(100, 42);
            this.txtPartName.Name = "txtPartName";
            this.txtPartName.ReadOnly = true;
            this.txtPartName.Size = new System.Drawing.Size(346, 20);
            this.txtPartName.TabIndex = 3;
            //
            // lblNomenclature
            //
            this.lblNomenclature.AutoSize = true;
            this.lblNomenclature.Location = new System.Drawing.Point(10, 75);
            this.lblNomenclature.Name = "lblNomenclature";
            this.lblNomenclature.Size = new System.Drawing.Size(73, 13);
            this.lblNomenclature.TabIndex = 4;
            this.lblNomenclature.Text = "Nomenclature:";
            //
            // txtNomenclature
            //
            this.txtNomenclature.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNomenclature.Location = new System.Drawing.Point(100, 72);
            this.txtNomenclature.Name = "txtNomenclature";
            this.txtNomenclature.ReadOnly = true;
            this.txtNomenclature.Size = new System.Drawing.Size(346, 20);
            this.txtNomenclature.TabIndex = 5;
            //
            // lblRevision
            //
            this.lblRevision.AutoSize = true;
            this.lblRevision.Location = new System.Drawing.Point(10, 105);
            this.lblRevision.Name = "lblRevision";
            this.lblRevision.Size = new System.Drawing.Size(51, 13);
            this.lblRevision.TabIndex = 6;
            this.lblRevision.Text = "Revision:";
            //
            // txtRevision
            //
            this.txtRevision.Location = new System.Drawing.Point(100, 102);
            this.txtRevision.Name = "txtRevision";
            this.txtRevision.ReadOnly = true;
            this.txtRevision.Size = new System.Drawing.Size(100, 20);
            this.txtRevision.TabIndex = 7;
            //
            // lblFilePath
            //
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(10, 135);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(51, 13);
            this.lblFilePath.TabIndex = 8;
            this.lblFilePath.Text = "File Path:";
            //
            // txtFilePath
            //
            this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilePath.Location = new System.Drawing.Point(100, 132);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(346, 20);
            this.txtFilePath.TabIndex = 9;
            //
            // lblFileType
            //
            this.lblFileType.AutoSize = true;
            this.lblFileType.Location = new System.Drawing.Point(10, 165);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(53, 13);
            this.lblFileType.TabIndex = 10;
            this.lblFileType.Text = "File Type:";
            //
            // txtFileType
            //
            this.txtFileType.Location = new System.Drawing.Point(100, 162);
            this.txtFileType.Name = "txtFileType";
            this.txtFileType.ReadOnly = true;
            this.txtFileType.Size = new System.Drawing.Size(100, 20);
            this.txtFileType.TabIndex = 11;
            //
            // lblStatusLabel
            //
            this.lblStatusLabel.AutoSize = true;
            this.lblStatusLabel.Location = new System.Drawing.Point(10, 195);
            this.lblStatusLabel.Name = "lblStatusLabel";
            this.lblStatusLabel.Size = new System.Drawing.Size(40, 13);
            this.lblStatusLabel.TabIndex = 12;
            this.lblStatusLabel.Text = "Status:";
            //
            // txtStatus
            //
            this.txtStatus.Location = new System.Drawing.Point(100, 192);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(100, 20);
            this.txtStatus.TabIndex = 13;
            //
            // txtLog
            //
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 368);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(860, 100);
            this.txtLog.TabIndex = 1;
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 474);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(700, 20);
            this.progressBar.TabIndex = 2;
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(12, 500);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(700, 18);
            this.lblStatus.TabIndex = 3;
            //
            // btnSync
            //
            this.btnSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSync.Location = new System.Drawing.Point(716, 474);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(75, 23);
            this.btnSync.TabIndex = 4;
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(797, 474);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // BomSyncForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(884, 527);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.splitContainer);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "BomSyncForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BOM Sync";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView tvBom;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.Panel panelDetails;
        private System.Windows.Forms.Label lblPartNumber;
        private System.Windows.Forms.TextBox txtPartNumber;
        private System.Windows.Forms.Label lblPartName;
        private System.Windows.Forms.TextBox txtPartName;
        private System.Windows.Forms.Label lblNomenclature;
        private System.Windows.Forms.TextBox txtNomenclature;
        private System.Windows.Forms.Label lblRevision;
        private System.Windows.Forms.TextBox txtRevision;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.TextBox txtFileType;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Button btnCancel;
    }
}
