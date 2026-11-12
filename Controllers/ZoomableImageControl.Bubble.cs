using System.Drawing;
using System.Drawing.Drawing2D;
using OmrDesktop.Models;

namespace OmrDesktop.Controls;

public sealed partial class ZoomableImageControl
{
    private bool _isAddingOmrBubble;

    /// <summary>
    /// در صورت فعال بودن، اندازه حباب‌های انتخاب‌شده قابل تغییر نیست.
    /// </summary>
    public bool LockOmrBubbleResize { get; set; }

    private int _omrBubbleAddRowIndex = -1;
    private RectangleF _bubblePreviewRect;

    /// <summary>
    /// ابعاد تصویر بارگذاری‌شده.
    /// </summary>
    public Size ImageSize => _image?.Size ?? Size.Empty;

    /// <summary>
    /// شروع رسم اولین حباب برای ردیف مشخص‌شده.
    /// </summary>
    public void BeginAddBubble(int rowIndex)
    {
        if (_image == null ||
            _omrLayout == null ||
            !IsValidRectangle(_omrLayout.MainRegion))
            return;

        if (rowIndex < 0 ||
            rowIndex >= _omrLayout.BubbleRows.Count)
            return;

        _isAdding = false;
        _isAddingOmrBubble = true;
        _omrBubbleAddRowIndex = rowIndex;

        _operation = EditorOperation.None;
        _bubblePreviewRect = RectangleF.Empty;

        ClearOmrSelection();

        Cursor = Cursors.Cross;
        Focus();
        Invalidate();
    }

    /// <summary>
    /// لغو حالت رسم حباب.
    /// </summary>
    private void CancelAddOmrBubble()
    {
        _isAddingOmrBubble = false;
        _omrBubbleAddRowIndex = -1;
        _bubblePreviewRect = RectangleF.Empty;

        if (_operation == EditorOperation.Add)
            _operation = EditorOperation.None;

        _isDragging = false;
        Capture = false;

        Cursor = Cursors.Default;
        Invalidate();
    }

    /// <summary>
    /// حذف حباب انتخاب‌شده.
    /// </summary>
    public void RemoveSelectedOmrBubble()
    {
        if (_selectedOmrType != OmrElementType.Bubble ||
            _omrLayout == null)
            return;

        if (_selectedOmrRowIndex < 0 ||
            _selectedOmrRowIndex >= _omrLayout.BubbleRows.Count)
            return;

        var row = _omrLayout.BubbleRows[_selectedOmrRowIndex];

        if (_selectedOmrBubbleIndex < 0 ||
            _selectedOmrBubbleIndex >= row.Bubbles.Count)
            return;

        row.Bubbles.RemoveAt(_selectedOmrBubbleIndex);

        ClearOmrSelection();

        OmrLayoutChanged?.Invoke();
        Invalidate();
    }

    /// <summary>
    /// تکمیل رسم حباب روی تصویر.
    /// مختصات نهایی نسبت به MainRegion ذخیره می‌شوند.
    /// </summary>
    private void CompleteOmrBubbleAdding(Point mousePosition)
    {
        if (_image == null ||
            _omrLayout == null ||
            _omrBubbleAddRowIndex < 0 ||
            _omrBubbleAddRowIndex >= _omrLayout.BubbleRows.Count)
        {
            CancelAddOmrBubble();
            return;
        }

        var main = _omrLayout.MainRegion;

        if (!IsValidRectangle(main))
        {
            CancelAddOmrBubble();
            return;
        }

        var mainRect = GetOmrImageRectangle(
            main,
            OmrElementType.MainRegion);

        var start = _dragStartImage;
        var end = ClientToImage(mousePosition);

        var left = Math.Clamp(
            Math.Min(start.X, end.X),
            mainRect.Left,
            mainRect.Right);

        var top = Math.Clamp(
            Math.Min(start.Y, end.Y),
            mainRect.Top,
            mainRect.Bottom);

        var right = Math.Clamp(
            Math.Max(start.X, end.X),
            mainRect.Left,
            mainRect.Right);

        var bottom = Math.Clamp(
            Math.Max(start.Y, end.Y),
            mainRect.Top,
            mainRect.Bottom);

        var width = right - left;
        var height = bottom - top;

        var minimum = Math.Max(
            2,
            _settings.MinMarkerSizePixels);

        if (width < minimum || height < minimum)
        {
            CancelAddOmrBubble();
            return;
        }

        var row = _omrLayout.BubbleRows[_omrBubbleAddRowIndex];

        var bubble = new OmrRectangle
        {
            Name = $"{row.Name}-{row.Bubbles.Count + 1}",

            X = (left - mainRect.Left) / mainRect.Width,
            Y = (top - mainRect.Top) / mainRect.Height,

            Width = width / mainRect.Width,
            Height = height / mainRect.Height
        };

        row.Bubbles.Add(bubble);

        _selectedOmrType = OmrElementType.Bubble;
        _selectedOmrRowIndex = _omrBubbleAddRowIndex;
        _selectedOmrBubbleIndex = row.Bubbles.Count - 1;

        _isAddingOmrBubble = false;
        _omrBubbleAddRowIndex = -1;
        _bubblePreviewRect = RectangleF.Empty;

        Cursor = Cursors.Default;

        OmrLayoutChanged?.Invoke();

        Invalidate();
    }

    /// <summary>
    /// نمایش پیش‌نمایش حباب هنگام رسم.
    /// </summary>
    private void DrawOmrBubblePreview(
        Graphics graphics,
        RectangleF imageRect)
    {
        if (!_isAddingOmrBubble ||
            !_isDragging ||
            _operation != EditorOperation.Add ||
            _image == null ||
            _omrLayout == null ||
            !IsValidRectangle(_omrLayout.MainRegion))
            return;

        var mainRect = GetOmrImageRectangle(
            _omrLayout.MainRegion,
            OmrElementType.MainRegion);

        var current = ClientToImage(PointToClient(MousePosition));

        var left = Math.Clamp(
            Math.Min(_dragStartImage.X, current.X),
            mainRect.Left,
            mainRect.Right);

        var top = Math.Clamp(
            Math.Min(_dragStartImage.Y, current.Y),
            mainRect.Top,
            mainRect.Bottom);

        var right = Math.Clamp(
            Math.Max(_dragStartImage.X, current.X),
            mainRect.Left,
            mainRect.Right);

        var bottom = Math.Clamp(
            Math.Max(_dragStartImage.Y, current.Y),
            mainRect.Top,
            mainRect.Bottom);

        var imagePreview = new RectangleF(
            left,
            top,
            right - left,
            bottom - top);

        var clientPreview = new RectangleF(
            imageRect.X + imagePreview.X * imageRect.Width / _image.Width,
            imageRect.Y + imagePreview.Y * imageRect.Height / _image.Height,
            imagePreview.Width * imageRect.Width / _image.Width,
            imagePreview.Height * imageRect.Height / _image.Height);

        using var pen = new Pen(Color.Lime, 2)
        {
            DashStyle = DashStyle.Dash
        };

        using var brush = new SolidBrush(
            Color.FromArgb(45, Color.Lime));

        graphics.FillRectangle(brush, clientPreview);
        graphics.DrawRectangle(
            pen,
            clientPreview.X,
            clientPreview.Y,
            clientPreview.Width,
            clientPreview.Height);
    }
}