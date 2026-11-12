
using System.Drawing;
using System.Drawing.Drawing2D;
using OmrDesktop.Models;
using OmrDesktop.Configuration;

namespace OmrDesktop.Controls;

public sealed partial class ZoomableImageControl : Control
{
    private Image? _image;
    private bool _panMoved;
    private readonly AlignmentSettings _settings;

    private readonly List<AlignmentMarker> _markers = [];

    private float _zoom = 1;
    private float _offsetX;
    private float _offsetY;

    private Point _lastMousePosition;

    private PointF _dragStartImage;
    private RectangleF _initialRect;
    private bool _isPanning;
    private Point _panStartMouse;
    private float _panStartOffsetX;
    private float _panStartOffsetY;
    private bool _isDragging;
    private bool _isAdding;

    private int _activeHandle = -1;

    private Guid? _selectedMarkerId;

    private EditorOperation _operation;

    private enum EditorOperation
    {
        None,
        Move,
        Resize,
        Add
    }

    public event Action? MarkersChanged;

    public event Action<Guid?>? SelectionChanged;

    public IReadOnlyList<AlignmentMarker> Markers => _markers;

    public Guid? SelectedMarkerId => _selectedMarkerId;

    public bool AddMarkerMode
    {
        get => _isAdding;
        set
        {
            _isAdding = value;
            _operation = EditorOperation.None;
            Cursor = value
                ? Cursors.Cross
                : Cursors.Default;
        }
    }

    public ZoomableImageControl(AlignmentSettings settings)
    {
        _settings = settings;

        DoubleBuffered = true;
        BackColor = Color.FromArgb(40, 40, 40);

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);

