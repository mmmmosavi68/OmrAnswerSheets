
using System.Drawing;
using OpenCvSharp.Extensions;
using OmrDesktop.Configuration;
using OmrDesktop.Models;
using OmrDesktop.Services;

namespace OmrDesktop.Controllers;

public sealed class MainFormController
{
    private readonly AlignmentService _alignmentService;
    private readonly AlignmentSettings _settings;

    public AlignmentProfile Profile { get; private set; }

    public event Action<Bitmap>? SourceImageReady;

    public event Action<Bitmap, string>? ImagesReady;

    public event Action<string>? StatusChanged;

    public event Action<IReadOnlyList<ImageFileItem>>? FilesLoaded;

    public MainFormController(
        AlignmentSettings settings)
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
            StatusChanged?.Invoke(
                "پوشه انتخاب‌شده وجود ندارد.");
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
            StatusChanged?.Invoke(
                "فایل تصویر پیدا نشد.");
            return;
        }

        try
        {
            using (var loaded = new Bitmap(imagePath))
            {
                SourceImageReady?.Invoke(
                    new Bitmap(loaded));
            }

            using var result = _alignmentService.Align(
                imagePath,
                Profile,
                _settings);

            var alignedBitmap =
                BitmapConverter.ToBitmap(
                    result.AlignedImage);

            var info =
                $"فایل: {Path.GetFileName(imagePath)}\n" +
                $"تعداد نشانگرها: {result.DetectedMarkers.Count}\n" +
                $"ضریب مقیاس: {result.Scale:F5}\n" +
                $"زاویه چرخش: {result.RotationAngle:F3}\n" +
                $"خطای RMS: {result.RmsError:F3} پیکسل\n" +
                $"اصلاح جهت 180 درجه: " +
                (result.WasRotated180 ? "بله" : "خیر");

            ImagesReady?.Invoke(
                alignedBitmap,
                info);

            StatusChanged?.Invoke(
                "تصویر با موفقیت تراز شد.");
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(
                "خطا در تراز تصویر: " + ex.Message);
        }
    }
}

public sealed class ImageFileItem
{
    public string FullPath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public override string ToString()
    {
        return FileName;
    }
}
