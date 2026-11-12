
using OpenCvSharp;
using OmrDesktop.Models;

using CvRect = OpenCvSharp.Rect;
using CvSize = OpenCvSharp.Size;

namespace OmrDesktop.Services;

public sealed class BubbleDetectionService
{
    private const double FilledThreshold = 0.35;
    private const double InnerMarginRatio = 0.15;

    public OmrBubbleDetectionResult Detect(Mat alignedImage, OmrLayoutProfile layout, TimingMarkDetectionResult timingResult)
    {
        if (alignedImage == null || alignedImage.Empty())
            throw new ArgumentException("تصویر ورودی معتبر نیست.");

        if (layout == null || !layout.Enabled)
            throw new InvalidOperationException(
                "تنظیمات OMR فعال نیست.");

        if (timingResult == null || !timingResult.IsSuccess)
            throw new InvalidOperationException(
                "تشخیص تایم‌مارکرها موفق نبود.");

        // برای دو ردیف حباب به حداقل دو مارکر چپ
        // و سه مارکر راست نیاز داریم.
        if (timingResult.LeftMarks == null ||
            timingResult.LeftMarks.Count < 2 ||
            timingResult.RightMarks == null ||
            timingResult.RightMarks.Count < 3)
        {
            throw new InvalidOperationException(
                "تعداد تایم‌مارکرها برای تعیین موقعیت دو ردیف حباب کافی نیست.");
        }

        if (layout.BubbleRows == null ||
            layout.BubbleRows.Count == 0)
        {
            throw new InvalidOperationException(
                "ردیف مرجع حباب‌ها در تنظیمات تعریف نشده است.");
        }

        var firstRow = layout.BubbleRows[0];

        if (firstRow.Bubbles == null ||
            firstRow.Bubbles.Count == 0)
        {
            throw new InvalidOperationException(
                "ردیف اول حبابی ندارد.");
        }

        if (firstRow.Bubbles.Count > 30)
        {
            throw new InvalidOperationException(
                "تعداد حباب‌ها نباید بیشتر از ۳۰ باشد.");
        }

        // تبدیل MainRegion به مختصات پیکسلی
        var main = ToPixelRect(
            layout.MainRegion,
            alignedImage.Width,
            alignedImage.Height);

        ValidateRect(main, alignedImage.Size());

        // تبدیل مختصات حباب‌های تعریف‌شده در پروفایل
        // در این مرحله فقط X، عرض و ارتفاع الگوی حباب‌ها
        // اهمیت دارد؛ موقعیت نهایی Y از تایم‌مارکرها می‌آید.
        var templateRects = firstRow.Bubbles
            .Select(bubble =>
                ToPixelRect(
                    bubble,
                    main.Width,
                    main.Height,
                    main.X,
                    main.Y))
            .ToList();

        foreach (var rect in templateRects)
            ValidateBubbleRect(rect, main);

        // -------------------------------------------------
        // تعیین خطوط افقی مرجع از روی تایم‌مارکرها
        // -------------------------------------------------

        // ردیف اول حباب:
        // تایم‌مارکر اول سمت چپ + تایم‌مارکر دوم سمت راست
        double firstRowCenterY = GetHorizontalReferenceY(
            timingResult.LeftMarks[0],
            timingResult.RightMarks[1]);

        // ردیف دوم حباب:
        // تایم‌مارکر دوم سمت چپ + تایم‌مارکر سوم سمت راست
        double secondRowCenterY = GetHorizontalReferenceY(
            timingResult.LeftMarks[1],
            timingResult.RightMarks[2]);

        // -------------------------------------------------
        // قرار دادن مرکز حباب‌ها روی خطوط افقی
        // -------------------------------------------------

        var firstRects = AlignBubbleRowToHorizontalLine(
            templateRects,
            firstRowCenterY);

        var secondRects = AlignBubbleRowToHorizontalLine(
            templateRects,
            secondRowCenterY);

        foreach (var rect in firstRects)
            ValidateBubbleRect(rect, main);

        foreach (var rect in secondRects)
            ValidateBubbleRect(rect, main);

        // -------------------------------------------------
        // خواندن حباب‌ها
        // -------------------------------------------------

        var firstResult = DetectRow(
            alignedImage,
            firstRow.Id,
            firstRow.Name,
            firstRects);

        var secondResult = DetectRow(
            alignedImage,
            Guid.NewGuid(),
            "کد رمز",
            secondRects);

        // خواندن ۳۰۰ سؤال از شش ستون
        var answerResults = DetectAnswers(
            alignedImage,
            layout,
            timingResult,
            main);

        bool success =
            firstResult.IsSuccess &&
            secondResult.IsSuccess &&
            answerResults.Count == 300;

        int expectedPassword = firstResult.Value + 500;

        string validationMessage =
            secondResult.Value == expectedPassword
                ? "کد رمز با شماره داوطلبی مطابقت دارد."
                : $"مقدار مورد انتظار کد رمز: {expectedPassword}";

        return new OmrBubbleDetectionResult
        {
            IsSuccess = success,

            Rows =
            [
                firstResult,
                secondResult
            ],

            FirstRowRectangles = firstRects,
            SecondRowRectangles = secondRects,
            AnswerQuestions = answerResults,

            Message =
                $"موقعیت ردیف‌ها با تایم‌مارکرها تعیین شد.\n" +
                $"خط افقی ردیف اول: Y={firstRowCenterY:F1}\n" +
                $"خط افقی ردیف دوم: Y={secondRowCenterY:F1}\n" +
                $"\nتعداد سؤال‌های خوانده‌شده: {answerResults.Count} از ۳۰۰" +
                validationMessage
        };
    }

