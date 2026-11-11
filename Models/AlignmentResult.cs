
using OpenCvSharp;

namespace OmrDesktop.Models;

public sealed class DetectedMarkerResult
{
    public Guid MarkerId { get; init; }

    public string Name { get; init; } = string.Empty;

    public Rect Bounds { get; init; }

    public Point2f Center { get; init; }
}

public sealed class AlignmentResult : IDisposable
{
    public Mat DebugImage { get; init; } = new();

    public Mat AlignedImage { get; init; } = new();

    public List<DetectedMarkerResult> DetectedMarkers { get; init; } = [];

    public bool WasRotated180 { get; init; }

    public double Scale { get; init; }

    public double RotationAngle { get; init; }

    public double RmsError { get; init; }

    public void Dispose()
    {
        DebugImage.Dispose();
        AlignedImage.Dispose();
    }
}
