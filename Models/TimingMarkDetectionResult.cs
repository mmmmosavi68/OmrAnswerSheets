
namespace OmrDesktop.Models;

public sealed class TimingMarkDetectionResult
{
    public bool IsSuccess { get; init; }

    public List<OpenCvSharp.Rect> LeftMarks { get; init; } = [];

    public List<OpenCvSharp.Rect> RightMarks { get; init; } = [];

    public string Message { get; init; } = string.Empty;
}
