
using System.Drawing;
using System.Drawing.Drawing2D;
using OmrDesktop.Models;

namespace OmrDesktop.Controls;

public sealed partial class ZoomableImageControl
{
    private OmrLayoutProfile? _omrLayout;

    private enum OmrElementType
    {
        MainRegion,
        LeftTimingMark,
        RightTimingMark,
        Bubble
    }

    private OmrElementType? _selectedOmrType;
    private int _selectedOmrRowIndex = -1;
    private int _selectedOmrBubbleIndex = -1;

    public OmrLayoutProfile? OmrLayout => _omrLayout;

    public event Action? OmrLayoutChanged;

    public bool HasSelectedOmrElement =>
        _selectedOmrType != null;

    public void SetOmrLayout(OmrLayoutProfile? layout)
    {
        _omrLayout = layout;
        ClearOmrSelection();
        Invalidate();
    }

    private void ClearOmrSelection()
    {
        _selectedOmrType = null;
        _selectedOmrRowIndex = -1;
        _selectedOmrBubbleIndex = -1;
    }

    private OmrRectangle? GetSelectedOmrRectangle()
    {
        if (_omrLayout == null || _selectedOmrType == null)
            return null;

        return _selectedOmrType.Value switch
        {
            OmrElementType.MainRegion =>
                _omrLayout.MainRegion,

            OmrElementType.LeftTimingMark =>
                _omrLayout.LeftTimingMark,

            OmrElementType.RightTimingMark =>
                _omrLayout.RightTimingMark,

            OmrElementType.Bubble =>
                GetSelectedBubble(),

            _ => null
        };
    }

    private OmrRectangle? GetSelectedBubble()
    {
        if (_omrLayout?.BubbleRows == null)
            return null;

        if (_selectedOmrRowIndex < 0 ||
            _selectedOmrRowIndex >= _omrLayout.BubbleRows.Count)
            return null;

        var bubbles =
            _omrLayout.BubbleRows[_selectedOmrRowIndex].Bubbles;

        if (bubbles == null ||
            _selectedOmrBubbleIndex < 0 ||
            _selectedOmrBubbleIndex >= bubbles.Count)
            return null;

        return bubbles[_selectedOmrBubbleIndex];
    }

    private void DrawOmrLayout(Graphics graphics, RectangleF imageRect)
    {
        if (_omrLayout == null)
            return;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var main = _omrLayout.MainRegion;

        if (!IsValidRectangle(main))
            return;

        var mainRect = ToClientRectangle(main, imageRect);

        DrawOmrRectangle(
            graphics,
            mainRect,
            Color.DodgerBlue,
            "محدوده اصلی",
            _selectedOmrType == OmrElementType.MainRegion);

        DrawChild(
            graphics,
            _omrLayout.LeftTimingMark,
            mainRect,
            Color.Orange,
            "تایم‌مارک چپ",
            OmrElementType.LeftTimingMark);

        DrawChild(
            graphics,
            _omrLayout.RightTimingMark,
            mainRect,
            Color.OrangeRed,
            "تایم‌مارک راست",
            OmrElementType.RightTimingMark);

        if (_omrLayout.BubbleRows == null)
            return;

        for (int rowIndex = 0;
             rowIndex < _omrLayout.BubbleRows.Count;
             rowIndex++)
        {
            var row = _omrLayout.BubbleRows[rowIndex];

            if (row.Bubbles == null)
                continue;

            for (int bubbleIndex = 0;
                 bubbleIndex < row.Bubbles.Count;
                 bubbleIndex++)
            {
                var bubble = row.Bubbles[bubbleIndex];

                if (!IsValidRectangle(bubble))
                    continue;

                var rect = ToChildClientRectangle(
                    bubble,
                    mainRect);

                var selected =
                    _selectedOmrType == OmrElementType.Bubble &&
                    _selectedOmrRowIndex == rowIndex &&
                    _selectedOmrBubbleIndex == bubbleIndex;

                DrawOmrRectangle(
                    graphics,
                    rect,
                    Color.LimeGreen,
                    $"{row.Name}-{bubbleIndex + 1}",
                    selected);
            }
        }
    }

