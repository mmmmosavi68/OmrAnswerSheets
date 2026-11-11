
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OmrDesktop.Configuration;
using OmrDesktop.Controls;
using OmrDesktop.Models;

namespace OmrDesktop.Forms;

public partial class OmrBubbleSetupForm : Form
{
    private readonly OmrLayoutProfile _layout;
    private readonly ZoomableImageControl _imageView;

    public OmrLayoutProfile Layout => _layout;

    private OmrBubbleRowProfile SelectedRow
    {
        get
        {
            var name = Convert.ToString(_rowSelector.SelectedItem);

            return _layout.BubbleRows.FirstOrDefault(
                x => x.Name == name)
                ?? throw new InvalidOperationException(
                    "ردیف حباب انتخاب نشده است.");
        }
    }

    public OmrBubbleSetupForm(
        string imagePath,
        OmrLayoutProfile layout,
        AlignmentSettings settings)
    {
        if (string.IsNullOrWhiteSpace(imagePath) ||
            !File.Exists(imagePath))
        {
            throw new FileNotFoundException(
                "تصویر مرجع پیدا نشد.",
                imagePath);
        }

        _layout = layout ?? new OmrLayoutProfile();

        EnsureRows();

        InitializeComponent();

        _imageView = new ZoomableImageControl(settings)
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 40)
        };

        using (var source = Image.FromFile(imagePath))
        {
            _imageView.SetImage(new Bitmap(source));
        }

        _imageView.SetOmrLayout(_layout);

        pnlImageHost.Controls.Add(_imageView);

        InitializeRowSelector();

        RegisterEvents();

        UpdateStatus();
    }

    private void EnsureRows()
    {
        _layout.BubbleRows ??= [];

        if (!_layout.BubbleRows.Any(
            x => x.Name == "شماره داوطلبی"))
        {
            _layout.BubbleRows.Add(new OmrBubbleRowProfile
            {
                Name = "شماره داوطلبی"
            });
        }

        if (!_layout.BubbleRows.Any(
            x => x.Name == "کد رمز"))
        {
            _layout.BubbleRows.Add(new OmrBubbleRowProfile
            {
                Name = "کد رمز"
            });
        }
    }

    private void InitializeRowSelector()
    {
        _rowSelector.Items.Clear();

        foreach (var row in _layout.BubbleRows)
            _rowSelector.Items.Add(row.Name);

        if (_rowSelector.Items.Count > 0)
            _rowSelector.SelectedIndex = 0;
    }

    private void RegisterEvents()
    {
        _drawFirstButton.Click += (_, _) =>
            BeginDrawFirstBubble();

        _generateButton.Click += (_, _) =>
            GenerateBubbles();

        _removeButton.Click += (_, _) =>
            _imageView.RemoveSelectedOmrBubble();

        _rowSelector.SelectedIndexChanged += (_, _) =>
            UpdateStatus();

        _acceptButton.Click += (_, _) =>
        {
            _layout.Enabled = _layout.BubbleRows.Any(
                row => row.Bubbles.Count > 0);

            DialogResult = DialogResult.OK;
            Close();
        };

        _cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
    }

    private void BeginDrawFirstBubble()
    {
        if (_layout.MainRegion.Width <= 0 ||
            _layout.MainRegion.Height <= 0)
        {
            MessageBox.Show(
                "ابتدا محدوده اصلی پاسخنامه را مشخص کنید.",
                "محدوده نامعتبر",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var rowIndex = _layout.BubbleRows.IndexOf(SelectedRow);

        _imageView.BeginAddBubble(rowIndex);

        _statusLabel.Text =
            "با ماوس حباب اول را روی تصویر رسم کنید.";
    }


    private void GenerateBubbles()
    {
        var row = SelectedRow;

        if (row.Bubbles.Count == 0)
        {
            MessageBox.Show(
                "ابتدا حباب اول را رسم کنید.",
                "حباب اول مشخص نشده",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var first = row.Bubbles[0];

        if (first.Width <= 0 || first.Height <= 0)
        {
            MessageBox.Show(
                "ابعاد حباب اول معتبر نیست.",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var imageWidth = _imageView.ImageSize.Width;
        var mainWidth = _layout.MainRegion.Width;

        if (imageWidth <= 0 || mainWidth <= 0)
        {
            MessageBox.Show(
                "ابعاد تصویر یا محدوده اصلی معتبر نیست.",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        // فاصله خالی بین لبه حباب‌ها بر حسب پیکسل تصویر
        var spacingPixels = (double)_spacing.Value;

        // عرض حباب اول بر حسب پیکسل تصویر
        var bubbleWidthPixels = first.Width * mainWidth * imageWidth;

        // فاصله لبه تا لبه به مختصات نسبی محدوده اصلی
        var spacingRatio = spacingPixels / (mainWidth * imageWidth);

        // فاصله مرکز تا مرکز = عرض حباب + فاصله خالی
        var totalDistanceRatio =
            first.Width + spacingRatio;

        var directionSign =
            Convert.ToString(_direction.SelectedItem) == "چپ"
                ? -1.0
                : 1.0;

        var count = (int)_bubbleCount.Value;

        var generated =
            new System.Collections.Generic.List<OmrRectangle>
            {
            first
            };

        for (int i = 1; i < count; i++)
        {
            var x = first.X +
                    directionSign * totalDistanceRatio * i;

            if (x < 0 || x + first.Width > 1)
            {
                MessageBox.Show(
                    $"حباب شماره {i + 1} خارج از محدوده اصلی قرار می‌گیرد.",
                    "محدوده نامعتبر",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            generated.Add(new OmrRectangle
            {
                Name = $"{row.Name}-{i + 1}",
                X = x,
                Y = first.Y,
                Width = first.Width,
                Height = first.Height
            });
        }

        row.Bubbles.Clear();
        row.Bubbles.AddRange(generated);

        _layout.Enabled = true;

        _imageView.SetOmrLayout(_layout);

        UpdateStatus();

        MessageBox.Show(
            $"{count} حباب برای ردیف «{row.Name}» ساخته شد.",
            "ساخت حباب‌ها",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }


    private void UpdateStatus()
    {
        if (_rowSelector.SelectedItem == null)
            return;

        var row = SelectedRow;

        _statusLabel.Text =
            $"ردیف: {row.Name} | تعداد حباب‌ها: {row.Bubbles.Count}";
    }
}
