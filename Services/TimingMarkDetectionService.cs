
using OpenCvSharp;
using OmrDesktop.Models;

using CvRect = OpenCvSharp.Rect;
using CvPoint = OpenCvSharp.Point;
using CvSize = OpenCvSharp.Size;

namespace OmrDesktop.Services;

public sealed class TimingMarkDetectionService
{
    private const int ExpectedLeftCount = 52;
    private const int ExpectedRightCount = 53;

    // میزان اختلاف مجاز ابعاد تایم‌مارک با نمونه
    private const double DimensionTolerancePercent = 0.25;

    // حداقل میزان سیاهی داخل مستطیل تشخیص داده‌شده
    private const double MinimumBlackFillRatio = 0.70;

    // تلورانس افقی برای جابه‌جایی جزئی تایم‌مارک‌ها
    private const double HorizontalTolerancePercent = 0.25;

    public TimingMarkDetectionResult Detect(
        Mat alignedImage,
        OmrLayoutProfile layout)
    {
        if (alignedImage == null || alignedImage.Empty())
            throw new ArgumentException("تصویر ورودی معتبر نیست.");

        if (layout == null || !layout.Enabled)
            throw new InvalidOperationException(
                "تنظیمات OMR فعال نیست.");

        // محدوده اصلی صفحه
        var main = ToPixelRect(
            layout.MainRegion,
            alignedImage.Width,
            alignedImage.Height);

        ValidateRect(main, alignedImage.Size());

        // این دو مستطیل باید دقیقاً روی اولین تایم‌مارک رسم شده باشند.
        var leftTemplate = ToPixelRect(
            layout.LeftTimingMark,
            main.Width,
            main.Height,
            main.X,
            main.Y);

        var rightTemplate = ToPixelRect(
            layout.RightTimingMark,
            main.Width,
            main.Height,
            main.X,
            main.Y);

        ValidateTemplate(leftTemplate, main, "چپ");
        ValidateTemplate(rightTemplate, main, "راست");

        // اسکن مستقل دو سمت
        var leftMarks = DetectInColumn(
            alignedImage,
            main,
            leftTemplate);

        var rightMarks = DetectInColumn(
            alignedImage,
            main,
            rightTemplate);

        bool leftValid =
            leftMarks.Count == ExpectedLeftCount;

        bool rightValid =
            rightMarks.Count == ExpectedRightCount;

        var message =
            $"تایم‌مارک چپ: {leftMarks.Count} از {ExpectedLeftCount}\n" +
            $"تایم‌مارک راست: {rightMarks.Count} از {ExpectedRightCount}";

        if (!leftValid || !rightValid)
        {
            message +=
                "\nتعداد تایم‌مارک‌ها با مقدار مورد انتظار مطابقت ندارد.";
        }
        else
        {
            message += "\nتشخیص تایم‌مارک‌ها موفق بود.";
        }

        return new TimingMarkDetectionResult
        {
            IsSuccess = leftValid && rightValid,
            LeftMarks = leftMarks,
            RightMarks = rightMarks,
            Message = message
        };
    }