    private void DrawChild(Graphics graphics, OmrRectangle rectangle, RectangleF parentRect, Color color, string title, OmrElementType type)
    {
        if (!IsValidRectangle(rectangle))
            return;

        var rect = ToChildClientRectangle(
            rectangle,
            parentRect);

        DrawOmrRectangle(
            graphics,
            rect,
            color,
            title,
            _selectedOmrType == type);
    }

    private void DrawOmrRectangle(Graphics graphics, RectangleF rect, Color color, string title, bool selected)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        using var pen = new Pen(
            selected ? Color.Lime : color,
            selected ? 3f : 2f)
        {
            DashStyle = DashStyle.Dash
        };

        using var brush = new SolidBrush(
            Color.FromArgb(25, color));

        graphics.FillRectangle(brush, rect);

        graphics.DrawRectangle(
            pen,
            rect.X,
            rect.Y,
            rect.Width,
            rect.Height);

        using var font = new Font(
            "Tahoma",
            8,
            FontStyle.Bold);

        var labelSize = graphics.MeasureString(title, font);

        var labelRect = new RectangleF(
            rect.X,
            Math.Max(0, rect.Y - labelSize.Height - 2),
            labelSize.Width + 6,
            labelSize.Height + 2);

        using var labelBrush = new SolidBrush(color);

        graphics.FillRectangle(labelBrush, labelRect);

        graphics.DrawString(
            title,
            font,
            Brushes.Black,
            labelRect.X + 3,
            labelRect.Y + 1);

        if (!selected)
            return;

        const float handleSize = 9f;