    private static List<AnswerQuestionDetectionResult> DetectAnswers(
        Mat image,
        OmrLayoutProfile layout,
        TimingMarkDetectionResult timingResult,
        CvRect main)
    {
        const int expectedColumnCount = 6;
        const int questionsPerColumn = 50;
        const int optionsPerQuestion = 4;

        if (timingResult.LeftMarks.Count < 52)
        {
            throw new InvalidOperationException(
                "برای خواندن سؤالات، حداقل ۵۲ تایم‌مارک سمت چپ لازم است.");
        }

        if (layout.AnswerColumns == null ||
            layout.AnswerColumns.Count != expectedColumnCount)
        {
            throw new InvalidOperationException(
                "باید دقیقاً شش ستون پاسخ در پروفایل تعریف شده باشد.");
        }

        var columns = layout.AnswerColumns
            .OrderBy(x => x.ColumnNumber)
            .ToList();

        var results = new List<AnswerQuestionDetectionResult>(
            expectedColumnCount * questionsPerColumn);

        foreach (var column in columns)
        {
            if (column.Bubbles == null ||
                column.Bubbles.Count != optionsPerQuestion)
            {
                throw new InvalidOperationException(
                    $"ستون {column.ColumnNumber} باید دقیقاً چهار حباب مرجع داشته باشد.");
            }

            if (column.ColumnNumber < 1 ||
                column.ColumnNumber > expectedColumnCount)
            {
                throw new InvalidOperationException(
                    $"شماره ستون {column.ColumnNumber} معتبر نیست.");
            }

            // تبدیل چهار حباب مرجع به مختصات پیکسلی
            var templates = column.Bubbles
                .Select(bubble =>
                    ToPixelRect(
                        bubble,
                        main.Width,
                        main.Height,
                        main.X,
                        main.Y))
                .ToList();

            foreach (var template in templates)
                ValidateBubbleRect(template, main);

            // ۵۰ ردیف هر ستون
            for (int rowIndex = 0;
                 rowIndex < questionsPerColumn;
                 rowIndex++)
            {
                // دو تایم‌مارک اول برای شماره داوطلبی و رمز هستند.
                // تایم‌مارک سوم به بعد مربوط به سؤال‌هاست.
                var timingMark = timingResult.LeftMarks[rowIndex + 2];

                double centerY =
                    timingMark.Y + timingMark.Height / 2.0;

                var optionResults = new List<BubbleDetectionResult>();

                // چهار گزینه از چپ به راست
                for (int optionIndex = 0;
                     optionIndex < optionsPerQuestion;
                     optionIndex++)
                {
                    var template = templates[optionIndex];

                    int y = (int)Math.Round(
                        centerY - template.Height / 2.0,
                        MidpointRounding.AwayFromZero);

                    var rect = new CvRect(
                        template.X,
                        y,
                        template.Width,
                        template.Height);

                    ValidateBubbleRect(rect, main);

                    double blackRatio =
                        CalculateBlackRatio(image, rect);

                    bool isFilled =
                        blackRatio >= FilledThreshold;

                    optionResults.Add(new BubbleDetectionResult
                    {
                        Index = optionIndex + 1,
                        BubbleId = column.Bubbles[optionIndex].Id,
                        Name = $"گزینه {optionIndex + 1}",
                        BlackRatio = blackRatio,
                        IsFilled = isFilled,
                        Message = isFilled ? "پر" : "خالی"
                    });
                }

                // تعیین پاسخ بر اساس تعداد گزینه‌های انتخاب‌شده
                var selectedOptions = optionResults
                    .Where(x => x.IsFilled)
                    .ToList();

                string answer = selectedOptions.Count switch
                {
                    0 => "0",
                    1 => selectedOptions[0].Index.ToString(),
                    _ => "؟"
                };

                int questionNumber =
                    ((column.ColumnNumber - 1) * questionsPerColumn)
                    + rowIndex + 1;

                results.Add(new AnswerQuestionDetectionResult
                {
                    QuestionNumber = questionNumber,
                    ColumnNumber = column.ColumnNumber,
                    RowNumber = rowIndex + 1,
                    Options = optionResults,
                    Value = answer,
                    IsSuccess = true
                });
            }
        }

        return results
            .OrderBy(x => x.QuestionNumber)
            .ToList();
    }

