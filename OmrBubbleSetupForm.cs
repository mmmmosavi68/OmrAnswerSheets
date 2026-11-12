
using DocumentFormat.OpenXml.Math;
using OmrDesktop.Configuration;
using OmrDesktop.Controls;
using OmrDesktop.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OmrDesktop.Forms;

public partial class OmrBubbleSetupForm : Form
{
    private const string AnswerRowName = "حباب‌های پاسخ";

    private readonly OmrLayoutProfile _layout;
    private readonly ZoomableImageControl _imageView;

    private bool _answerLayoutModified;

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

    private OmrBubbleRowProfile AnswerRow =>
        _layout.BubbleRows.First(
            x => x.Name == AnswerRowName);

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
        _layout.AnswerColumns ??= [];

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

        var answerRow = _layout.BubbleRows.FirstOrDefault(
            x => x.Name == AnswerRowName);

        if (answerRow == null)
        {
            answerRow = new OmrBubbleRowProfile
            {
                Name = AnswerRowName
            };

            _layout.BubbleRows.Add(answerRow);
        }

        answerRow.Bubbles ??= [];

        // مختصات ذخیره‌شده قبلی را برای نمایش و ویرایش
        // به یک ردیف موقت تبدیل می‌کنیم.
        if (answerRow.Bubbles.Count == 0)
        {
            foreach (var column in _layout.AnswerColumns
                         .OrderBy(x => x.ColumnNumber))
            {
                if (column.Bubbles == null)
                    continue;

                foreach (var bubble in column.Bubbles)
                {
                    answerRow.Bubbles.Add(CloneRectangle(bubble));
                }
            }
        }
    }

    private static OmrRectangle CloneRectangle(OmrRectangle source)
    {
        return new OmrRectangle
        {
            Id = Guid.NewGuid(),
            Name = source.Name,
            X = source.X,
            Y = source.Y,
            Width = source.Width,
            Height = source.Height
        };
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
            RemoveSelectedBubble();

        _rowSelector.SelectedIndexChanged += (_, _) =>
            UpdateStatus();

        _acceptButton.Click += (_, _) =>
            AcceptLayout();

        _cancelButton.Click += (_, _) =>
        {
            // ردیف پاسخ در این فرم موقت است.
            // در صورت انصراف نباید در BubbleRows باقی بماند.
            RemoveTemporaryAnswerRow();

            DialogResult = DialogResult.Cancel;
            Close();
        };

        // این خط را اضافه کن
        _imageView.OmrLayoutChanged += UpdateStatus;
    }

    private void BeginDrawFirstBubble()
    {
        var row = SelectedRow;

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

        if (row.Name == AnswerRowName &&
            row.Bubbles.Count > 0)
        {
            MessageBox.Show(
                "برای رسم مجدد حباب مرجع، ابتدا دکمه بازنشانی حباب‌ها را بزنید.",
                "حباب مرجع موجود است",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        var rowIndex = _layout.BubbleRows.IndexOf(row);

        _imageView.LockOmrBubbleResize = false;
        _imageView.BeginAddBubble(rowIndex);

        _statusLabel.Text =
            "با ماوس حباب مرجع را روی تصویر رسم کنید.";
    }

    private void GenerateBubbles()
    {
        var row = SelectedRow;

        if (row.Bubbles.Count == 0)
        {
            MessageBox.Show(
                "ابتدا حباب مرجع را رسم کنید.",
                "حباب مرجع مشخص نشده",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (row.Name == AnswerRowName)
            GenerateAnswerBubbles(row);
        else
            GenerateOrdinaryBubbles(row);
    }

    /// <summary>
    /// تولید حباب‌های پاسخ به صورت افقی:
    /// تعداد حباب در هر ستون × تعداد ستون‌ها.
    /// </summary>
    private void GenerateAnswerBubbles(OmrBubbleRowProfile row)
    {
        var totalCount = (int)_bubbleCount.Value;
        var bubblesPerColumn = (int)_bubblesPerColumn.Value;
        var columnCount = (int)_columnCount.Value;

        if (totalCount != bubblesPerColumn * columnCount)
        {
            MessageBox.Show(
                "تعداد کل حباب‌ها باید برابر حاصل‌ضرب تعداد حباب هر ستون در تعداد ستون‌ها باشد.",
                "تنظیمات نامعتبر",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var first = row.Bubbles[0];

        if (first.Width <= 0 || first.Height <= 0)
        {
            MessageBox.Show(
                "ابعاد حباب مرجع معتبر نیست.",
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

        // عرض حباب بر حسب پیکسل تصویر
        var mainWidthPixels = mainWidth * imageWidth;
        var bubbleWidthPixels = first.Width * mainWidthPixels;

        // فاصله خالی بین حباب‌های یک ستون
        var bubbleSpacing = (double)_spacing.Value;

        // فاصله خالی بین دو ستون
        var columnSpacing = (double)_columnSpacing.Value;

        var directionSign =
            Convert.ToString(_direction.SelectedItem) == "چپ"
                ? -1.0
                : 1.0;

        var generated = new System.Collections.Generic.List<OmrRectangle>();

        for (int i = 0; i < totalCount; i++)
        {
            var columnIndex = i / bubblesPerColumn;
            var bubbleIndexInColumn = i % bubblesPerColumn;

            // فاصله حباب‌ها درون هر ستون
            var withinColumnOffset =
                bubbleIndexInColumn *
                (bubbleWidthPixels + bubbleSpacing);

            // طول کامل هر ستون، شامل حباب‌ها و فاصله‌های داخلی
            var columnWidth =
                bubblesPerColumn * bubbleWidthPixels +
                (bubblesPerColumn - 1) * bubbleSpacing;

            // فاصله ستون‌های قبلی
            var previousColumnsOffset =
                columnIndex * (columnWidth + columnSpacing);

            var offsetPixels =
                withinColumnOffset + previousColumnsOffset;

            var x = first.X +
                    directionSign * offsetPixels / mainWidthPixels;

            if (x < 0 || x + first.Width > 1)
            {
                MessageBox.Show(
                    $"حباب شماره {i + 1} از محدوده اصلی پاسخنامه خارج می‌شود.\n\n" +
                    "فاصله‌ها، جهت تولید یا محل حباب مرجع را اصلاح کنید.",
                    "محدوده نامعتبر",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            generated.Add(new OmrRectangle
            {
                Name = $"Answer-Column-{columnIndex + 1}-Bubble-{bubbleIndexInColumn + 1}",
                X = x,
                Y = first.Y,
                Width = first.Width,
                Height = first.Height
            });
        }

        row.Bubbles.Clear();
        row.Bubbles.AddRange(generated);

        _answerLayoutModified = true;

        _imageView.SetOmrLayout(_layout);
        _imageView.LockOmrBubbleResize = true;

        UpdateStatus();

        MessageBox.Show(
            $"{totalCount} حباب با اندازه یکسان ساخته شد.\n\n" +
            "اکنون می‌توانید هر حباب را با ماوس جابه‌جا کنید.",
            "ساخت حباب‌ها",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// منطق قبلی تولید حباب برای شماره داوطلبی و کد رمز.
    /// </summary>
    private void GenerateOrdinaryBubbles(OmrBubbleRowProfile row)
    {
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

        var spacingPixels = (double)_spacing.Value;
        var bubbleWidthPixels = first.Width * mainWidth * imageWidth;
        var spacingRatio = spacingPixels / (mainWidth * imageWidth);

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
                CloneRectangle(first)
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

        _imageView.SetOmrLayout(_layout);

        UpdateStatus();

        MessageBox.Show(
            $"{count} حباب برای ردیف «{row.Name}» ساخته شد.",
            "ساخت حباب‌ها",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void RemoveSelectedBubble()
    {
        var row = SelectedRow;

        if (row.Name == AnswerRowName)
        {
            if (row.Bubbles.Count == 0)
                return;

            var result = MessageBox.Show(
                "تمام حباب‌های پاسخ حذف شوند؟\nبرای شروع مجدد باید حباب مرجع را دوباره رسم کنید.",
                "بازنشانی حباب‌ها",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            row.Bubbles.Clear();
            _answerLayoutModified = true;

            _imageView.LockOmrBubbleResize = false;
            _imageView.SetOmrLayout(_layout);

            UpdateStatus();
            return;
        }

        _imageView.RemoveSelectedOmrBubble();
        UpdateStatus();
    }

    private void AcceptLayout()
    {
        var answerRow = AnswerRow;

        if (answerRow.Bubbles.Count > 0)
        {
            var expectedCount =
                (int)_bubblesPerColumn.Value *
                (int)_columnCount.Value;

            if (answerRow.Bubbles.Count != expectedCount)
            {
                MessageBox.Show(
                    $"تعداد حباب‌های پاسخ {answerRow.Bubbles.Count} است، در حالی که طبق تنظیمات باید {expectedCount} حباب باشد.\n\n" +
                    "ابتدا حباب‌ها را دوباره تولید کنید.",
                    "تعداد حباب‌ها نامعتبر است",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _layout.AnswerColumns.Clear();

            var bubblesPerColumn = (int)_bubblesPerColumn.Value;
            var columnCount = (int)_columnCount.Value;

            for (int columnIndex = 0;
                 columnIndex < columnCount;
                 columnIndex++)
            {
                var column = new OmrAnswerColumnProfile
                {
                    ColumnNumber = columnIndex + 1
                };

                for (int bubbleIndex = 0;
                     bubbleIndex < bubblesPerColumn;
                     bubbleIndex++)
                {
                    var sourceIndex =
                        columnIndex * bubblesPerColumn + bubbleIndex;

                    column.Bubbles.Add(
                        CloneRectangle(answerRow.Bubbles[sourceIndex]));
                }

                _layout.AnswerColumns.Add(column);
            }
        }
        else if (_answerLayoutModified)
        {
            _layout.AnswerColumns.Clear();
        }

        RemoveTemporaryAnswerRow();

        _layout.Enabled =
            _layout.BubbleRows.Any(row => row.Bubbles.Count > 0) ||
            _layout.AnswerColumns.Any(column => column.Bubbles.Count > 0);

        DialogResult = DialogResult.OK;
        Close();
    }

    private void RemoveTemporaryAnswerRow()
    {
        var row = _layout.BubbleRows.FirstOrDefault(
            x => x.Name == AnswerRowName);

        if (row != null)
            _layout.BubbleRows.Remove(row);
    }

    private void UpdateStatus()
    {
        if (_rowSelector.SelectedItem == null)
            return;

        var row = SelectedRow;
        var isAnswerRow = row.Name == AnswerRowName;

        _imageView.LockOmrBubbleResize =
            isAnswerRow && row.Bubbles.Count >= 2;

        lblBubblesPerColumn.Visible = isAnswerRow;
        _bubblesPerColumn.Visible = isAnswerRow;

        lblColumnCount.Visible = isAnswerRow;
        _columnCount.Visible = isAnswerRow;

        lblColumnSpacing.Visible = isAnswerRow;
        _columnSpacing.Visible = isAnswerRow;

        _removeButton.Text = isAnswerRow
            ? "بازنشانی حباب‌ها"
            : "حذف حباب انتخاب‌شده";

        _statusLabel.Text =
            isAnswerRow
                ? $"حباب‌های پاسخ | تعداد فعلی: {row.Bubbles.Count} | اندازه حباب‌ها پس از تولید قفل می‌شود."
                : $"ردیف: {row.Name} | تعداد حباب‌ها: {row.Bubbles.Count}";
    }
}
