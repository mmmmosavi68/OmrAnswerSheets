
using OmrDesktop.Models;

namespace OmrDesktop.Models;

/// <summary>
/// نتیجه پردازش یک تصویر پاسخنامه
/// </summary>
public sealed class ImageProcessingResult : IDisposable
{
    public AlignmentResult? Alignment { get; init; }

    public TimingMarkDetectionResult? TimingMarks { get; init; }

    public OmrBubbleDetectionResult? Bubbles { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public bool IsSuccess =>
        Alignment != null &&
        TimingMarks?.IsSuccess == true &&
        Bubbles?.IsSuccess == true &&
        string.IsNullOrWhiteSpace(ErrorMessage);

    public void Dispose()
    {
        Alignment?.Dispose();
    }
}
