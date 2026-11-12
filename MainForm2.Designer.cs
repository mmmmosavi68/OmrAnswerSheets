namespace OmrDesktop
{
    partial class MainForm2
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
            txtInfo = new TextBox();
            lstFiles = new ListBox();
            gridMarkers = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            lblStatus = new Label();
            btnSelectFile = new Button();
            btnSelectFolder = new Button();
            btnAddMarker = new Button();
            btnRemoveMarker = new Button();
            btnConfigureBubbles = new Button();
            btnAlign = new Button();
            btnSaveProfile = new Button();
            btnLoadProfile = new Button();
            pnlAlignedHost = new Panel();
            lblAlignedTitle = new Label();
            pnlOriginalHost = new Panel();
            lblOriginalTitle = new Label();
            btnRunBatch = new Button();
            txtBatchFolderPath = new TextBox();
            progressBatch = new ProgressBar();
            lblBatchProgress = new Label();
            ((System.ComponentModel.ISupportInitialize)gridMarkers).BeginInit();
            SuspendLayout();
            // 
            // txtInfo
            // 
            txtInfo.Location = new Point(12, 202);
            txtInfo.Multiline = true;
            txtInfo.Name = "txtInfo";
            txtInfo.RightToLeft = RightToLeft.Yes;
            txtInfo.Size = new Size(326, 200);
            txtInfo.TabIndex = 8;
            // 
            // lstFiles
            // 
            lstFiles.FormattingEnabled = true;
            lstFiles.Location = new Point(12, 12);
            lstFiles.Name = "lstFiles";
            lstFiles.Size = new Size(326, 184);
            lstFiles.TabIndex = 9;
            // 
            // gridMarkers
            // 
            gridMarkers.AllowUserToAddRows = false;
            gridMarkers.AllowUserToDeleteRows = false;
            gridMarkers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridMarkers.BackgroundColor = Color.White;
            gridMarkers.ColumnHeadersHeight = 29;
            gridMarkers.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3 });
            gridMarkers.Location = new Point(12, 408);
            gridMarkers.MultiSelect = false;
            gridMarkers.Name = "gridMarkers";
            gridMarkers.RightToLeft = RightToLeft.Yes;
            gridMarkers.RowHeadersVisible = false;
            gridMarkers.RowHeadersWidth = 51;
            gridMarkers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridMarkers.Size = new Size(326, 134);
            gridMarkers.TabIndex = 10;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "نام نشانگر";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "X مقصد (%)";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Y مقصد (%)";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // lblStatus
            // 
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.Location = new Point(12, 545);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(8);
            lblStatus.RightToLeft = RightToLeft.Yes;
            lblStatus.Size = new Size(326, 174);
            lblStatus.TabIndex = 11;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnSelectFile
            // 
            btnSelectFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelectFile.Location = new Point(1551, 12);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.RightToLeft = RightToLeft.Yes;
            btnSelectFile.Size = new Size(119, 48);
            btnSelectFile.TabIndex = 15;
            btnSelectFile.Text = "انتخاب فایل";
            btnSelectFile.UseVisualStyleBackColor = true;
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelectFolder.Location = new Point(1426, 12);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.RightToLeft = RightToLeft.Yes;
            btnSelectFolder.Size = new Size(119, 48);
            btnSelectFolder.TabIndex = 14;
            btnSelectFolder.Text = "انتخاب پوشه";
            btnSelectFolder.UseVisualStyleBackColor = true;
            // 
            // btnAddMarker
            // 
            btnAddMarker.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddMarker.Location = new Point(1301, 12);
            btnAddMarker.Name = "btnAddMarker";
            btnAddMarker.RightToLeft = RightToLeft.Yes;
            btnAddMarker.Size = new Size(119, 48);
            btnAddMarker.TabIndex = 13;
            btnAddMarker.Text = "افزودن نشانگر";
            btnAddMarker.UseVisualStyleBackColor = true;
            // 
            // btnRemoveMarker
            // 
            btnRemoveMarker.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRemoveMarker.Location = new Point(1176, 12);
            btnRemoveMarker.Name = "btnRemoveMarker";
            btnRemoveMarker.RightToLeft = RightToLeft.Yes;
            btnRemoveMarker.Size = new Size(119, 48);
            btnRemoveMarker.TabIndex = 12;
            btnRemoveMarker.Text = "حذف نشانگر";
            btnRemoveMarker.UseVisualStyleBackColor = true;
            // 
            // btnConfigureBubbles
            // 
            btnConfigureBubbles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfigureBubbles.Location = new Point(1051, 12);
            btnConfigureBubbles.Name = "btnConfigureBubbles";
            btnConfigureBubbles.RightToLeft = RightToLeft.Yes;
            btnConfigureBubbles.Size = new Size(119, 48);
            btnConfigureBubbles.TabIndex = 11;
            btnConfigureBubbles.Text = "تنظیم حباب‌ها";
            btnConfigureBubbles.UseVisualStyleBackColor = true;
            // 
            // btnAlign
            // 
            btnAlign.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAlign.Location = new Point(926, 12);
            btnAlign.Name = "btnAlign";
            btnAlign.RightToLeft = RightToLeft.Yes;
            btnAlign.Size = new Size(119, 48);
            btnAlign.TabIndex = 10;
            btnAlign.Text = "تراز تصویر";
            btnAlign.UseVisualStyleBackColor = true;
            // 
            // btnSaveProfile
            // 
            btnSaveProfile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveProfile.Location = new Point(801, 12);
            btnSaveProfile.Name = "btnSaveProfile";
            btnSaveProfile.RightToLeft = RightToLeft.Yes;
            btnSaveProfile.Size = new Size(119, 48);
            btnSaveProfile.TabIndex = 9;
            btnSaveProfile.Text = "ذخیره پروفایل";
            btnSaveProfile.UseVisualStyleBackColor = true;
            // 
            // btnLoadProfile
            // 
            btnLoadProfile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoadProfile.Location = new Point(676, 12);
            btnLoadProfile.Name = "btnLoadProfile";
            btnLoadProfile.RightToLeft = RightToLeft.Yes;
            btnLoadProfile.Size = new Size(119, 48);
            btnLoadProfile.TabIndex = 8;
            btnLoadProfile.Text = "بارگذاری پروفایل";
            btnLoadProfile.UseVisualStyleBackColor = true;
            // 
            // pnlAlignedHost
            // 
            pnlAlignedHost.BackColor = Color.FromArgb(40, 40, 40);
            pnlAlignedHost.Location = new Point(1010, 129);
            pnlAlignedHost.Name = "pnlAlignedHost";
            pnlAlignedHost.Size = new Size(660, 590);
            pnlAlignedHost.TabIndex = 24;
            // 
            // lblAlignedTitle
            // 
            lblAlignedTitle.AutoSize = true;
            lblAlignedTitle.Location = new Point(1501, 106);
            lblAlignedTitle.Name = "lblAlignedTitle";
            lblAlignedTitle.RightToLeft = RightToLeft.Yes;
            lblAlignedTitle.Size = new Size(166, 20);
            lblAlignedTitle.TabIndex = 22;
            lblAlignedTitle.Text = "تصویر استاندارد تراز شده";
            // 
            // pnlOriginalHost
            // 
            pnlOriginalHost.BackColor = Color.FromArgb(40, 40, 40);
            pnlOriginalHost.Location = new Point(344, 129);
            pnlOriginalHost.Name = "pnlOriginalHost";
            pnlOriginalHost.Size = new Size(660, 590);
            pnlOriginalHost.TabIndex = 23;
            // 
            // lblOriginalTitle
            // 
            lblOriginalTitle.AutoSize = true;
            lblOriginalTitle.Location = new Point(796, 106);
            lblOriginalTitle.Name = "lblOriginalTitle";
            lblOriginalTitle.RightToLeft = RightToLeft.Yes;
            lblOriginalTitle.Size = new Size(208, 20);
            lblOriginalTitle.TabIndex = 22;
            lblOriginalTitle.Text = "تصویر اصلی - ویرایش نشانگرها";
            // 
            // btnRunBatch
            // 
            btnRunBatch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRunBatch.Location = new Point(551, 12);
            btnRunBatch.Name = "btnRunBatch";
            btnRunBatch.Size = new Size(119, 48);
            btnRunBatch.TabIndex = 25;
            btnRunBatch.Text = "خواندن تصاویر";
            btnRunBatch.UseVisualStyleBackColor = true;
            // 
            // txtBatchFolderPath
            // 
            txtBatchFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtBatchFolderPath.Location = new Point(344, 66);
            txtBatchFolderPath.Name = "txtBatchFolderPath";
            txtBatchFolderPath.Size = new Size(1326, 27);
            txtBatchFolderPath.TabIndex = 26;
            txtBatchFolderPath.Text = "txtBatchFolderPath";
            // 
            // progressBatch
            // 
            progressBatch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressBatch.Location = new Point(344, 12);
            progressBatch.Name = "progressBatch";
            progressBatch.Size = new Size(201, 29);
            progressBatch.TabIndex = 27;
            // 
            // lblBatchProgress
            // 
            lblBatchProgress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBatchProgress.AutoSize = true;
            lblBatchProgress.Location = new Point(344, 44);
            lblBatchProgress.Name = "lblBatchProgress";
            lblBatchProgress.Size = new Size(119, 20);
            lblBatchProgress.TabIndex = 28;
            lblBatchProgress.Text = "lblBatchProgress";
            // 
            // MainForm2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1679, 728);
            Controls.Add(lblBatchProgress);
            Controls.Add(progressBatch);
            Controls.Add(txtBatchFolderPath);
            Controls.Add(btnRunBatch);
            Controls.Add(btnSelectFile);
            Controls.Add(btnSelectFolder);
            Controls.Add(pnlAlignedHost);
            Controls.Add(btnAddMarker);
            Controls.Add(pnlOriginalHost);
            Controls.Add(btnRemoveMarker);
            Controls.Add(lblAlignedTitle);
            Controls.Add(btnConfigureBubbles);
            Controls.Add(lblOriginalTitle);
            Controls.Add(btnAlign);
            Controls.Add(lblStatus);
            Controls.Add(btnSaveProfile);
            Controls.Add(gridMarkers);
            Controls.Add(btnLoadProfile);
            Controls.Add(txtInfo);
            Controls.Add(lstFiles);
            Name = "MainForm2";
            Text = "MainForm2";
            ((System.ComponentModel.ISupportInitialize)gridMarkers).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtInfo;
        private ListBox lstFiles;
        private DataGridView gridMarkers;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private Label lblStatus;
        private Button btnSelectFile;
        private Button btnSelectFolder;
        private Button btnAddMarker;
        private Button btnRemoveMarker;
        private Button btnConfigureBubbles;
        private Button btnAlign;
        private Button btnSaveProfile;
        private Button btnLoadProfile;
        private Panel pnlAlignedHost;
        private Label lblAlignedTitle;
        private Panel pnlOriginalHost;
        private Label lblOriginalTitle;
        private Button btnRunBatch;
        private TextBox txtBatchFolderPath;
        private ProgressBar progressBatch;
        private Label lblBatchProgress;
    }
}