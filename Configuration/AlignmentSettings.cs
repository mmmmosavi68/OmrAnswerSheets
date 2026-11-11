
namespace OmrDesktop.Configuration;

public sealed class AlignmentSettings
{
    // ابعاد تصویر استاندارد پاسخنامه A4 با وضوح تقریبی 300 DPI
    public int CanvasWidth { get; set; } = 2480;
    public int CanvasHeight { get; set; } = 3508;

    // مختصات نسبی مرکز مربع بالا-چپ در تصویر استاندارد
    public double TopLeftTargetXRatio { get; set; } = 0.185;
    public double TopLeftTargetYRatio { get; set; } = 0.105;

    // مختصات نسبی مرکز مربع پایین-راست در تصویر استاندارد
    public double BottomRightTargetXRatio { get; set; } = 0.911;
    public double BottomRightTargetYRatio { get; set; } = 0.960;

    // آستانه تبدیل تصویر خاکستری به تصویر باینری
    // پیکسل‌های تیره‌تر از این مقدار مشکی در نظر گرفته می‌شوند
    public double ThresholdValue { get; set; } = 100;

    // حداقل و حداکثر مساحت کانتور مربع نسبت به کل تصویر
    // این مقادیر نسبت هستند، نه تعداد پیکسل
    public double MinMarkerAreaRatio { get; set; } = 0.00004;
    public double MaxMarkerAreaRatio { get; set; } = 0.00050;

    // محدوده مجاز نسبت عرض به ارتفاع مربع
    public double MinAspectRatio { get; set; } = 0.75;
    public double MaxAspectRatio { get; set; } = 1.25;

    // حداقل نسبت مساحت کانتور به مساحت محدوده محدب آن (Solidity)
    public double MinSolidity { get; set; } = 0.80;

    // حداقل پُری مستطیل محاطی کانتور
    public double MinExtent { get; set; } = 0.65;

    // محدوده جستجوی مربع‌ها اطراف مختصات نسبی مورد انتظار
    // برای مثال 0.10 یعنی 10 درصد عرض یا ارتفاع تصویر
    public double SearchHalfWidthRatio { get; set; } = 0.10;
    public double SearchHalfHeightRatio { get; set; } = 0.10;

    // حداکثر امتیاز مجاز برای پذیرش موقعیت یک مربع
    public double MaxPositionScore { get; set; } = 1.5;

    // محدوده ضریب مقیاس قابل قبول تبدیل تصویر
    public double MinScale { get; set; } = 0.5;
    public double MaxScale { get; set; } = 3.5;

    // حداکثر اختلاف نسبت ابعاد تصویر ورودی و بوم استاندارد
    // برای جلوگیری از پذیرش تصاویر شدیداً بریده‌شده
    public double MaxPageAspectRatioDifference { get; set; } = 0.08;

    // رنگ پس‌زمینه خارج از تصویر پس از WarpAffine
    public byte BorderColor { get; set; } = 255;

    // فعال بودن نمایش Debug
    public bool EnableDebug { get; set; } = true;


    // حداکثر خطای RMS نسبت به قطر بوم استاندارد
    public double MaxRmsErrorRatio { get; set; } = 0.003;

    // حداقل تعداد نشانگر برای محاسبه تبدیل
    public int MinMarkerCount { get; set; } = 2;

    // حداقل اندازه محدوده نشانگر در تصویر بر حسب پیکسل
    public int MinMarkerSizePixels { get; set; } = 8;

    // سرعت جابه‌جایی با Shift
    public int FastMoveStepPixels { get; set; } = 10;

    // اندازه دستگیره‌های تغییر ابعاد در رابط کاربری
    public int ResizeHandleSize { get; set; } = 8;

}
