
namespace OmrDesktop.Models;

public sealed class AlignmentMarker
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "Marker";

    // مختصات نرمال‌شده محدوده جستجو در تصویر ورودی
    public double SearchX { get; set; }
    public double SearchY { get; set; }
    public double SearchWidth { get; set; }
    public double SearchHeight { get; set; }

    // مختصات نسبی مرکز نشانگر در تصویر استاندارد
    public double TargetXRatio { get; set; }
    public double TargetYRatio { get; set; }
}
