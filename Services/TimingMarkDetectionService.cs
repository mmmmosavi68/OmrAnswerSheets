
using OpenCvSharp;
using OmrDesktop.Models;

using CvRect = OpenCvSharp.Rect;
using CvPoint = OpenCvSharp.Point;
using CvSize = OpenCvSharp.Size;

namespace OmrDesktop.Services;

public sealed class TimingMarkDetectionService
{
    private const int ExpectedLeftCount = 52;
    private const int ExpectedRightCount = 53;

    private const double DimensionTolerancePercent = 0.40;
    private const double HorizontalTolerancePercent = 0.50;

    private const double MinimumBlackFillRatio = 0.55;
    private const double MinimumContourFillRatio = 0.45;

    private static readonly int[] ThresholdValues =
        [70, 85, 100, 115, 130, 145];

    public TimingMarkDetectionResult Detect(
        Mat alignedImage,
        OmrLayoutProfile layout)
    {
        if (alignedImage == null || alignedImage.Empty())
            throw new ArgumentException("تصویر ورودی معتبر نیست.");

        if (layout == null || !layout.Enabled)
            throw new InvalidOperationException(
                "تنظیمات OMR فعال نیست.");

        var main = ToPixelRect(
            layout.MainRegion,
            alignedImage.Width,
            alignedImage.Height);

        ValidateRect(main, alignedImage.Size());

        var leftTemplate = ToPixelRect(
            layout.LeftTimingMark,
            main.Width,
            main.Height,
            main.X,
            main.Y);

        var rightTemplate = ToPixelRect(
            layout.RightTimingMark,
            main.Width,
            main.Height,
            main.X,
            main.Y);

        ValidateTemplate(leftTemplate, main, "چپ");
        ValidateTemplate(rightTemplate, main, "راست");

        var leftCandidates = DetectCandidates(
            alignedImage,
            main,
            leftTemplate);

        var rightCandidates = DetectCandidates(
            alignedImage,
            main,
            rightTemplate);

        var leftMarks = SelectSequence(
            leftCandidates,
            ExpectedLeftCount,
            initialIrregularCount: 2);

        var rightMarks = SelectSequence(
            rightCandidates,
            ExpectedRightCount,
            initialIrregularCount: 3);

        bool leftValid = leftMarks.Count == ExpectedLeftCount;
        bool rightValid = rightMarks.Count == ExpectedRightCount;

        var message =
            $"کاندیداهای چپ: {leftCandidates.Count}\n" +
            $"تایم‌مارک انتخاب‌شده چپ: {leftMarks.Count} از {ExpectedLeftCount}\n\n" +
            $"کاندیداهای راست: {rightCandidates.Count}\n" +
            $"تایم‌مارک انتخاب‌شده راست: {rightMarks.Count} از {ExpectedRightCount}";

        if (!leftValid || !rightValid)
        {
            message +=
                "\n\nتشخیص کامل تایم‌مارک‌ها انجام نشد.";
        }
        else
        {
            message +=
                "\n\nتشخیص تایم‌مارک‌ها موفق بود.";
        }

        return new TimingMarkDetectionResult
        {
            IsSuccess = leftValid && rightValid,
            LeftMarks = leftMarks,
            RightMarks = rightMarks,
            Message = message
        };
    }

