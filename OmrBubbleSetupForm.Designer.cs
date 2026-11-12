
namespace OmrDesktop.Forms;

partial class OmrBubbleSetupForm
{
    private System.ComponentModel.IContainer? components = null;

    private System.Windows.Forms.TableLayoutPanel rootLayout;
    private System.Windows.Forms.FlowLayoutPanel toolbarPanel;
    private System.Windows.Forms.Panel pnlImageHost;
    private System.Windows.Forms.FlowLayoutPanel bottomPanel;

    private System.Windows.Forms.Label lblRow;
    private System.Windows.Forms.Label lblCount;
    private System.Windows.Forms.Label lblSpacing;
    private System.Windows.Forms.Label lblDirection;
    private System.Windows.Forms.Label lblBubblesPerColumn;
    private System.Windows.Forms.Label lblColumnCount;
    private System.Windows.Forms.Label lblColumnSpacing;

    private System.Windows.Forms.ComboBox _rowSelector;
    private System.Windows.Forms.NumericUpDown _bubbleCount;
    private System.Windows.Forms.NumericUpDown _spacing;
    private System.Windows.Forms.NumericUpDown _bubblesPerColumn;
    private System.Windows.Forms.NumericUpDown _columnCount;
    private System.Windows.Forms.NumericUpDown _columnSpacing;
    private System.Windows.Forms.ComboBox _direction;

    private System.Windows.Forms.Button _drawFirstButton;
    private System.Windows.Forms.Button _generateButton;
    private System.Windows.Forms.Button _removeButton;
    private System.Windows.Forms.Button _acceptButton;
    private System.Windows.Forms.Button _cancelButton;

    private System.Windows.Forms.Label _statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        rootLayout = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        pnlImageHost = new Panel();
        bottomPanel = new FlowLayoutPanel();

        lblRow = new Label();
        lblCount = new Label();
        lblSpacing = new Label();
        lblDirection = new Label();
        lblBubblesPerColumn = new Label();
        lblColumnCount = new Label();
        lblColumnSpacing = new Label();

        _rowSelector = new ComboBox();
        _bubbleCount = new NumericUpDown();
        _spacing = new NumericUpDown();
        _bubblesPerColumn = new NumericUpDown();
        _columnCount = new NumericUpDown();
        _columnSpacing = new NumericUpDown();
        _direction = new ComboBox();

        _drawFirstButton = new Button();
        _generateButton = new Button();
        _removeButton = new Button();
        _acceptButton = new Button();
        _cancelButton = new Button();

        _statusLabel = new Label();

        rootLayout.SuspendLayout();
        toolbarPanel.SuspendLayout();
        bottomPanel.SuspendLayout();

