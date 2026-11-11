using System.Text.Json;
using System.Text.Json.Serialization;
using OmrDesktop.Models;


namespace OmrDesktop.Services;

public sealed class AlignmentProfileService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters =
{
new JsonStringEnumConverter()
}
    };

    public void Save(string filePath, AlignmentProfile profile)
    {
        Validate(profile);

        var json = JsonSerializer.Serialize(profile, _options);

        var fullPath = Path.GetFullPath(filePath);

        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, json);
    }

    public AlignmentProfile Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
            "فایل پروفایل پیدا نشد.");

        var json = File.ReadAllText(filePath);

        var profile =
        JsonSerializer.Deserialize<AlignmentProfile>(
        json,
        _options);

        if (profile == null)
            throw new InvalidOperationException(
            "محتوای پروفایل معتبر نیست.");

        // سازگاری با پروفایل‌های قدیمی
        profile.OmrLayout ??= new OmrLayoutProfile();

        Validate(profile);

        return profile;
    }

    private static void Validate(AlignmentProfile profile)
    {
        if (profile == null)
            throw new InvalidOperationException(
            "پروفایل معتبر نیست.");

        if (profile.CanvasWidth <= 0 ||
        profile.CanvasHeight <= 0)
            throw new InvalidOperationException(
            "ابعاد بوم پروفایل معتبر نیست.");

        if (profile.Markers == null ||
        profile.Markers.Count < 2)
            throw new InvalidOperationException(
            "حداقل دو نشانگر برای تراز تصویر لازم است.");

        if (profile.Markers.Select(x => x.Id).Distinct().Count()
        != profile.Markers.Count)
            throw new InvalidOperationException(
            "شناسه نشانگرها باید یکتا باشد.");

        foreach (var marker in profile.Markers)
        {
            if (string.IsNullOrWhiteSpace(marker.Name))
                throw new InvalidOperationException(
                "نام نشانگر نمی‌تواند خالی باشد.");

            if (marker.SearchX < 0 ||
            marker.SearchY < 0 ||
            marker.SearchWidth <= 0 ||
            marker.SearchHeight <= 0 ||
            marker.SearchX + marker.SearchWidth > 1 ||
            marker.SearchY + marker.SearchHeight > 1)
                throw new InvalidOperationException(
                $"محدوده جستجوی نشانگر {marker.Name} معتبر نیست.");

            if (marker.TargetXRatio < 0 ||
            marker.TargetXRatio > 1 ||
            marker.TargetYRatio < 0 ||
            marker.TargetYRatio > 1)
                throw new InvalidOperationException(
                $"مختصات مقصد نشانگر {marker.Name} معتبر نیست.");
        }

        ValidateOmrLayout(profile.OmrLayout);
    }

    private static void ValidateOmrLayout(OmrLayoutProfile layout)
    {
        if (layout == null)
            throw new InvalidOperationException(
            "تنظیمات OMR معتبر نیست.");

        if (!layout.Enabled)
            return;

        ValidateRectangle(layout.MainRegion, "محدوده اصلی");

        ValidateRectangle(layout.LeftTimingMark, "تایم مارک چپ");

        ValidateRectangle(layout.RightTimingMark, "تایم مارک راست");

        if (layout.BubbleRows == null)
            throw new InvalidOperationException(
            "فهرست ردیف‌های حباب معتبر نیست.");

        if (layout.BubbleRows.Select(x => x.Id).Distinct().Count()
        != layout.BubbleRows.Count)
            throw new InvalidOperationException(
            "شناسه ردیف‌های حباب باید یکتا باشد.");

        foreach (var row in layout.BubbleRows)
        {
            if (string.IsNullOrWhiteSpace(row.Name))
                throw new InvalidOperationException(
                "نام ردیف حباب نمی‌تواند خالی باشد.");

            if (row.Bubbles == null)
                throw new InvalidOperationException(
                $"فهرست حباب‌های ردیف {row.Name} معتبر نیست.");

            if (row.Bubbles.Select(x => x.Id).Distinct().Count()
            != row.Bubbles.Count)
                throw new InvalidOperationException(
                $"شناسه حباب‌های ردیف {row.Name} باید یکتا باشد.");

            foreach (var bubble in row.Bubbles)
            {
                ValidateRectangle(
                bubble,
                $"حباب ردیف {row.Name}");
            }
        }
    }

    private static void ValidateRectangle(
    OmrRectangle rectangle,
    string title)
    {
        if (rectangle == null)
            throw new InvalidOperationException(
            $"{title} تعریف نشده است.");

        if (string.IsNullOrWhiteSpace(rectangle.Name))
            throw new InvalidOperationException(
            $"نام {title} نمی‌تواند خالی باشد.");

        if (rectangle.X < 0 ||
        rectangle.Y < 0 ||
        rectangle.Width <= 0 ||
        rectangle.Height <= 0 ||
        rectangle.X + rectangle.Width > 1 ||
        rectangle.Y + rectangle.Height > 1)
            throw new InvalidOperationException(
            $"مختصات یا ابعاد {title} معتبر نیست.");
    }
}