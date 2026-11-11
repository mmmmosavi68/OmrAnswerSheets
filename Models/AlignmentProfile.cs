
namespace OmrDesktop.Models;

public sealed class AlignmentProfile
{
    public string Name { get; set; } = "Default";

    public int CanvasWidth { get; set; } = 2480;

    public int CanvasHeight { get; set; } = 3508;

    public List<AlignmentMarker> Markers { get; set; } = [];

    public static AlignmentProfile CreateDefault()
    {
        return new AlignmentProfile
        {
            Name = "AnswerSheet-1405",

            CanvasWidth = 2480,
            CanvasHeight = 3508,

            Markers =
            [
                new AlignmentMarker
                {
                    Name = "TopLeft",

                    SearchX = 0.1615,
                    SearchY = 0.0805,
                    SearchWidth = 0.05,
                    SearchHeight = 0.05,

                    TargetXRatio = 0.185,
                    TargetYRatio = 0.105
                },

                new AlignmentMarker
                {
                    Name = "BottomRight",

                    SearchX = 0.887,
                    SearchY = 0.937,
                    SearchWidth = 0.05,
                    SearchHeight = 0.05,

                    TargetXRatio = 0.911,
                    TargetYRatio = 0.960
                }
            ]
        };
    }
}