        ((System.ComponentModel.ISupportInitialize)_bubbleCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_spacing).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_bubblesPerColumn).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_columnCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_columnSpacing).BeginInit();

        SuspendLayout();

        // rootLayout
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 100F));
        rootLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 58F));

        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Name = "rootLayout";

        rootLayout.Controls.Add(toolbarPanel, 0, 0);
        rootLayout.Controls.Add(pnlImageHost, 0, 1);
        rootLayout.Controls.Add(bottomPanel, 0, 2);

        // toolbarPanel
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.FlowDirection = FlowDirection.RightToLeft;
        toolbarPanel.WrapContents = true;
        toolbarPanel.AutoScroll = true;
        toolbarPanel.Padding = new Padding(8);
        toolbarPanel.Name = "toolbarPanel";

        // lblRow
        lblRow.Text = "ردیف:";
        lblRow.AutoSize = true;
        lblRow.Padding = new Padding(5, 8, 5, 0);

        // _rowSelector
        _rowSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _rowSelector.Width = 140;
        _rowSelector.Name = "_rowSelector";

        // lblCount
        lblCount.Text = "تعداد کل:";
        lblCount.AutoSize = true;
        lblCount.Padding = new Padding(5, 8, 5, 0);

        // _bubbleCount
        _bubbleCount.Minimum = 1;
        _bubbleCount.Maximum = 500;
        _bubbleCount.Value = 24;
        _bubbleCount.Width = 65;
        _bubbleCount.Name = "_bubbleCount";

        // lblSpacing
        lblSpacing.Text = "فاصله حباب‌ها (پیکسل):";
        lblSpacing.AutoSize = true;
        lblSpacing.Padding = new Padding(5, 8, 5, 0);

        // _spacing
        _spacing.Minimum = 0;
        _spacing.Maximum = 2000;
        _spacing.DecimalPlaces = 1;
        _spacing.Increment = 0.5M;
        _spacing.Value = 5;
        _spacing.Width = 75;
        _spacing.Name = "_spacing";

        // lblBubblesPerColumn
        lblBubblesPerColumn.Text = "حباب در هر ستون:";
        lblBubblesPerColumn.AutoSize = true;
        lblBubblesPerColumn.Padding = new Padding(5, 8, 5, 0);

        // _bubblesPerColumn
        _bubblesPerColumn.Minimum = 1;
        _bubblesPerColumn.Maximum = 100;
        _bubblesPerColumn.Value = 4;
        _bubblesPerColumn.Width = 60;
        _bubblesPerColumn.Name = "_bubblesPerColumn";

        // lblColumnCount
        lblColumnCount.Text = "تعداد ستون:";
        lblColumnCount.AutoSize = true;
        lblColumnCount.Padding = new Padding(5, 8, 5, 0);

        // _columnCount
        _columnCount.Minimum = 1;
        _columnCount.Maximum = 50;
        _columnCount.Value = 6;
        _columnCount.Width = 60;
        _columnCount.Name = "_columnCount";

        // lblColumnSpacing
        lblColumnSpacing.Text = "فاصله ستون‌ها (پیکسل):";
        lblColumnSpacing.AutoSize = true;
        lblColumnSpacing.Padding = new Padding(5, 8, 5, 0);

        // _columnSpacing
        _columnSpacing.Minimum = 0;
        _columnSpacing.Maximum = 3000;
        _columnSpacing.DecimalPlaces = 1;
        _columnSpacing.Increment = 1;
        _columnSpacing.Value = 15;
        _columnSpacing.Width = 75;
        _columnSpacing.Name = "_columnSpacing";

        // lblDirection
        lblDirection.Text = "جهت:";
        lblDirection.AutoSize = true;
        lblDirection.Padding = new Padding(5, 8, 5, 0);

        // _direction
        _direction.DropDownStyle = ComboBoxStyle.DropDownList;
        _direction.Width = 75;
        _direction.Items.AddRange(new object[] { "چپ", "راست" });
        _direction.SelectedIndex = 1;
        _direction.Name = "_direction";

        // Buttons
        ConfigureButton(_drawFirstButton, "رسم حباب مرجع");
        ConfigureButton(_generateButton, "ساخت حباب‌ها");
        ConfigureButton(_removeButton, "حذف حباب انتخاب‌شده");
        ConfigureButton(_acceptButton, "ذخیره");
        ConfigureButton(_cancelButton, "انصراف");

        _drawFirstButton.Name = "_drawFirstButton";
        _generateButton.Name = "_generateButton";
        _removeButton.Name = "_removeButton";
        _acceptButton.Name = "_acceptButton";
        _cancelButton.Name = "_cancelButton";

        // Add toolbar controls
        toolbarPanel.Controls.Add(lblRow);
        toolbarPanel.Controls.Add(_rowSelector);

        toolbarPanel.Controls.Add(lblCount);
        toolbarPanel.Controls.Add(_bubbleCount);

        toolbarPanel.Controls.Add(lblSpacing);
        toolbarPanel.Controls.Add(_spacing);

        toolbarPanel.Controls.Add(lblBubblesPerColumn);
        toolbarPanel.Controls.Add(_bubblesPerColumn);

        toolbarPanel.Controls.Add(lblColumnCount);
        toolbarPanel.Controls.Add(_columnCount);

        toolbarPanel.Controls.Add(lblColumnSpacing);
        toolbarPanel.Controls.Add(_columnSpacing);

        toolbarPanel.Controls.Add(lblDirection);
        toolbarPanel.Controls.Add(_direction);

        toolbarPanel.Controls.Add(_drawFirstButton);
        toolbarPanel.Controls.Add(_generateButton);
        toolbarPanel.Controls.Add(_removeButton);

        // pnlImageHost
        pnlImageHost.Dock = DockStyle.Fill;
        pnlImageHost.BackColor = Color.FromArgb(40, 40, 40);
        pnlImageHost.Name = "pnlImageHost";

        // bottomPanel
        bottomPanel.Dock = DockStyle.Fill;
        bottomPanel.FlowDirection = FlowDirection.RightToLeft;
        bottomPanel.WrapContents = false;
        bottomPanel.Padding = new Padding(8);
        bottomPanel.Name = "bottomPanel";

        // _statusLabel
        _statusLabel.AutoSize = true;
        _statusLabel.Padding = new Padding(8, 8, 8, 0);
        _statusLabel.Text = "آماده";
        _statusLabel.Name = "_statusLabel";

        bottomPanel.Controls.Add(_acceptButton);
        bottomPanel.Controls.Add(_cancelButton);
        bottomPanel.Controls.Add(_statusLabel);

        // Form
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;

        ClientSize = new Size(1500, 950);
        MinimumSize = new Size(1100, 750);

        Name = "OmrBubbleSetupForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "تنظیم حباب‌های پاسخنامه";

        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;

        Controls.Add(rootLayout);

        AcceptButton = _acceptButton;
        CancelButton = _cancelButton;

        rootLayout.ResumeLayout(false);
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        bottomPanel.ResumeLayout(false);
        bottomPanel.PerformLayout();

        ((System.ComponentModel.ISupportInitialize)_bubbleCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)_spacing).EndInit();
        ((System.ComponentModel.ISupportInitialize)_bubblesPerColumn).EndInit();
        ((System.ComponentModel.ISupportInitialize)_columnCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)_columnSpacing).EndInit();

        ResumeLayout(false);
    }

    private static void ConfigureButton(
        System.Windows.Forms.Button button,
        string text)
    {
        button.Text = text;
        button.Width = 125;
        button.Height = 32;
        button.Margin = new Padding(4);
        button.UseVisualStyleBackColor = true;
    }
}
