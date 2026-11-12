
using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;

using OmrDesktop.Configuration;
using OmrDesktop.Controls;
using OmrDesktop.Controllers;
using OmrDesktop.Forms;
using OmrDesktop.Models;
using OmrDesktop.Services;

namespace OmrDesktop;
//test
public partial class MainForm2 : Form
{
    private readonly AlignmentSettings _settings;
    private readonly MainFormController _controller;
    private readonly AlignmentProfileService _profileService;

    private readonly ZoomableImageControl _originalView;
    private readonly ZoomableImageControl _alignedView;

    private string? _currentImagePath;
    private bool _refreshingGrid;

    private readonly string _defaultProfilePath =
        Path.Combine(AppContext.BaseDirectory, "AnswerSheet-1405.json");

    private List<BatchProcessingRecord> _lastBatchResults = [];

    public MainForm2()
    {
        InitializeComponent();

        lstFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        txtInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        gridMarkers.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Left;

        lblStatus.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Left;

        ConfigureBatchControls();
        ConfigureToolbarButtons();

        _settings = new AlignmentSettings();
        _controller = new MainFormController(_settings);
        _profileService = new AlignmentProfileService();

        _originalView = new ZoomableImageControl(_settings)
        {
            Dock = DockStyle.Fill
        };

        _alignedView = new ZoomableImageControl(_settings)
        {
            Dock = DockStyle.Fill
        };

        pnlOriginalHost.Controls.Add(_originalView);
        pnlAlignedHost.Controls.Add(_alignedView);

        _originalView.SetOmrLayout(_controller.Profile.OmrLayout);

        RegisterEvents();

        LoadDefaultProfile();
    }

    #region Initialization


