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
    private readonly EncoderCapabilitiesService _encoderCapabilitiesService;
    private readonly TranscodeService _transcodeService;

    private AppSettings _settings;
    private List<Preset> _userPresets;
    private readonly List<LogEntry> _logEntries = new();

    private CancellationTokenSource? _queueCts;
    private bool _isProcessing;
    private bool _isUpdatingSelection;
    private Color _trimPanelDefaultColor;

    public MainForm()
    {
        InitializeComponent();

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

        WireEvents();
        InitializeUiFromSettings();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ApplySplitterDistance();
        var resolved = await _ffmpegLocator.ResolveAsync(_settings, this);
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

        btnApplyPreset.Click += (_, _) => ApplyPresetToSelected();
        btnNewPreset.Click += (_, _) => CreatePreset();
        btnEditPreset.Click += (_, _) => EditPreset();
        btnDuplicatePreset.Click += (_, _) => DuplicatePreset();
        btnDeletePreset.Click += (_, _) => DeletePreset();
        comboActivePreset.SelectedIndexChanged += (_, _) => OnActivePresetChanged();

        checkEnableTrim.CheckedChanged += (_, _) => ApplyTrimSettings();
        numTrimStartHours.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimStartMinutes.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimStartSeconds.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimStartMilliseconds.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimEndHours.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimEndMinutes.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimEndSeconds.ValueChanged += (_, _) => ApplyTrimSettings();
        numTrimEndMilliseconds.ValueChanged += (_, _) => ApplyTrimSettings();

        checkSameAsSource.CheckedChanged += (_, _) => OnOutputModeChanged();
        textOutputFolder.TextChanged += (_, _) => OnOutputFolderChanged();
        btnBrowseOutput.Click += (_, _) => BrowseOutputFolder();
        comboRecentOutputs.SelectedIndexChanged += (_, _) => SelectRecentOutputFolder();

        comboPriority.SelectedIndexChanged += (_, _) => ApplyPriorityToSelected();
        btnFfmpegSettings.Click += (_, _) => OpenFfmpegSettings();

        listQueue.SelectedIndexChanged += (_, _) => UpdateSelectionUi();
        listQueue.ItemChecked += (_, _) => UpdateStatusIndicators();
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

        comboRecentOutputs.Items.Clear();
        foreach (var path in _settings.RecentOutputFolders)
        {
            comboRecentOutputs.Items.Add(path);
        }

        LoadPresetsIntoUi();
        comboPriority.SelectedIndex = 0;

        UpdateTrimInputState();
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
                Status = "Queued"
            };

            var listItem = new ListViewItem(file);
            EnsureListViewSubItems(listItem);
            listItem.Tag = item;
            listItem.Checked = true;
            listQueue.Items.Add(listItem);

            await ProbeAndApplyPresetAsync(item);
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

    private void ApplyPreset(QueueItem item, Preset preset)
    {
        item.PresetName = preset.Name;
        var snapshot = preset.Clone();
        snapshot.EncoderPriority = ParsePriority(comboPriority.SelectedItem?.ToString()) ?? EncoderPriority.AutoHW;
        item.PresetSnapshot = snapshot;
    }

    private void ApplyPresetToSelected()
    {
        var preset = GetActivePreset();
        if (preset == null)
        {
            return;
        }

        foreach (ListViewItem listItem in listQueue.SelectedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            ApplyPreset(item, preset);
            UpdateListViewItem(item, listItem);
        }

        UpdateStatusIndicators();
    }

    private void ApplyPriorityToSelected()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        var priority = ParsePriority(comboPriority.SelectedItem?.ToString());
        if (priority == null)
        {
            return;
        }

        foreach (ListViewItem listItem in listQueue.SelectedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            item.PresetSnapshot.EncoderPriority = priority.Value;
            UpdateListViewItem(item, listItem);
        }
    }

    private void ApplyTrimSettings()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        QueueItem? firstItem = null;
        var trimStart = FormatTrimTime(GetTrimStartTime());
        var trimEnd = FormatTrimTime(GetTrimEndTime());

        if (checkEnableTrim.Checked && trimEnd == FormatTrimTime(TimeSpan.Zero))
        {
            var selectedItem = listQueue.SelectedItems.Count > 0 ? listQueue.SelectedItems[0].Tag as QueueItem : null;
            if (selectedItem?.ProbeInfo?.DurationSeconds > 0)
            {
                trimEnd = FormatDuration(selectedItem.ProbeInfo.DurationSeconds);
                _isUpdatingSelection = true;
                SetTrimEndControls(ParseTrimTime(trimEnd));
                _isUpdatingSelection = false;
            }
        }

        foreach (ListViewItem listItem in listQueue.SelectedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            firstItem ??= item;
            item.TrimEnabled = checkEnableTrim.Checked;
            item.TrimStart = trimStart;
            item.TrimEnd = trimEnd;

            if (item.TrimEnabled && item.ProbeInfo?.DurationSeconds > 0 && string.IsNullOrWhiteSpace(item.TrimEnd))
            {
                item.TrimEnd = FormatDuration(item.ProbeInfo.DurationSeconds);
            }

            UpdateListViewItem(item, listItem);
        }

        if (firstItem != null && firstItem.TrimEnabled && !string.IsNullOrWhiteSpace(firstItem.TrimEnd))
        {
            _isUpdatingSelection = true;
            SetTrimEndControls(ParseTrimTime(firstItem.TrimEnd));
            _isUpdatingSelection = false;
        }

        UpdateTrimInputState();
        UpdateTrimValidation();
        UpdateStatusIndicators();
    }

    private void UpdateTrimInputState()
    {
        var enabled = checkEnableTrim.Checked;
        numTrimStartHours.Enabled = enabled;
        numTrimStartMinutes.Enabled = enabled;
        numTrimStartSeconds.Enabled = enabled;
        numTrimStartMilliseconds.Enabled = enabled;
        numTrimEndHours.Enabled = enabled;
        numTrimEndMinutes.Enabled = enabled;
        numTrimEndSeconds.Enabled = enabled;
        numTrimEndMilliseconds.Enabled = enabled;
    }


    private void UpdateSelectionUi()
    {
        if (listQueue.SelectedItems.Count == 0)
        {
            _isUpdatingSelection = true;
            checkEnableTrim.Checked = false;
            comboPriority.SelectedIndex = 0;
            SetTrimStartControls(TimeSpan.Zero);
            SetTrimEndControls(TimeSpan.Zero);
            _isUpdatingSelection = false;
            UpdateTrimInputState();
            UpdateTrimValidation();
            UpdateStatusIndicators();
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item)
        {
            return;
        }

        _isUpdatingSelection = true;
        checkEnableTrim.Checked = item.TrimEnabled;
        SetTrimStartControls(ParseTrimTime(item.TrimStart));
        if (item.TrimEnabled && string.IsNullOrWhiteSpace(item.TrimEnd) && item.ProbeInfo?.DurationSeconds > 0)
        {
            item.TrimEnd = FormatDuration(item.ProbeInfo.DurationSeconds);
        }
        SetTrimEndControls(ParseTrimTime(item.TrimEnd));
        comboPriority.SelectedItem = PriorityLabel(item.PresetSnapshot.EncoderPriority);
        _isUpdatingSelection = false;

        UpdateTrimInputState();
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
        var preset = new Preset { Name = "New Preset" };
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
        listItem.SubItems[1].Text = item.PresetName;
        listItem.SubItems[2].Text = item.TrimEnabled
            ? $"{item.TrimStart ?? ""}-{item.TrimEnd ?? ""}".Trim('-')
            : string.Empty;
        listItem.SubItems[3].Text = GetOutputPath(item);
        listItem.SubItems[4].Text = FormatEstimatedSize(item);
        listItem.SubItems[5].Text = item.Status;
        listItem.ForeColor = item.IsAv1 ? listQueue.ForeColor : Color.Red;
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

        var items = listQueue.CheckedItems.Cast<ListViewItem>()
            .Select(i => i.Tag)
            .OfType<QueueItem>()
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

        foreach (var item in items)
        {
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

            var result = await _transcodeService.RunAsync(item, _settings.ResolvedFfmpegPath!, capabilities!, outputPath, AddLog, _queueCts.Token);
            if (_queueCts.IsCancellationRequested)
            {
                item.Status = "Canceled";
            }
            else
            {
                item.Status = result.Success ? "Done" : "Failed";
            }

            UpdateItemStatus(item);

            if (!result.Success)
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
            var folder = textOutputFolder.Text.Trim();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show(this, "Output folder is invalid.", "Output", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        foreach (ListViewItem listItem in listQueue.CheckedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            if (!item.IsAv1)
            {
                continue;
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

        var outputOk = checkSameAsSource.Checked || (!string.IsNullOrWhiteSpace(textOutputFolder.Text) && Directory.Exists(textOutputFolder.Text));
        labelOutputStatus.Text = outputOk ? "Output: OK" : "Output: invalid";
        labelOutputStatus.ForeColor = outputOk ? Color.DarkGreen : Color.DarkRed;

        var bitrateOk = true;
        var trimOk = true;
        IEnumerable<ListViewItem> itemsToCheck = listQueue.CheckedItems.Count > 0
            ? listQueue.CheckedItems.Cast<ListViewItem>()
            : listQueue.SelectedItems.Cast<ListViewItem>();

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

    private void UpdateTrimValidation()
    {
        var invalid = checkEnableTrim.Checked && GetTrimStartTime() > GetTrimEndTime();
        trimPanel.BackColor = invalid ? Color.MistyRose : _trimPanelDefaultColor;
    }

    private static bool IsTrimInvalid(QueueItem item)
    {
        if (!item.TrimEnabled)
        {
            return false;
        }

        if (!TryParseTrimTime(item.TrimStart, out var start) || !TryParseTrimTime(item.TrimEnd, out var end))
        {
            return false;
        }

        return start > end;
    }

    private ProbeInfo? GetSelectedProbeInfo()
    {
        return listQueue.SelectedItems.Count > 0 ? (listQueue.SelectedItems[0].Tag as QueueItem)?.ProbeInfo : null;
    }

    private TimeSpan GetTrimStartTime()
    {
        return BuildTrimTime(numTrimStartHours, numTrimStartMinutes, numTrimStartSeconds, numTrimStartMilliseconds);
    }

    private TimeSpan GetTrimEndTime()
    {
        return BuildTrimTime(numTrimEndHours, numTrimEndMinutes, numTrimEndSeconds, numTrimEndMilliseconds);
    }

    private static TimeSpan BuildTrimTime(NumericUpDown hours, NumericUpDown minutes, NumericUpDown seconds, NumericUpDown milliseconds)
    {
        var totalMilliseconds = (((int)hours.Value * 60 + (int)minutes.Value) * 60 + (int)seconds.Value) * 1000 + (int)milliseconds.Value;
        return TimeSpan.FromMilliseconds(totalMilliseconds);
    }

    private void SetTrimStartControls(TimeSpan time)
    {
        SetTrimControls(time, numTrimStartHours, numTrimStartMinutes, numTrimStartSeconds, numTrimStartMilliseconds);
    }

    private void SetTrimEndControls(TimeSpan time)
    {
        SetTrimControls(time, numTrimEndHours, numTrimEndMinutes, numTrimEndSeconds, numTrimEndMilliseconds);
    }

    private static void SetTrimControls(TimeSpan time, NumericUpDown hours, NumericUpDown minutes, NumericUpDown seconds, NumericUpDown milliseconds)
    {
        var totalHours = (int)Math.Clamp(time.TotalHours, 0, 99);
        hours.Value = totalHours;
        minutes.Value = time.Minutes;
        seconds.Value = time.Seconds;
        milliseconds.Value = time.Milliseconds;
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

        var parts = value.Split(':');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var hours) || hours < 0 || hours > 99)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var minutes) || minutes < 0 || minutes > 59)
        {
            return false;
        }

        var secParts = parts[2].Split('.');
        if (secParts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(secParts[0], out var seconds) || seconds < 0 || seconds > 59)
        {
            return false;
        }

        if (!int.TryParse(secParts[1], out var milliseconds) || milliseconds < 0 || milliseconds > 999)
        {
            return false;
        }

        var totalMilliseconds = (((hours * 60) + minutes) * 60 + seconds) * 1000 + milliseconds;
        time = TimeSpan.FromMilliseconds(totalMilliseconds);
        return true;
    }

    private static string FormatTrimTime(TimeSpan time)
    {
        var totalHours = (int)Math.Clamp(time.TotalHours, 0, 99);
        return $"{totalHours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
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
