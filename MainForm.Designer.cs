
namespace OmrDesktop;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    private System.Windows.Forms.TableLayoutPanel rootLayout;
    private System.Windows.Forms.FlowLayoutPanel toolbarPanel;

    private System.Windows.Forms.Button btnSelectFile;
    private System.Windows.Forms.Button btnSelectFolder;
    private System.Windows.Forms.Button btnAddMarker;
    private System.Windows.Forms.Button btnRemoveMarker;
    private System.Windows.Forms.Button btnAlign;
    private System.Windows.Forms.Button btnSaveProfile;
    private System.Windows.Forms.Button btnLoadProfile;

    private System.Windows.Forms.SplitContainer mainSplit;
    private System.Windows.Forms.SplitContainer leftSplit;
    private System.Windows.Forms.SplitContainer imageSplit;

    private System.Windows.Forms.ListBox lstFiles;
    private System.Windows.Forms.DataGridView gridMarkers;

    private System.Windows.Forms.Label lblOriginalTitle;
    private System.Windows.Forms.Label lblAlignedTitle;

    private System.Windows.Forms.Panel pnlOriginalHost;
    private System.Windows.Forms.Panel pnlAlignedHost;

    private System.Windows.Forms.TextBox txtInfo;
    private System.Windows.Forms.Label lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        rootLayout = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        btnSelectFile = new Button();
        btnSelectFolder = new Button();
        btnAddMarker = new Button();
        btnRemoveMarker = new Button();
        btnAlign = new Button();
        btnSaveProfile = new Button();
        btnLoadProfile = new Button();
        mainSplit = new SplitContainer();
        leftSplit = new SplitContainer();
        lstFiles = new ListBox();
        gridMarkers = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        imageSplit = new SplitContainer();
        pnlOriginalHost = new Panel();
        lblOriginalTitle = new Label();
        pnlAlignedHost = new Panel();
        lblAlignedTitle = new Label();
        txtInfo = new TextBox();
        lblStatus = new Label();
        rootLayout.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
        mainSplit.Panel1.SuspendLayout();
        mainSplit.Panel2.SuspendLayout();
        mainSplit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)leftSplit).BeginInit();
        leftSplit.Panel1.SuspendLayout();
        leftSplit.Panel2.SuspendLayout();
        leftSplit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridMarkers).BeginInit();
        ((System.ComponentModel.ISupportInitialize)imageSplit).BeginInit();
        imageSplit.Panel1.SuspendLayout();
        imageSplit.Panel2.SuspendLayout();
        imageSplit.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        rootLayout.Controls.Add(toolbarPanel, 0, 0);
        rootLayout.Controls.Add(mainSplit, 0, 1);
        rootLayout.Controls.Add(txtInfo, 0, 2);
        rootLayout.Controls.Add(lblStatus, 0, 3);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.RowCount = 4;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        rootLayout.Size = new Size(1500, 900);
        rootLayout.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.Controls.Add(btnSelectFile);
        toolbarPanel.Controls.Add(btnSelectFolder);
        toolbarPanel.Controls.Add(btnAddMarker);
        toolbarPanel.Controls.Add(btnRemoveMarker);
        toolbarPanel.Controls.Add(btnAlign);
        toolbarPanel.Controls.Add(btnSaveProfile);
        toolbarPanel.Controls.Add(btnLoadProfile);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(3, 3);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(5);
        toolbarPanel.Size = new Size(1494, 42);
        toolbarPanel.TabIndex = 0;
        toolbarPanel.WrapContents = false;
        // 
        // btnSelectFile
        // 
        btnSelectFile.Location = new Point(8, 8);
        btnSelectFile.Name = "btnSelectFile";
        btnSelectFile.Size = new Size(75, 23);
        btnSelectFile.TabIndex = 0;
        // 
        // btnSelectFolder
        // 
        btnSelectFolder.Location = new Point(89, 8);
        btnSelectFolder.Name = "btnSelectFolder";
        btnSelectFolder.Size = new Size(75, 23);
        btnSelectFolder.TabIndex = 1;
        // 
        // btnAddMarker
        // 
        btnAddMarker.Location = new Point(170, 8);
        btnAddMarker.Name = "btnAddMarker";
        btnAddMarker.Size = new Size(75, 23);
        btnAddMarker.TabIndex = 2;
        // 
        // btnRemoveMarker
        // 
        btnRemoveMarker.Location = new Point(251, 8);
        btnRemoveMarker.Name = "btnRemoveMarker";
        btnRemoveMarker.Size = new Size(75, 23);
        btnRemoveMarker.TabIndex = 3;
        // 
        // btnAlign
        // 
        btnAlign.Location = new Point(332, 8);
        btnAlign.Name = "btnAlign";
        btnAlign.Size = new Size(75, 23);
        btnAlign.TabIndex = 4;
        // 
        // btnSaveProfile
        // 
        btnSaveProfile.Location = new Point(413, 8);
        btnSaveProfile.Name = "btnSaveProfile";
        btnSaveProfile.Size = new Size(75, 23);
        btnSaveProfile.TabIndex = 5;
        // 
        // btnLoadProfile
        // 
        btnLoadProfile.Location = new Point(494, 8);
        btnLoadProfile.Name = "btnLoadProfile";
        btnLoadProfile.Size = new Size(75, 23);
        btnLoadProfile.TabIndex = 6;
        // 
        // mainSplit
        // 
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.Location = new Point(3, 51);
        mainSplit.Name = "mainSplit";
        // 
        // mainSplit.Panel1
        // 
        mainSplit.Panel1.Controls.Add(leftSplit);
        mainSplit.Panel1MinSize = 220;
        // 
        // mainSplit.Panel2
        // 
        mainSplit.Panel2.Controls.Add(imageSplit);
        mainSplit.Panel2MinSize = 500;
        mainSplit.Size = new Size(1494, 706);
        mainSplit.SplitterDistance = 990;
        mainSplit.TabIndex = 1;
        // 
        // leftSplit
        // 
        leftSplit.Dock = DockStyle.Fill;
        leftSplit.Location = new Point(0, 0);
        leftSplit.Name = "leftSplit";
        leftSplit.Orientation = Orientation.Horizontal;
        // 
        // leftSplit.Panel1
        // 
        leftSplit.Panel1.Controls.Add(lstFiles);
        leftSplit.Panel1MinSize = 100;
        // 
        // leftSplit.Panel2
        // 
        leftSplit.Panel2.Controls.Add(gridMarkers);
        leftSplit.Panel2MinSize = 150;
        leftSplit.Size = new Size(990, 706);
        leftSplit.SplitterDistance = 501;
        leftSplit.TabIndex = 0;
        // 
        // lstFiles
        // 
        lstFiles.Dock = DockStyle.Fill;
        lstFiles.IntegralHeight = false;
        lstFiles.Location = new Point(0, 0);
        lstFiles.Name = "lstFiles";
        lstFiles.Size = new Size(990, 501);
        lstFiles.TabIndex = 0;
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
        gridMarkers.Location = new Point(0, 0);
        gridMarkers.MultiSelect = false;
        gridMarkers.Name = "gridMarkers";
        gridMarkers.RowHeadersVisible = false;
        gridMarkers.RowHeadersWidth = 51;
        gridMarkers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridMarkers.Size = new Size(990, 201);
        gridMarkers.TabIndex = 0;
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
        // imageSplit
        // 
        imageSplit.Dock = DockStyle.Fill;
        imageSplit.Location = new Point(0, 0);
        imageSplit.Name = "imageSplit";
        // 
        // imageSplit.Panel1
        // 
        imageSplit.Panel1.Controls.Add(pnlOriginalHost);
        imageSplit.Panel1.Controls.Add(lblOriginalTitle);
        imageSplit.Panel1MinSize = 250;
        // 
        // imageSplit.Panel2
        // 
        imageSplit.Panel2.Controls.Add(pnlAlignedHost);
        imageSplit.Panel2.Controls.Add(lblAlignedTitle);
        imageSplit.Size = new Size(500, 706);
        imageSplit.SplitterDistance = 250;
        imageSplit.TabIndex = 0;
        // 
        // pnlOriginalHost
        // 
        pnlOriginalHost.BackColor = Color.FromArgb(40, 40, 40);
        pnlOriginalHost.Dock = DockStyle.Fill;
        pnlOriginalHost.Location = new Point(0, 30);
        pnlOriginalHost.Name = "pnlOriginalHost";
        pnlOriginalHost.Size = new Size(250, 676);
        pnlOriginalHost.TabIndex = 0;
        // 
        // lblOriginalTitle
        // 
        lblOriginalTitle.Dock = DockStyle.Top;
        lblOriginalTitle.Font = new Font("Tahoma", 9F, FontStyle.Bold);
        lblOriginalTitle.Location = new Point(0, 0);
        lblOriginalTitle.Name = "lblOriginalTitle";
        lblOriginalTitle.Size = new Size(250, 30);
        lblOriginalTitle.TabIndex = 1;
        lblOriginalTitle.Text = "تصویر اصلی - ویرایش نشانگرها";
        lblOriginalTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlAlignedHost
        // 
        pnlAlignedHost.BackColor = Color.FromArgb(40, 40, 40);
        pnlAlignedHost.Dock = DockStyle.Fill;
        pnlAlignedHost.Location = new Point(0, 30);
        pnlAlignedHost.Name = "pnlAlignedHost";
        pnlAlignedHost.Size = new Size(246, 676);
        pnlAlignedHost.TabIndex = 0;
        // 
        // lblAlignedTitle
        // 
        lblAlignedTitle.Dock = DockStyle.Top;
        lblAlignedTitle.Font = new Font("Tahoma", 9F, FontStyle.Bold);
        lblAlignedTitle.Location = new Point(0, 0);
        lblAlignedTitle.Name = "lblAlignedTitle";
        lblAlignedTitle.Size = new Size(246, 30);
        lblAlignedTitle.TabIndex = 1;
        lblAlignedTitle.Text = "تصویر استاندارد تراز شده";
        lblAlignedTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // txtInfo
        // 
        txtInfo.Dock = DockStyle.Fill;
        txtInfo.Location = new Point(3, 763);
        txtInfo.Multiline = true;
        txtInfo.Name = "txtInfo";
        txtInfo.ReadOnly = true;
        txtInfo.ScrollBars = ScrollBars.Vertical;
        txtInfo.Size = new Size(1494, 104);
        txtInfo.TabIndex = 2;
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.Location = new Point(3, 870);
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new Padding(8);
        lblStatus.Size = new Size(1494, 30);
        lblStatus.TabIndex = 3;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1500, 900);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1100, 700);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "OMR - Answer Sheet Alignment";
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        mainSplit.Panel1.ResumeLayout(false);
        mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
        mainSplit.ResumeLayout(false);
        leftSplit.Panel1.ResumeLayout(false);
        leftSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)leftSplit).EndInit();
        leftSplit.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridMarkers).EndInit();
        imageSplit.Panel1.ResumeLayout(false);
        imageSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)imageSplit).EndInit();
        imageSplit.ResumeLayout(false);
        ResumeLayout(false);
    }

    private static void ConfigureButton(Button button, string text)
    {
        button.Text = text;
        button.Width = 120;
        button.Height = 34;
        button.Margin = new Padding(4);
    }

    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
}
