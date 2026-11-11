
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

            using var result = _alignmentService.Align(
                imagePath,
                Profile,
                _settings);

            var alignedBitmap =
                BitmapConverter.ToBitmap(result.AlignedImage);

            var info =
                $"فایل: {Path.GetFileName(imagePath)}\n" +
                $"تعداد نشانگرها: {result.DetectedMarkers.Count}\n" +
                $"ضریب مقیاس: {result.Scale:F5}\n" +
                $"زاویه چرخش: {result.RotationAngle:F3}\n" +
                $"خطای RMS: {result.RmsError:F3} پیکسل\n" +
                $"اصلاح جهت 180 درجه: " +
                (result.WasRotated180 ? "بله" : "خیر");

            ImagesReady?.Invoke(alignedBitmap, info);

            if (!Profile.OmrLayout.Enabled)
            {
                StatusChanged?.Invoke(
                    "تصویر با موفقیت تراز شد؛ تنظیمات OMR فعال نیست.");

                return;
            }

            try
            {
                // تشخیص تایم‌مارک‌ها
                var timingResult = _timingMarkService.Detect(
     result.AlignedImage,
     Profile.OmrLayout);

                TimingMarksDetected?.Invoke(timingResult);

                if (!timingResult.IsSuccess)
                {
                    StatusChanged?.Invoke(
                        "تصویر تراز شد، اما تشخیص تایم‌مارک‌ها ناموفق بود.");

                    return;
                }

                // تشخیص حباب‌ها
                var bubbleResult = _bubbleDetectionService.Detect(
    result.AlignedImage,
    Profile.OmrLayout,
    timingResult);

                // رسم مستطیل‌های حباب‌ها و تایم‌مارک‌ها
                DrawOmrDebugRectangles(
                    result.AlignedImage,
                    Profile.OmrLayout,
                    bubbleResult);

                // ارسال تصویر علامت‌گذاری‌شده به فرم اصلی
                var debugBitmap = BitmapConverter.ToBitmap(
                    result.AlignedImage);

                OmrDebugImageReady?.Invoke(debugBitmap);

                // ارسال نتیجه خواندن حباب‌ها
                BubblesDetected?.Invoke(bubbleResult);

                StatusChanged?.Invoke(
                    bubbleResult.IsSuccess
                        ? "تراز تصویر، تایم‌مارک‌ها و حباب‌ها با موفقیت خوانده شدند."
                        : "خطا در خواندن حباب‌ها.");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(
                    "تصویر تراز شد؛ خطا در تشخیص OMR: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(
                "خطا در پردازش تصویر: " + ex.Message);
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
