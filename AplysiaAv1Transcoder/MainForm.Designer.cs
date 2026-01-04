using System.Drawing;
using System.Windows.Forms;

namespace AplysiaAv1Transcoder;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private TabControl tabControl;
    private TabPage tabQueue;
    private TabPage tabLogs;
    private SplitContainer splitQueue;
    private TableLayoutPanel queueLeftLayout;
    private FlowLayoutPanel queueButtonsPanel;
    private Button btnAdd;
    private Button btnRemove;
    private Button btnClear;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private ListView listQueue;
    private FlowLayoutPanel queueBottomPanel;
    private Button btnStart;
    private Button btnCancel;
    private FlowLayoutPanel rightPanel;
    private GroupBox groupPresets;
    private ComboBox comboActivePreset;
    private Button btnApplyPreset;
    private Button btnNewPreset;
    private Button btnEditPreset;
    private Button btnDuplicatePreset;
    private Button btnDeletePreset;
    private GroupBox groupAuto;
    private CheckBox checkAutoMatch;
    private Label labelAutoMatch;
    private ComboBox comboDefaultTarget;
    private GroupBox groupTrim;
    private CheckBox checkEnableTrim;
    private TextBox textTrimStart;
    private TextBox textTrimEnd;
    private Label labelTrimStart;
    private Label labelTrimEnd;
    private GroupBox groupOutput;
    private CheckBox checkSameAsSource;
    private TextBox textOutputFolder;
    private Button btnBrowseOutput;
    private ComboBox comboRecentOutputs;
    private Label labelOutputFolder;
    private Label labelRecentOutputs;
    private GroupBox groupPriority;
    private ComboBox comboPriority;
    private Label labelPriority;
    private Button btnFfmpegSettings;
    private GroupBox groupStatus;
    private Label labelFfmpegStatus;
    private Label labelOutputStatus;
    private Label labelBitrateStatus;

    private TableLayoutPanel logsLayout;
    private FlowLayoutPanel logsTopPanel;
    private Button btnCopySelected;
    private Button btnCopyDiagnostics;
    private Button btnClearLog;
    private ComboBox comboLogFilter;
    private CheckBox checkAutoScroll;
    private RichTextBox richLogs;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tabControl = new TabControl();
        tabQueue = new TabPage();
        tabLogs = new TabPage();
        splitQueue = new SplitContainer();
        queueLeftLayout = new TableLayoutPanel();
        queueButtonsPanel = new FlowLayoutPanel();
        btnAdd = new Button();
        btnRemove = new Button();
        btnClear = new Button();
        btnMoveUp = new Button();
        btnMoveDown = new Button();
        listQueue = new ListView();
        queueBottomPanel = new FlowLayoutPanel();
        btnStart = new Button();
        btnCancel = new Button();

        rightPanel = new FlowLayoutPanel();
        groupPresets = new GroupBox();
        comboActivePreset = new ComboBox();
        btnApplyPreset = new Button();
        btnNewPreset = new Button();
        btnEditPreset = new Button();
        btnDuplicatePreset = new Button();
        btnDeletePreset = new Button();
        groupAuto = new GroupBox();
        checkAutoMatch = new CheckBox();
        labelAutoMatch = new Label();
        comboDefaultTarget = new ComboBox();
        groupTrim = new GroupBox();
        checkEnableTrim = new CheckBox();
        textTrimStart = new TextBox();
        textTrimEnd = new TextBox();
        labelTrimStart = new Label();
        labelTrimEnd = new Label();
        groupOutput = new GroupBox();
        checkSameAsSource = new CheckBox();
        textOutputFolder = new TextBox();
        btnBrowseOutput = new Button();
        comboRecentOutputs = new ComboBox();
        labelOutputFolder = new Label();
        labelRecentOutputs = new Label();
        groupPriority = new GroupBox();
        comboPriority = new ComboBox();
        labelPriority = new Label();
        btnFfmpegSettings = new Button();
        groupStatus = new GroupBox();
        labelFfmpegStatus = new Label();
        labelOutputStatus = new Label();
        labelBitrateStatus = new Label();

        logsLayout = new TableLayoutPanel();
        logsTopPanel = new FlowLayoutPanel();
        btnCopySelected = new Button();
        btnCopyDiagnostics = new Button();
        btnClearLog = new Button();
        comboLogFilter = new ComboBox();
        checkAutoScroll = new CheckBox();
        richLogs = new RichTextBox();

        SuspendLayout();

        tabControl.Dock = DockStyle.Fill;
        tabControl.Controls.Add(tabQueue);
        tabControl.Controls.Add(tabLogs);

        tabQueue.Text = "Queue";
        tabQueue.Padding = new Padding(8);
        tabQueue.Controls.Add(splitQueue);

        splitQueue.Dock = DockStyle.Fill;
        splitQueue.FixedPanel = FixedPanel.Panel2;
        splitQueue.Orientation = Orientation.Vertical;
        splitQueue.Panel1.Controls.Add(queueLeftLayout);
        splitQueue.Panel2.Controls.Add(rightPanel);

        queueLeftLayout.Dock = DockStyle.Fill;
        queueLeftLayout.ColumnCount = 1;
        queueLeftLayout.RowCount = 3;
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        queueLeftLayout.Controls.Add(queueButtonsPanel, 0, 0);
        queueLeftLayout.Controls.Add(listQueue, 0, 1);
        queueLeftLayout.Controls.Add(queueBottomPanel, 0, 2);

        queueButtonsPanel.AutoSize = true;
        queueButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
        queueButtonsPanel.Controls.Add(btnAdd);
        queueButtonsPanel.Controls.Add(btnRemove);
        queueButtonsPanel.Controls.Add(btnClear);
        queueButtonsPanel.Controls.Add(btnMoveUp);
        queueButtonsPanel.Controls.Add(btnMoveDown);

        btnAdd.Text = "Add...";
        btnRemove.Text = "Remove";
        btnClear.Text = "Clear";
        btnMoveUp.Text = "Move Up";
        btnMoveDown.Text = "Move Down";

        listQueue.Dock = DockStyle.Fill;
        listQueue.CheckBoxes = true;
        listQueue.FullRowSelect = true;
        listQueue.MultiSelect = true;
        listQueue.View = View.Details;
        listQueue.HideSelection = false;
        listQueue.AllowDrop = true;
        listQueue.Columns.Add("File", 420);
        listQueue.Columns.Add("Preset", 140);
        listQueue.Columns.Add("Trim", 120);
        listQueue.Columns.Add("Target", 220);
        listQueue.Columns.Add("Est.Size", 90);
        listQueue.Columns.Add("Status", 90);

        queueBottomPanel.AutoSize = true;
        queueBottomPanel.FlowDirection = FlowDirection.LeftToRight;
        queueBottomPanel.Controls.Add(btnStart);
        queueBottomPanel.Controls.Add(btnCancel);

        btnStart.Text = "Start";
        btnCancel.Text = "Cancel";
        btnCancel.Enabled = false;

        rightPanel.Dock = DockStyle.Fill;
        rightPanel.FlowDirection = FlowDirection.TopDown;
        rightPanel.WrapContents = false;
        rightPanel.AutoScroll = true;

        groupPresets.Text = "Presets";
        groupPresets.AutoSize = true;
        groupPresets.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupPresets.Margin = new Padding(8);
        var presetsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(6), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        presetsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        presetsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        presetsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        comboActivePreset.Dock = DockStyle.Fill;
        comboActivePreset.DropDownStyle = ComboBoxStyle.DropDownList;
        btnApplyPreset.Text = "Apply to Selected";
        btnApplyPreset.Dock = DockStyle.Top;
        var presetButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        btnNewPreset.Text = "New";
        btnEditPreset.Text = "Edit";
        btnDuplicatePreset.Text = "Duplicate";
        btnDeletePreset.Text = "Delete";
        presetButtons.Controls.Add(btnNewPreset);
        presetButtons.Controls.Add(btnEditPreset);
        presetButtons.Controls.Add(btnDuplicatePreset);
        presetButtons.Controls.Add(btnDeletePreset);
        presetsLayout.Controls.Add(comboActivePreset, 0, 0);
        presetsLayout.Controls.Add(btnApplyPreset, 0, 1);
        presetsLayout.Controls.Add(presetButtons, 0, 2);
        groupPresets.Controls.Add(presetsLayout);

        groupAuto.Text = "Auto settings";
        groupAuto.AutoSize = true;
        groupAuto.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupAuto.Margin = new Padding(8);
        var autoLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(6), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        autoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        autoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        checkAutoMatch.Text = "Auto match for new files";
        autoLayout.SetColumnSpan(checkAutoMatch, 2);
        labelAutoMatch.Text = "Auto-match bitrate to keep quality";
        labelAutoMatch.AutoSize = true;
        autoLayout.SetColumnSpan(labelAutoMatch, 2);
        autoLayout.Controls.Add(checkAutoMatch, 0, 0);
        autoLayout.Controls.Add(labelAutoMatch, 0, 1);
        autoLayout.Controls.Add(new Label { Text = "Default target", AutoSize = true }, 0, 2);
        comboDefaultTarget.Dock = DockStyle.Fill;
        comboDefaultTarget.DropDownStyle = ComboBoxStyle.DropDownList;
        autoLayout.Controls.Add(comboDefaultTarget, 1, 2);
        groupAuto.Controls.Add(autoLayout);

        groupTrim.Text = "Trim";
        groupTrim.AutoSize = true;
        groupTrim.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupTrim.Margin = new Padding(8);
        var trimLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(6), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        trimLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        trimLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        checkEnableTrim.Text = "Enable trim";
        trimLayout.SetColumnSpan(checkEnableTrim, 2);
        trimLayout.Controls.Add(checkEnableTrim, 0, 0);
        labelTrimStart.Text = "Start";
        labelTrimStart.AutoSize = true;
        textTrimStart.Dock = DockStyle.Fill;
        labelTrimEnd.Text = "End";
        labelTrimEnd.AutoSize = true;
        textTrimEnd.Dock = DockStyle.Fill;
        trimLayout.Controls.Add(labelTrimStart, 0, 1);
        trimLayout.Controls.Add(textTrimStart, 1, 1);
        trimLayout.Controls.Add(labelTrimEnd, 0, 2);
        trimLayout.Controls.Add(textTrimEnd, 1, 2);
        groupTrim.Controls.Add(trimLayout);

        groupOutput.Text = "Output";
        groupOutput.AutoSize = true;
        groupOutput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupOutput.Margin = new Padding(8);
        var outputLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 4, Padding = new Padding(6), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        checkSameAsSource.Text = "Same as source";
        outputLayout.SetColumnSpan(checkSameAsSource, 3);
        labelOutputFolder.Text = "Output folder";
        labelOutputFolder.AutoSize = true;
        textOutputFolder.Dock = DockStyle.Fill;
        btnBrowseOutput.Text = "Browse";
        labelRecentOutputs.Text = "Recent output folders";
        labelRecentOutputs.AutoSize = true;
        comboRecentOutputs.Dock = DockStyle.Fill;
        comboRecentOutputs.DropDownStyle = ComboBoxStyle.DropDownList;
        outputLayout.Controls.Add(checkSameAsSource, 0, 0);
        outputLayout.Controls.Add(labelOutputFolder, 0, 1);
        outputLayout.Controls.Add(textOutputFolder, 1, 1);
        outputLayout.Controls.Add(btnBrowseOutput, 2, 1);
        outputLayout.Controls.Add(labelRecentOutputs, 0, 2);
        outputLayout.Controls.Add(comboRecentOutputs, 1, 2);
        outputLayout.SetColumnSpan(comboRecentOutputs, 2);
        groupOutput.Controls.Add(outputLayout);

        groupPriority.Text = "Encoder priority";
        groupPriority.AutoSize = true;
        groupPriority.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupPriority.Margin = new Padding(8);
        var priorityLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(6), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        priorityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        priorityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        labelPriority.Text = "Priority";
        labelPriority.AutoSize = true;
        comboPriority.Dock = DockStyle.Fill;
        comboPriority.DropDownStyle = ComboBoxStyle.DropDownList;
        btnFfmpegSettings.Text = "FFmpeg...";
        btnFfmpegSettings.Dock = DockStyle.Fill;
        priorityLayout.Controls.Add(labelPriority, 0, 0);
        priorityLayout.Controls.Add(comboPriority, 1, 0);
        priorityLayout.Controls.Add(btnFfmpegSettings, 1, 1);
        groupPriority.Controls.Add(priorityLayout);

        groupStatus.Text = "Status";
        groupStatus.AutoSize = true;
        groupStatus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupStatus.Margin = new Padding(8);
        var statusLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(6), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        labelFfmpegStatus.AutoSize = true;
        labelOutputStatus.AutoSize = true;
        labelBitrateStatus.AutoSize = true;
        statusLayout.Controls.Add(labelFfmpegStatus, 0, 0);
        statusLayout.Controls.Add(labelOutputStatus, 0, 1);
        statusLayout.Controls.Add(labelBitrateStatus, 0, 2);
        groupStatus.Controls.Add(statusLayout);

        rightPanel.Controls.Add(groupPresets);
        rightPanel.Controls.Add(groupAuto);
        rightPanel.Controls.Add(groupTrim);
        rightPanel.Controls.Add(groupOutput);
        rightPanel.Controls.Add(groupPriority);
        rightPanel.Controls.Add(groupStatus);

        tabLogs.Text = "Logs";
        tabLogs.Padding = new Padding(8);
        tabLogs.Controls.Add(logsLayout);

        logsLayout.Dock = DockStyle.Fill;
        logsLayout.ColumnCount = 1;
        logsLayout.RowCount = 2;
        logsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        logsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        logsLayout.Controls.Add(logsTopPanel, 0, 0);
        logsLayout.Controls.Add(richLogs, 0, 1);

        logsTopPanel.AutoSize = true;
        logsTopPanel.FlowDirection = FlowDirection.LeftToRight;
        btnCopySelected.Text = "Copy Selected";
        btnCopyDiagnostics.Text = "Copy Diagnostics";
        btnClearLog.Text = "Clear";
        comboLogFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        comboLogFilter.Items.AddRange(new object[] { "All", "Errors", "Warnings", "Commands" });
        comboLogFilter.SelectedIndex = 0;
        checkAutoScroll.Text = "Auto-scroll";
        checkAutoScroll.Checked = true;
        logsTopPanel.Controls.Add(btnCopySelected);
        logsTopPanel.Controls.Add(btnCopyDiagnostics);
        logsTopPanel.Controls.Add(btnClearLog);
        logsTopPanel.Controls.Add(new Label { Text = "Filter:", AutoSize = true, Padding = new Padding(8, 6, 0, 0) });
        logsTopPanel.Controls.Add(comboLogFilter);
        logsTopPanel.Controls.Add(checkAutoScroll);

        richLogs.Dock = DockStyle.Fill;
        richLogs.ReadOnly = true;
        richLogs.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
        richLogs.WordWrap = false;

        Controls.Add(tabControl);
        Text = "Aplysia AV1 Transcoder";
        Width = 1100;
        Height = 700;

        ResumeLayout(false);
    }
}