    /// <summary>
    /// محاسبه ارتفاع مشترک یک خط افقی از روی مرکز عمودی
    /// دو تایم‌مارکر متناظر.
    /// </summary>
    private static double GetHorizontalReferenceY(CvRect leftMark, CvRect rightMark)
    {
        double leftCenterY =
            leftMark.Y + leftMark.Height / 2.0;

        double rightCenterY =
            rightMark.Y + rightMark.Height / 2.0;

        // میانگین دو مرکز، یک Y مشترک ایجاد می‌کند.
        // در نتیجه خط مرجع هیچ شیبی نخواهد داشت.
        return (leftCenterY + rightCenterY) / 2.0;
    }

    /// <summary>
    /// انتقال عمودی تمام حباب‌های یک ردیف به‌گونه‌ای که
    /// مرکز عمودی هر حباب دقیقاً روی خط افقی مرجع قرار بگیرد.
    /// مختصات افقی و ابعاد حباب‌ها بدون تغییر می‌مانند.
    /// </summary>
    private static List<CvRect> AlignBubbleRowToHorizontalLine(List<CvRect> templateRects, double centerY)
    {
        return templateRects
            .Select(rect =>
            {
                int newY = (int)Math.Round(
                    centerY - rect.Height / 2.0,
                    MidpointRounding.AwayFromZero);

                return new CvRect(
                    rect.X,
                    newY,
                    rect.Width,
                    rect.Height);
            })
            .ToList();
    }