    private void ConfigureToolbarButtons()
    {
        btnSelectFile.AutoSize = true;
        btnSelectFile.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnSelectFile.Dock = DockStyle.None;
        btnSelectFile.Anchor = AnchorStyles.None;
        btnSelectFile.Margin = new Padding(3, 5, 3, 5);
        var buttons = new Button[]
        {
        btnRunBatch,
        btnSelectFile,
        btnAlign,
        btnAddMarker,
        btnRemoveMarker,
        btnConfigureBubbles,
        btnSaveProfile,
        btnLoadProfile,
        btnSelectFolder
        };

        foreach (var button in buttons)
        {
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            button.Dock = DockStyle.None;
            button.Anchor = AnchorStyles.None;

            button.MinimumSize = new Size(0, 36);
            button.Padding = new Padding(8, 4, 8, 4);
            button.Margin = new Padding(3, 5, 3, 5);

            button.TextAlign = ContentAlignment.MiddleCenter;
        }

        // فعال کردن اسکرول افقی در صورت کمبود فضا
        toolbarLayout.AutoScroll = true;

        // ستون اول مخصوص نمایش وضعیت پردازش گروهی
        toolbarLayout.ColumnStyles.Clear();

        toolbarLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 201F));

        // عرض سایر ستون‌ها متناسب با دکمه داخل آن‌ها
        for (int i = 0; i < 9; i++)
        {
            toolbarLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.AutoSize));
        }

    }



    private void ConfigureBatchControls()
    {
        txtBatchFolderPath.ReadOnly = true;
        txtBatchFolderPath.RightToLeft = RightToLeft.No;
        txtBatchFolderPath.TextAlign = HorizontalAlignment.Left;
        txtBatchFolderPath.Text = "ابتدا پوشه تصاویر را انتخاب کنید";
        btnRunBatch.Enabled = false;
        progressBatch.Minimum = 0;
        progressBatch.Value = 0;
        lblBatchProgress.Text = "آماده";
        lblBatchProgress.RightToLeft = RightToLeft.Yes;
    }

    private void LoadDefaultProfile()
    {
        try
        {
            if (!File.Exists(_defaultProfilePath))
            {
                lblStatus.Text =
                    "فایل پروفایل پیش‌فرض پیدا نشد؛ از تنظیمات اولیه استفاده می‌شود.";

                _originalView.SetMarkers(_controller.Profile.Markers);
                RefreshMarkerGrid();

                return;
            }

            var profile = _profileService.Load(_defaultProfilePath);

            _controller.SetProfile(profile);
            _originalView.SetMarkers(profile.Markers);
            _originalView.SetOmrLayout(profile.OmrLayout);

            RefreshMarkerGrid();

            lblStatus.Text = "پروفایل پیش‌فرض با موفقیت بارگذاری شد.";
        }
        catch (Exception ex)
        {
            _originalView.SetMarkers(_controller.Profile.Markers);
            RefreshMarkerGrid();

            MessageBox.Show(
                ex.Message,
                "خطای بارگذاری پروفایل پیش‌فرض",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Events

    private void RegisterEvents()
    {
        // شروع پردازش گروهی
        btnRunBatch.Click += async (_, _) =>
        {
            await RunBatchProcessingAsync();
        };

        // انتخاب پوشه
        btnSelectFolder.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "پوشه حاوی تصاویر پاسخنامه را انتخاب کنید."
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            txtBatchFolderPath.Text = dialog.SelectedPath;

            _controller.LoadFolder(dialog.SelectedPath);

            btnRunBatch.Enabled = true;
        };

        // انتخاب فایل تکی
        btnSelectFile.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.tif;*.tiff"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                LoadSelectedImage(dialog.FileName);
        };

        lstFiles.SelectedIndexChanged += (_, _) =>
        {
            if (lstFiles.SelectedItem is ImageFileItem item)
                LoadSelectedImage(item.FullPath);
        };

        btnAddMarker.Click += (_, _) =>
        {
            _originalView.AddMarkerMode = true;

            lblStatus.Text =
                "برای افزودن نشانگر، روی تصویر مستطیل بکشید.";

            _originalView.Focus();
        };

        btnRemoveMarker.Click += (_, _) =>
        {
            _originalView.RemoveSelectedMarker();
            RefreshMarkerGrid();
        };

        btnAlign.Click += (_, _) =>
        {
            if (_currentImagePath == null)
                return;

            UpdateProfileFromEditor();
            _controller.LoadImage(_currentImagePath);
        };

        btnSaveProfile.Click += (_, _) => SaveProfile();

        btnLoadProfile.Click += (_, _) => LoadProfile();

        // تصاویر
        _controller.SourceImageReady += bitmap =>
        {
            _originalView.SetImage(bitmap);
        };

        _controller.ImagesReady += (bitmap, info) =>
        {
            _alignedView.SetImage(bitmap);
            txtInfo.Text = info;
        };

        _controller.OmrDebugImageReady += bitmap =>
        {
            _alignedView.SetImage(bitmap);
        };

        _controller.StatusChanged += message =>
        {
            lblStatus.Text = message;
        };

        _controller.FilesLoaded += files =>
        {
            lstFiles.Items.Clear();

            foreach (var file in files)
                lstFiles.Items.Add(file);
        };

        // نتیجه خواندن حباب‌ها
        _controller.BubblesDetected += result =>
        {
            var message = new System.Text.StringBuilder();

            message.AppendLine(result.Message);
            message.AppendLine();

            foreach (var row in result.Rows)
            {
                message.AppendLine($"ردیف: {row.RowName}");
                message.AppendLine($"مقدار عددی ردیف: {row.Value}");
                message.AppendLine();

                foreach (var bubble in row.Bubbles)
                {
                    message.AppendLine(
                        $"حباب {bubble.Index}: " +
                        $"{bubble.Message} - " +
                        $"درصد سیاهی: {bubble.BlackRatio:P1}");
                }

                message.AppendLine();
                message.AppendLine("--------------------------------");
                message.AppendLine();
            }

            var resultForm = new Form
            {
                Text = "نتیجه خواندن حباب‌ها",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(650, 650),
                MinimumSize = new Size(450, 350),
                MaximizeBox = true,
                MinimizeBox = false,
                ShowInTaskbar = false,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };

            var txtResult = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10),
                Text = message.ToString(),
                BackColor = SystemColors.Window
            };

            var btnClose = new Button
            {
                Text = "بستن",
                Dock = DockStyle.Bottom,
                Height = 40,
                DialogResult = DialogResult.OK
            };

            resultForm.Controls.Add(txtResult);
            resultForm.Controls.Add(btnClose);
            resultForm.AcceptButton = btnClose;

            resultForm.ShowDialog(this);
        };

        // نتیجه تایم‌مارک‌ها
        _controller.TimingMarksDetected += result =>
        {
            MessageBox.Show(
                result.Message,
                result.IsSuccess
                    ? "نتیجه تشخیص تایم‌مارک‌ها"
                    : "خطا در تشخیص تایم‌مارک‌ها",
                MessageBoxButtons.OK,
                result.IsSuccess
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);
        };

        // تغییرات نشانگرها
        _originalView.MarkersChanged += () =>
        {
            UpdateProfileFromEditor();
            RefreshMarkerGrid();
        };

        _originalView.SelectionChanged += SelectGridMarker;

        gridMarkers.CellEndEdit += (_, e) =>
        {
            UpdateMarkerFromGrid(e.RowIndex);
        };

        gridMarkers.SelectionChanged += (_, _) =>
        {
            if (_refreshingGrid)
                return;

            if (gridMarkers.CurrentRow?.Tag is AlignmentMarker marker)
                _originalView.SelectMarker(marker.Id);
        };

        _originalView.OmrLayoutChanged += () =>
        {
            lblStatus.Text = "تنظیمات هندسی پاسخنامه تغییر کرد.";
        };

        // تنظیم حباب‌ها
        btnConfigureBubbles.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_currentImagePath))
            {
                MessageBox.Show(
                    "ابتدا تصویر مرجع را انتخاب کنید.",
                    "تصویر انتخاب نشده",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(
                    _controller.Profile.OmrLayout);

                var temporaryLayout =
                    JsonSerializer.Deserialize<OmrLayoutProfile>(json)
                    ?? new OmrLayoutProfile();

                using var form = new OmrBubbleSetupForm(
                    _currentImagePath,
                    temporaryLayout,
                    _settings);

                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                _controller.Profile.OmrLayout = form.Layout;

                _originalView.SetOmrLayout(form.Layout);

                SaveProfile();

                lblStatus.Text =
                    "تنظیمات حباب‌ها با موفقیت ذخیره شد.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطای تنظیم حباب‌ها",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };
    }

    #endregion

    #region Batch Processing

    private async Task RunBatchProcessingAsync()
    {
        string folderPath = txtBatchFolderPath.Text;

        if (!Directory.Exists(folderPath))
        {
            MessageBox.Show(
                "ابتدا یک پوشه معتبر انتخاب کنید.",
                "پوشه انتخاب نشده",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        List<string> files;

        try
        {
            files = Directory
                .EnumerateFiles(folderPath, "*.jpg", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطای دسترسی به پوشه",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        if (files.Count == 0)
        {
            MessageBox.Show(
                "هیچ فایل JPG در پوشه انتخاب‌شده پیدا نشد.",
                "فایل پیدا نشد",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        UpdateProfileFromEditor();
        _controller.SetProfile(GetCurrentProfile());

        _lastBatchResults.Clear();

        progressBatch.Minimum = 0;
        progressBatch.Maximum = files.Count;
        progressBatch.Value = 0;

        lblBatchProgress.Text = $"0 از {files.Count}";

        btnRunBatch.Enabled = false;
        btnSelectFolder.Enabled = false;
        SetToolbarEnabled(false);
        lstFiles.Enabled = false;

        int successCount = 0;
        int errorCount = 0;

        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                string filePath = files[i];
                string fileName = Path.GetFileName(filePath);

                lblBatchProgress.Text = $"{i + 1} از {files.Count}";
                lblStatus.Text = $"در حال پردازش: {fileName}";

                try
                {
                    using var processing = await Task.Run(
                        () => _controller.ProcessSingleImage(filePath));

                    int? candidateNumber = null;
                    int? password = null;

                    var rows = processing.Bubbles?.Rows;

                    if (rows != null && rows.Count >= 2)
                    {
                        candidateNumber = rows[0].Value;
                        password = rows[1].Value;
                    }

                    bool readSuccess =
                        processing.IsSuccess &&
                        candidateNumber.HasValue &&
                        password.HasValue;

                    bool passwordValid =
                        readSuccess &&
                        (long)password!.Value ==
                        (long)candidateNumber!.Value + 500;

                    string errorMessage = string.Empty;

                    if (!processing.IsSuccess)
                    {
                        errorMessage = processing.ErrorMessage;

                        if (string.IsNullOrWhiteSpace(errorMessage))
                            errorMessage = "پردازش تصویر با موفقیت انجام نشد.";
                    }
                    else if (!candidateNumber.HasValue || !password.HasValue)
                    {
                        errorMessage =
                            "اطلاعات دو ردیف شماره داوطلبی و رمز خوانده نشد.";
                    }
                    else if (!passwordValid)
                    {
                        errorMessage =
                            $"رمز خوانده‌شده معتبر نیست. " +
                            $"شماره داوطلبی: {candidateNumber.Value}، " +
                            $"رمز خوانده‌شده: {password.Value}، " +
                            $"رمز مورد انتظار: {(long)candidateNumber.Value + 500}";
                    }

                    var answers = processing.Bubbles?.AnswerQuestions
      .ToDictionary(
          x => x.QuestionNumber,
          x => x.Value)
      ?? [];

                    _lastBatchResults.Add(
                        new BatchProcessingRecord
                        {
                            OriginalPath = filePath,
                            CandidateNumber = candidateNumber,
                            Password = password,
                            IsPasswordValid = passwordValid,
                            Status = passwordValid ? "موفق" : "ناموفق",
                            ErrorMessage = errorMessage,
                            Answers = answers
                        });

                    if (passwordValid)
                        successCount++;
                    else
                        errorCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;

                    _lastBatchResults.Add(
                        new BatchProcessingRecord
                        {
                            OriginalPath = filePath,
                            Status = "خطای پردازش",
                            ErrorMessage = ex.Message
                        });
                }

                progressBatch.Value = i + 1;

                // فرصت به‌روزرسانی رابط کاربری
                await Task.Yield();
            }

            // ساخت پوشه خروجی، کپی تصاویر و تولید Excel
            string reportPath = string.Empty;
            string reportError = string.Empty;

            try
            {
                reportPath = FinalizeBatchResults(folderPath);
            }
            catch (Exception ex)
            {
                reportError = ex.Message;
            }

            int copiedCount = _lastBatchResults.Count(
                x => x.CopyStatus == "کپی شد");

            int duplicateCount = _lastBatchResults.Count(
                x => x.CopyStatus == "کپی نشد - شماره تکراری");

            int conflictCount = _lastBatchResults.Count(
                x => x.CopyStatus == "کپی نشد - فایل مقصد موجود است");

            int copyErrorCount = _lastBatchResults.Count(
                x => x.CopyStatus == "خطا در کپی");

            string summary =
                $"پردازش پوشه به پایان رسید.\n\n" +
                $"تعداد کل تصاویر: {files.Count}\n" +
                $"پردازش موفق و رمز معتبر: {successCount}\n" +
                $"ناموفق یا دارای رمز نامعتبر: {errorCount}\n" +
                $"تصاویر دارای شماره تکراری: {duplicateCount}\n" +
                $"تداخل فایل مقصد: {conflictCount}\n" +
                $"خطا در کپی تصاویر: {copyErrorCount}\n" +
                $"تعداد تصاویر کپی‌شده: {copiedCount}\n\n" +
                $"پوشه تصاویر خروجی:\n{Path.Combine(folderPath, "Processed")}";

            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                summary += $"\n\nگزارش Excel:\n{reportPath}";
            }
            else
            {
                summary +=
                    $"\n\nخطا در ساخت گزارش Excel:\n{reportError}";
            }

            txtInfo.Text = summary;
            lblStatus.Text = "پردازش گروهی به پایان رسید.";

            MessageBox.Show(
                summary,
                "نتیجه پردازش گروهی",
                MessageBoxButtons.OK,
                string.IsNullOrWhiteSpace(reportPath)
                    ? MessageBoxIcon.Warning
                    : MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطای پردازش گروهی",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnRunBatch.Enabled = true;
            btnSelectFolder.Enabled = true;
            SetToolbarEnabled(true);
            lstFiles.Enabled = true;
        }
    }

    #endregion

    #region Copy Images and Excel Report

    private string FinalizeBatchResults(string folderPath)
    {
        string processedFolder = Path.Combine(
            folderPath,
            "Processed");

        var validRecords = _lastBatchResults
            .Where(x =>
                x.IsPasswordValid &&
                x.CandidateNumber.HasValue)
            .ToList();

        // شناسایی شماره‌های تکراری
        var duplicateNumbers = validRecords
            .GroupBy(x => x.CandidateNumber!.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        if (validRecords.Count > 0)
            Directory.CreateDirectory(processedFolder);

        foreach (var record in _lastBatchResults)
        {
            if (!record.IsPasswordValid ||
                !record.CandidateNumber.HasValue)
            {
                record.CopyStatus = "کپی نشد - پاسخنامه نامعتبر";
                continue;
            }

            int candidateNumber = record.CandidateNumber.Value;

            if (duplicateNumbers.Contains(candidateNumber))
            {
                record.Status = "شماره داوطلبی تکراری";
                record.CopyStatus = "کپی نشد - شماره تکراری";

                AppendError(
                    record,
                    $"شماره داوطلبی {candidateNumber} در چند تصویر تکرار شده است.");

                continue;
            }

            string destinationPath = Path.Combine(
                processedFolder,
                $"{candidateNumber}.jpg");

            record.DestinationPath = destinationPath;

            // هرگز فایل قبلی را بازنویسی نمی‌کنیم
            if (File.Exists(destinationPath))
            {
                record.Status = "تداخل نام مقصد";
                record.CopyStatus = "کپی نشد - فایل مقصد موجود است";

                AppendError(
                    record,
                    $"فایل مقصد از قبل وجود دارد: {destinationPath}");

                continue;
            }

            try
            {
                File.Copy(
                    record.OriginalPath,
                    destinationPath,
                    overwrite: false);

                record.CopyStatus = "کپی شد";
            }
            catch (Exception ex)
            {
                record.CopyStatus = "خطا در کپی";

                AppendError(
                    record,
                    $"خطا هنگام کپی تصویر: {ex.Message}");
            }
        }

        // تولید گزارش Excel
        return CreateExcelReport(folderPath);
    }


    private string CreateExcelReport(string folderPath)
    {
        string reportPath = GetUniqueReportPath(folderPath);

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("گزارش پردازش");

        var headers = new List<string>
        {
            "نام فایل اصلی",
            "شماره داوطلبی",
            "رمز خوانده‌شده",
            "رمز مورد انتظار",
            "وضعیت خواندن",
            "وضعیت کپی",
            "مسیر فایل اصلی",
            "مسیر فایل کپی",
            "شرح خطا",
            "پاسخنامه کامل"
        };

        // افزودن ستون‌های ۳۰۰ سؤال
        for (int questionNumber = 1; questionNumber <= 300; questionNumber++)
        {
            headers.Add($"سؤال {questionNumber}");
        }

        // عنوان ستون‌ها
        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        int row = 2;

        foreach (var record in _lastBatchResults)
        {
            // اطلاعات اصلی
            worksheet.Cell(row, 1).Value =
                Path.GetFileName(record.OriginalPath);

            if (record.CandidateNumber.HasValue)
                worksheet.Cell(row, 2).Value = record.CandidateNumber.Value;

            if (record.Password.HasValue)
                worksheet.Cell(row, 3).Value = record.Password.Value;

            if (record.ExpectedPassword.HasValue)
                worksheet.Cell(row, 4).Value = record.ExpectedPassword.Value;

            worksheet.Cell(row, 5).Value = record.Status;
            worksheet.Cell(row, 6).Value = record.CopyStatus;
            worksheet.Cell(row, 7).Value = record.OriginalPath;
            worksheet.Cell(row, 8).Value = record.DestinationPath;
            worksheet.Cell(row, 9).Value = record.ErrorMessage;

            // ساخت رشته پیوسته پاسخ‌های ۳۰۰ سؤال
            var answerString = new System.Text.StringBuilder(300);

            for (int questionNumber = 1; questionNumber <= 300; questionNumber++)
            {
                string answer = "0";

                if (record.Answers.TryGetValue(questionNumber, out var detectedAnswer))
                {
                    answer = detectedAnswer;
                }

                // ثبت پاسخ هر سؤال در ستون مستقل
                worksheet.Cell(row, 10 + questionNumber).Value = answer;

                // افزودن پاسخ به رشته پیوسته
                answerString.Append(answer);
            }

            // ثبت پاسخنامه کامل در ستون ۱۰
            worksheet.Cell(row, 10).Value = answerString.ToString();

            row++;
        }

        // قالب‌بندی عنوان‌ها
        var headerRange = worksheet.Range(
            1, 1,
            1, headers.Count);

        headerRange.Style.Font.Bold = true;

        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        headerRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        headerRange.Style.Fill.BackgroundColor =
            XLColor.LightBlue;

        worksheet.SheetView.FreezeRows(1);

        worksheet.RangeUsed()?.SetAutoFilter();

        // تنظیم عرض ستون‌های اطلاعات اصلی
        worksheet.Columns(1, 9).AdjustToContents();

        worksheet.Column(7).Width = 45;
        worksheet.Column(8).Width = 45;
        worksheet.Column(9).Width = 60;

        worksheet.Column(9).Style.Alignment.WrapText = true;

        // ستون پاسخنامه کامل
        worksheet.Column(10).Width = 60;

        // ستون‌های سؤال ۱ تا ۳۰۰ (ستون ۱۱ تا ۳۱۰)
        worksheet.Columns(11, 310).Width = 10;

        if (row > 2)
        {
            // قالب‌بندی پاسخنامه کامل
            var fullAnswerRange = worksheet.Range(2, 10, row - 1, 10);

            fullAnswerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Left;

            // قالب‌بندی پاسخ‌های تک‌تک سؤالات
            var answerRange = worksheet.Range(2, 11, row - 1, 310);

            answerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            answerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
        }

        // ذخیره فایل اکسل
        workbook.SaveAs(reportPath);

        return reportPath;
    }



    private static string GetUniqueReportPath(string folderPath)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        string reportPath = Path.Combine(
            folderPath,
            $"OMR-Report-{timestamp}.xlsx");

        int counter = 1;

        while (File.Exists(reportPath))
        {
            reportPath = Path.Combine(
                folderPath,
                $"OMR-Report-{timestamp}-{counter}.xlsx");

            counter++;
        }

        return reportPath;
    }

    private static void AppendError(BatchProcessingRecord record, string message)
    {
        if (string.IsNullOrWhiteSpace(record.ErrorMessage))
        {
            record.ErrorMessage = message;
        }
        else
        {
            record.ErrorMessage += Environment.NewLine + message;
        }
    }

    #endregion

    #region Batch Result Model

    private sealed class BatchProcessingRecord
    {
        public string OriginalPath { get; set; } = string.Empty;

        public int? CandidateNumber { get; set; }

        public int? Password { get; set; }

        public long? ExpectedPassword =>
            CandidateNumber.HasValue
                ? (long)CandidateNumber.Value + 500
                : null;

        public bool IsPasswordValid { get; set; }

        public string Status { get; set; } = string.Empty;

        public string CopyStatus { get; set; } = string.Empty;

        public string DestinationPath { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public Dictionary<int, string> Answers { get; set; } = [];
    }

    #endregion

    #region Image and Profile

    private void LoadSelectedImage(string path)
    {
        _currentImagePath = path;

        _controller.SetProfile(GetCurrentProfile());

        _controller.LoadImage(path);
    }

    private AlignmentProfile GetCurrentProfile()
    {
        var profile = _controller.Profile;
        profile.Markers = _originalView.Markers.ToList();

        return profile;
    }

    private void UpdateProfileFromEditor()
    {
        _controller.Profile.Markers =
            _originalView.Markers.ToList();
    }

    private void RefreshMarkerGrid()
    {
        _refreshingGrid = true;

        try
        {
            var selectedId = _originalView.SelectedMarkerId;

            gridMarkers.Rows.Clear();

            foreach (var marker in _originalView.Markers)
            {
                var index = gridMarkers.Rows.Add(
                    marker.Name,
                    marker.TargetXRatio * 100,
                    marker.TargetYRatio * 100);

                gridMarkers.Rows[index].Tag = marker;

                if (selectedId == marker.Id)
                {
                    gridMarkers.Rows[index].Selected = true;
                    gridMarkers.CurrentCell =
                        gridMarkers.Rows[index].Cells[0];
                }
            }
        }
        finally
        {
            _refreshingGrid = false;
        }
    }

    private void SelectGridMarker(Guid? id)
    {
        if (_refreshingGrid || id == null)
            return;

        foreach (DataGridViewRow row in gridMarkers.Rows)
        {
            if (row.Tag is not AlignmentMarker marker ||
                marker.Id != id)
                continue;

            _refreshingGrid = true;

            try
            {
                row.Selected = true;
                gridMarkers.CurrentCell = row.Cells[0];
            }
            finally
            {
                _refreshingGrid = false;
            }

            return;
        }
    }

    private void UpdateMarkerFromGrid(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= gridMarkers.Rows.Count)
            return;

        var row = gridMarkers.Rows[rowIndex];

        if (row.Tag is not AlignmentMarker marker)
            return;

        var name = Convert.ToString(row.Cells["Name"].Value);

        var xText = Convert.ToString(row.Cells["TargetX"].Value);
        var yText = Convert.ToString(row.Cells["TargetY"].Value);

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("نام نشانگر نمی‌تواند خالی باشد.");
            RefreshMarkerGrid();
            return;
        }

        if (!double.TryParse(
                xText,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out var x) ||
            !double.TryParse(
                yText,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out var y) ||
            x < 0 || x > 100 ||
            y < 0 || y > 100)
        {
            MessageBox.Show("مختصات مقصد باید بین صفر تا صد باشد.");
            RefreshMarkerGrid();
            return;
        }

        marker.Name = name.Trim();
        marker.TargetXRatio = x / 100;
        marker.TargetYRatio = y / 100;

        _originalView.NotifyMarkerChanged();

        lblStatus.Text = "مختصات مقصد نشانگر به‌روزرسانی شد.";
    }

    private void SaveProfile()
    {
        try
        {
            UpdateProfileFromEditor();

            _profileService.Save(
                _defaultProfilePath,
                GetCurrentProfile());

            lblStatus.Text = "پروفایل پیش‌فرض با موفقیت ذخیره شد.";

            MessageBox.Show(
                $"پروفایل با موفقیت ذخیره شد.\n\nمسیر فایل:\n{_defaultProfilePath}",
                "ذخیره پروفایل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطای ذخیره پروفایل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadProfile()
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON Profile|*.json"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var profile = _profileService.Load(dialog.FileName);

            _controller.SetProfile(profile);
            _originalView.SetMarkers(profile.Markers);
            _originalView.SetOmrLayout(profile.OmrLayout);

            RefreshMarkerGrid();

            lblStatus.Text = "پروفایل با موفقیت بارگذاری شد.";

            if (_currentImagePath != null)
                _controller.LoadImage(_currentImagePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطای بارگذاری پروفایل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    #endregion



    private void SetToolbarEnabled(bool enabled)
    {
        btnSelectFile.Enabled = enabled;
        btnSelectFolder.Enabled = enabled;
        btnAddMarker.Enabled = enabled;
        btnRemoveMarker.Enabled = enabled;
        btnConfigureBubbles.Enabled = enabled;
        btnAlign.Enabled = enabled;
        btnSaveProfile.Enabled = enabled;
        btnLoadProfile.Enabled = enabled;
    }

    private void lblAlignedTitle_Click(object sender, EventArgs e)
    {

    }


}
