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
    private List<Preset> _builtInPresets;
    private readonly List<LogEntry> _logEntries = new();

    private CancellationTokenSource? _queueCts;
    private bool _isProcessing;
    private bool _isUpdatingSelection;

    public MainForm()
    {
        InitializeComponent();

        _storage = new StorageService();
        _settingsService = new SettingsService(_storage);
        _presetService = new PresetService(_storage);
        _ffmpegLocator = new FfmpegLocator(_storage);
        _ffprobeService = new FfprobeService();
        _encoderCapabilitiesService = new EncoderCapabilitiesService();
        _transcodeService = new TranscodeService();

        _settings = _settingsService.Load();
        _userPresets = _presetService.LoadPresets();
        _builtInPresets = PresetService.GetBuiltInAutoPresets();

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

        checkAutoMatch.CheckedChanged += (_, _) => { _settings.AutoMatchForNewFiles = checkAutoMatch.Checked; UpdateStatusIndicators(); };
        comboDefaultTarget.SelectedIndexChanged += (_, _) => SaveDefaultTarget();

        checkEnableTrim.CheckedChanged += (_, _) => ApplyTrimSettings();
        textTrimStart.TextChanged += (_, _) => ApplyTrimSettings();
        textTrimEnd.TextChanged += (_, _) => ApplyTrimSettings();

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
        comboDefaultTarget.Items.AddRange(new object[] { "H264", "H265" });
        comboPriority.Items.AddRange(new object[] { "Auto(HW)", "NVENC", "Intel QSV", "AMD AMF", "CPU" });

        checkAutoMatch.Checked = _settings.AutoMatchForNewFiles;
        comboDefaultTarget.SelectedItem = _settings.DefaultTargetCodec.ToString();

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

    private void LoadPresetsIntoUi()
    {
        var allPresets = _builtInPresets.Concat(_userPresets).ToList();
        comboActivePreset.DisplayMember = "Name";
        comboActivePreset.Items.Clear();
        foreach (var preset in allPresets)
        {
            comboActivePreset.Items.Add(preset);
        }

        if (!string.IsNullOrWhiteSpace(_settings.LastSelectedPreset))
        {
            var match = allPresets.FirstOrDefault(p => string.Equals(p.Name, _settings.LastSelectedPreset, StringComparison.OrdinalIgnoreCase));
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
        }

        Preset? preset;
        if (checkAutoMatch.Checked)
        {
            var target = _settings.DefaultTargetCodec;
            preset = _builtInPresets.FirstOrDefault(p => p.TargetCodec == target) ?? _builtInPresets.First();
        }
        else
        {
            preset = GetActivePreset();
        }

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
        if (preset.IsBuiltInAuto)
        {
            var bitrate = ComputeAutoBitrate(item.ProbeInfo, preset.TargetCodec);
            snapshot.BitrateKbps = bitrate;
            item.AutoMatchedBitrateKbps = bitrate;
        }
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
        foreach (ListViewItem listItem in listQueue.SelectedItems)
        {
            if (listItem.Tag is not QueueItem item)
            {
                continue;
            }

            firstItem ??= item;
            item.TrimEnabled = checkEnableTrim.Checked;
            item.TrimStart = string.IsNullOrWhiteSpace(textTrimStart.Text) ? null : textTrimStart.Text.Trim();
            item.TrimEnd = string.IsNullOrWhiteSpace(textTrimEnd.Text) ? null : textTrimEnd.Text.Trim();

            if (item.TrimEnabled && string.IsNullOrWhiteSpace(item.TrimEnd) && item.ProbeInfo?.DurationSeconds > 0)
            {
                item.TrimEnd = FormatDuration(item.ProbeInfo.DurationSeconds);
            }

            UpdateListViewItem(item, listItem);
        }

        if (firstItem != null && firstItem.TrimEnabled && !string.IsNullOrWhiteSpace(firstItem.TrimEnd))
        {
            _isUpdatingSelection = true;
            textTrimEnd.Text = firstItem.TrimEnd;
            _isUpdatingSelection = false;
        }

        UpdateTrimInputState();
    }

    private void UpdateTrimInputState()
    {
        textTrimStart.Enabled = checkEnableTrim.Checked;
        textTrimEnd.Enabled = checkEnableTrim.Checked;
    }

    private void UpdateSelectionUi()
    {
        if (listQueue.SelectedItems.Count == 0)
        {
            _isUpdatingSelection = true;
            checkEnableTrim.Checked = false;
            textTrimStart.Text = string.Empty;
            textTrimEnd.Text = string.Empty;
            comboPriority.SelectedIndex = 0;
            _isUpdatingSelection = false;
            UpdateTrimInputState();
            UpdateStatusIndicators();
            return;
        }

        if (listQueue.SelectedItems[0].Tag is not QueueItem item)
        {
            return;
        }

        _isUpdatingSelection = true;
        checkEnableTrim.Checked = item.TrimEnabled;
        textTrimStart.Text = item.TrimStart ?? string.Empty;
        textTrimEnd.Text = item.TrimEnd ?? string.Empty;
        comboPriority.SelectedItem = PriorityLabel(item.PresetSnapshot.EncoderPriority);
        _isUpdatingSelection = false;

        UpdateTrimInputState();
        UpdateStatusIndicators();
    }

    private void OnActivePresetChanged()
    {
        if (comboActivePreset.SelectedItem is Preset preset)
        {
            _settings.LastSelectedPreset = preset.Name;
            if (listQueue.SelectedItems.Count == 0)
            {
                _isUpdatingSelection = true;
                comboPriority.SelectedItem = PriorityLabel(preset.EncoderPriority);
                _isUpdatingSelection = false;
            }
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

        btnEditPreset.Enabled = !preset.IsBuiltInAuto;
        btnDeletePreset.Enabled = !preset.IsBuiltInAuto;
        btnDuplicatePreset.Enabled = true;
    }

    private void CreatePreset()
    {
        var preset = new Preset { Name = "New Preset" };
        using var dialog = new PresetEditorForm(preset, "New Preset");
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
        if (comboActivePreset.SelectedItem is not Preset preset || preset.IsBuiltInAuto)
        {
            return;
        }

        using var dialog = new PresetEditorForm(preset.Clone(), "Edit Preset");
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
        using var dialog = new PresetEditorForm(copy, "Duplicate Preset");
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
        if (comboActivePreset.SelectedItem is not Preset preset || preset.IsBuiltInAuto)
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

    private void SaveDefaultTarget()
    {
        if (comboDefaultTarget.SelectedItem == null)
        {
            return;
        }

        _settings.DefaultTargetCodec = Enum.Parse<TargetCodec>(comboDefaultTarget.SelectedItem.ToString()!, true);
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
        listItem.Text = item.FilePath;
        listItem.SubItems[1].Text = item.PresetName;
        listItem.SubItems[2].Text = item.TrimEnabled
            ? $"{item.TrimStart ?? ""}-{item.TrimEnd ?? ""}".Trim('-')
            : string.Empty;
        listItem.SubItems[3].Text = GetOutputPath(item);
        listItem.SubItems[4].Text = FormatEstimatedSize(item);
        listItem.SubItems[5].Text = item.Status;
    }

    private string FormatEstimatedSize(QueueItem item)
    {
        if (item.ProbeInfo == null || item.PresetSnapshot.BitrateKbps <= 0)
        {
            return string.Empty;
        }

        var sizeKb = item.ProbeInfo.DurationSeconds * (item.PresetSnapshot.BitrateKbps / 8.0);
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

            if (item.PresetSnapshot.BitrateKbps <= 0)
            {
                MessageBox.Show(this, "One or more items have an invalid bitrate.", "Bitrate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        builder.AppendLine($"Auto match: {_settings.AutoMatchForNewFiles}");
        builder.AppendLine($"Default target: {_settings.DefaultTargetCodec}");
        builder.AppendLine("Presets:");
        foreach (var preset in _userPresets)
        {
            builder.AppendLine($"- {preset.Name} ({preset.TargetCodec}, {preset.BitrateKbps} kbps)");
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
        _settings.AutoMatchForNewFiles = checkAutoMatch.Checked;
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
        IEnumerable<ListViewItem> itemsToCheck = listQueue.CheckedItems.Count > 0
            ? listQueue.CheckedItems.Cast<ListViewItem>()
            : listQueue.SelectedItems.Cast<ListViewItem>();

        var hasItems = false;
        foreach (var listItem in itemsToCheck)
        {
            hasItems = true;
            if (listItem.Tag is QueueItem item && item.PresetSnapshot.BitrateKbps <= 0)
            {
                bitrateOk = false;
                break;
            }
        }

        if (!hasItems && GetActivePreset() is Preset active)
        {
            bitrateOk = active.IsBuiltInAuto || active.BitrateKbps > 0;
        }

        labelBitrateStatus.Text = bitrateOk ? "Bitrate: OK" : "Bitrate: missing";
        labelBitrateStatus.ForeColor = bitrateOk ? Color.DarkGreen : Color.DarkRed;

        btnStart.Enabled = ffmpegOk && outputOk && bitrateOk && !_isProcessing;
    }

    private static string FormatDuration(double seconds)
    {
        return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss\.fff");
    }

    private int ComputeAutoBitrate(ProbeInfo? info, TargetCodec target)
    {
        if (info == null)
        {
            return 0;
        }

        var videoKbps = info.VideoBitrateKbps;
        if (videoKbps == null || videoKbps <= 0)
        {
            var audio = info.AudioBitrateKbps ?? 192;
            var overall = info.OverallBitrateKbps ?? 0;
            if (overall > 0)
            {
                videoKbps = Math.Max(0, overall - audio);
            }
        }

        if (videoKbps == null || videoKbps <= 0)
        {
            return 0;
        }

        var multiplier = target == TargetCodec.H264 ? 1.25 : 1.10;
        if (info.Fps >= 60)
        {
            multiplier += 0.05;
        }

        var targetKbps = (int)Math.Round(videoKbps.Value * multiplier);
        var min = target == TargetCodec.H264 ? 4000 : 3000;
        return Math.Clamp(targetKbps, min, 80000);
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
