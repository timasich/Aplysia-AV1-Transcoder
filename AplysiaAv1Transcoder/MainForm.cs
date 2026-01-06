using System.Drawing;
using System.Text;
using AplysiaAv1Transcoder.Models;
using AplysiaAv1Transcoder.Services;

namespace AplysiaAv1Transcoder;

public partial class MainForm : Form
{
    private readonly StorageService _storage;
    private readonly SettingsService _settingsService;
    private readonly PresetService _presetService;
    private readonly FfmpegLocator _ffmpegLocator;
    private readonly FfprobeService _ffprobeService;
    private readonly DurationService _durationService;
    private readonly EncoderCapabilitiesService _encoderCapabilitiesService;
    private readonly TranscodeService _transcodeService;

    private AppSettings _settings;
    private List<Preset> _userPresets;
    private readonly List<LogEntry> _logEntries = new();

    private CancellationTokenSource? _queueCts;
    private bool _isProcessing;
    private bool _isUpdatingSelection;
    private bool _suppressTrimText;
    private Color _trimPanelDefaultColor;
    private Color _trimTextBoxDefaultColor;
    private ContextMenuStrip? _queueContextMenu;

    public MainForm()
    {
        InitializeComponent();

        InitializeTrayIcon();
        ConfigureQueueColumns();

        _storage = new StorageService();
        _settingsService = new SettingsService(_storage);
        _presetService = new PresetService(_storage);
        _ffmpegLocator = new FfmpegLocator(_storage);
        _ffprobeService = new FfprobeService();
        _encoderCapabilitiesService = new EncoderCapabilitiesService();
        _transcodeService = new TranscodeService();

        _settings = _settingsService.Load();
        _userPresets = _presetService.LoadPresets();
        _trimPanelDefaultColor = trimPanel.BackColor;
        _trimTextBoxDefaultColor = textTrimStart.BackColor;
        _durationService = new DurationService(
            new (DurationSource, IVideoDurationProvider)[]
            {
                (DurationSource.Shell, new ShellDurationProvider(AddLog)),
                (DurationSource.MediaFoundation, new MediaFoundationDurationProvider(AddLog)),
                (DurationSource.Ffprobe, new FfprobeDurationProvider(() => _settings.ResolvedFfprobePath, AddLog))
            },
            AddLog);

        WireEvents();
        InitializeUiFromSettings();
    }