    private static List<CvRect> DetectCandidates(
        Mat image,
        CvRect main,
        CvRect template)
    {
        var candidates = new List<CvRect>();

        int widthTolerance = Math.Max(
            3,
            (int)Math.Round(
                template.Width * DimensionTolerancePercent));

        int heightTolerance = Math.Max(
            3,
            (int)Math.Round(
                template.Height * DimensionTolerancePercent));

        int minWidth = Math.Max(1, template.Width - widthTolerance);
        int maxWidth = template.Width + widthTolerance;

        int minHeight = Math.Max(1, template.Height - heightTolerance);
        int maxHeight = template.Height + heightTolerance;

        int horizontalTolerance = Math.Max(
            3,
            (int)Math.Round(
                template.Width * HorizontalTolerancePercent));

        int searchX = Math.Max(
            main.X,
            template.X - horizontalTolerance);

        int searchRight = Math.Min(
            main.Right,
            template.Right + horizontalTolerance);

        int searchWidth = searchRight - searchX;

        if (searchWidth <= 0)
            return candidates;

        var searchRegion = new CvRect(
            searchX,
            main.Y,
            searchWidth,
            main.Height);

        using var roi = new Mat(image, searchRegion);
        using var gray = new Mat();

        if (roi.Channels() == 1)
        {
            roi.CopyTo(gray);
        }
        else if (roi.Channels() == 3)
        {
            Cv2.CvtColor(
                roi,
                gray,
                ColorConversionCodes.BGR2GRAY);
        }
        else if (roi.Channels() == 4)
        {
            Cv2.CvtColor(
                roi,
                gray,
                ColorConversionCodes.BGRA2GRAY);
        }
        else
        {
            throw new InvalidOperationException(
                "فرمت رنگی تصویر پشتیبانی نمی‌شود.");
        }

        double templateCenterX =
            template.X + template.Width / 2.0;

        foreach (int thresholdValue in ThresholdValues)
        {
            using var binary = new Mat();

            Cv2.Threshold(
                gray,
                binary,
                thresholdValue,
                255,
                ThresholdTypes.BinaryInv);

            Cv2.FindContours(
                binary,
                out CvPoint[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                var bounds = Cv2.BoundingRect(contour);

                if (bounds.Width < minWidth ||
                    bounds.Width > maxWidth ||
                    bounds.Height < minHeight ||
                    bounds.Height > maxHeight)
                {
                    continue;
                }

                int absoluteX = searchRegion.X + bounds.X;
                int absoluteY = searchRegion.Y + bounds.Y;

                double candidateCenterX =
                    absoluteX + bounds.Width / 2.0;

                if (Math.Abs(
                    candidateCenterX - templateCenterX)
                    > horizontalTolerance)
                {
                    continue;
                }

                using var candidateMask =
                    new Mat(binary, bounds);

                double blackFillRatio =
                    Cv2.CountNonZero(candidateMask) /
                    (double)(bounds.Width * bounds.Height);

                if (blackFillRatio < MinimumBlackFillRatio)
                    continue;

                double contourArea =
                    Cv2.ContourArea(contour);

                double contourFillRatio =
                    contourArea /
                    (bounds.Width * (double)bounds.Height);

                if (contourFillRatio < MinimumContourFillRatio)
                    continue;

                var candidate = new CvRect(
                    absoluteX,
                    absoluteY,
                    bounds.Width,
                    bounds.Height);

                AddCandidate(
                    candidates,
                    candidate,
                    template);
            }
        }

        return candidates
            .OrderBy(x => x.Y)
            .ToList();
    }

    private static void AddCandidate(
        List<CvRect> candidates,
        CvRect candidate,
        CvRect template)
    {
        double candidateCenterX =
            candidate.X + candidate.Width / 2.0;

        double candidateCenterY =
            candidate.Y + candidate.Height / 2.0;

        int duplicateX = Math.Max(
            2,
            (int)Math.Round(template.Width * 0.45));

        int duplicateY = Math.Max(
            2,
            (int)Math.Round(template.Height * 0.45));

        for (int i = 0; i < candidates.Count; i++)
        {
            var existing = candidates[i];

            double existingCenterX =
                existing.X + existing.Width / 2.0;

            double existingCenterY =
                existing.Y + existing.Height / 2.0;

            bool sameCandidate =
                Math.Abs(candidateCenterX - existingCenterX)
                    <= duplicateX &&
                Math.Abs(candidateCenterY - existingCenterY)
                    <= duplicateY;

            if (!sameCandidate)
                continue;

            double oldScore = GetTemplateDifference(
                existing,
                template);

            double newScore = GetTemplateDifference(
                candidate,
                template);

            if (newScore < oldScore)
                candidates[i] = candidate;

            return;
        }

        candidates.Add(candidate);
    }

    private static double GetTemplateDifference(
        CvRect candidate,
        CvRect template)
    {
        double widthDifference =
            Math.Abs(candidate.Width - template.Width) /
            (double)template.Width;

        double heightDifference =
            Math.Abs(candidate.Height - template.Height) /
            (double)template.Height;

        return widthDifference + heightDifference;
    }

    private static List<CvRect> SelectSequence(
        List<CvRect> candidates,
        int expectedCount,
        int initialIrregularCount)
    {
        var sorted = candidates
            .OrderBy(x => x.Y)
            .ToList();

        // اگر تعداد کاندیداها دقیقاً برابر تعداد مورد انتظار باشد،
        // نیازی به انتخاب مجدد توالی نیست.
        if (sorted.Count <= expectedCount)
            return sorted;

        int regularCount =
            expectedCount - initialIrregularCount;

        if (regularCount < 2)
            return [];

        List<CvRect>? bestSequence = null;
        double bestScore = double.MaxValue;

        // شروع بخش منظم را در بین کاندیداها بررسی می‌کنیم.
        for (int startIndex = initialIrregularCount;
             startIndex < sorted.Count;
             startIndex++)
        {
            var startMark = sorted[startIndex];

            // کاندیداهای قبل از شروع بخش منظم،
            // به عنوان بخش ابتدایی با فاصله متفاوت بررسی می‌شوند.
            var initialMarks = sorted
                .Take(startIndex)
                .TakeLast(initialIrregularCount)
                .ToList();

            if (initialMarks.Count != initialIrregularCount)
                continue;

            if (initialMarks.Any(x => x.Y >= startMark.Y))
                continue;

            // فاصله‌های ممکن از روی خود کاندیداها تخمین زده می‌شوند.
            var possiblePitches = new HashSet<double>();

            for (int i = startIndex + 1;
                 i < sorted.Count;
                 i++)
            {
                int distanceInRows = i - startIndex;

                // امکان وجود کاندیدای اضافی یا حذف‌شده
                // در مرحله تشخیص در نظر گرفته می‌شود.
                for (int skipped = 0; skipped <= 3; skipped++)
                {
                    int steps = distanceInRows + skipped;

                    if (steps <= 0)
                        continue;

                    double pitch =
                        (sorted[i].Y - startMark.Y) / steps;

                    if (pitch > 0)
                        possiblePitches.Add(pitch);
                }
            }

            foreach (double pitch in possiblePitches)
            {
                if (pitch < 2)
                    continue;

                double tolerance = Math.Max(
                    3,
                    pitch * 0.24);

                var sequence = new List<CvRect>();
                sequence.AddRange(initialMarks);

                sequence.Add(startMark);

                int lastUsedIndex = startIndex;
                double totalError = 0;
                bool valid = true;

                for (int step = 1;
                     step < regularCount;
                     step++)
                {
                    double expectedY =
                        startMark.Y + step * pitch;

                    int selectedIndex = -1;
                    double selectedError = double.MaxValue;

                    for (int j = lastUsedIndex + 1;
                         j < sorted.Count;
                         j++)
                    {
                        double error =
                            Math.Abs(sorted[j].Y - expectedY);

                        if (error > tolerance)
                        {
                            if (sorted[j].Y > expectedY)
                                break;

                            continue;
                        }

                        if (error < selectedError)
                        {
                            selectedError = error;
                            selectedIndex = j;
                        }
                    }

                    if (selectedIndex < 0)
                    {
                        valid = false;
                        break;
                    }

                    sequence.Add(sorted[selectedIndex]);
                    lastUsedIndex = selectedIndex;
                    totalError += selectedError / pitch;
                }

                if (!valid || sequence.Count != expectedCount)
                    continue;

                // امتیاز کمتر یعنی انطباق بهتر با الگوی فاصله.
                double score =
                    totalError / Math.Max(1, regularCount - 1);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestSequence = sequence;
                }
            }
        }

        return bestSequence ?? [];
    }

