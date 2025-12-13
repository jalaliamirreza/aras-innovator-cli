namespace ArasCatiaAddin.Forms
{
    partial class SearchForm
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
            this.lblDocumentType = new System.Windows.Forms.Label();
            this.cboDocumentType = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.btnAction = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panelSearch = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.panelSearch.SuspendLayout();
            this.SuspendLayout();
            //
            // panelSearch
            //
            this.panelSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSearch.Controls.Add(this.lblItemNumber);
            this.panelSearch.Controls.Add(this.txtItemNumber);
            this.panelSearch.Controls.Add(this.lblName);
            this.panelSearch.Controls.Add(this.txtName);
            this.panelSearch.Controls.Add(this.lblDocumentType);
            this.panelSearch.Controls.Add(this.cboDocumentType);
            this.panelSearch.Controls.Add(this.btnSearch);
            this.panelSearch.Location = new System.Drawing.Point(0, 0);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(784, 45);
            this.panelSearch.TabIndex = 0;
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
            this.txtItemNumber.Location = new System.Drawing.Point(88, 12);
            this.txtItemNumber.Name = "txtItemNumber";
            this.txtItemNumber.Size = new System.Drawing.Size(130, 20);
            this.txtItemNumber.TabIndex = 1;
            this.txtItemNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtItemNumber_KeyDown);
            //
            // lblName
            //
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(230, 15);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "Name:";
            //
            // txtName
            //
            this.txtName.Location = new System.Drawing.Point(274, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(150, 20);
            this.txtName.TabIndex = 3;
            this.txtName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtName_KeyDown);
            //
            // lblDocumentType
            //
            this.lblDocumentType.AutoSize = true;
            this.lblDocumentType.Location = new System.Drawing.Point(440, 15);
            this.lblDocumentType.Name = "lblDocumentType";
            this.lblDocumentType.Size = new System.Drawing.Size(34, 13);
            this.lblDocumentType.TabIndex = 4;
            this.lblDocumentType.Text = "Type:";
            //
            // cboDocumentType
            //
            this.cboDocumentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDocumentType.FormattingEnabled = true;
            this.cboDocumentType.Location = new System.Drawing.Point(480, 12);
            this.cboDocumentType.Name = "cboDocumentType";
            this.cboDocumentType.Size = new System.Drawing.Size(120, 21);
            this.cboDocumentType.TabIndex = 5;
            //
            // btnSearch
            //
            this.btnSearch.Location = new System.Drawing.Point(620, 10);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // dgvResults
            //
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Location = new System.Drawing.Point(12, 51);
            this.dgvResults.MultiSelect = false;
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.Size = new System.Drawing.Size(760, 350);
            this.dgvResults.TabIndex = 1;
            this.dgvResults.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvResults_CellDoubleClick);
            //
            // btnAction
            //
            this.btnAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAction.Location = new System.Drawing.Point(616, 430);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(75, 23);
            this.btnAction.TabIndex = 2;
            this.btnAction.Text = "Open";
            this.btnAction.UseVisualStyleBackColor = true;
            this.btnAction.Click += new System.EventHandler(this.btnAction_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(697, 430);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(12, 410);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(598, 18);
            this.lblStatus.TabIndex = 4;
            //
            // SearchForm
            //
            this.AcceptButton = this.btnSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAction);
            this.Controls.Add(this.dgvResults);
            this.Controls.Add(this.panelSearch);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "SearchForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search Documents";
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Label lblItemNumber;
        private System.Windows.Forms.TextBox txtItemNumber;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblDocumentType;
        private System.Windows.Forms.ComboBox cboDocumentType;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Button btnAction;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;
    }
}
