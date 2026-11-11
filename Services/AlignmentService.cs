
using OpenCvSharp;
using OmrDesktop.Configuration;
using OmrDesktop.Models;

using CvPoint = OpenCvSharp.Point;
using CvPoint2f = OpenCvSharp.Point2f;
using CvSize = OpenCvSharp.Size;
using CvRect = OpenCvSharp.Rect;

namespace OmrDesktop.Services;

public sealed class AlignmentService
{
    public AlignmentResult Align(
        string imagePath,
        AlignmentProfile profile,
        AlignmentSettings settings)
    {
        using var source = Cv2.ImRead(
            imagePath,
            ImreadModes.Color);

        if (source.Empty())
            throw new InvalidOperationException(
                "تصویر انتخاب‌شده قابل بارگذاری نیست.");

        ValidateImage(source, profile, settings);

        var normal = TryDetect(
            source,
            profile,
            settings);

        using var rotated = new Mat();

        Cv2.Rotate(
            source,
            rotated,
            RotateFlags.Rotate180);

        var reverse = TryDetect(
            rotated,
            profile,
            settings);

        DetectionAttempt? selected = null;
        bool wasRotated = false;

        if (normal != null)
            selected = normal;

        if (reverse != null &&
            (selected == null ||
             reverse.TotalScore < selected.TotalScore))
        {
            selected = reverse;
            wasRotated = true;
        }

        if (selected == null)
            throw new InvalidOperationException(
                "نشانگرهای تراز شناسایی نشدند یا خطای هندسی زیاد است. " +
                "محدوده جستجو و تنظیمات پروفایل را بررسی کنید.");

        var working = wasRotated ? rotated : source;

        using var transform = CalculateTransform(
            selected.Detected,
            profile);


        // کنترل ضریب مقیاس تبدیل
        if (transform.Scale < settings.MinScale ||
            transform.Scale > settings.MaxScale)
        {
            return null;
        }

        var aligned = new Mat();

        Cv2.WarpAffine(
            working,
            aligned,
            transform.Matrix,
            new CvSize(
                profile.CanvasWidth,
                profile.CanvasHeight),
            InterpolationFlags.Linear,
            BorderTypes.Constant,
            new Scalar(
                settings.BorderColor,
                settings.BorderColor,
                settings.BorderColor));

        var debug = source.Clone();

        if (settings.EnableDebug)
        {
            DrawDebug(
                debug,
                selected.Detected,
                wasRotated,
                source.Size());
        }

        var detectedResults = selected.Detected
            .Select(item => new DetectedMarkerResult
            {
                MarkerId = item.Marker.Id,
                Name = item.Marker.Name,
                Bounds = wasRotated
                    ? ToOriginalRect(item.Bounds, source.Size())
                    : item.Bounds,
                Center = wasRotated
                    ? ToOriginalPoint(item.Center, source.Size())
                    : item.Center
            })
            .ToList();

        return new AlignmentResult
        {
            DebugImage = debug,
            AlignedImage = aligned,
            DetectedMarkers = detectedResults,
            WasRotated180 = wasRotated,
            Scale = transform.Scale,
            RotationAngle = transform.Angle,
            RmsError = selected.RmsError
        };
    }

    private static void ValidateImage(
        Mat image,
        AlignmentProfile profile,
        AlignmentSettings settings)
    {
        if (profile.Markers.Count < settings.MinMarkerCount)
            throw new InvalidOperationException(
                "تعداد نشانگرهای پروفایل کافی نیست.");

        var sourceRatio =
            (double)image.Width / image.Height;

        var targetRatio =
            (double)profile.CanvasWidth / profile.CanvasHeight;

        if (Math.Abs(sourceRatio - targetRatio) >
            settings.MaxPageAspectRatioDifference)
            throw new InvalidOperationException(
                "نسبت ابعاد تصویر ورودی با پروفایل مطابقت ندارد.");
    }

    private static DetectionAttempt? TryDetect(
        Mat image,
        AlignmentProfile profile,
        AlignmentSettings settings)
    {
        using var gray = new Mat();
        using var binary = new Mat();

        Cv2.CvtColor(
            image,
            gray,
            ColorConversionCodes.BGR2GRAY);

        Cv2.Threshold(
            gray,
            binary,
            settings.ThresholdValue,
            255,
            ThresholdTypes.BinaryInv);

        var detected = new List<DetectedMarker>();

        foreach (var marker in profile.Markers)
        {
            var result = FindMarker(
                binary,
                marker,
                settings);

            if (result == null)
                return null;

            detected.Add(result);
        }

        using var transform = CalculateTransform(
            detected,
            profile);

        var rms = CalculateRmsError(
            detected,
            profile,
            transform.Matrix);

        var diagonal = Math.Sqrt(
            (double)profile.CanvasWidth * profile.CanvasWidth +
            (double)profile.CanvasHeight * profile.CanvasHeight);

        var rmsRatio = rms / diagonal;

        if (rmsRatio > settings.MaxRmsErrorRatio)
            return null;

        var score =
            rmsRatio * 100 +
            detected.Average(x => x.PositionScore);

        return new DetectionAttempt
        {
            Detected = detected,
            RmsError = rms,
            TotalScore = score
        };
    }

