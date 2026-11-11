
using System.Globalization;
using OmrDesktop.Configuration;
using OmrDesktop.Controls;
using OmrDesktop.Controllers;
using OmrDesktop.Models;
using OmrDesktop.Services;

namespace OmrDesktop;

public partial class MainForm : Form
{
    private readonly AlignmentSettings _settings;
    private readonly MainFormController _controller;
    private readonly AlignmentProfileService _profileService;

    private readonly ZoomableImageControl _originalView;
    private readonly ZoomableImageControl _alignedView;

    private string? _currentImagePath;
    private bool _refreshingGrid;
    private readonly string _defaultProfilePath = Path.Combine(AppContext.BaseDirectory, "AnswerSheet-1405.json");
    public MainForm()
    {
        InitializeComponent();

        // تنظیم متن و ظاهر دکمه‌ها
        ConfigureToolbarButtons();

        _settings = new AlignmentSettings();
        _controller = new MainFormController(_settings);
        _profileService = new AlignmentProfileService();

        _originalView = new ZoomableImageControl(_settings)
        {
            Dock = DockStyle.Fill
        };

        _alignedView = new ZoomableImageControl(_settings)
        {
            Dock = DockStyle.Fill
        };

        pnlOriginalHost.Controls.Add(_originalView);
        pnlAlignedHost.Controls.Add(_alignedView);

        RegisterEvents();

        LoadDefaultProfile();
    }

    /// <summary>
    /// تنظیم متن، اندازه و ظاهر دکمه‌های نوار ابزار
    /// </summary>
    private void ConfigureToolbarButtons()
    {
        var buttons = new (Button Button, string Text)[]
        {
            (btnSelectFile, "انتخاب فایل"),
            (btnSelectFolder, "انتخاب پوشه"),
            (btnAddMarker, "افزودن نشانگر"),
            (btnRemoveMarker, "حذف نشانگر"),
            (btnAlign, "تراز تصویر"),
            (btnSaveProfile, "ذخیره پروفایل"),
            (btnLoadProfile, "بارگذاری پروفایل")
        };

        foreach (var item in buttons)
        {
            item.Button.Text = item.Text;
            item.Button.AutoSize = true;
            item.Button.MinimumSize = new Size(100, 36);
            item.Button.Padding = new Padding(8, 4, 8, 4);
            item.Button.Margin = new Padding(4);
            item.Button.Font = new Font("Tahoma", 9F);
            item.Button.TextAlign = ContentAlignment.MiddleCenter;
            item.Button.UseVisualStyleBackColor = true;
        }
    }
    private void LoadDefaultProfile()
    {
        try
        {
            if (!File.Exists(_defaultProfilePath))
            {
                lblStatus.Text = "فایل پروفایل پیش‌فرض پیدا نشد؛ از تنظیمات اولیه استفاده می‌شود.";

                _originalView.SetMarkers(_controller.Profile.Markers);
                RefreshMarkerGrid();

                return;
            }

            var profile = _profileService.Load(_defaultProfilePath);

            _controller.SetProfile(profile);
            _originalView.SetMarkers(profile.Markers);

            RefreshMarkerGrid();

            lblStatus.Text = "پروفایل پیش‌فرض با موفقیت بارگذاری شد.";
        }
        catch (Exception ex)
        {
            _originalView.SetMarkers(_controller.Profile.Markers);
            RefreshMarkerGrid();

            MessageBox.Show(
                ex.Message,
                "خطای بارگذاری پروفایل پیش‌فرض",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    private void RegisterEvents()
    {
        btnSelectFile.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.tif;*.tiff"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                LoadSelectedImage(dialog.FileName);
        };

        btnSelectFolder.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
                _controller.LoadFolder(dialog.SelectedPath);
        };

        lstFiles.SelectedIndexChanged += (_, _) =>
        {
            if (lstFiles.SelectedItem is ImageFileItem item)
                LoadSelectedImage(item.FullPath);
        };

        btnAddMarker.Click += (_, _) =>
        {
            _originalView.AddMarkerMode = true;

            lblStatus.Text =
                "برای افزودن نشانگر، روی تصویر مستطیل بکشید.";

            _originalView.Focus();
        };

        btnRemoveMarker.Click += (_, _) =>
        {
            _originalView.RemoveSelectedMarker();
            RefreshMarkerGrid();
        };

        btnAlign.Click += (_, _) =>
        {
            if (_currentImagePath == null)
                return;

            UpdateProfileFromEditor();
            _controller.LoadImage(_currentImagePath);
        };

        btnSaveProfile.Click += (_, _) => SaveProfile();

        btnLoadProfile.Click += (_, _) => LoadProfile();

        _controller.SourceImageReady += bitmap =>
        {
            _originalView.SetImage(bitmap);
        };

        _controller.ImagesReady += (bitmap, info) =>
        {
            _alignedView.SetImage(bitmap);
            txtInfo.Text = info;
        };

        _controller.StatusChanged += message =>
        {
            lblStatus.Text = message;
        };

        _controller.FilesLoaded += files =>
        {
            lstFiles.Items.Clear();

            foreach (var file in files)
                lstFiles.Items.Add(file);
        };

        _originalView.MarkersChanged += () =>
        {
            UpdateProfileFromEditor();
            RefreshMarkerGrid();
        };

        _originalView.SelectionChanged += SelectGridMarker;

        gridMarkers.CellEndEdit += (_, e) =>
        {
            UpdateMarkerFromGrid(e.RowIndex);
        };

        gridMarkers.SelectionChanged += (_, _) =>
        {
            if (_refreshingGrid)
                return;

            if (gridMarkers.CurrentRow?.Tag is AlignmentMarker marker)
                _originalView.SelectMarker(marker.Id);
        };
    }

    private void LoadSelectedImage(string path)
    {
        _currentImagePath = path;

        _controller.SetProfile(GetCurrentProfile());

        _controller.LoadImage(path);
    }

    private AlignmentProfile GetCurrentProfile()
    {
        var profile = _controller.Profile;
        profile.Markers = _originalView.Markers.ToList();
        return profile;
    }

    private void UpdateProfileFromEditor()
    {
        _controller.Profile.Markers =
            _originalView.Markers.ToList();
    }

    private void RefreshMarkerGrid()
    {
        _refreshingGrid = true;

        try
        {
            var selectedId = _originalView.SelectedMarkerId;

            gridMarkers.Rows.Clear();

            foreach (var marker in _originalView.Markers)
            {
                var index = gridMarkers.Rows.Add(
                    marker.Name,
                    marker.TargetXRatio * 100,
                    marker.TargetYRatio * 100);

                gridMarkers.Rows[index].Tag = marker;

                if (selectedId == marker.Id)
                {
                    gridMarkers.Rows[index].Selected = true;
                    gridMarkers.CurrentCell =
                        gridMarkers.Rows[index].Cells[0];
                }
            }
        }
        finally
        {
            _refreshingGrid = false;
        }
    }

    private void SelectGridMarker(Guid? id)
    {
        if (_refreshingGrid || id == null)
            return;

        foreach (DataGridViewRow row in gridMarkers.Rows)
        {
            if (row.Tag is not AlignmentMarker marker ||
                marker.Id != id)
                continue;

            _refreshingGrid = true;

            try
            {
                row.Selected = true;
                gridMarkers.CurrentCell = row.Cells[0];
            }
            finally
            {
                _refreshingGrid = false;
            }

            return;
        }
    }

    private void UpdateMarkerFromGrid(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= gridMarkers.Rows.Count)
            return;

        var row = gridMarkers.Rows[rowIndex];

        if (row.Tag is not AlignmentMarker marker)
            return;

        var name = Convert.ToString(row.Cells["Name"].Value);

        var xText = Convert.ToString(row.Cells["TargetX"].Value);
        var yText = Convert.ToString(row.Cells["TargetY"].Value);

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("نام نشانگر نمی‌تواند خالی باشد.");
            RefreshMarkerGrid();
            return;
        }

