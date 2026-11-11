
namespace OmrDesktop.Models;

/// <summary>
/// نتیجه تشخیص یک حباب
/// </summary>
public sealed class BubbleDetectionResult
{
    public int Index { get; init; }

    public Guid BubbleId { get; init; }

    public string Name { get; init; } = string.Empty;

    public double BlackRatio { get; init; }

    public bool IsFilled { get; init; }

    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// نتیجه خواندن یک ردیف حباب
/// </summary>
public sealed class BubbleRowDetectionResult
{
    public Guid RowId { get; init; }

    public string RowName { get; init; } = string.Empty;

    public List<BubbleDetectionResult> Bubbles { get; init; } = [];

    public int Value { get; init; }

    public bool IsSuccess { get; init; }

    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// نتیجه کلی خواندن حباب‌های پاسخنامه
/// </summary>
public sealed class OmrBubbleDetectionResult
{
    public bool IsSuccess { get; init; }

    public List<BubbleRowDetectionResult> Rows { get; init; } = [];

    public string Message { get; init; } = string.Empty;
    public List<OpenCvSharp.Rect> FirstRowRectangles { get; init; } = [];

    public List<OpenCvSharp.Rect> SecondRowRectangles { get; init; } = [];
}