    private static CvRect ToPixelRect(
        OmrRectangle rectangle,
        int parentWidth,
        int parentHeight,
        int offsetX = 0,
        int offsetY = 0)
    {
        return new CvRect(
            offsetX + (int)Math.Round(
                rectangle.X * parentWidth),

            offsetY + (int)Math.Round(
                rectangle.Y * parentHeight),

            (int)Math.Round(
                rectangle.Width * parentWidth),

            (int)Math.Round(
                rectangle.Height * parentHeight));
    }

    private static void ValidateTemplate(
        CvRect template,
        CvRect main,
        string side)
    {
        if (template.Width <= 0 || template.Height <= 0)
        {
            throw new InvalidOperationException(
                $"مستطیل نمونه تایم‌مارک سمت {side} معتبر نیست.");
        }

        if (template.X < main.X ||
            template.Y < main.Y ||
            template.Right > main.Right ||
            template.Bottom > main.Bottom)
        {
            throw new InvalidOperationException(
                $"مستطیل نمونه تایم‌مارک سمت {side} " +
                "باید داخل MainRegion باشد.");
        }
    }

    private static void ValidateRect(
        CvRect rect,
        CvSize imageSize)
    {
        if (rect.Width <= 0 ||
            rect.Height <= 0 ||
            rect.X < 0 ||
            rect.Y < 0 ||
            rect.Right > imageSize.Width ||
            rect.Bottom > imageSize.Height)
        {
            throw new InvalidOperationException(
                "محدوده MainRegion خارج از محدوده تصویر است.");
        }
    }
}