    private static DetectedMarker? FindMarker(
        Mat binary,
        AlignmentMarker marker,
        AlignmentSettings settings)
    {
        var x = (int)Math.Round(
            marker.SearchX * binary.Width);

        var y = (int)Math.Round(
            marker.SearchY * binary.Height);

        var width = (int)Math.Round(
            marker.SearchWidth * binary.Width);

        var height = (int)Math.Round(
            marker.SearchHeight * binary.Height);

        var roiRect = new CvRect(
            x,
            y,
            width,
            height);

        if (roiRect.Width <= 0 ||
            roiRect.Height <= 0 ||
            roiRect.X < 0 ||
            roiRect.Y < 0 ||
            roiRect.Right > binary.Width ||
            roiRect.Bottom > binary.Height)
            return null;

        using var roi = new Mat(binary, roiRect);

        Cv2.FindContours(
            roi,
            out CvPoint[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        var imageArea =
            (double)binary.Width * binary.Height;

        var minArea =
            imageArea * settings.MinMarkerAreaRatio;

        var maxArea =
            imageArea * settings.MaxMarkerAreaRatio;

        DetectedMarker? best = null;

        foreach (var contour in contours)
        {
            var area = Cv2.ContourArea(contour);

            if (area < minArea || area > maxArea)
                continue;

            var bounds = Cv2.BoundingRect(contour);

            if (bounds.Width <= 0 || bounds.Height <= 0)
                continue;

            var aspectRatio =
                (double)bounds.Width / bounds.Height;

            if (aspectRatio < settings.MinAspectRatio ||
                aspectRatio > settings.MaxAspectRatio)
                continue;

            var extent =
                area / ((double)bounds.Width * bounds.Height);

            if (extent < settings.MinExtent)
                continue;

            CvPoint[] hull = Cv2.ConvexHull(contour);

            var hullArea = Cv2.ContourArea(hull);

            if (hullArea <= 0)
                continue;

            var solidity = area / hullArea;

            if (solidity < settings.MinSolidity)
                continue;

            var center = new CvPoint2f(
                roiRect.X + bounds.X + bounds.Width / 2f,
                roiRect.Y + bounds.Y + bounds.Height / 2f);

            var dx =
                (center.X - (roiRect.X + roiRect.Width / 2.0)) /
                (roiRect.Width / 2.0);

            var dy =
                (center.Y - (roiRect.Y + roiRect.Height / 2.0)) /
                (roiRect.Height / 2.0);

            var positionScore =
                Math.Sqrt(dx * dx + dy * dy);

            if (positionScore > settings.MaxPositionScore)
                continue;

            var candidate = new DetectedMarker
            {
                Marker = marker,
                Bounds = new CvRect(
                    roiRect.X + bounds.X,
                    roiRect.Y + bounds.Y,
                    bounds.Width,
                    bounds.Height),
                Center = center,
                PositionScore = positionScore
            };

            if (best == null ||
                candidate.PositionScore < best.PositionScore)
                best = candidate;
        }

        return best;
    }

    // محاسبه تبدیل Similarity با استفاده از تمام نقاط متناظر
    private static TransformData CalculateTransform(
        IReadOnlyList<DetectedMarker> detected,
        AlignmentProfile profile)
    {
        var count = detected.Count;

        if (count < 2)
            throw new InvalidOperationException(
                "حداقل دو نشانگر برای محاسبه تبدیل لازم است.");

        double sourceCenterX = 0;
        double sourceCenterY = 0;

        double targetCenterX = 0;
        double targetCenterY = 0;

        for (int i = 0; i < count; i++)
        {
            sourceCenterX += detected[i].Center.X;
            sourceCenterY += detected[i].Center.Y;

            targetCenterX +=
                detected[i].Marker.TargetXRatio *
                profile.CanvasWidth;

            targetCenterY +=
                detected[i].Marker.TargetYRatio *
                profile.CanvasHeight;
        }

        sourceCenterX /= count;
        sourceCenterY /= count;

        targetCenterX /= count;
        targetCenterY /= count;

        double a = 0;
        double b = 0;
        double denominator = 0;

        for (int i = 0; i < count; i++)
        {
            var sx = detected[i].Center.X - sourceCenterX;
            var sy = detected[i].Center.Y - sourceCenterY;

            var tx =
                detected[i].Marker.TargetXRatio *
                profile.CanvasWidth - targetCenterX;

            var ty =
                detected[i].Marker.TargetYRatio *
                profile.CanvasHeight - targetCenterY;

            a += sx * tx + sy * ty;
            b += sx * ty - sy * tx;

            denominator += sx * sx + sy * sy;
        }

        if (denominator <= 0)
            throw new InvalidOperationException(
                "نقاط نشانگرها برای محاسبه تبدیل مناسب نیستند.");

        var alpha = a / denominator;
        var beta = b / denominator;

        var scale = Math.Sqrt(
            alpha * alpha + beta * beta);

        var angle = Math.Atan2(beta, alpha);

        var txTranslation =
            targetCenterX -
            alpha * sourceCenterX +
            beta * sourceCenterY;

        var tyTranslation =
            targetCenterY -
            beta * sourceCenterX -
            alpha * sourceCenterY;

        var matrix = new Mat(
            2,
            3,
            MatType.CV_64FC1);

        matrix.Set(0, 0, alpha);
        matrix.Set(0, 1, -beta);
        matrix.Set(0, 2, txTranslation);

        matrix.Set(1, 0, beta);
        matrix.Set(1, 1, alpha);
        matrix.Set(1, 2, tyTranslation);

        return new TransformData
        {
            Matrix = matrix,
            Scale = scale,
            Angle = angle * 180 / Math.PI
        };
    }

    private static double CalculateRmsError(
        IReadOnlyList<DetectedMarker> detected,
        AlignmentProfile profile,
        Mat matrix)
    {
        double sum = 0;

        foreach (var item in detected)
        {
            var x = item.Center.X;
            var y = item.Center.Y;

            var mappedX =
                matrix.At<double>(0, 0) * x +
                matrix.At<double>(0, 1) * y +
                matrix.At<double>(0, 2);

            var mappedY =
                matrix.At<double>(1, 0) * x +
                matrix.At<double>(1, 1) * y +
                matrix.At<double>(1, 2);

            var targetX =
                item.Marker.TargetXRatio * profile.CanvasWidth;

            var targetY =
                item.Marker.TargetYRatio * profile.CanvasHeight;

            var dx = mappedX - targetX;
            var dy = mappedY - targetY;

            sum += dx * dx + dy * dy;
        }

        return Math.Sqrt(sum / detected.Count);
    }

    private static void DrawDebug(
        Mat debug,
        IReadOnlyList<DetectedMarker> detected,
        bool rotated,
        CvSize size)
    {
        foreach (var item in detected)
        {
            var rect = rotated
                ? ToOriginalRect(item.Bounds, size)
                : item.Bounds;

            var center = rotated
                ? ToOriginalPoint(item.Center, size)
                : item.Center;

            Cv2.Rectangle(
                debug,
                rect,
                Scalar.LimeGreen,
                3);

            Cv2.Circle(
                debug,
                new CvPoint(
                    (int)center.X,
                    (int)center.Y),
                6,
                Scalar.Yellow,
                -1);

            Cv2.PutText(
                debug,
                item.Marker.Name,
                new CvPoint(
                    rect.X,
                    Math.Max(20, rect.Y - 10)),
                HersheyFonts.HersheySimplex,
                0.6,
                Scalar.Red,
                2);
        }
    }

    private static CvRect ToOriginalRect(
        CvRect rect,
        CvSize size)
    {
        return new CvRect(
            size.Width - rect.X - rect.Width,
            size.Height - rect.Y - rect.Height,
            rect.Width,
            rect.Height);
    }

    private static CvPoint2f ToOriginalPoint(
        CvPoint2f point,
        CvSize size)
    {
        return new CvPoint2f(
            size.Width - 1 - point.X,
            size.Height - 1 - point.Y);
    }

    private sealed class DetectedMarker
    {
        public AlignmentMarker Marker { get; init; } = new();

        public CvRect Bounds { get; init; }

        public CvPoint2f Center { get; init; }

        public double PositionScore { get; init; }
    }

    private sealed class DetectionAttempt
    {
        public List<DetectedMarker> Detected { get; init; } = [];

        public double RmsError { get; init; }

        public double TotalScore { get; init; }
    }

    private sealed class TransformData : IDisposable
    {
        public Mat Matrix { get; init; } = new();

        public double Scale { get; init; }

        public double Angle { get; init; }

        public void Dispose()
        {
            Matrix.Dispose();
        }
    }
}
