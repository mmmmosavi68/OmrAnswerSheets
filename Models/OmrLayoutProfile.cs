
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
    /// محدوده تایم‌مارک سمت چپ.
    /// مختصات نسبت به MainRegion است.
    /// </summary>
    public OmrRectangle LeftTimingMark { get; set; } = new()
    {
        Name = "LeftTimingMark"
    };

    /// <summary>
    /// محدوده تایم‌مارک سمت راست.
    /// مختصات نسبت به MainRegion است.
    /// </summary>
    public OmrRectangle RightTimingMark { get; set; } = new()
    {
        Name = "RightTimingMark"
    };

    /// <summary>
    /// ردیف‌های حباب شماره داوطلبی و کد رمز.
    /// </summary>
    public List<OmrBubbleRowProfile> BubbleRows { get; set; } = [];

    /// <summary>
    /// شش ستون پاسخ سؤالات.
    /// در هر ستون چهار حباب مرجع برای گزینه‌ها ذخیره می‌شود.
    /// </summary>
    public List<OmrAnswerColumnProfile> AnswerColumns { get; set; } = [];
}

/// <summary>
/// تنظیمات یک ردیف از حباب‌های شماره داوطلبی یا رمز.
/// </summary>
public sealed class OmrBubbleRowProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "BubbleRow";

    public List<OmrRectangle> Bubbles { get; set; } = [];
}

/// <summary>
/// تنظیمات حباب‌های مرجع یک ستون پاسخ.
/// </summary>
public sealed class OmrAnswerColumnProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// شماره ستون از چپ، از ۱ تا ۶.
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// چهار مستطیل مرجع گزینه‌ها از چپ به راست.
    /// مختصات نسبت به MainRegion هستند.
    /// </summary>
    public List<OmrRectangle> Bubbles { get; set; } = [];
}
