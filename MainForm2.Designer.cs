
namespace OmrDesktop
{
    partial class MainForm2
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel rootLayout;
        private TableLayoutPanel leftLayout;
        private TableLayoutPanel rightLayout;
        private TableLayoutPanel toolbarLayout;
        private TableLayoutPanel batchProgressLayout;
        private TableLayoutPanel imageTitleLayout;
        private TableLayoutPanel imageLayout;

        private TextBox txtInfo;
        private ListBox lstFiles;
        private DataGridView gridMarkers;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private Label lblStatus;
        private Button btnAddMarker;
        private Button btnRemoveMarker;
        private Button btnConfigureBubbles;
        private Button btnAlign;
        private Button btnSaveProfile;
        private Button btnLoadProfile;
        private Button btnRunBatch;

        private Panel pnlAlignedHost;
        private Panel pnlOriginalHost;

        private Label lblAlignedTitle;
        private Label lblOriginalTitle;
        private TextBox txtBatchFolderPath;
        private ProgressBar progressBatch;
        private Label lblBatchProgress;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            rootLayout = new TableLayoutPanel();
            leftLayout = new TableLayoutPanel();
            lstFiles = new ListBox();
            txtInfo = new TextBox();
            gridMarkers = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            lblStatus = new Label();
            rightLayout = new TableLayoutPanel();
            toolbarLayout = new TableLayoutPanel();
            batchProgressLayout = new TableLayoutPanel();
            progressBatch = new ProgressBar();
            lblBatchProgress = new Label();
            btnRunBatch = new Button();
            btnSelectFile = new Button();
            btnAlign = new Button();
            btnAddMarker = new Button();
            btnRemoveMarker = new Button();
            btnConfigureBubbles = new Button();
            btnSaveProfile = new Button();
            btnLoadProfile = new Button();
            btnSelectFolder = new Button();
            txtBatchFolderPath = new TextBox();
            imageTitleLayout = new TableLayoutPanel();
            lblOriginalTitle = new Label();
            lblAlignedTitle = new Label();
            imageLayout = new TableLayoutPanel();
            pnlOriginalHost = new Panel();
            pnlAlignedHost = new Panel();
            rootLayout.SuspendLayout();
            leftLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridMarkers).BeginInit();
            rightLayout.SuspendLayout();
            toolbarLayout.SuspendLayout();
            batchProgressLayout.SuspendLayout();
            imageTitleLayout.SuspendLayout();
            imageLayout.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.ColumnCount = 2;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 332F));
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.Controls.Add(leftLayout, 0, 0);
            rootLayout.Controls.Add(rightLayout, 1, 0);
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.Location = new Point(0, 0);
            rootLayout.Margin = new Padding(0);
            rootLayout.Name = "rootLayout";
            rootLayout.Padding = new Padding(10);
            rootLayout.RowCount = 1;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.Size = new Size(1258, 728);
            rootLayout.TabIndex = 0;
            // 
            // leftLayout
            // 
            leftLayout.ColumnCount = 1;
            leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftLayout.Controls.Add(lstFiles, 0, 0);
            leftLayout.Controls.Add(txtInfo, 0, 1);
            leftLayout.Controls.Add(gridMarkers, 0, 2);
            leftLayout.Controls.Add(lblStatus, 0, 3);
            leftLayout.Dock = DockStyle.Fill;
            leftLayout.Location = new Point(10, 10);
            leftLayout.Margin = new Padding(0, 0, 6, 0);
            leftLayout.Name = "leftLayout";
            leftLayout.RowCount = 4;
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 26F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 19F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 27F));
            leftLayout.Size = new Size(326, 708);
            leftLayout.TabIndex = 0;
            // 
            // lstFiles
            // 
            lstFiles.Dock = DockStyle.Fill;
            lstFiles.FormattingEnabled = true;
            lstFiles.Location = new Point(0, 0);
            lstFiles.Margin = new Padding(0, 0, 0, 6);
            lstFiles.Name = "lstFiles";
            lstFiles.Size = new Size(326, 178);
            lstFiles.TabIndex = 0;
            // 
            // txtInfo
            // 
            txtInfo.Dock = DockStyle.Fill;
            txtInfo.Location = new Point(0, 184);
            txtInfo.Margin = new Padding(0, 0, 0, 6);
            txtInfo.Multiline = true;
            txtInfo.Name = "txtInfo";
            txtInfo.RightToLeft = RightToLeft.Yes;
            txtInfo.Size = new Size(326, 192);
            txtInfo.TabIndex = 1;
            txtInfo.Text = "اطلاعات";
            // 
            // gridMarkers
            // 
            gridMarkers.AllowUserToAddRows = false;
            gridMarkers.AllowUserToDeleteRows = false;
            gridMarkers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridMarkers.BackgroundColor = Color.White;
            gridMarkers.ColumnHeadersHeight = 29;
            gridMarkers.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3 });
            gridMarkers.Dock = DockStyle.Fill;
            gridMarkers.Location = new Point(0, 382);
            gridMarkers.Margin = new Padding(0, 0, 0, 6);
            gridMarkers.MultiSelect = false;
            gridMarkers.Name = "gridMarkers";
            gridMarkers.RightToLeft = RightToLeft.Yes;
            gridMarkers.RowHeadersVisible = false;
            gridMarkers.RowHeadersWidth = 51;
            gridMarkers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridMarkers.Size = new Size(326, 128);
            gridMarkers.TabIndex = 2;
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
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Location = new Point(0, 516);
            lblStatus.Margin = new Padding(0);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(8);
            lblStatus.RightToLeft = RightToLeft.Yes;
            lblStatus.Size = new Size(326, 192);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "وضعیت";
            // 
            // rightLayout
            // 
            rightLayout.ColumnCount = 1;
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rightLayout.Controls.Add(toolbarLayout, 0, 0);
            rightLayout.Controls.Add(txtBatchFolderPath, 0, 1);
            rightLayout.Controls.Add(imageTitleLayout, 0, 2);
            rightLayout.Controls.Add(imageLayout, 0, 3);
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.Location = new Point(342, 10);
            rightLayout.Margin = new Padding(0);
            rightLayout.Name = "rightLayout";
            rightLayout.RowCount = 4;
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rightLayout.Size = new Size(906, 708);
            rightLayout.TabIndex = 1;
            // 
            // toolbarLayout
            // 
            toolbarLayout.ColumnCount = 11;
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 201F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1111F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            toolbarLayout.Controls.Add(batchProgressLayout, 0, 0);
            toolbarLayout.Controls.Add(btnRunBatch, 1, 0);
            toolbarLayout.Controls.Add(btnSelectFile, 9, 0);
            toolbarLayout.Controls.Add(btnAlign, 2, 0);
            toolbarLayout.Controls.Add(btnAddMarker, 8, 0);
            toolbarLayout.Controls.Add(btnRemoveMarker, 7, 0);
            toolbarLayout.Controls.Add(btnConfigureBubbles, 6, 0);
            toolbarLayout.Controls.Add(btnSaveProfile, 5, 0);
            toolbarLayout.Controls.Add(btnLoadProfile, 4, 0);
            toolbarLayout.Controls.Add(btnSelectFolder, 3, 0);
            toolbarLayout.Dock = DockStyle.Fill;
            toolbarLayout.Location = new Point(0, 0);
            toolbarLayout.Margin = new Padding(0);
            toolbarLayout.Name = "toolbarLayout";
            toolbarLayout.RowCount = 1;
            toolbarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            toolbarLayout.Size = new Size(906, 54);
            toolbarLayout.TabIndex = 0;
            // 
            // batchProgressLayout
            // 
            batchProgressLayout.ColumnCount = 1;
            batchProgressLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            batchProgressLayout.Controls.Add(progressBatch, 0, 0);
            batchProgressLayout.Controls.Add(lblBatchProgress, 0, 1);
            batchProgressLayout.Dock = DockStyle.Fill;
            batchProgressLayout.Location = new Point(0, 0);
            batchProgressLayout.Margin = new Padding(0, 0, 3, 0);
            batchProgressLayout.Name = "batchProgressLayout";
            batchProgressLayout.RowCount = 2;
            batchProgressLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            batchProgressLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            batchProgressLayout.Size = new Size(198, 54);
            batchProgressLayout.TabIndex = 0;
            // 
            // progressBatch
            // 
            progressBatch.Dock = DockStyle.Fill;
            progressBatch.Location = new Point(0, 0);
            progressBatch.Margin = new Padding(0, 0, 0, 1);
            progressBatch.Name = "progressBatch";
            progressBatch.Size = new Size(198, 28);
            progressBatch.TabIndex = 0;
            // 
            // lblBatchProgress
            // 
            lblBatchProgress.Dock = DockStyle.Fill;
            lblBatchProgress.Location = new Point(2, 29);
            lblBatchProgress.Margin = new Padding(2, 0, 0, 0);
            lblBatchProgress.Name = "lblBatchProgress";
            lblBatchProgress.RightToLeft = RightToLeft.Yes;
            lblBatchProgress.Size = new Size(196, 25);
            lblBatchProgress.TabIndex = 1;
            lblBatchProgress.Text = "آماده";
            lblBatchProgress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnRunBatch
            // 
            btnRunBatch.AutoSize = true;
            btnRunBatch.Enabled = false;
            btnRunBatch.Location = new Point(204, 3);
            btnRunBatch.Name = "btnRunBatch";
            btnRunBatch.RightToLeft = RightToLeft.Yes;
            btnRunBatch.Size = new Size(70, 36);
            btnRunBatch.TabIndex = 1;
            btnRunBatch.Text = "9-خواندن تصاویر";
            // 
            // btnSelectFile
            // 
            btnSelectFile.AutoSize = true;
            btnSelectFile.Location = new Point(812, 3);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.RightToLeft = RightToLeft.Yes;
            btnSelectFile.Size = new Size(70, 36);
            btnSelectFile.TabIndex = 9;
            btnSelectFile.Text = "1-انتخاب فایل";
            // 
            // btnAlign
            // 
            btnAlign.AutoSize = true;
            btnAlign.Location = new Point(280, 3);
            btnAlign.Name = "btnAlign";
            btnAlign.RightToLeft = RightToLeft.Yes;
            btnAlign.Size = new Size(70, 36);
            btnAlign.TabIndex = 4;
            btnAlign.Text = "8-تراز تصویر";
            // 
            // btnAddMarker
            // 
            btnAddMarker.AutoSize = true;
            btnAddMarker.Location = new Point(736, 3);
            btnAddMarker.Name = "btnAddMarker";
            btnAddMarker.RightToLeft = RightToLeft.Yes;
            btnAddMarker.Size = new Size(70, 36);
            btnAddMarker.TabIndex = 7;
            btnAddMarker.Text = "2-افزودن نشانگر";
            // 
            // btnRemoveMarker
            // 
            btnRemoveMarker.AutoSize = true;
            btnRemoveMarker.Location = new Point(660, 3);
            btnRemoveMarker.Name = "btnRemoveMarker";
            btnRemoveMarker.RightToLeft = RightToLeft.Yes;
            btnRemoveMarker.Size = new Size(70, 36);
            btnRemoveMarker.TabIndex = 6;
            btnRemoveMarker.Text = "3-حذف نشانگر";
            // 
            // btnConfigureBubbles
            // 
            btnConfigureBubbles.AutoSize = true;
            btnConfigureBubbles.Location = new Point(584, 3);
            btnConfigureBubbles.Name = "btnConfigureBubbles";
            btnConfigureBubbles.RightToLeft = RightToLeft.Yes;
            btnConfigureBubbles.Size = new Size(70, 36);
            btnConfigureBubbles.TabIndex = 5;
            btnConfigureBubbles.Text = "4-تنظیم حباب‌ها";
            // 
            // btnSaveProfile
            // 
            btnSaveProfile.AutoSize = true;
            btnSaveProfile.Location = new Point(508, 3);
            btnSaveProfile.Name = "btnSaveProfile";
            btnSaveProfile.RightToLeft = RightToLeft.Yes;
            btnSaveProfile.Size = new Size(70, 36);
            btnSaveProfile.TabIndex = 3;
            btnSaveProfile.Text = "5-ذخیره پروفایل";
            // 
            // btnLoadProfile
            // 
            btnLoadProfile.AutoSize = true;
            btnLoadProfile.Location = new Point(432, 3);
            btnLoadProfile.Name = "btnLoadProfile";
            btnLoadProfile.RightToLeft = RightToLeft.Yes;
            btnLoadProfile.Size = new Size(70, 36);
            btnLoadProfile.TabIndex = 2;
            btnLoadProfile.Text = "6-بارگذاری پروفایل";
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.AutoSize = true;
            btnSelectFolder.Location = new Point(356, 3);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.RightToLeft = RightToLeft.Yes;
            btnSelectFolder.Size = new Size(70, 36);
            btnSelectFolder.TabIndex = 8;
            btnSelectFolder.Text = "7-انتخاب پوشه";
            // 
            // txtBatchFolderPath
            // 
            txtBatchFolderPath.Dock = DockStyle.Fill;
            txtBatchFolderPath.Location = new Point(0, 54);
            txtBatchFolderPath.Margin = new Padding(0, 0, 0, 3);
            txtBatchFolderPath.Name = "txtBatchFolderPath";
            txtBatchFolderPath.ReadOnly = true;
            txtBatchFolderPath.RightToLeft = RightToLeft.No;
            txtBatchFolderPath.Size = new Size(906, 27);
            txtBatchFolderPath.TabIndex = 1;
            txtBatchFolderPath.Text = "ابتدا پوشه تصاویر را انتخاب کنید";
            // 
            // imageTitleLayout
            // 
            imageTitleLayout.ColumnCount = 2;
            imageTitleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            imageTitleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            imageTitleLayout.Controls.Add(lblOriginalTitle, 0, 0);
            imageTitleLayout.Controls.Add(lblAlignedTitle, 1, 0);
            imageTitleLayout.Dock = DockStyle.Fill;
            imageTitleLayout.Location = new Point(0, 84);
            imageTitleLayout.Margin = new Padding(0);
            imageTitleLayout.Name = "imageTitleLayout";
            imageTitleLayout.RowCount = 1;
            imageTitleLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            imageTitleLayout.Size = new Size(906, 28);
            imageTitleLayout.TabIndex = 2;
            // 
            // lblOriginalTitle
            // 
            lblOriginalTitle.Dock = DockStyle.Fill;
            lblOriginalTitle.Location = new Point(3, 0);
            lblOriginalTitle.Name = "lblOriginalTitle";
            lblOriginalTitle.RightToLeft = RightToLeft.Yes;
            lblOriginalTitle.Size = new Size(447, 28);
            lblOriginalTitle.TabIndex = 0;
            lblOriginalTitle.Text = "تصویر اصلی - ویرایش نشانگرها";
            lblOriginalTitle.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblAlignedTitle
            // 
            lblAlignedTitle.Dock = DockStyle.Fill;
            lblAlignedTitle.Location = new Point(456, 0);
            lblAlignedTitle.Name = "lblAlignedTitle";
            lblAlignedTitle.RightToLeft = RightToLeft.Yes;
            lblAlignedTitle.Size = new Size(447, 28);
            lblAlignedTitle.TabIndex = 1;
            lblAlignedTitle.Text = "تصویر استاندارد تراز شده";
            lblAlignedTitle.TextAlign = ContentAlignment.MiddleRight;
            // 
            // imageLayout
            // 
            imageLayout.ColumnCount = 2;
            imageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            imageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            imageLayout.Controls.Add(pnlOriginalHost, 0, 0);
            imageLayout.Controls.Add(pnlAlignedHost, 1, 0);
            imageLayout.Dock = DockStyle.Fill;
            imageLayout.Location = new Point(0, 112);
            imageLayout.Margin = new Padding(0);
            imageLayout.Name = "imageLayout";
            imageLayout.RowCount = 1;
            imageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            imageLayout.Size = new Size(906, 596);
            imageLayout.TabIndex = 3;
            // 
            // pnlOriginalHost
            // 
            pnlOriginalHost.BackColor = Color.FromArgb(40, 40, 40);
            pnlOriginalHost.Dock = DockStyle.Fill;
            pnlOriginalHost.Location = new Point(0, 0);
            pnlOriginalHost.Margin = new Padding(0, 0, 3, 0);
            pnlOriginalHost.Name = "pnlOriginalHost";
            pnlOriginalHost.Size = new Size(450, 596);
            pnlOriginalHost.TabIndex = 0;
            // 
            // pnlAlignedHost
            // 
            pnlAlignedHost.BackColor = Color.FromArgb(40, 40, 40);
            pnlAlignedHost.Dock = DockStyle.Fill;
            pnlAlignedHost.Location = new Point(456, 0);
            pnlAlignedHost.Margin = new Padding(3, 0, 0, 0);
            pnlAlignedHost.Name = "pnlAlignedHost";
            pnlAlignedHost.Size = new Size(450, 596);
            pnlAlignedHost.TabIndex = 1;
            // 
            // MainForm2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1258, 728);
            Controls.Add(rootLayout);
            MinimumSize = new Size(1100, 600);
            Name = "MainForm2";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "سامانه پردازش پاسخنامه";
            WindowState = FormWindowState.Maximized;
            rootLayout.ResumeLayout(false);
            leftLayout.ResumeLayout(false);
            leftLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridMarkers).EndInit();
            rightLayout.ResumeLayout(false);
            rightLayout.PerformLayout();
            toolbarLayout.ResumeLayout(false);
            toolbarLayout.PerformLayout();
            batchProgressLayout.ResumeLayout(false);
            imageTitleLayout.ResumeLayout(false);
            imageLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void ConfigureLayoutButton(
            Button button,
            string text,
            string name)
        {
            button.Name = name;
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(3, 5, 3, 5);
            button.RightToLeft = RightToLeft.Yes;
            button.UseVisualStyleBackColor = true;
            button.AutoSize = false;
        }

        private Button btnSelectFolder;
        private Button btnSelectFile;
    }
}
