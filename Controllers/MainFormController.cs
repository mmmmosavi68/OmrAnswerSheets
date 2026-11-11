
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OmrDesktop.Configuration;
using OmrDesktop.Models;
using OmrDesktop.Services;

namespace OmrDesktop.Controllers;

public sealed class MainFormController
{
    private readonly AlignmentService _alignmentService;
    private readonly AlignmentSettings _settings;

    private readonly TimingMarkDetectionService _timingMarkService = new();
    private readonly BubbleDetectionService _bubbleDetectionService = new();

    public AlignmentProfile Profile { get; private set; }

    public event Action<Bitmap>? SourceImageReady;
    public event Action<Bitmap, string>? ImagesReady;
    public event Action<string>? StatusChanged;
    public event Action<IReadOnlyList<ImageFileItem>>? FilesLoaded;
    public event Action<TimingMarkDetectionResult>? TimingMarksDetected;
    public event Action<OmrBubbleDetectionResult>? BubblesDetected;
    public event Action<Bitmap>? OmrDebugImageReady;

    public MainFormController(AlignmentSettings settings)
    {
        _settings = settings;
        _alignmentService = new AlignmentService();
        Profile = AlignmentProfile.CreateDefault();
    }

    public void SetProfile(AlignmentProfile profile)
    {
        Profile = profile;
    }