    private static List<CvRect> DetectInColumn(
        Mat image,
        CvRect main,
        CvRect template)
    {
        var result = new List<CvRect>();

        // تلورانس ابعاد بر اساس اندازه مستطیل نمونه
        int widthTolerance = Math.Max(
            3,
            (int)Math.Round(
                template.Width * DimensionTolerancePercent));

        int heightTolerance = Math.Max(
            3,
            (int)Math.Round(
                template.Height * DimensionTolerancePercent));

        int minWidth = Math.Max(
            1,
            template.Width - widthTolerance);

        int maxWidth = template.Width + widthTolerance;

        int minHeight = Math.Max(
            1,
            template.Height - heightTolerance);

        int maxHeight = template.Height + heightTolerance;

        // اجازه جابه‌جایی چند پیکسلی در محور افقی
        int horizontalTolerance = Math.Max(
            3,
            (int)Math.Round(
                template.Width * HorizontalTolerancePercent));

        // محدوده اسکن افقی اطراف نمونه
        int searchX = Math.Max(
            main.X,
            template.X - horizontalTolerance);

        int searchRight = Math.Min(
            main.Right,
            template.Right + horizontalTolerance);

        int searchWidth = searchRight - searchX;

        if (searchWidth <= 0)
            return result;

        // اسکن تمام ارتفاع MainRegion
        var searchRegion = new CvRect(
            searchX,
            main.Y,
            searchWidth,
            main.Height);

        using var roi = new Mat(image, searchRegion);
        using var gray = new Mat();
        using var binary = new Mat();

        // تبدیل تصویر به خاکستری
        if (roi.Channels() == 1)
        {
            roi.CopyTo(gray);
        }
        else if (roi.Channels() == 3)
        {
            Cv2.CvtColor(
                roi,
                gray,
                ColorConversionCodes.BGR2GRAY);
        }
        else if (roi.Channels() == 4)
        {
            Cv2.CvtColor(
                roi,
                gray,
                ColorConversionCodes.BGRA2GRAY);
        }
        else
        {
            throw new InvalidOperationException(
                "فرمت رنگی تصویر پشتیبانی نمی‌شود.");
        }

        // استخراج قسمت‌های مشکی
        Cv2.Threshold(
            gray,
            binary,
            100,
            255,
            ThresholdTypes.BinaryInv);

        Cv2.FindContours(
            binary,
            out CvPoint[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        double templateCenterX =
            template.X + template.Width / 2.0;

        foreach (var contour in contours)
        {
            var bounds = Cv2.BoundingRect(contour);

            // بررسی ابعاد
            if (bounds.Width < minWidth ||
                bounds.Width > maxWidth)
                continue;

            if (bounds.Height < minHeight ||
                bounds.Height > maxHeight)
                continue;

            // تبدیل مختصات محلی به مختصات تصویر
            int absoluteX = searchRegion.X + bounds.X;
            int absoluteY = searchRegion.Y + bounds.Y;

            double candidateCenterX =
                absoluteX + bounds.Width / 2.0;

            // بررسی موقعیت افقی نسبت به نمونه
            if (Math.Abs(
                candidateCenterX - templateCenterX)
                > horizontalTolerance)
                continue;

            // بررسی اینکه مستطیل واقعاً مشکی و پر باشد
            using var candidateMask =
                new Mat(binary, bounds);

            double blackFillRatio =
                Cv2.CountNonZero(candidateMask) /
                (double)(bounds.Width * bounds.Height);

            if (blackFillRatio < MinimumBlackFillRatio)
                continue;

            // بررسی میزان پر بودن کانتور
            double contourArea =
                Cv2.ContourArea(contour);

            double contourFillRatio =
                contourArea /
                (bounds.Width * (double)bounds.Height);

            if (contourFillRatio < MinimumBlackFillRatio)
                continue;

            result.Add(new CvRect(
                absoluteX,
                absoluteY,
                bounds.Width,
                bounds.Height));
        }

        // مرتب‌سازی از بالا به پایین
        return result
            .OrderBy(x => x.Y)
            .ToList();
    }

    private static CvRect ToPixelRect(
        OmrRectangle rectangle,
        int parentWidth,
        int parentHeight,
        int offsetX = 0,
        int offsetY = 0)
    {
        return new CvRect(
            offsetX + (int)Math.Round(
                rectangle.X * parentWidth),

            offsetY + (int)Math.Round(
                rectangle.Y * parentHeight),

            (int)Math.Round(
                rectangle.Width * parentWidth),

            (int)Math.Round(
                rectangle.Height * parentHeight));
    }

    private static void ValidateTemplate(
        CvRect template,
        CvRect main,
        string side)
    {
        if (template.Width <= 0 || template.Height <= 0)
        {
            throw new InvalidOperationException(
                $"مستطیل نمونه تایم‌مارک سمت {side} معتبر نیست.");
        }

        if (template.X < main.X ||
            template.Y < main.Y ||
            template.Right > main.Right ||
            template.Bottom > main.Bottom)
        {
            throw new InvalidOperationException(
                $"مستطیل نمونه تایم‌مارک سمت {side} " +
                "باید داخل MainRegion باشد.");
        }
    }

    private static void ValidateRect(
        CvRect rect,
        CvSize imageSize)
    {
        if (rect.Width <= 0 ||
            rect.Height <= 0 ||
            rect.X < 0 ||
            rect.Y < 0 ||
            rect.Right > imageSize.Width ||
            rect.Bottom > imageSize.Height)
        {
            throw new InvalidOperationException(
                "محدوده MainRegion خارج از محدوده تصویر است.");
        }
    }
}
