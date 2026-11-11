using OpenCvSharp;

using OpenCvSharp;

namespace OmrDesktop.Models;

public sealed class MarkerCandidate
{
    // محدوده مستطیلی مربع در تصویر
    public Rect Bounds { get; set; }

    // مرکز مربع
    public Point2f Center { get; set; }

    // مساحت کانتور
    public double Area { get; set; }

    // نسبت عرض به ارتفاع
    public double AspectRatio { get; set; }

    // میزان Solidity
    public double Solidity { get; set; }

    // میزان پُری مستطیل محاطی
    public double Extent { get; set; }

    // امتیاز تطبیق موقعیت
    public double PositionScore { get; set; }
}