        if (!double.TryParse(xText, NumberStyles.Float,
                CultureInfo.CurrentCulture, out var x) ||
            !double.TryParse(yText, NumberStyles.Float,
                CultureInfo.CurrentCulture, out var y) ||
            x < 0 || x > 100 || y < 0 || y > 100)
        {
            MessageBox.Show("مختصات مقصد باید بین صفر تا صد باشد.");
            RefreshMarkerGrid();
            return;
        }

        marker.Name = name.Trim();
        marker.TargetXRatio = x / 100;
        marker.TargetYRatio = y / 100;

        _originalView.NotifyMarkerChanged();

        lblStatus.Text = "مختصات مقصد نشانگر به‌روزرسانی شد.";
    }

    private void SaveProfile()
    {
        try
        {
            UpdateProfileFromEditor();

            _profileService.Save(
                _defaultProfilePath,
                GetCurrentProfile());

            lblStatus.Text = "پروفایل پیش‌فرض با موفقیت ذخیره شد.";

            MessageBox.Show(
                $"پروفایل با موفقیت ذخیره شد.\n\nمسیر فایل:\n{_defaultProfilePath}",
                "ذخیره پروفایل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطای ذخیره پروفایل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadProfile()
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON Profile|*.json"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var profile = _profileService.Load(dialog.FileName);

            _controller.SetProfile(profile);
            _originalView.SetMarkers(profile.Markers);

            RefreshMarkerGrid();

            lblStatus.Text = "پروفایل با موفقیت بارگذاری شد.";

            if (_currentImagePath != null)
                _controller.LoadImage(_currentImagePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطای بارگذاری پروفایل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
