namespace OmrDesktop.Models;


/// <summary>
/// تنظیمات هندسی و چیدمان نواحی OMR پاسخنامه.
/// </summary>
public sealed class OmrLayoutProfile
{
    public bool Enabled { get; set; }

    /// <summary>
    /// محدوده اصلی OMR نسبت به کل تصویر.
    /// </summary>
    public OmrRectangle MainRegion { get; set; } = new()
    {
        Name = "MainRegion"
    };

    /// <summary>
    /// محدوده تایم مارک سمت چپ.
    /// مختصات نسبت به MainRegion است.
    /// </summary>
    public OmrRectangle LeftTimingMark { get; set; } = new()
    {
        Name = "LeftTimingMark"
    };

    /// <summary>
    /// محدوده تایم مارک سمت راست.
    /// مختصات نسبت به MainRegion است.
    /// </summary>
    public OmrRectangle RightTimingMark { get; set; } = new()
    {
        Name = "RightTimingMark"
    };

    /// <summary>
    /// ردیف‌های حباب.
    /// هر ردیف می‌تواند تعداد حباب متفاوتی داشته باشد.
    /// </summary>
    public List<OmrBubbleRowProfile> BubbleRows { get; set; } = [];
}

/// <summary>
/// تنظیمات یک ردیف از حباب‌ها.
/// </summary>
public sealed class OmrBubbleRowProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "BubbleRow";

    public List<OmrRectangle> Bubbles { get; set; } = [];
}