    /// <summary>
    /// خواندن وضعیت حباب‌های یک ردیف.
    /// </summary>
    private static BubbleRowDetectionResult DetectRow(Mat image, Guid rowId, string rowName, List<CvRect> rectangles)
    {
        var results = new List<BubbleDetectionResult>();

        int value = 0;

        for (int i = 0; i < rectangles.Count; i++)
        {
            var rect = rectangles[i];

            double blackRatio =
                CalculateBlackRatio(image, rect);

            bool isFilled =
                blackRatio >= FilledThreshold;

            // حباب اول = 1
            // حباب دوم = 2
            // حباب سوم = 4
            // ...
            if (isFilled)
                value |= (1 << i);

            results.Add(new BubbleDetectionResult
            {
                Index = i + 1,
                Name = $"حباب {i + 1}",
                BlackRatio = blackRatio,
                IsFilled = isFilled,
                Message = isFilled ? "پر" : "خالی"
            });
        }

        return new BubbleRowDetectionResult
        {
            RowId = rowId,
            RowName = rowName,
            Bubbles = results,
            Value = value,
            IsSuccess = true,
            Message =
                $"تعداد حباب‌ها: {results.Count} - مقدار: {value}"
        };
    }

    /// <summary>
    /// محاسبه درصد سیاهی داخل حباب.
    /// </summary>
    private static double CalculateBlackRatio(Mat image, CvRect bubbleRect)
    {
        int marginX = (int)Math.Round(
            bubbleRect.Width * InnerMarginRatio);

        int marginY = (int)Math.Round(
            bubbleRect.Height * InnerMarginRatio);

        var innerRect = new CvRect(
            bubbleRect.X + marginX,
            bubbleRect.Y + marginY,
            bubbleRect.Width - 2 * marginX,
            bubbleRect.Height - 2 * marginY);

        if (innerRect.Width <= 0 ||
            innerRect.Height <= 0)
        {
            throw new InvalidOperationException(
                "ابعاد داخلی حباب معتبر نیست.");
        }

        using var roi = new Mat(image, innerRect);
        using var gray = new Mat();
        using var binary = new Mat();

        ConvertToGray(roi, gray);

        Cv2.Threshold(
            gray,
            binary,
            100,
            255,
            ThresholdTypes.BinaryInv);

        double blackPixels =
            Cv2.CountNonZero(binary);

        double totalPixels =
            innerRect.Width * (double)innerRect.Height;

        return blackPixels / totalPixels;
    }

    private static void ConvertToGray(Mat source, Mat destination)
    {
        if (source.Channels() == 1)
        {
            source.CopyTo(destination);
        }
        else if (source.Channels() == 3)
        {
            Cv2.CvtColor(
                source,
                destination,
                ColorConversionCodes.BGR2GRAY);
        }
        else if (source.Channels() == 4)
        {
            Cv2.CvtColor(
                source,
                destination,
                ColorConversionCodes.BGRA2GRAY);
        }
        else
        {
            throw new InvalidOperationException(
                "فرمت رنگی تصویر پشتیبانی نمی‌شود.");
        }
    }

    private static CvRect ToPixelRect(OmrRectangle rectangle, int parentWidth, int parentHeight, int offsetX = 0, int offsetY = 0)
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

    private static void ValidateBubbleRect(CvRect rect, CvRect main)
    {
        if (rect.Width <= 0 ||
            rect.Height <= 0 ||
            rect.X < main.X ||
            rect.Y < main.Y ||
            rect.Right > main.Right ||
            rect.Bottom > main.Bottom)
        {
            throw new InvalidOperationException(
                "محدوده یکی از حباب‌ها خارج از MainRegion است.");
        }
    }

    private static void ValidateRect(CvRect rect, CvSize imageSize)
    {
        if (rect.Width <= 0 ||
            rect.Height <= 0 ||
            rect.X < 0 ||
            rect.Y < 0 ||
            rect.Right > imageSize.Width ||
            rect.Bottom > imageSize.Height)
        {
            throw new InvalidOperationException(
                "محدوده MainRegion معتبر نیست.");
        }
    }
}