        TabStop = true;
    }

    public void SetImage(Image image)
    {
        var oldImage = _image;

        _image = image;

        _zoom = 1;
        _offsetX = 0;
        _offsetY = 0;

        oldImage?.Dispose();

        Invalidate();
    }

    public void SetMarkers(IEnumerable<AlignmentMarker> markers)
    {
        _markers.Clear();

        _markers.AddRange(markers);

        _selectedMarkerId = null;

        MarkersChanged?.Invoke();
        SelectionChanged?.Invoke(null);

        Invalidate();
    }

    public void AddMarker()
    {
        if (_image == null)
            return;

        var marker = new AlignmentMarker
        {
            Name = $"Marker-{_markers.Count + 1}",

            SearchX = 0.45,
            SearchY = 0.45,
            SearchWidth = 0.05,
            SearchHeight = 0.05,

            TargetXRatio = 0.475,
            TargetYRatio = 0.475
        };

        _markers.Add(marker);

        _selectedMarkerId = marker.Id;

        AddMarkerMode = false;

        MarkersChanged?.Invoke();
        SelectionChanged?.Invoke(marker.Id);

        Invalidate();
        Focus();
    }

    public void RemoveSelectedMarker()
    {
        if (_selectedMarkerId == null)
            return;

        var marker = FindSelectedMarker();

        if (marker == null)
            return;

        _markers.Remove(marker);

        _selectedMarkerId = null;

        MarkersChanged?.Invoke();
        SelectionChanged?.Invoke(null);

        Invalidate();
    }

    public void SelectMarker(Guid? id)
    {
        if (id != null &&
            !_markers.Any(x => x.Id == id))
            return;

        _selectedMarkerId = id;

        SelectionChanged?.Invoke(id);

        Invalidate();
    }

    public void NotifyMarkerChanged()
    {
        MarkersChanged?.Invoke();
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData)
    {
        var key = keyData & Keys.KeyCode;

        if (key == Keys.Left ||
            key == Keys.Right ||
            key == Keys.Up ||
            key == Keys.Down ||
            key == Keys.Insert ||
            key == Keys.Delete ||
            key == Keys.Escape)
            return true;

        return base.IsInputKey(keyData);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(BackColor);

        if (_image == null)
            return;

        e.Graphics.InterpolationMode =
            InterpolationMode.HighQualityBicubic;

        var imageRect = GetImageRect();

        e.Graphics.DrawImage(
            _image,
            imageRect);

        foreach (var marker in _markers)
        {
            DrawMarker(e.Graphics, marker, imageRect);
        }

        DrawOmrLayout(e.Graphics, imageRect);

        DrawOmrBubblePreview(e.Graphics, imageRect);
    }

    private void DrawMarker(Graphics graphics, AlignmentMarker marker, RectangleF imageRect)
    {
        var rect = GetMarkerClientRect(marker, imageRect);

        var selected = marker.Id == _selectedMarkerId;

        using var pen = new Pen(
            selected ? Color.Lime : Color.Yellow,
            selected ? 2.5f : 1.5f);

        using var brush = new SolidBrush(
            Color.FromArgb(25, Color.Yellow));

        graphics.FillRectangle(brush, rect);
        graphics.DrawRectangle(
            pen,
            rect.X,
            rect.Y,
            rect.Width,
            rect.Height);

        using var font = new Font(
            "Tahoma",
            9,
            FontStyle.Bold);

        graphics.DrawString(
            marker.Name,
            font,
            Brushes.White,
            rect.X,
            rect.Y - font.Height - 2);

        if (!selected)
            return;

        var handleSize = (float)_settings.ResizeHandleSize;

        foreach (var handle in GetHandles(rect))
        {
            var handleRect = new RectangleF(
                handle.X - handleSize / 2,
                handle.Y - handleSize / 2,
                handleSize,
                handleSize);

            graphics.FillRectangle(
                Brushes.White,
                handleRect);

            graphics.DrawRectangle(
                Pens.Lime,
                handleRect.X,
                handleRect.Y,
                handleRect.Width,
                handleRect.Height);
        }
    }

    private static PointF[] GetHandles(RectangleF rect)
    {
        return
        [
            new PointF(rect.Left, rect.Top),
            new PointF(rect.Right, rect.Top),
            new PointF(rect.Left, rect.Bottom),
            new PointF(rect.Right, rect.Bottom),

            new PointF(rect.Left + rect.Width / 2, rect.Top),
            new PointF(rect.Left + rect.Width / 2, rect.Bottom),

            new PointF(rect.Left, rect.Top + rect.Height / 2),
            new PointF(rect.Right, rect.Top + rect.Height / 2)
        ];
    }

    private RectangleF GetImageRect()
    {
        if (_image == null)
            return RectangleF.Empty;

        var fitScale = Math.Min(
            (float)Width / _image.Width,
            (float)Height / _image.Height);

        var scale = fitScale * _zoom;

        var drawWidth = _image.Width * scale;
        var drawHeight = _image.Height * scale;

        return new RectangleF(
            (Width - drawWidth) / 2 + _offsetX,
            (Height - drawHeight) / 2 + _offsetY,
            drawWidth,
            drawHeight);
    }

    private RectangleF GetMarkerClientRect(AlignmentMarker marker, RectangleF imageRect)
    {
        return new RectangleF(
            imageRect.X + (float)(marker.SearchX * imageRect.Width),
            imageRect.Y + (float)(marker.SearchY * imageRect.Height),
            (float)(marker.SearchWidth * imageRect.Width),
            (float)(marker.SearchHeight * imageRect.Height));
    }

    private PointF ClientToImage(Point point)
    {
        var rect = GetImageRect();

        if (rect.Width <= 0 || rect.Height <= 0)
            return PointF.Empty;

        return new PointF(
            (point.X - rect.X) * _image!.Width / rect.Width,
            (point.Y - rect.Y) * _image.Height / rect.Height);
    }

    private PointF ImageToClient(PointF point)
    {
        var rect = GetImageRect();

        return new PointF(
            rect.X + point.X * rect.Width / _image!.Width,
            rect.Y + point.Y * rect.Height / _image.Height);
    }

    private RectangleF GetMarkerImageRect(AlignmentMarker marker)
    {
        return new RectangleF(
            (float)(marker.SearchX * _image!.Width),
            (float)(marker.SearchY * _image.Height),
            (float)(marker.SearchWidth * _image.Width),
            (float)(marker.SearchHeight * _image.Height));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        Focus();

        if (_image == null || e.Button != MouseButtons.Left)
            return;
        if (_isAddingOmrBubble)
        {
            _operation = EditorOperation.Add;
            _dragStartImage = ClientToImage(e.Location);
            _isDragging = true;
            Capture = true;
            Invalidate();
            return;
        }
        if (_isAdding)
        {
            _operation = EditorOperation.Add;
            _dragStartImage = ClientToImage(e.Location);
            _initialRect = RectangleF.Empty;
            _isDragging = true;
            Capture = true;
            return;
        }

        var imageRect = GetImageRect();

        // ابتدا عناصر OMR بررسی می‌شوند.
        if (TrySelectOmrElement(e.Location, imageRect))
        {
            _selectedMarkerId = null;
            SelectionChanged?.Invoke(null);

            var omrRectangle = GetSelectedOmrRectangle();

            if (omrRectangle == null || _selectedOmrType == null)
                return;

            var clientRect = GetImageRectForOmr(
                omrRectangle,
                imageRect);

            _activeHandle = FindHandle(clientRect, e.Location);

            // حباب‌های تولیدشده فقط قابل جابه‌جایی هستند.
            if (_selectedOmrType == OmrElementType.Bubble &&
                LockOmrBubbleResize)
            {
                _activeHandle = -1;
            }

            _operation = _activeHandle >= 0
                ? EditorOperation.Resize
                : EditorOperation.Move;

            _initialRect = GetOmrImageRectangle(
                omrRectangle,
                _selectedOmrType.Value);

            _dragStartImage = ClientToImage(e.Location);

            _isDragging = true;
            Capture = true;

            Invalidate();
            return;
        }

        // اگر عنصر OMR انتخاب نشد، انتخاب قبلی آن لغو می‌شود.
        ClearOmrSelection();

        var selected = FindMarkerAt(e.Location, imageRect);

        // اگر روی نشانگر کلیک نشده، امکان حرکت تصویر فعال می‌شود.
        if (selected == null)
        {
            if (imageRect.Contains(e.Location))
            {
                _isPanning = true;
                _panMoved = false;

                _panStartMouse = e.Location;
                _panStartOffsetX = _offsetX;
                _panStartOffsetY = _offsetY;

                Cursor = Cursors.Hand;
                Capture = true;
            }
            else
            {
                SelectMarker(null);
            }

            return;
        }

        _selectedMarkerId = selected.Id;

        SelectionChanged?.Invoke(selected.Id);

        var markerRect = GetMarkerClientRect(selected, imageRect);

        _activeHandle = FindHandle(markerRect, e.Location);

        _operation = _activeHandle >= 0
            ? EditorOperation.Resize
            : EditorOperation.Move;

        _initialRect = GetMarkerImageRect(selected);
        _dragStartImage = ClientToImage(e.Location);

        _isDragging = true;
        Capture = true;

        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isPanning)
        {
            var panDx = e.X - _panStartMouse.X;
            var panDy = e.Y - _panStartMouse.Y;

            if (Math.Abs(panDx) > 2 || Math.Abs(panDy) > 2)
                _panMoved = true;

            _offsetX = _panStartOffsetX + panDx;
            _offsetY = _panStartOffsetY + panDy;

            Invalidate();
            return;
        }
        if (!_isDragging)
        {
            UpdateCursor(e.Location);
            return;
        }

        if (_image == null)
            return;

        var current = ClientToImage(e.Location);

        var dx = current.X - _dragStartImage.X;
        var dy = current.Y - _dragStartImage.Y;

        switch (_operation)
        {
            case EditorOperation.Add:
                Invalidate();
                break;

            case EditorOperation.Move:
                if (_selectedOmrType != null)
                    MoveSelectedOmr(dx, dy);
                else
                    MoveSelected(dx, dy);
                break;

            case EditorOperation.Resize:
                if (_selectedOmrType != null)
                    ResizeSelectedOmr(dx, dy);
                else
                    ResizeSelected(dx, dy);
                break;
        }

        if (_operation != EditorOperation.Add)
            Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (_isPanning)
        {
            _isPanning = false;
            Capture = false;

            if (!_panMoved)
                SelectMarker(null);

            Cursor = Cursors.Default;
            return;
        }

        if (!_isDragging)
            return;

        if (_operation == EditorOperation.Add)
            CompleteAdding(e.Location);

        if (_operation == EditorOperation.Move ||
            _operation == EditorOperation.Resize)
        {
            if (_selectedOmrType != null)
                OmrLayoutChanged?.Invoke();
            else
                MarkersChanged?.Invoke();
        }

        _isDragging = false;
        _operation = EditorOperation.None;
        _activeHandle = -1;

        Capture = false;

        Invalidate();
    }
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Focus();
    }
    private void CompleteAdding(Point mousePosition)
    {
        if (_image == null)
            return;
        if (_isAddingOmrBubble)
        {
            CompleteOmrBubbleAdding(mousePosition);
            return;
        }
        var start = _dragStartImage;
        var end = ClientToImage(mousePosition);

        var left = Math.Clamp(
            Math.Min(start.X, end.X),
            0,
            _image.Width);

        var top = Math.Clamp(
            Math.Min(start.Y, end.Y),
            0,
            _image.Height);

        var right = Math.Clamp(
            Math.Max(start.X, end.X),
            0,
            _image.Width);

        var bottom = Math.Clamp(
            Math.Max(start.Y, end.Y),
            0,
            _image.Height);

        var width = right - left;
        var height = bottom - top;

        if (width < _settings.MinMarkerSizePixels ||
            height < _settings.MinMarkerSizePixels)
            return;

        var marker = new AlignmentMarker
        {
            Name = $"Marker-{_markers.Count + 1}",

            SearchX = left / _image.Width,
            SearchY = top / _image.Height,
            SearchWidth = width / _image.Width,
            SearchHeight = height / _image.Height,

            // مختصات مقصد اولیه از مرکز محدوده انتخاب‌شده گرفته می‌شود
            TargetXRatio = (left + width / 2) / _image.Width,
            TargetYRatio = (top + height / 2) / _image.Height
        };

        _markers.Add(marker);

        _selectedMarkerId = marker.Id;
        _isAdding = false;

        MarkersChanged?.Invoke();
        SelectionChanged?.Invoke(marker.Id);
    }

    private void MoveSelected(float dx, float dy)
    {
        var marker = FindSelectedMarker();

        if (marker == null || _image == null)
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

        SetMarkerRect(
            marker,
            left,
            top,
            rect.Width,
            rect.Height);
    }

    private void ResizeSelected(float dx, float dy)
    {
        var marker = FindSelectedMarker();

        if (marker == null || _image == null)
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

        left = Math.Clamp(
            left,
            0,
            right - _settings.MinMarkerSizePixels);

        top = Math.Clamp(
            top,
            0,
            bottom - _settings.MinMarkerSizePixels);

        right = Math.Clamp(
            right,
            left + _settings.MinMarkerSizePixels,
            _image.Width);

        bottom = Math.Clamp(
            bottom,
            top + _settings.MinMarkerSizePixels,
            _image.Height);

        SetMarkerRect(
            marker,
            left,
            top,
            right - left,
            bottom - top);
    }

    private void SetMarkerRect(AlignmentMarker marker, double x, double y, double width, double height)
    {
        marker.SearchX = x / _image!.Width;
        marker.SearchY = y / _image.Height;
        marker.SearchWidth = width / _image.Width;
        marker.SearchHeight = height / _image.Height;

        Invalidate();
    }

    private AlignmentMarker? FindMarkerAt(Point point, RectangleF imageRect)
    {
        for (int i = _markers.Count - 1; i >= 0; i--)
        {
            var marker = _markers[i];

            var rect = GetMarkerClientRect(
                marker,
                imageRect);

            if (rect.Contains(point))
                return marker;
        }

        return null;
    }

    private int FindHandle(RectangleF rect, Point point)
    {
        var handles = GetHandles(rect);

        var handleSize =
            _settings.ResizeHandleSize * 1.5f;

        for (int i = 0; i < handles.Length; i++)
        {
            var handleRect = new RectangleF(
                handles[i].X - handleSize / 2,
                handles[i].Y - handleSize / 2,
                handleSize,
                handleSize);

            if (handleRect.Contains(point))
                return i;
        }

        return -1;
    }


    private void UpdateCursor(Point point)
    {
        if (_isAddingOmrBubble)
        {
            Cursor = Cursors.Cross;
            return;
        }
        if (_isAdding)
        {
            Cursor = Cursors.Cross;
            return;
        }

        if (_selectedOmrType != null)
        {
            var rectangle = GetSelectedOmrRectangle();

            if (rectangle != null)
            {
                var rect = GetImageRectForOmr(
                    rectangle,
                    GetImageRect());

                var handle = FindHandle(rect, point);

                Cursor = handle switch
                {
                    0 or 3 => Cursors.SizeNWSE,
                    1 or 2 => Cursors.SizeNESW,
                    4 or 5 => Cursors.SizeNS,
                    6 or 7 => Cursors.SizeWE,
                    _ when rect.Contains(point) => Cursors.SizeAll,
                    _ => Cursors.Default
                };

                return;
            }
        }

        var marker = FindSelectedMarker();

        if (marker == null)
        {
            Cursor = Cursors.Default;
            return;
        }

        var markerRect = GetMarkerClientRect(
            marker,
            GetImageRect());

        var markerHandle = FindHandle(markerRect, point);

        Cursor = markerHandle switch
        {
            0 or 3 => Cursors.SizeNWSE,
            1 or 2 => Cursors.SizeNESW,
            4 or 5 => Cursors.SizeNS,
            6 or 7 => Cursors.SizeWE,
            _ when markerRect.Contains(point) => Cursors.SizeAll,
            _ => Cursors.Default
        };
    }


    private AlignmentMarker? FindSelectedMarker()
    {
        if (_selectedMarkerId == null)
            return null;

        return _markers.FirstOrDefault(
            x => x.Id == _selectedMarkerId);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (_image == null)
            return;

        var oldRect = GetImageRect();

        var imagePoint = ClientToImage(e.Location);

        _zoom *= e.Delta > 0 ? 1.15f : 0.87f;

        _zoom = Math.Clamp(_zoom, 0.5f, 10f);

        var newRect = GetImageRect();

        _offsetX +=
            (e.X - newRect.X -
             imagePoint.X * newRect.Width / _image.Width);

        _offsetY +=
            (e.Y - newRect.Y -
             imagePoint.Y * newRect.Height / _image.Height);

        Invalidate();
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Escape)
        {
            AddMarkerMode = false;
            CancelAddOmrBubble();
            _isDragging = false;
            Capture = false;
            Invalidate();
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Insert)
        {
            AddMarkerMode = true;
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Delete)
        {
            RemoveSelectedMarker();
            e.Handled = true;
            return;
        }

        if (_image == null)
            return;

        // ابتدا بررسی می‌کنیم آیا یک عنصر OMR انتخاب شده است.
        if (_selectedOmrType != null)
        {
            var step = e.Shift
                ? _settings.FastMoveStepPixels
                : 1;

            if (AdjustSelectedOmrByKeyboard(
                e.KeyCode,
                step,
                e.Control))
            {
                OmrLayoutChanged?.Invoke();

                e.Handled = true;
                e.SuppressKeyPress = true;

                Invalidate();
            }

            return;
        }

        // در غیر این صورت رفتار قبلی Alignment Marker اجرا می‌شود.
        var marker = FindSelectedMarker();

        if (marker == null)
            return;

        var markerStep = e.Shift
            ? _settings.FastMoveStepPixels
            : 1;

        var dx = 0.0;
        var dy = 0.0;

        switch (e.KeyCode)
        {
            case Keys.Left:
                dx = -markerStep;
                break;

            case Keys.Right:
                dx = markerStep;
                break;

            case Keys.Up:
                dy = -markerStep;
                break;

            case Keys.Down:
                dy = markerStep;
                break;

            default:
                return;
        }

        var x = marker.SearchX * _image.Width;
        var y = marker.SearchY * _image.Height;

        var width = marker.SearchWidth * _image.Width;
        var height = marker.SearchHeight * _image.Height;

        if (e.Control)
        {
            // تغییر اندازه با Ctrl + جهت‌نما
            if (e.KeyCode == Keys.Left ||
                e.KeyCode == Keys.Right)
                width += dx;

            if (e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.Down)
                height += dy;

            width = Math.Clamp(
                width,
                _settings.MinMarkerSizePixels,
                _image.Width - x);

            height = Math.Clamp(
                height,
                _settings.MinMarkerSizePixels,
                _image.Height - y);
        }
        else
        {
            // جابه‌جایی با جهت‌نما
            x = Math.Clamp(
                x + dx,
                0,
                _image.Width - width);

            y = Math.Clamp(
                y + dy,
                0,
                _image.Height - height);
        }

        SetMarkerRect(
            marker,
            x,
            y,
            width,
            height);

        MarkersChanged?.Invoke();

        Invalidate();

        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    public void ResetView()
    {
        _zoom = 1;
        _offsetX = 0;
        _offsetY = 0;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _image = null;
        }

        base.Dispose(disposing);
    }
}