    private void InitializeTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "icon.ico");
        if (File.Exists(iconPath))
        {
            notifyIcon.Icon = new Icon(iconPath);
        }
        else
        {
            notifyIcon.Icon = Icon;
        }

        notifyIcon.Text = Text;
        notifyIcon.Visible = true;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ApplySplitterDistance();
        var resolved = await _ffmpegLocator.ResolveAsync(_settings, this, AddLog, UpdateFfmpegDownloadStatus);
        if (resolved)
        {
            _settingsService.Save(_settings);
        }

        UpdateStatusIndicators();
    }

    private void WireEvents()
    {
        btnAdd.Click += async (_, _) => await AddFilesFromDialogAsync();
        btnRemove.Click += (_, _) => RemoveSelectedItems();
        btnClear.Click += (_, _) => ClearQueue();
        btnMoveUp.Click += (_, _) => MoveSelected(-1);
        btnMoveDown.Click += (_, _) => MoveSelected(1);
        btnStart.Click += async (_, _) => await StartQueueAsync();
        btnCancel.Click += (_, _) => CancelQueue();
        checkUncheckAfterRender.CheckedChanged += (_, _) => OnUncheckAfterRenderChanged();

        btnApplyPreset.Click += (_, _) => ApplyPresetToSelected();
        btnNewPreset.Click += (_, _) => CreatePreset();
        btnEditPreset.Click += (_, _) => EditPreset();
        btnDuplicatePreset.Click += (_, _) => DuplicatePreset();
        btnDeletePreset.Click += (_, _) => DeletePreset();
        comboActivePreset.SelectedIndexChanged += (_, _) => OnActivePresetChanged();

        checkEnableTrim.CheckedChanged += (_, _) => ApplyTrimSettings();
        trimTimeline.RangeChanged += (_, _) => OnTrimTimelineChanged();
        textTrimStart.TextChanged += (_, _) => OnTrimTextChanged();
        textTrimEnd.TextChanged += (_, _) => OnTrimTextChanged();
        btnResetTrim.Click += (_, _) => ResetTrimForSelected();

        checkSameAsSource.CheckedChanged += (_, _) => OnOutputModeChanged();
        textOutputFolder.TextChanged += (_, _) => OnOutputFolderChanged();
        btnBrowseOutput.Click += (_, _) => BrowseOutputFolder();
        comboRecentOutputs.SelectedIndexChanged += (_, _) => SelectRecentOutputFolder();

        comboPriority.SelectedIndexChanged += (_, _) => OnEncoderPriorityChanged();
        trackSpeedQuality.ValueChanged += (_, _) => OnSpeedQualityChanged();
        btnFfmpegSettings.Click += (_, _) => OpenFfmpegSettings();

        listQueue.SelectedIndexChanged += (_, _) => UpdateSelectionUi();
        listQueue.ItemChecked += (_, e) => OnQueueItemChecked(e);
        listQueue.MouseDown += ListQueueOnMouseDown;
        listQueue.DragEnter += ListQueueOnDragEnter;
        listQueue.DragDrop += async (_, e) => await ListQueueOnDragDropAsync(e);
        splitQueue.SplitterMoved += (_, _) => SaveSplitterDistance();

        btnCopySelected.Click += (_, _) => CopySelectedLogs();
        btnCopyDiagnostics.Click += (_, _) => CopyDiagnostics();
        btnClearLog.Click += (_, _) => ClearLogs();
        comboLogFilter.SelectedIndexChanged += (_, _) => RefreshLogView();

        FormClosing += (_, _) => SaveState();
    }

    private void InitializeUiFromSettings()
    {
        comboPriority.Items.AddRange(new object[] { "Auto(HW)", "NVENC", "Intel QSV", "AMD AMF", "CPU" });

        checkSameAsSource.Checked = false;
        textOutputFolder.Text = _settings.LastOutputFolder ?? string.Empty;
        checkUncheckAfterRender.Checked = _settings.UncheckAfterRender;

        comboRecentOutputs.Items.Clear();
        foreach (var path in _settings.RecentOutputFolders)
        {
            comboRecentOutputs.Items.Add(path);
        }

        LoadPresetsIntoUi();
        comboPriority.SelectedItem = PriorityLabel(_settings.EncoderPriority);
        if (comboPriority.SelectedIndex < 0)
        {
            comboPriority.SelectedIndex = 0;
        }

        trackSpeedQuality.Minimum = 1;
        trackSpeedQuality.Maximum = 9;
        var speedQuality = _settings.SpeedQuality <= 0 ? 5 : _settings.SpeedQuality;
        trackSpeedQuality.Value = Math.Clamp(speedQuality, trackSpeedQuality.Minimum, trackSpeedQuality.Maximum);

        UpdateFfmpegDownloadStatus(string.Empty);
        groupTrim.Visible = true;
        groupTrim.Enabled = false;
        UpdateStatusIndicators();
    }

    private void ConfigureQueueColumns()
    {
        if (listQueue.Columns.Count > 0)
        {
            return;
        }

        listQueue.Columns.Add("File", 420);
        listQueue.Columns.Add("Preset", 140);
        listQueue.Columns.Add("Trim", 120);
        listQueue.Columns.Add("Output", 240);
        listQueue.Columns.Add("Est. size", 90);
        listQueue.Columns.Add("Status", 120);
    }

    private void LoadPresetsIntoUi()
    {
        comboActivePreset.DisplayMember = "Name";
        comboActivePreset.Items.Clear();
        foreach (var preset in _userPresets)
        {
            comboActivePreset.Items.Add(preset);
        }

        if (!string.IsNullOrWhiteSpace(_settings.LastSelectedPreset))
        {
            var match = _userPresets.FirstOrDefault(p => string.Equals(p.Name, _settings.LastSelectedPreset, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                comboActivePreset.SelectedItem = match;
                UpdatePresetButtons();
                return;
            }
        }

        if (comboActivePreset.Items.Count > 0)
        {
            comboActivePreset.SelectedIndex = 0;
        }
    }

    private Preset? GetActivePreset()
    {
        return comboActivePreset.SelectedItem as Preset;
    }

    private async Task AddFilesFromDialogAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Video files|*.mkv;*.mp4;*.webm;*.mov;*.avi;*.m4v|All files|*.*",
            Multiselect = true,
            Title = "Add files"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await AddFilesAsync(dialog.FileNames);
    }

    private async Task AddFilesAsync(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                continue;
            }

            var item = new QueueItem
            {
                FilePath = file,
                Render = true,
                Status = "Queued"
            };

            var listItem = new ListViewItem(file);
            EnsureListViewSubItems(listItem);
            listItem.Tag = item;
            listItem.Checked = true;
            listQueue.Items.Add(listItem);

            _ = UpdateDurationForItemAsync(item);
            await ProbeAndApplyPresetAsync(item);
            InitializeTrimDefaults(item);
            UpdateListViewItem(item, listItem);
        }

        AutoResizeQueueColumns();
        UpdateStatusIndicators();
    }

    private async Task ProbeAndApplyPresetAsync(QueueItem item)
    {
        if (!string.IsNullOrWhiteSpace(_settings.ResolvedFfprobePath) && File.Exists(_settings.ResolvedFfprobePath))
        {
            var info = await _ffprobeService.ProbeAsync(_settings.ResolvedFfprobePath, item.FilePath, CancellationToken.None);
            item.ProbeInfo = info;
            if (!string.IsNullOrWhiteSpace(info?.VideoCodec))
            {
                item.IsAv1 = string.Equals(info.VideoCodec, "av1", StringComparison.OrdinalIgnoreCase);
            }
        }

        var preset = GetActivePreset();

        if (preset == null)
        {
            return;
        }

        ApplyPreset(item, preset);
    }

    private async Task UpdateDurationForItemAsync(QueueItem item)
    {
        try
        {
            var (duration, source) = await _durationService.TryGetDurationAsync(item.FilePath, CancellationToken.None);
            item.Duration = duration;
            item.DurationSource = source;

            if (duration.HasValue)
            {
                if (!item.TrimWasEditedByUser)
                {
                    item.TrimStart = FormatTrimTime(TimeSpan.Zero);
                    item.TrimEnd = FormatTrimTime(duration.Value);
                }

                AddLog(new LogEntry
                {
                    Level = LogLevel.Info,
                    Message = $"Duration detected: {FormatTrimTime(duration.Value)} (Source: {source})"
                });
            }
            else
            {
                AddLog(new LogEntry
                {
                    Level = LogLevel.Info,
                    Message = "Duration unavailable (Shell/MF/Ffprobe failed)"
                });
            }

            var listItem = FindListViewItem(item);
            if (listItem != null)
            {
                UpdateListViewItem(item, listItem);
            }

            if (listQueue.SelectedItems.Count == 1 && listQueue.SelectedItems[0].Tag == item)
            {
                _isUpdatingSelection = true;
                UpdateTrimUiFromItem(item);
                _isUpdatingSelection = false;
            }
        }
        catch (Exception ex)
        {
            AddLog(new LogEntry { Level = LogLevel.Warning, Message = $"Duration detection failed: {ex.Message}" });
        }
    }

    private void ApplyPreset(QueueItem item, Preset preset)
    {
        item.PresetId = preset.Id;
        item.PresetName = preset.Name;
        var snapshot = preset.Clone();
        item.PresetSnapshot = snapshot;
    }

    private void ApplyPresetToSelected()
    {
        var preset = GetActivePreset();
        if (preset == null)
        {
            return;
        }

        ApplyPresetToSelection(preset);
    }

    private void OnEncoderPriorityChanged()
    {
        _settings.EncoderPriority = ParsePriority(comboPriority.SelectedItem?.ToString()) ?? EncoderPriority.AutoHW;
    }

    private void OnSpeedQualityChanged()
    {
        _settings.SpeedQuality = trackSpeedQuality.Value;
    }

    private void OnUncheckAfterRenderChanged()
    {
        _settings.UncheckAfterRender = checkUncheckAfterRender.Checked;
    }

    private void ApplyTrimSettings()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        if (listQueue.SelectedItems.Count != 1)
        {
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item)
        {
            return;
        }

        item.TrimEnabled = checkEnableTrim.Checked;
        SetTrimInputsEnabled(item.TrimEnabled);
        if (!item.TrimEnabled)
        {
            labelTrimError.Visible = false;
            UpdateListViewItem(item, listQueue.SelectedItems[0]);
            UpdateTrimValidation();
            UpdateStatusIndicators();
            return;
        }

        EnsureTrimDefaults(item);
        UpdateTrimUiFromItem(item);
        UpdateListViewItem(item, listQueue.SelectedItems[0]);
        UpdateTrimValidation();
        UpdateStatusIndicators();
    }

    private void InitializeTrimDefaults(QueueItem item)
    {
        item.TrimStart = FormatTrimTime(TimeSpan.Zero);
        if (item.Duration.HasValue && item.Duration.Value > TimeSpan.Zero)
        {
            item.TrimEnd = FormatTrimTime(item.Duration.Value);
        }
        else
        {
            item.TrimEnd = string.Empty;
        }

        item.TrimEnabled = false;
        item.TrimWasEditedByUser = false;
    }

    private void EnsureTrimDefaults(QueueItem item)
    {
        if (string.IsNullOrWhiteSpace(item.TrimStart))
        {
            item.TrimStart = FormatTrimTime(TimeSpan.Zero);
        }

        if (string.IsNullOrWhiteSpace(item.TrimEnd))
        {
            item.TrimEnd = item.Duration.HasValue && item.Duration.Value > TimeSpan.Zero
                ? FormatTrimTime(item.Duration.Value)
                : string.Empty;
        }

        if (item.TrimWasEditedByUser)
        {
            return;
        }

        if (item.Duration.HasValue && item.Duration.Value > TimeSpan.Zero)
        {
            var duration = item.Duration.Value;
            if (!TryParseTrimTime(item.TrimStart, out var start) || start < TimeSpan.Zero)
            {
                start = TimeSpan.Zero;
            }

            if (start > duration)
            {
                start = duration;
            }

            if (!TryParseTrimTime(item.TrimEnd, out var end) || end == TimeSpan.Zero)
            {
                end = duration;
            }
            else if (end > duration)
            {
                end = duration;
            }

            item.TrimStart = FormatTrimTime(start);
            item.TrimEnd = FormatTrimTime(end);
        }
    }

    private void ResetTrimForSelected()
    {
        if (listQueue.SelectedItems.Count != 1)
        {
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item)
        {
            return;
        }

        InitializeTrimDefaults(item);
        _isUpdatingSelection = true;
        checkEnableTrim.Checked = item.TrimEnabled;
        UpdateTrimUiFromItem(item);
        _isUpdatingSelection = false;

        SetTrimInputsEnabled(checkEnableTrim.Checked);
        UpdateListViewItem(item, listQueue.SelectedItems[0]);
        AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Trim reset: {item.FilePath}" });
        UpdateTrimValidation();
        UpdateStatusIndicators();
    }

    private void SetTrimInputsEnabled(bool enabled)
    {
        trimTimeline.Enabled = enabled && trimTimeline.DurationSeconds > 0;
        textTrimStart.Enabled = enabled;
        textTrimEnd.Enabled = enabled;
        btnResetTrim.Enabled = enabled;
    }

    private void UpdateSelectionUi()
    {
        if (listQueue.SelectedItems.Count != 1)
        {
            groupTrim.Enabled = true;
            trimPanel.Visible = true;
            labelTrimHint.Visible = true;
            trimTimeline.Visible = false;
            labelTrimStart.Visible = false;
            labelTrimEnd.Visible = false;
            textTrimStart.Visible = false;
            textTrimEnd.Visible = false;
            labelTrimError.Visible = false;
            btnResetTrim.Visible = false;
            SetTrimInputsEnabled(false);
            UpdateStatusIndicators();
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item)
        {
            return;
        }

        groupTrim.Enabled = true;
        trimPanel.Visible = true;
        trimTimeline.Visible = true;
        labelTrimStart.Visible = true;
        labelTrimEnd.Visible = true;
        textTrimStart.Visible = true;
        textTrimEnd.Visible = true;
        btnResetTrim.Visible = true;
        groupTrim.PerformLayout();
        trimPanel.PerformLayout();

        _isUpdatingSelection = true;
        checkEnableTrim.Checked = item.TrimEnabled;
        EnsureTrimDefaults(item);
        UpdateTrimUiFromItem(item);
        _isUpdatingSelection = false;

        SetTrimInputsEnabled(checkEnableTrim.Checked);
        UpdateTrimValidation();
        UpdateStatusIndicators();
    }

    private void OnActivePresetChanged()
    {
        if (comboActivePreset.SelectedItem is Preset preset)
        {
            _settings.LastSelectedPreset = preset.Name;
        }

        UpdatePresetButtons();
        UpdateStatusIndicators();
    }

    private void UpdatePresetButtons()
    {
        if (comboActivePreset.SelectedItem is not Preset preset)
        {
            btnEditPreset.Enabled = false;
            btnDeletePreset.Enabled = false;
            btnDuplicatePreset.Enabled = false;
            return;
        }

        btnEditPreset.Enabled = true;
        btnDeletePreset.Enabled = true;
        btnDuplicatePreset.Enabled = true;
    }

    private void CreatePreset()
    {
        var preset = new Preset { Id = Guid.NewGuid(), Name = "New Preset" };
        using var dialog = new PresetEditorForm(preset, "New Preset", GetSelectedProbeInfo);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ResultPreset == null)
        {
            return;
        }

        _userPresets.Add(dialog.ResultPreset);
        SavePresets();
        LoadPresetsIntoUi();
    }

    private void EditPreset()
    {
        if (comboActivePreset.SelectedItem is not Preset preset)
        {
            return;
        }

        using var dialog = new PresetEditorForm(preset.Clone(), "Edit Preset", GetSelectedProbeInfo);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ResultPreset == null)
        {
            return;
        }

        var index = _userPresets.FindIndex(p => string.Equals(p.Name, preset.Name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            _userPresets[index] = dialog.ResultPreset;
            SavePresets();
            LoadPresetsIntoUi();
        }
    }

    private void DuplicatePreset()
    {
        if (comboActivePreset.SelectedItem is not Preset preset)
        {
            return;
        }

        var copy = preset.Clone($"{preset.Name} Copy");
        copy.Id = Guid.NewGuid();
        using var dialog = new PresetEditorForm(copy, "Duplicate Preset", GetSelectedProbeInfo);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ResultPreset == null)
        {
            return;
        }

        _userPresets.Add(dialog.ResultPreset);
        SavePresets();
        LoadPresetsIntoUi();
    }

    private void DeletePreset()
    {
        if (comboActivePreset.SelectedItem is not Preset preset)
        {
            return;
        }

        var result = MessageBox.Show(this, $"Delete preset '{preset.Name}'?", "Presets", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            return;
        }

        _userPresets.RemoveAll(p => string.Equals(p.Name, preset.Name, StringComparison.OrdinalIgnoreCase));
        SavePresets();
        LoadPresetsIntoUi();
    }

    private void SavePresets()
    {
        _presetService.SavePresets(_userPresets);
    }

    private void OnOutputModeChanged()
    {
        var enabled = !checkSameAsSource.Checked;
        textOutputFolder.Enabled = enabled;
        btnBrowseOutput.Enabled = enabled;
        comboRecentOutputs.Enabled = enabled;
        UpdateTargets();
        UpdateStatusIndicators();
    }

    private void OnOutputFolderChanged()
    {
        _settings.LastOutputFolder = textOutputFolder.Text.Trim();
        UpdateTargets();
        UpdateStatusIndicators();
    }

    private void BrowseOutputFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        textOutputFolder.Text = dialog.SelectedPath;
        AddRecentOutputFolder(dialog.SelectedPath);
    }

    private void SelectRecentOutputFolder()
    {
        if (comboRecentOutputs.SelectedItem is string path)
        {
            textOutputFolder.Text = path;
        }
    }

    private void AddRecentOutputFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        _settings.RecentOutputFolders.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        _settings.RecentOutputFolders.Insert(0, path);
        if (_settings.RecentOutputFolders.Count > 10)
        {
            _settings.RecentOutputFolders = _settings.RecentOutputFolders.Take(10).ToList();
        }

        comboRecentOutputs.Items.Clear();
        foreach (var item in _settings.RecentOutputFolders)
        {
            comboRecentOutputs.Items.Add(item);
        }
    }

    private void OpenFfmpegSettings()
    {
        using var dialog = new FfmpegSettingsForm(_settings.ResolvedFfmpegPath ?? _settings.FfmpegPath);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (!_ffmpegLocator.TrySetFromPath(_settings, dialog.FfmpegPath))
        {
            MessageBox.Show(this, "The selected ffmpeg.exe is invalid or ffprobe.exe was not found in the same folder.",
                "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _settingsService.Save(_settings);
        UpdateStatusIndicators();
    }

    private void UpdateListViewItem(QueueItem item, ListViewItem listItem)
    {
        EnsureListViewSubItems(listItem);
        listItem.Text = item.IsAv1 ? item.FilePath : $"[NOT AV1] {item.FilePath}";
        listItem.SubItems[1].Text = string.IsNullOrWhiteSpace(item.PresetName) ? "None" : item.PresetName;
        listItem.SubItems[2].Text = item.TrimEnabled
            ? $"{item.TrimStart ?? ""}-{item.TrimEnd ?? ""}".Trim('-')
            : string.Empty;
        listItem.SubItems[3].Text = GetOutputPath(item);
        listItem.SubItems[4].Text = FormatEstimatedSize(item);
        listItem.SubItems[5].Text = item.Status;
        listItem.ForeColor = item.IsAv1 ? listQueue.ForeColor : Color.Red;
    }

    private ListViewItem? FindListViewItem(QueueItem item)
    {
        foreach (ListViewItem listItem in listQueue.Items)
        {
            if (ReferenceEquals(listItem.Tag, item))
            {
                return listItem;
            }
        }

        return null;
    }

    private string FormatEstimatedSize(QueueItem item)
    {
        if (item.ProbeInfo == null || !item.IsAv1)
        {
            return string.Empty;
        }

        var targetKbps = TranscodeService.ResolveTargetBitrate(item.PresetSnapshot, item.ProbeInfo);
        if (targetKbps <= 0)
        {
            return string.Empty;
        }

        var sizeKb = item.ProbeInfo.DurationSeconds * (targetKbps / 8.0);
        if (sizeKb <= 0)
        {
            return string.Empty;
        }

        return $"{Math.Round(sizeKb):N0} KB";
    }

    private void UpdateTargets()
    {
        foreach (ListViewItem listItem in listQueue.Items)
        {
            if (listItem.Tag is QueueItem item)
            {
                EnsureListViewSubItems(listItem);
                listItem.SubItems[3].Text = GetOutputPath(item);
            }
        }
    }

    private string GetOutputPath(QueueItem item)
    {
        var folder = GetOutputFolderForItem(item);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return string.Empty;
        }

        var name = Path.GetFileNameWithoutExtension(item.FilePath);
        var suffix = item.PresetSnapshot.TargetCodec == TargetCodec.H264 ? "_h264" : "_h265";
        return Path.Combine(folder, name + suffix + ".mp4");
    }

    private string GetOutputFolderForItem(QueueItem item)
    {
        if (checkSameAsSource.Checked)
        {
            return Path.GetDirectoryName(item.FilePath) ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(item.OutputFolder))
        {
            return item.OutputFolder;
        }

        return textOutputFolder.Text.Trim();
    }

    private void RemoveSelectedItems()
    {
        foreach (ListViewItem item in listQueue.SelectedItems)
        {
            listQueue.Items.Remove(item);
        }

        UpdateStatusIndicators();
    }

    private void ClearQueue()
    {
        listQueue.Items.Clear();
        UpdateStatusIndicators();
    }

    private void EnsureListViewSubItems(ListViewItem listItem)
    {
        while (listItem.SubItems.Count < 6)
        {
            listItem.SubItems.Add(string.Empty);
        }
    }

    private void AutoResizeQueueColumns()
    {
        if (listQueue.Columns.Count == 0)
        {
            return;
        }

        listQueue.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        var fileColumn = listQueue.Columns[0];
        if (fileColumn.Width < 420)
        {
            fileColumn.Width = 420;
        }
    }

    private void MoveSelected(int delta)
    {
        if (listQueue.SelectedItems.Count == 0)
        {
            return;
        }

        var indices = listQueue.SelectedIndices.Cast<int>().ToList();
        if (delta < 0)
        {
            indices.Sort();
        }
        else
        {
            indices.Sort((a, b) => b.CompareTo(a));
        }

        foreach (var index in indices)
        {
            var newIndex = index + delta;
            if (newIndex < 0 || newIndex >= listQueue.Items.Count)
            {
                continue;
            }

            var item = listQueue.Items[index];
            listQueue.Items.RemoveAt(index);
            listQueue.Items.Insert(newIndex, item);
            item.Selected = true;
        }
    }

    private void OnQueueItemChecked(ItemCheckedEventArgs e)
    {
        if (e.Item?.Tag is QueueItem item)
        {
            item.Render = e.Item.Checked;
        }

        UpdateStatusIndicators();
    }

    private void ListQueueOnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        var hit = listQueue.GetItemAt(e.X, e.Y);
        if (hit == null)
        {
            ShowQueueContextMenu(e.Location, false);
            return;
        }

        if (!hit.Selected)
        {
            listQueue.SelectedItems.Clear();
            hit.Selected = true;
        }

        ShowQueueContextMenu(e.Location, true);
    }

    private void ShowQueueContextMenu(Point location, bool hasSelection)
    {
        _queueContextMenu?.Dispose();
        _queueContextMenu = new ContextMenuStrip();

        if (!hasSelection)
        {
            var addFiles = new ToolStripMenuItem("Add files...");
            addFiles.Click += async (_, _) => await AddFilesFromDialogAsync();
            _queueContextMenu.Items.Add(addFiles);
            _queueContextMenu.Show(listQueue, location);
            return;
        }

        var presetMenu = new ToolStripMenuItem("Preset");
        if (_userPresets.Count == 0)
        {
            presetMenu.Enabled = false;
        }
        else
        {
            foreach (var preset in _userPresets)
            {
                var item = new ToolStripMenuItem(preset.Name) { Tag = preset };
                item.Click += (_, _) => ApplyPresetToSelection(preset);
                presetMenu.DropDownItems.Add(item);
            }
        }

        var outputMenu = new ToolStripMenuItem("Output folder");
        if (_settings.RecentOutputFolders.Count == 0)
        {
            outputMenu.DropDownItems.Add(new ToolStripMenuItem("(No recent folders)") { Enabled = false });
        }
        else
        {
            foreach (var path in _settings.RecentOutputFolders)
            {
                var item = new ToolStripMenuItem(path);
                item.Click += (_, _) => SetOutputFolderForSelected(path);
                outputMenu.DropDownItems.Add(item);
            }
        }
        outputMenu.DropDownItems.Add(new ToolStripSeparator());
        var browseItem = new ToolStripMenuItem("Browse...");
        browseItem.Click += (_, _) => BrowseOutputFolderForSelected();
        outputMenu.DropDownItems.Add(browseItem);

        var toggleRender = new ToolStripMenuItem("Toggle Render");
        toggleRender.Click += (_, _) => ToggleRenderForSelected();

        var removeSelected = new ToolStripMenuItem("Remove selected");
        removeSelected.Click += (_, _) => RemoveSelectedItems();

        _queueContextMenu.Items.Add(presetMenu);
        _queueContextMenu.Items.Add(outputMenu);
        _queueContextMenu.Items.Add(toggleRender);
        _queueContextMenu.Items.Add(new ToolStripSeparator());
        _queueContextMenu.Items.Add(removeSelected);
        _queueContextMenu.Show(listQueue, location);
    }

    private void ApplyPresetToSelection(Preset preset)
    {
        foreach (ListViewItem listItem in listQueue.SelectedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            ApplyPreset(item, preset);
            UpdateListViewItem(item, listItem);
            AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Preset applied: {preset.Name} -> {item.FilePath}" });
        }

        UpdateStatusIndicators();
    }

    private void SetOutputFolderForSelected(string path)
    {
        foreach (ListViewItem listItem in listQueue.SelectedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            item.OutputFolder = path;
            UpdateListViewItem(item, listItem);
        }

        AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Output folder set for selection: {path}" });
        UpdateStatusIndicators();
    }

    private void BrowseOutputFolderForSelected()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var path = dialog.SelectedPath;
        AddRecentOutputFolder(path);
        SetOutputFolderForSelected(path);
    }

    private void ToggleRenderForSelected()
    {
        var selected = listQueue.SelectedItems.Cast<ListViewItem>().ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var shouldCheck = selected.Any(item => !item.Checked);
        foreach (var listItem in selected)
        {
            listItem.Checked = shouldCheck;
        }
    }

    private void ListQueueOnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private async Task ListQueueOnDragDropAsync(DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files)
        {
            return;
        }

        await AddFilesAsync(files);
    }

    private async Task StartQueueAsync()
    {
        if (_isProcessing)
        {
            return;
        }

        if (!ValidateBeforeStart())
        {
            return;
        }

        if (!checkSameAsSource.Checked)
        {
            AddRecentOutputFolder(textOutputFolder.Text.Trim());
        }

        var items = listQueue.Items.Cast<ListViewItem>()
            .Select(i => (listItem: i, item: i.Tag as QueueItem))
            .Where(pair => pair.item != null && pair.item.Render)
            .Select(pair => (pair.listItem, item: pair.item!))
            .ToList();

        if (items.Count == 0)
        {
            MessageBox.Show(this, "No checked items to process.", "Queue", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _queueCts = new CancellationTokenSource();
        _isProcessing = true;
        btnStart.Enabled = false;
        btnCancel.Enabled = true;

        EncoderCapabilities? capabilities = null;
        if (!string.IsNullOrWhiteSpace(_settings.ResolvedFfmpegPath))
        {
            capabilities = await _encoderCapabilitiesService.GetCapabilitiesAsync(_settings.ResolvedFfmpegPath);
        }

        foreach (var entry in items)
        {
            var item = entry.item;
            if (_queueCts.IsCancellationRequested)
            {
                item.Status = "Canceled";
                UpdateItemStatus(item);
                continue;
            }

            if (!item.IsAv1)
            {
                item.Status = "Skipped (not AV1)";
                UpdateItemStatus(item);
                AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Skipped (not AV1): {item.FilePath}" });
                continue;
            }

            item.Status = "Running";
            UpdateItemStatus(item);

            var outputPath = GetOutputPath(item);
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                var folder = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            var result = await _transcodeService.RunAsync(
                item,
                _settings.ResolvedFfmpegPath!,
                capabilities!,
                outputPath,
                _settings.EncoderPriority,
                _settings.SpeedQuality,
                AddLog,
                _queueCts.Token);
            if (_queueCts.IsCancellationRequested)
            {
                item.Status = "Canceled";
            }
            else
            {
                item.Status = result.Success ? "Done" : "Failed";
            }

            UpdateItemStatus(item);

            if (result.Success && _settings.UncheckAfterRender && OutputSucceeded(outputPath))
            {
                entry.listItem.Checked = false;
                AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Auto-unchecked after render: {item.FilePath}" });
            }
            else if (!result.Success)
            {
                tabControl.SelectedTab = tabLogs;
                ScrollLogsToEnd();
            }
        }

        _isProcessing = false;
        btnStart.Enabled = true;
        btnCancel.Enabled = false;
    }

    private void UpdateItemStatus(QueueItem item)
    {
        foreach (ListViewItem listItem in listQueue.Items)
        {
            if (ReferenceEquals(listItem.Tag, item))
            {
                EnsureListViewSubItems(listItem);
                listItem.SubItems[5].Text = item.Status;
                break;
            }
        }
    }

    private static bool OutputSucceeded(string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath) || !File.Exists(outputPath))
        {
            return false;
        }

        var info = new FileInfo(outputPath);
        return info.Length > 0;
    }

    private void CancelQueue()
    {
        _queueCts?.Cancel();
        btnCancel.Enabled = false;
    }

    private bool ValidateBeforeStart()
    {
        if (!_ffmpegLocator.IsValid(_settings))
        {
            MessageBox.Show(this, "FFmpeg path is invalid. Configure FFmpeg first.", "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!checkSameAsSource.Checked)
        {
            var globalFolder = textOutputFolder.Text.Trim();
            foreach (ListViewItem listItem in listQueue.Items)
            {
                if (listItem.Tag is not QueueItem item || !item.Render)
                {
                    continue;
                }

                var folder = string.IsNullOrWhiteSpace(item.OutputFolder) ? globalFolder : item.OutputFolder;
                if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                {
                    MessageBox.Show(this, "Output folder is invalid.", "Output", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
        }

        foreach (ListViewItem listItem in listQueue.Items)
        {
            if (listItem.Tag is not QueueItem item || !item.Render)
            {
                continue;
            }

            if (!item.IsAv1)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.PresetName))
            {
                MessageBox.Show(this, "One or more items have no preset assigned.", "Preset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var targetKbps = TranscodeService.ResolveTargetBitrate(item.PresetSnapshot, item.ProbeInfo);
            if (targetKbps <= 0)
            {
                MessageBox.Show(this, "One or more items have an invalid bitrate.", "Bitrate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (IsTrimInvalid(item))
            {
                MessageBox.Show(this, "One or more items have an invalid trim range.", "Trim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        return true;
    }

    private void AddLog(LogEntry entry)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AddLog(entry));
            return;
        }

        _logEntries.Add(entry);
        if (FilterAllows(entry))
        {
            var line = FormatLog(entry);
            richLogs.AppendText(line + Environment.NewLine);
            if (checkAutoScroll.Checked)
            {
                ScrollLogsToEnd();
            }
        }
    }

    private void ScrollLogsToEnd()
    {
        richLogs.SelectionStart = richLogs.TextLength;
        richLogs.ScrollToCaret();
    }

    private string FormatLog(LogEntry entry)
    {
        return $"[{entry.Timestamp:HH:mm:ss}] {entry.Level}: {entry.Message}";
    }

    private bool FilterAllows(LogEntry entry)
    {
        var filter = comboLogFilter.SelectedItem?.ToString() ?? "All";
        return filter switch
        {
            "Errors" => entry.Level == LogLevel.Error,
            "Warnings" => entry.Level == LogLevel.Warning,
            "Commands" => entry.Level == LogLevel.Command,
            _ => true
        };
    }

    private void RefreshLogView()
    {
        richLogs.Clear();
        foreach (var entry in _logEntries)
        {
            if (FilterAllows(entry))
            {
                richLogs.AppendText(FormatLog(entry) + Environment.NewLine);
            }
        }
        ScrollLogsToEnd();
    }

    private void CopySelectedLogs()
    {
        if (!string.IsNullOrWhiteSpace(richLogs.SelectedText))
        {
            Clipboard.SetText(richLogs.SelectedText);
        }
    }

    private void CopyDiagnostics()
    {
        var builder = new StringBuilder();
        builder.AppendLine("AplysiaAv1Transcoder diagnostics");
        builder.AppendLine($"Data folder: {_storage.DataFolder}");
        builder.AppendLine($"FFmpeg: {_settings.ResolvedFfmpegPath}");
        builder.AppendLine("Presets:");
        foreach (var preset in _userPresets)
        {
            var bitrateLabel = preset.BitrateMode == BitrateMode.FixedKbps
                ? $"{preset.BitrateKbps} kbps"
                : $"x{preset.Multiplier:0.00}";
            builder.AppendLine($"- {preset.Name} ({preset.TargetCodec}, {bitrateLabel})");
        }
        builder.AppendLine();
        builder.AppendLine("Logs:");
        foreach (var entry in _logEntries)
        {
            builder.AppendLine(FormatLog(entry));
        }

        Clipboard.SetText(builder.ToString());
    }

    private void ClearLogs()
    {
        _logEntries.Clear();
        richLogs.Clear();
    }

    private void SaveState()
    {
        _settings.LastOutputFolder = textOutputFolder.Text.Trim();
        SaveSplitterDistance();
        _settingsService.Save(_settings);
        SavePresets();
    }

    private void ApplySplitterDistance()
    {
        splitQueue.Panel1MinSize = 320;
        splitQueue.Panel2MinSize = 320;
        var minPanel2 = splitQueue.Panel2MinSize > 0 ? splitQueue.Panel2MinSize : 320;
        var defaultPanel2 = 360;
        var maxDistance = Math.Max(0, splitQueue.Width - minPanel2);
        var defaultDistance = Math.Max(0, splitQueue.Width - defaultPanel2);

        if (_settings.QueueSplitterDistance.HasValue)
        {
            var distance = Math.Clamp(_settings.QueueSplitterDistance.Value, 0, maxDistance);
            splitQueue.SplitterDistance = distance;
        }
        else
        {
            splitQueue.SplitterDistance = Math.Clamp(defaultDistance, 0, maxDistance);
        }
    }

    private void SaveSplitterDistance()
    {
        _settings.QueueSplitterDistance = splitQueue.SplitterDistance;
    }

    private void UpdateStatusIndicators()
    {
        var ffmpegOk = _ffmpegLocator.IsValid(_settings);
        labelFfmpegStatus.Text = ffmpegOk ? "FFmpeg: OK" : "FFmpeg: missing";
        labelFfmpegStatus.ForeColor = ffmpegOk ? Color.DarkGreen : Color.DarkRed;

        var bitrateOk = true;
        var trimOk = true;
        var renderItems = listQueue.Items.Cast<ListViewItem>()
            .Where(item => item.Tag is QueueItem queueItem && queueItem.Render)
            .ToList();
        IEnumerable<ListViewItem> itemsToCheck = renderItems.Count > 0
            ? renderItems
            : listQueue.SelectedItems.Cast<ListViewItem>();
        var outputOk = true;
        if (checkSameAsSource.Checked)
        {
            outputOk = true;
        }
        else
        {
            var globalFolder = textOutputFolder.Text.Trim();
            outputOk = itemsToCheck.All(listItem =>
            {
                if (listItem.Tag is not QueueItem item)
                {
                    return true;
                }

                var folder = string.IsNullOrWhiteSpace(item.OutputFolder) ? globalFolder : item.OutputFolder;
                return !string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder);
            });
        }
        labelOutputStatus.Text = outputOk ? "Output: OK" : "Output: invalid";
        labelOutputStatus.ForeColor = outputOk ? Color.DarkGreen : Color.DarkRed;

        var hasItems = false;
        foreach (var listItem in itemsToCheck)
        {
            hasItems = true;
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            if (item.IsAv1)
            {
                var targetKbps = TranscodeService.ResolveTargetBitrate(item.PresetSnapshot, item.ProbeInfo);
                if (targetKbps <= 0)
                {
                    bitrateOk = false;
                }
            }

            if (item.IsAv1 && IsTrimInvalid(item))
            {
                trimOk = false;
            }
        }

        if (!hasItems && GetActivePreset() is Preset active)
        {
            bitrateOk = active.BitrateMode != BitrateMode.FixedKbps || active.BitrateKbps > 0;
        }

        labelBitrateStatus.Text = bitrateOk ? "Bitrate: OK" : "Bitrate: missing";
        labelBitrateStatus.ForeColor = bitrateOk ? Color.DarkGreen : Color.DarkRed;
        labelTrimStatus.Text = trimOk ? "Trim: OK" : "Trim: invalid";
        labelTrimStatus.ForeColor = trimOk ? Color.DarkGreen : Color.DarkRed;

        btnStart.Enabled = ffmpegOk && outputOk && bitrateOk && trimOk && !_isProcessing;
    }

    private void UpdateFfmpegDownloadStatus(string status)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateFfmpegDownloadStatus(status));
            return;
        }

        labelFfmpegDownloadStatus.Text = status;
    }

    private void UpdateTrimValidation()
    {
        if (listQueue.SelectedItems.Count != 1)
        {
            trimPanel.BackColor = _trimPanelDefaultColor;
            labelTrimError.Visible = false;
            SetTrimTextBoxState(true, true);
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item || !item.TrimEnabled)
        {
            trimPanel.BackColor = _trimPanelDefaultColor;
            labelTrimError.Visible = false;
            SetTrimTextBoxState(true, true);
            return;
        }

        var startValid = TryParseTrimTime(textTrimStart.Text, out var start);
        var endValid = TryParseTrimTime(textTrimEnd.Text, out var end);
        SetTrimTextBoxState(startValid, endValid);

        var invalid = !startValid || !endValid;
        var errorMessage = string.Empty;
        if (invalid)
        {
            errorMessage = "Invalid time format";
        }
        else
        {
            var duration = item.Duration;
            if (duration.HasValue && duration.Value > TimeSpan.Zero)
            {
                if (start > duration.Value || end > duration.Value)
                {
                    invalid = true;
                    errorMessage = "Time exceeds duration";
                }
            }

            if (!invalid && start >= end)
            {
                invalid = true;
                errorMessage = "Start must be < End";
            }
        }

        trimPanel.BackColor = invalid ? Color.MistyRose : _trimPanelDefaultColor;
        labelTrimError.Text = errorMessage;
        labelTrimError.Visible = invalid;
    }

    private static bool IsTrimInvalid(QueueItem item)
    {
        if (!item.TrimEnabled)
        {
            return false;
        }

        if (!TryParseTrimTime(item.TrimStart, out var start) || !TryParseTrimTime(item.TrimEnd, out var end))
        {
            return true;
        }

        if (item.Duration.HasValue && item.Duration.Value > TimeSpan.Zero)
        {
            if (start > item.Duration.Value || end > item.Duration.Value)
            {
                return true;
            }
        }

        return start >= end;
    }

    private ProbeInfo? GetSelectedProbeInfo()
    {
        return listQueue.SelectedItems.Count > 0 ? (listQueue.SelectedItems[0].Tag as QueueItem)?.ProbeInfo : null;
    }

    private void SetTrimTextBoxState(bool startValid, bool endValid)
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        textTrimStart.BackColor = startValid ? _trimTextBoxDefaultColor : Color.MistyRose;
        textTrimEnd.BackColor = endValid ? _trimTextBoxDefaultColor : Color.MistyRose;
    }

    private void UpdateTrimUiFromItem(QueueItem item)
    {
        var durationSeconds = Math.Max(0, item.Duration?.TotalSeconds ?? 0);
        trimTimeline.DurationSeconds = durationSeconds;

        var start = ParseTrimTime(item.TrimStart);
        var end = ParseTrimTime(item.TrimEnd);
        if (durationSeconds > 0 && end == TimeSpan.Zero && !item.TrimWasEditedByUser)
        {
            end = TimeSpan.FromSeconds(durationSeconds);
            item.TrimEnd = FormatTrimTime(end);
        }

        _suppressTrimText = true;
        textTrimStart.Text = FormatTrimTime(start);
        textTrimEnd.Text = FormatTrimTime(end);
        _suppressTrimText = false;

        trimTimeline.StartSeconds = start.TotalSeconds;
        trimTimeline.EndSeconds = end.TotalSeconds;
        trimTimeline.Enabled = checkEnableTrim.Checked && durationSeconds > 0;

        textTrimStart.BackColor = _trimTextBoxDefaultColor;
        textTrimEnd.BackColor = _trimTextBoxDefaultColor;
        labelTrimError.Visible = false;

        if (durationSeconds <= 0)
        {
            labelTrimHint.Text = "Duration unavailable";
            labelTrimHint.Visible = true;
        }
        else
        {
            labelTrimHint.Visible = false;
        }
    }

    private void OnTrimTimelineChanged()
    {
        if (_isUpdatingSelection || listQueue.SelectedItems.Count != 1)
        {
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item)
        {
            return;
        }

        if (!checkEnableTrim.Checked)
        {
            _isUpdatingSelection = true;
            checkEnableTrim.Checked = true;
            _isUpdatingSelection = false;
        }

        var start = TimeSpan.FromSeconds(trimTimeline.StartSeconds);
        var end = TimeSpan.FromSeconds(trimTimeline.EndSeconds);

        item.TrimEnabled = checkEnableTrim.Checked;
        item.TrimStart = FormatTrimTime(start);
        item.TrimEnd = FormatTrimTime(end);
        item.TrimWasEditedByUser = true;

        _suppressTrimText = true;
        textTrimStart.Text = item.TrimStart;
        textTrimEnd.Text = item.TrimEnd;
        _suppressTrimText = false;

        UpdateListViewItem(item, listQueue.SelectedItems[0]);
        AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Trim updated: {item.FilePath} ({item.TrimStart}-{item.TrimEnd})" });
        UpdateTrimValidation();
        UpdateStatusIndicators();
    }

    private void OnTrimTextChanged()
    {
        if (_isUpdatingSelection || _suppressTrimText || listQueue.SelectedItems.Count != 1)
        {
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item || !checkEnableTrim.Checked)
        {
            return;
        }

        var startValid = TryParseTrimTime(textTrimStart.Text, out var start);
        var endValid = TryParseTrimTime(textTrimEnd.Text, out var end);
        SetTrimTextBoxState(startValid, endValid);

        if (!startValid || !endValid)
        {
            labelTrimError.Text = "Invalid time format";
            labelTrimError.Visible = true;
            UpdateTrimValidation();
            return;
        }

        var durationSeconds = item.Duration?.TotalSeconds ?? 0;
        if (durationSeconds > 0)
        {
            var duration = TimeSpan.FromSeconds(durationSeconds);
            if (start > duration || end > duration)
            {
                labelTrimError.Text = "Time exceeds duration";
                labelTrimError.Visible = true;
                UpdateTrimValidation();
                return;
            }
        }

        if (start >= end)
        {
            labelTrimError.Text = "Start must be < End";
            labelTrimError.Visible = true;
            UpdateTrimValidation();
            return;
        }

        labelTrimError.Visible = false;

        item.TrimEnabled = true;
        item.TrimStart = FormatTrimTime(start);
        item.TrimEnd = FormatTrimTime(end);
        item.TrimWasEditedByUser = true;

        if (durationSeconds > 0)
        {
            _isUpdatingSelection = true;
            trimTimeline.DurationSeconds = Math.Max(0, durationSeconds);
            trimTimeline.StartSeconds = start.TotalSeconds;
            trimTimeline.EndSeconds = end.TotalSeconds;
            _isUpdatingSelection = false;
        }

        UpdateListViewItem(item, listQueue.SelectedItems[0]);
        AddLog(new LogEntry { Level = LogLevel.Info, Message = $"Trim updated: {item.FilePath} ({item.TrimStart}-{item.TrimEnd})" });
        UpdateTrimValidation();
        UpdateStatusIndicators();
    }

    private static TimeSpan ParseTrimTime(string? value)
    {
        return TryParseTrimTime(value, out var time) ? time : TimeSpan.Zero;
    }

    private static bool TryParseTrimTime(string? value, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Trim().Split(':');
        if (parts.Length is < 1 or > 3)
        {
            return false;
        }

        var hours = 0;
        var minutes = 0;
        var secondsPart = string.Empty;

        if (parts.Length == 1)
        {
            secondsPart = parts[0];
        }
        else if (parts.Length == 2)
        {
            if (!int.TryParse(parts[0], out minutes))
            {
                return false;
            }
            secondsPart = parts[1];
        }
        else
        {
            if (!int.TryParse(parts[0], out hours) || !int.TryParse(parts[1], out minutes))
            {
                return false;
            }
            secondsPart = parts[2];
        }

        if (hours < 0 || hours > 99 || minutes < 0 || minutes > 59)
        {
            return false;
        }

        var secParts = secondsPart.Split('.');
        if (secParts.Length is < 1 or > 2)
        {
            return false;
        }

        if (!int.TryParse(secParts[0], out var seconds) || seconds < 0 || seconds > 59)
        {
            return false;
        }

        var milliseconds = 0;
        if (secParts.Length == 2 && (!int.TryParse(secParts[1], out milliseconds) || milliseconds < 0 || milliseconds > 999))
        {
            return false;
        }

        time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        return true;
    }

    private static string FormatTrimTime(TimeSpan time)
    {
        var totalHours = (int)Math.Clamp(time.TotalHours, 0, 99);
        return $"{totalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }

    private static string FormatDuration(double seconds)
    {
        return FormatTrimTime(TimeSpan.FromSeconds(seconds));
    }

    private static EncoderPriority? ParsePriority(string? text)
    {
        return text switch
        {
            "Auto(HW)" => EncoderPriority.AutoHW,
            "NVENC" => EncoderPriority.NVENC,
            "Intel QSV" => EncoderPriority.QSV,
            "AMD AMF" => EncoderPriority.AMF,
            "CPU" => EncoderPriority.CPU,
            _ => null
        };
    }

    private static string PriorityLabel(EncoderPriority priority)
    {
        return priority switch
        {
            EncoderPriority.AutoHW => "Auto(HW)",
            EncoderPriority.NVENC => "NVENC",
            EncoderPriority.QSV => "Intel QSV",
            EncoderPriority.AMF => "AMD AMF",
            EncoderPriority.CPU => "CPU",
            _ => "Auto(HW)"
        };
    }
}
