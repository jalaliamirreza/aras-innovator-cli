namespace ArasCatiaAddin.Forms
{
    partial class CheckInForm
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
            this.lblItemNumber = new System.Windows.Forms.Label();
            this.txtItemNumber = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDocumentType = new System.Windows.Forms.Label();
            this.cboDocumentType = new System.Windows.Forms.ComboBox();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblItemNumber
            //
            this.lblItemNumber.AutoSize = true;
            this.lblItemNumber.Location = new System.Drawing.Point(12, 15);
            this.lblItemNumber.Name = "lblItemNumber";
            this.lblItemNumber.Size = new System.Drawing.Size(70, 13);
            this.lblItemNumber.TabIndex = 0;
            this.lblItemNumber.Text = "Item Number:";
            //
            // txtItemNumber
            //
            this.txtItemNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtItemNumber.Location = new System.Drawing.Point(110, 12);
            this.txtItemNumber.Name = "txtItemNumber";
            this.txtItemNumber.Size = new System.Drawing.Size(312, 20);
            this.txtItemNumber.TabIndex = 1;
            //
            // lblName
            //
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 45);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "Name:";
            //
            // txtName
            //
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(110, 42);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(312, 20);
            this.txtName.TabIndex = 3;
            //
            // lblDescription
            //
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(12, 75);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 4;
            this.lblDescription.Text = "Description:";
            //
            // txtDescription
            //
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(110, 72);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(312, 60);
            this.txtDescription.TabIndex = 5;
            //
            // lblDocumentType
            //
            this.lblDocumentType.AutoSize = true;
            this.lblDocumentType.Location = new System.Drawing.Point(12, 145);
            this.lblDocumentType.Name = "lblDocumentType";
            this.lblDocumentType.Size = new System.Drawing.Size(87, 13);
            this.lblDocumentType.TabIndex = 6;
            this.lblDocumentType.Text = "Document Type:";
            //
            // cboDocumentType
            //
            this.cboDocumentType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDocumentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDocumentType.FormattingEnabled = true;
            this.cboDocumentType.Location = new System.Drawing.Point(110, 142);
            this.cboDocumentType.Name = "cboDocumentType";
            this.cboDocumentType.Size = new System.Drawing.Size(312, 21);
            this.cboDocumentType.TabIndex = 7;
            //
            // lblFilePath
            //
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(12, 175);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(51, 13);
            this.lblFilePath.TabIndex = 8;
            this.lblFilePath.Text = "File Path:";
            //
            // txtFilePath
            //
            this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilePath.Location = new System.Drawing.Point(110, 172);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(312, 20);
            this.txtFilePath.TabIndex = 9;
            //
            // btnOK
            //
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(266, 260);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "Check In";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(347, 260);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 230);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(410, 20);
            this.progressBar.TabIndex = 12;
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(12, 205);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(410, 18);
            this.lblStatus.TabIndex = 13;
            //
            // CheckInForm
            //
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(434, 295);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.cboDocumentType);
            this.Controls.Add(this.lblDocumentType);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtItemNumber);
            this.Controls.Add(this.lblItemNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckInForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Check In Document";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblItemNumber;
        private System.Windows.Forms.TextBox txtItemNumber;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDocumentType;
        private System.Windows.Forms.ComboBox cboDocumentType;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
    }
}