    public void LoadFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            StatusChanged?.Invoke("پوشه انتخاب‌شده وجود ندارد.");
            return;
        }

        var extensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".tif",
            ".tiff"
        };

        var files = Directory
            .EnumerateFiles(folderPath)
            .Where(file => extensions.Contains(
                Path.GetExtension(file)))
            .OrderBy(file => file)
            .Select(file => new ImageFileItem
            {
                FullPath = file,
                FileName = Path.GetFileName(file)
            })
            .ToList();

        FilesLoaded?.Invoke(files);

        StatusChanged?.Invoke(
            $"تعداد {files.Count} تصویر پیدا شد.");
    }


    public void LoadImage(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            StatusChanged?.Invoke("فایل تصویر پیدا نشد.");
            return;
        }

        try
        {
            using (var loaded = new Bitmap(imagePath))
            {
                SourceImageReady?.Invoke(new Bitmap(loaded));
            }

            using var processing = ProcessSingleImage(imagePath);

            if (processing.Alignment == null)
            {
                StatusChanged?.Invoke(
                    "خطا در پردازش تصویر: " + processing.ErrorMessage);

                return;
            }

            var alignment = processing.Alignment;

            var alignedBitmap =
                BitmapConverter.ToBitmap(alignment.AlignedImage);

            var info =
                $"فایل: {Path.GetFileName(imagePath)}\n" +
                $"تعداد نشانگرها: {alignment.DetectedMarkers.Count}\n" +
                $"ضریب مقیاس: {alignment.Scale:F5}\n" +
                $"زاویه چرخش: {alignment.RotationAngle:F3}\n" +
                $"خطای RMS: {alignment.RmsError:F3} پیکسل\n" +
                $"اصلاح جهت 180 درجه: " +
                (alignment.WasRotated180 ? "بله" : "خیر");

            ImagesReady?.Invoke(alignedBitmap, info);

            if (processing.TimingMarks != null)
                TimingMarksDetected?.Invoke(processing.TimingMarks);

            if (processing.Bubbles != null)
            {
                var debugBitmap =
                    BitmapConverter.ToBitmap(alignment.AlignedImage);

                OmrDebugImageReady?.Invoke(debugBitmap);

                BubblesDetected?.Invoke(processing.Bubbles);
            }

            StatusChanged?.Invoke(
                processing.IsSuccess
                    ? "تراز تصویر و خواندن حباب‌ها با موفقیت انجام شد."
                    : "پردازش تصویر ناموفق بود: " +
                      processing.ErrorMessage);
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(
                "خطا در نمایش نتیجه تصویر: " + ex.Message);
        }
    }

    /// <summary>
    /// پردازش مستقل یک تصویر، بدون وابستگی به رابط کاربری
    /// </summary>
    public ImageProcessingResult ProcessSingleImage(string imagePath)
    {
        AlignmentResult? alignment = null;

        try
        {
            if (!File.Exists(imagePath))
            {
                return new ImageProcessingResult
                {
                    ErrorMessage = "فایل تصویر پیدا نشد."
                };
            }

            // مرحله اول: تراز تصویر
            alignment = _alignmentService.Align(
                imagePath,
                Profile,
                _settings);

            if (!Profile.OmrLayout.Enabled)
            {
                return new ImageProcessingResult
                {
                    Alignment = alignment,
                    ErrorMessage = "تنظیمات OMR فعال نیست."
                };
            }

            // مرحله دوم: تشخیص تایم‌مارک‌ها
            var timingResult = _timingMarkService.Detect(
                alignment.AlignedImage,
                Profile.OmrLayout);

            if (!timingResult.IsSuccess)
            {
                return new ImageProcessingResult
                {
                    Alignment = alignment,
                    TimingMarks = timingResult,
                    ErrorMessage = timingResult.Message
                };
            }

            // مرحله سوم: تشخیص و خواندن حباب‌ها
            var bubbleResult = _bubbleDetectionService.Detect(
                alignment.AlignedImage,
                Profile.OmrLayout,
                timingResult);

            // رسم مستطیل‌های تشخیص داده‌شده
            DrawOmrDebugRectangles(
                alignment.AlignedImage,
                Profile.OmrLayout,
                bubbleResult);

            return new ImageProcessingResult
            {
                Alignment = alignment,
                TimingMarks = timingResult,
                Bubbles = bubbleResult,
                ErrorMessage = bubbleResult.IsSuccess
                    ? string.Empty
                    : bubbleResult.Message
            };
        }
        catch (Exception ex)
        {
            return new ImageProcessingResult
            {
                Alignment = alignment,
                ErrorMessage = ex.Message
            };
        }
    }


    // رسم مستطیل‌های حباب‌ها و تایم‌مارک‌ها
    private static void DrawOmrDebugRectangles(
        Mat image,
        OmrLayoutProfile layout,
        OmrBubbleDetectionResult result)
    {
        var red = new Scalar(0, 0, 255);

        // تبدیل MainRegion به مختصات تصویر
        var main = ToPixelRect(
            layout.MainRegion,
            image.Width,
            image.Height);

        // تایم‌مارک مرجع چپ
        var leftTiming = ToPixelRect(
            layout.LeftTimingMark,
            main.Width,
            main.Height,
            main.X,
            main.Y);

        // تایم‌مارک مرجع راست
        var rightTiming = ToPixelRect(
            layout.RightTimingMark,
            main.Width,
            main.Height,
            main.X,
            main.Y);

        DrawRectangleWithLabel(
            image,
            leftTiming,
            "L",
            red);

        DrawRectangleWithLabel(
            image,
            rightTiming,
            "R",
            red);

        // حباب‌های ردیف اول: 1 تا 30
        for (int i = 0; i < result.FirstRowRectangles.Count; i++)
        {
            DrawRectangleWithLabel(
                image,
                result.FirstRowRectangles[i],
                (i + 1).ToString(),
                red);
        }

        // حباب‌های ردیف دوم: 31 تا 60
        for (int i = 0; i < result.SecondRowRectangles.Count; i++)
        {
            DrawRectangleWithLabel(
                image,
                result.SecondRowRectangles[i],
                (i + 31).ToString(),
                red);
        }
    }

    // رسم مستطیل و شماره آن
    private static void DrawRectangleWithLabel(
        Mat image,
        OpenCvSharp.Rect rect,
        string label,
        Scalar color)
    {
        Cv2.Rectangle(
            image,
            rect,
            color,
            2);

        Cv2.PutText(
            image,
            label,
            new OpenCvSharp.Point(
                rect.X,
                Math.Max(15, rect.Y - 4)),
            HersheyFonts.HersheySimplex,
            0.45,
            color,
            1);
    }

    // تبدیل مختصات نسبی به مختصات پیکسلی
    private static OpenCvSharp.Rect ToPixelRect(
        OmrRectangle rectangle,
        int parentWidth,
        int parentHeight,
        int offsetX = 0,
        int offsetY = 0)
    {
        return new OpenCvSharp.Rect(
            offsetX + (int)Math.Round(
                rectangle.X * parentWidth),

            offsetY + (int)Math.Round(
                rectangle.Y * parentHeight),

            (int)Math.Round(
                rectangle.Width * parentWidth),

            (int)Math.Round(
                rectangle.Height * parentHeight));
    }
}

// اطلاعات فایل تصویر
public sealed class ImageFileItem
{
    public string FullPath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public override string ToString()
    {
        return FileName;
    }
}