        foreach (var handle in GetHandles(rect))
        {
            var handleRect = new RectangleF(
                handle.X - handleSize / 2,
                handle.Y - handleSize / 2,
                handleSize,
                handleSize);

            graphics.FillRectangle(Brushes.White, handleRect);

            graphics.DrawRectangle(
                Pens.Lime,
                handleRect.X,
                handleRect.Y,
                handleRect.Width,
                handleRect.Height);
        }
    }

    private bool TrySelectOmrElement(Point point, RectangleF imageRect)
    {
        if (_omrLayout == null ||
            !IsValidRectangle(_omrLayout.MainRegion))
            return false;

        var mainRect = ToClientRectangle(
            _omrLayout.MainRegion,
            imageRect);

        // حباب‌ها اولویت انتخاب بالاتری دارند.
        if (_omrLayout.BubbleRows != null)
        {
            for (int rowIndex = _omrLayout.BubbleRows.Count - 1;
                 rowIndex >= 0;
                 rowIndex--)
            {
                var row = _omrLayout.BubbleRows[rowIndex];

                if (row.Bubbles == null)
                    continue;

                for (int bubbleIndex = row.Bubbles.Count - 1;
                     bubbleIndex >= 0;
                     bubbleIndex--)
                {
                    var bubble = row.Bubbles[bubbleIndex];

                    if (!IsValidRectangle(bubble))
                        continue;

                    var rect = ToChildClientRectangle(
                        bubble,
                        mainRect);

                    if (!HitTestOmrRectangle(rect, point))
                        continue;

                    _selectedOmrType = OmrElementType.Bubble;
                    _selectedOmrRowIndex = rowIndex;
                    _selectedOmrBubbleIndex = bubbleIndex;

                    return true;
                }
            }
        }

        if (TrySelectChild(
            _omrLayout.RightTimingMark,
            mainRect,
            point,
            OmrElementType.RightTimingMark))
            return true;

        if (TrySelectChild(
            _omrLayout.LeftTimingMark,
            mainRect,
            point,
            OmrElementType.LeftTimingMark))
            return true;

        if (HitTestOmrRectangle(mainRect, point))
        {
            _selectedOmrType = OmrElementType.MainRegion;
            _selectedOmrRowIndex = -1;
            _selectedOmrBubbleIndex = -1;

            return true;
        }

        return false;
    }

    private bool TrySelectChild(OmrRectangle rectangle, RectangleF parentRect, Point point, OmrElementType type)
    {
        if (!IsValidRectangle(rectangle))
            return false;

        var rect = ToChildClientRectangle(
            rectangle,
            parentRect);

        if (!HitTestOmrRectangle(rect, point))
            return false;

        _selectedOmrType = type;
        _selectedOmrRowIndex = -1;
        _selectedOmrBubbleIndex = -1;

        return true;
    }

    private bool HitTestOmrRectangle(RectangleF rect, Point point)
    {
        if (FindHandle(rect, point) >= 0)
            return true;

        return rect.Contains(point);
    }

    private RectangleF GetOmrImageRectangle(OmrRectangle rectangle, OmrElementType type)
    {
        if (_image == null || _omrLayout == null)
            return RectangleF.Empty;

        if (type == OmrElementType.MainRegion)
        {
            return new RectangleF(
                (float)(rectangle.X * _image.Width),
                (float)(rectangle.Y * _image.Height),
                (float)(rectangle.Width * _image.Width),
                (float)(rectangle.Height * _image.Height));
        }

        var main = _omrLayout.MainRegion;

        var mainX = main.X * _image.Width;
        var mainY = main.Y * _image.Height;
        var mainWidth = main.Width * _image.Width;
        var mainHeight = main.Height * _image.Height;

        return new RectangleF(
            (float)(mainX + rectangle.X * mainWidth),
            (float)(mainY + rectangle.Y * mainHeight),
            (float)(rectangle.Width * mainWidth),
            (float)(rectangle.Height * mainHeight));
    }

    private void SetOmrImageRectangle(OmrRectangle rectangle, RectangleF imageRect, OmrElementType type)
    {
        if (_image == null || _omrLayout == null)
            return;

        if (type == OmrElementType.MainRegion)
        {
            rectangle.X = imageRect.X / _image.Width;
            rectangle.Y = imageRect.Y / _image.Height;
            rectangle.Width = imageRect.Width / _image.Width;
            rectangle.Height = imageRect.Height / _image.Height;

            return;
        }

        var main = _omrLayout.MainRegion;

        var mainX = main.X * _image.Width;
        var mainY = main.Y * _image.Height;
        var mainWidth = main.Width * _image.Width;
        var mainHeight = main.Height * _image.Height;

        if (mainWidth <= 0 || mainHeight <= 0)
            return;

        rectangle.X = (imageRect.X - mainX) / mainWidth;
        rectangle.Y = (imageRect.Y - mainY) / mainHeight;
        rectangle.Width = imageRect.Width / mainWidth;
        rectangle.Height = imageRect.Height / mainHeight;
    }

    private void MoveSelectedOmr(float dx, float dy)
    {
        var rectangle = GetSelectedOmrRectangle();

        if (rectangle == null ||
            _image == null ||
            _selectedOmrType == null)
            return;

        var rect = _initialRect;

        var left = Math.Clamp(
            rect.X + dx,
            0,
            _image.Width - rect.Width);

        var top = Math.Clamp(
            rect.Y + dy,
            0,
            _image.Height - rect.Height);

        if (_selectedOmrType != OmrElementType.MainRegion)
        {
            var main = GetOmrImageRectangle(
                _omrLayout!.MainRegion,
                OmrElementType.MainRegion);

            left = Math.Clamp(
                left,
                main.Left,
                main.Right - rect.Width);

            top = Math.Clamp(
                top,
                main.Top,
                main.Bottom - rect.Height);
        }

        SetOmrImageRectangle(
            rectangle,
            new RectangleF(
                left,
                top,
                rect.Width,
                rect.Height),
            _selectedOmrType.Value);
    }

    private void ResizeSelectedOmr(float dx, float dy)
    {
        var rectangle = GetSelectedOmrRectangle();

        if (rectangle == null ||
            _image == null ||
            _selectedOmrType == null)
            return;

        var rect = _initialRect;

        var left = rect.Left;
        var top = rect.Top;
        var right = rect.Right;
        var bottom = rect.Bottom;

        switch (_activeHandle)
        {
            case 0:
                left += dx;
                top += dy;
                break;

            case 1:
                right += dx;
                top += dy;
                break;

            case 2:
                left += dx;
                bottom += dy;
                break;

            case 3:
                right += dx;
                bottom += dy;
                break;

            case 4:
                top += dy;
                break;

            case 5:
                bottom += dy;
                break;

            case 6:
                left += dx;
                break;

            case 7:
                right += dx;
                break;
        }

        var minimum = Math.Max(
            2,
            _settings.MinMarkerSizePixels);

        RectangleF bounds;

        if (_selectedOmrType == OmrElementType.MainRegion)
        {
            bounds = new RectangleF(
                0,
                0,
                _image.Width,
                _image.Height);
        }
        else
        {
            bounds = GetOmrImageRectangle(
                _omrLayout!.MainRegion,
                OmrElementType.MainRegion);
        }

        left = Math.Clamp(
            left,
            bounds.Left,
            right - minimum);

        top = Math.Clamp(
            top,
            bounds.Top,
            bottom - minimum);

        right = Math.Clamp(
            right,
            left + minimum,
            bounds.Right);

        bottom = Math.Clamp(
            bottom,
            top + minimum,
            bounds.Bottom);

        SetOmrImageRectangle(
            rectangle,
            new RectangleF(
                left,
                top,
                right - left,
                bottom - top),
            _selectedOmrType.Value);
    }

    private RectangleF GetImageRectForOmr(OmrRectangle rectangle, RectangleF imageRect)
    {
        if (_selectedOmrType == OmrElementType.MainRegion)
            return ToClientRectangle(rectangle, imageRect);

        var mainRect = ToClientRectangle(
            _omrLayout!.MainRegion,
            imageRect);

        return ToChildClientRectangle(
            rectangle,
            mainRect);
    }

    private static RectangleF ToClientRectangle(OmrRectangle rectangle, RectangleF imageRect)
    {
        return new RectangleF(
            imageRect.X + (float)(rectangle.X * imageRect.Width),
            imageRect.Y + (float)(rectangle.Y * imageRect.Height),
            (float)(rectangle.Width * imageRect.Width),
            (float)(rectangle.Height * imageRect.Height));
    }

    private static RectangleF ToChildClientRectangle(OmrRectangle rectangle, RectangleF parentRect)
    {
        return new RectangleF(
            parentRect.X + (float)(rectangle.X * parentRect.Width),
            parentRect.Y + (float)(rectangle.Y * parentRect.Height),
            (float)(rectangle.Width * parentRect.Width),
            (float)(rectangle.Height * parentRect.Height));
    }

    private static bool IsValidRectangle(OmrRectangle? rectangle)
    {
        return rectangle != null &&
               rectangle.Width > 0 &&
               rectangle.Height > 0;
    }


    private bool AdjustSelectedOmrByKeyboard(Keys key, double step, bool resize)
    {
        var rectangle = GetSelectedOmrRectangle();

        if (rectangle == null ||
            _image == null ||
            _omrLayout == null ||
            _selectedOmrType == null)
            return false;

        float dx = 0;
        float dy = 0;

        switch (key)
        {
            case Keys.Left:
                dx = -(float)step;
                break;

            case Keys.Right:
                dx = (float)step;
                break;

            case Keys.Up:
                dy = -(float)step;
                break;

            case Keys.Down:
                dy = (float)step;
                break;

            default:
                return false;
        }

        var rect = GetOmrImageRectangle(
            rectangle,
            _selectedOmrType.Value);

        if (rect.Width <= 0 || rect.Height <= 0)
            return false;

        RectangleF bounds;

        if (_selectedOmrType == OmrElementType.MainRegion)
        {
            bounds = new RectangleF(
                0,
                0,
                _image.Width,
                _image.Height);
        }
        else
        {
            bounds = GetOmrImageRectangle(
                _omrLayout.MainRegion,
                OmrElementType.MainRegion);
        }

        if (resize)
        {
            float width = rect.Width;
            float height = rect.Height;

            if (key == Keys.Left || key == Keys.Right)
                width += dx;

            if (key == Keys.Up || key == Keys.Down)
                height += dy;

            var minimum = (float)Math.Max(
                2,
                _settings.MinMarkerSizePixels);

            width = Math.Clamp(
                width,
                minimum,
                Math.Max(minimum, bounds.Right - rect.X));

            height = Math.Clamp(
                height,
                minimum,
                Math.Max(minimum, bounds.Bottom - rect.Y));

            rect.Width = width;
            rect.Height = height;
        }
        else
        {
            rect.X = Math.Clamp(
                rect.X + dx,
                bounds.Left,
                Math.Max(bounds.Left, bounds.Right - rect.Width));

            rect.Y = Math.Clamp(
                rect.Y + dy,
                bounds.Top,
                Math.Max(bounds.Top, bounds.Bottom - rect.Height));
        }

        SetOmrImageRectangle(
            rectangle,
            rect,
            _selectedOmrType.Value);

        return true;
    }

}
