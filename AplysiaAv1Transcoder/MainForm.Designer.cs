using System.Drawing;
using System.Windows.Forms;

namespace AplysiaAv1Transcoder;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private TabControl tabControl;
    private TabPage tabQueue;
    private TabPage tabLogs;
    private Panel queueRootPanel;
    private Panel logsRootPanel;
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
    private CheckBox checkUncheckAfterRender;
    private Panel rightPanelHost;
    private TableLayoutPanel rightLayout;
    private GroupBox groupPresets;
    private ComboBox comboActivePreset;
    private Button btnApplyPreset;
    private Button btnNewPreset;
    private Button btnEditPreset;
    private Button btnDuplicatePreset;
    private Button btnDeletePreset;
    private GroupBox groupTrim;
    private CheckBox checkEnableTrim;
    private Panel trimPanel;
    private TrimTimelineControl trimTimeline;
    private Label labelTrimStart;
    private Label labelTrimEnd;
    private TextBox textTrimStart;
    private TextBox textTrimEnd;
    private Label labelTrimError;
    private Label labelTrimHint;
    private Button btnResetTrim;
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
    private Label labelSpeedQuality;
    private TrackBar trackSpeedQuality;
    private Button btnFfmpegSettings;
    private GroupBox groupStatus;
    private Label labelFfmpegStatus;
    private Label labelOutputStatus;
    private Label labelBitrateStatus;
    private Label labelTrimStatus;
    private Label labelFfmpegDownloadStatus;

    private TableLayoutPanel logsLayout;
    private FlowLayoutPanel logsTopPanel;
    private Button btnCopySelected;
    private Button btnCopyDiagnostics;
    private Button btnClearLog;
    private Label labelLogFilter;
    private ComboBox comboLogFilter;
    private CheckBox checkAutoScroll;
    private RichTextBox richLogs;
    private ToolTip toolTip;
    private NotifyIcon notifyIcon;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        tabControl = new TabControl();
        tabQueue = new TabPage();
        queueRootPanel = new Panel();
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
        checkUncheckAfterRender = new CheckBox();
        rightPanelHost = new Panel();
        rightLayout = new TableLayoutPanel();
        groupPresets = new GroupBox();
        presetsLayout = new TableLayoutPanel();
        comboActivePreset = new ComboBox();
        btnApplyPreset = new Button();
        presetButtons = new FlowLayoutPanel();
        btnNewPreset = new Button();
        btnEditPreset = new Button();
        btnDuplicatePreset = new Button();
        btnDeletePreset = new Button();
        groupTrim = new GroupBox();
        trimPanel = new Panel();
        trimLayout = new TableLayoutPanel();
        checkEnableTrim = new CheckBox();
        labelTrimHint = new Label();
        trimTimeline = new TrimTimelineControl();
        labelTrimStart = new Label();
        textTrimStart = new TextBox();
        labelTrimEnd = new Label();
        textTrimEnd = new TextBox();
        labelTrimError = new Label();
        btnResetTrim = new Button();
        groupOutput = new GroupBox();
        outputLayout = new TableLayoutPanel();
        checkSameAsSource = new CheckBox();
        labelOutputFolder = new Label();
        textOutputFolder = new TextBox();
        btnBrowseOutput = new Button();
        labelRecentOutputs = new Label();
        comboRecentOutputs = new ComboBox();
        groupPriority = new GroupBox();
        priorityLayout = new TableLayoutPanel();
        labelPriority = new Label();
        comboPriority = new ComboBox();
        labelSpeedQuality = new Label();
        trackSpeedQuality = new TrackBar();
        btnFfmpegSettings = new Button();
        groupStatus = new GroupBox();
        statusLayout = new TableLayoutPanel();
        labelFfmpegStatus = new Label();
        labelOutputStatus = new Label();
        labelBitrateStatus = new Label();
        labelTrimStatus = new Label();
        labelFfmpegDownloadStatus = new Label();
        tabLogs = new TabPage();
        logsRootPanel = new Panel();
        logsLayout = new TableLayoutPanel();
        logsTopPanel = new FlowLayoutPanel();
        btnCopySelected = new Button();
        btnCopyDiagnostics = new Button();
        btnClearLog = new Button();
        labelLogFilter = new Label();
        comboLogFilter = new ComboBox();
        checkAutoScroll = new CheckBox();
        richLogs = new RichTextBox();
        toolTip = new ToolTip(components);
        notifyIcon = new NotifyIcon(components);
        tabControl.SuspendLayout();
        tabQueue.SuspendLayout();
        queueRootPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitQueue).BeginInit();
        splitQueue.Panel1.SuspendLayout();
        splitQueue.Panel2.SuspendLayout();
        splitQueue.SuspendLayout();
        queueLeftLayout.SuspendLayout();
        queueButtonsPanel.SuspendLayout();
        queueBottomPanel.SuspendLayout();
        rightPanelHost.SuspendLayout();
        rightLayout.SuspendLayout();
        groupPresets.SuspendLayout();
        presetsLayout.SuspendLayout();
        presetButtons.SuspendLayout();
        groupTrim.SuspendLayout();
        trimPanel.SuspendLayout();
        trimLayout.SuspendLayout();
        groupOutput.SuspendLayout();
        outputLayout.SuspendLayout();
        groupPriority.SuspendLayout();
        priorityLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)trackSpeedQuality).BeginInit();
        groupStatus.SuspendLayout();
        statusLayout.SuspendLayout();
        tabLogs.SuspendLayout();
        logsRootPanel.SuspendLayout();
        logsLayout.SuspendLayout();
        logsTopPanel.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl
        // 
        tabControl.Controls.Add(tabQueue);
        tabControl.Controls.Add(tabLogs);
        tabControl.Dock = DockStyle.Fill;
        tabControl.Location = new Point(0, 0);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(1084, 661);
        tabControl.TabIndex = 0;
        // 
        // tabQueue
        // 
        tabQueue.Controls.Add(queueRootPanel);
        tabQueue.Location = new Point(4, 24);
        tabQueue.Name = "tabQueue";
        tabQueue.Padding = new Padding(8);
        tabQueue.Size = new Size(1076, 633);
        tabQueue.TabIndex = 0;
        tabQueue.Text = "Queue";
        // 
        // queueRootPanel
        // 
        queueRootPanel.Controls.Add(splitQueue);
        queueRootPanel.Dock = DockStyle.Fill;
        queueRootPanel.Location = new Point(8, 8);
        queueRootPanel.Name = "queueRootPanel";
        queueRootPanel.Size = new Size(1060, 617);
        queueRootPanel.TabIndex = 0;
        // 
        // splitQueue
        // 
        splitQueue.Dock = DockStyle.Fill;
        splitQueue.FixedPanel = FixedPanel.Panel2;
        splitQueue.Location = new Point(0, 0);
        splitQueue.Name = "splitQueue";
        // 
        // splitQueue.Panel1
        // 
        splitQueue.Panel1.Controls.Add(queueLeftLayout);
        splitQueue.Panel1MinSize = 320;
        // 
        // splitQueue.Panel2
        // 
        splitQueue.Panel2.Controls.Add(rightPanelHost);
        splitQueue.Panel2MinSize = 320;
        splitQueue.Size = new Size(1060, 617);
        splitQueue.SplitterDistance = 320;
        splitQueue.TabIndex = 0;
        // 
        // queueLeftLayout
        // 
        queueLeftLayout.ColumnCount = 1;
        queueLeftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        queueLeftLayout.Controls.Add(queueButtonsPanel, 0, 0);
        queueLeftLayout.Controls.Add(listQueue, 0, 1);
        queueLeftLayout.Controls.Add(queueBottomPanel, 0, 2);
        queueLeftLayout.Dock = DockStyle.Fill;
        queueLeftLayout.Location = new Point(0, 0);
        queueLeftLayout.Name = "queueLeftLayout";
        queueLeftLayout.RowCount = 3;
        queueLeftLayout.RowStyles.Add(new RowStyle());
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        queueLeftLayout.RowStyles.Add(new RowStyle());
        queueLeftLayout.Size = new Size(320, 617);
        queueLeftLayout.TabIndex = 0;
        // 
        // queueButtonsPanel
        // 
        queueButtonsPanel.AutoSize = true;
        queueButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        queueButtonsPanel.Controls.Add(btnAdd);
        queueButtonsPanel.Controls.Add(btnRemove);
        queueButtonsPanel.Controls.Add(btnClear);
        queueButtonsPanel.Controls.Add(btnMoveUp);
        queueButtonsPanel.Controls.Add(btnMoveDown);
        queueButtonsPanel.Dock = DockStyle.Fill;
        queueButtonsPanel.Location = new Point(0, 0);
        queueButtonsPanel.Margin = new Padding(0, 0, 0, 6);
        queueButtonsPanel.Name = "queueButtonsPanel";
        queueButtonsPanel.Size = new Size(320, 29);
        queueButtonsPanel.TabIndex = 0;
        queueButtonsPanel.WrapContents = false;
        // 
        // btnAdd
        // 
        btnAdd.Location = new Point(3, 3);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(75, 23);
        btnAdd.TabIndex = 0;
        btnAdd.Text = "Add...";
        // 
        // btnRemove
        // 
        btnRemove.Location = new Point(84, 3);
        btnRemove.Name = "btnRemove";
        btnRemove.Size = new Size(75, 23);
        btnRemove.TabIndex = 1;
        btnRemove.Text = "Remove";
        // 
        // btnClear
        // 
        btnClear.Location = new Point(165, 3);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(75, 23);
        btnClear.TabIndex = 2;
        btnClear.Text = "Clear";
        // 
        // btnMoveUp
        // 
        btnMoveUp.Location = new Point(246, 3);
        btnMoveUp.Name = "btnMoveUp";
        btnMoveUp.Size = new Size(75, 23);
        btnMoveUp.TabIndex = 3;
        btnMoveUp.Text = "Move Up";
        // 
        // btnMoveDown
        // 
        btnMoveDown.Location = new Point(327, 3);
        btnMoveDown.Name = "btnMoveDown";
        btnMoveDown.Size = new Size(75, 23);
        btnMoveDown.TabIndex = 4;
        btnMoveDown.Text = "Move Down";
        // 
        // listQueue
        // 
        listQueue.AllowDrop = true;
        listQueue.CheckBoxes = true;
        listQueue.Dock = DockStyle.Fill;
        listQueue.FullRowSelect = true;
        listQueue.Location = new Point(3, 38);
        listQueue.Name = "listQueue";
        listQueue.Size = new Size(314, 541);
        listQueue.TabIndex = 1;
        listQueue.UseCompatibleStateImageBehavior = false;
        listQueue.View = View.Details;
        // 
        // queueBottomPanel
        // 
        queueBottomPanel.AutoSize = true;
        queueBottomPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        queueBottomPanel.Controls.Add(btnStart);
        queueBottomPanel.Controls.Add(btnCancel);
        queueBottomPanel.Controls.Add(checkUncheckAfterRender);
        queueBottomPanel.Dock = DockStyle.Fill;
        queueBottomPanel.Location = new Point(0, 588);
        queueBottomPanel.Margin = new Padding(0, 6, 0, 0);
        queueBottomPanel.Name = "queueBottomPanel";
        queueBottomPanel.Size = new Size(320, 29);
        queueBottomPanel.TabIndex = 2;
        queueBottomPanel.WrapContents = false;
        // 
        // btnStart
        // 
        btnStart.Location = new Point(3, 3);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(75, 23);
        btnStart.TabIndex = 0;
        btnStart.Text = "Start";
        // 
        // btnCancel
        // 
        btnCancel.Enabled = false;
        btnCancel.Location = new Point(84, 3);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 23);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        // 
        // checkUncheckAfterRender
        // 
        checkUncheckAfterRender.AutoSize = true;
        checkUncheckAfterRender.Location = new Point(174, 6);
        checkUncheckAfterRender.Margin = new Padding(12, 6, 3, 3);
        checkUncheckAfterRender.Name = "checkUncheckAfterRender";
        checkUncheckAfterRender.Size = new Size(225, 19);
        checkUncheckAfterRender.TabIndex = 2;
        checkUncheckAfterRender.Text = "Uncheck items after successful render";
        checkUncheckAfterRender.UseVisualStyleBackColor = true;
        // 
        // rightPanelHost
        // 
        rightPanelHost.AutoScroll = true;
        rightPanelHost.Controls.Add(rightLayout);
        rightPanelHost.Dock = DockStyle.Fill;
        rightPanelHost.Location = new Point(0, 0);
        rightPanelHost.Name = "rightPanelHost";
        rightPanelHost.Padding = new Padding(8);
        rightPanelHost.Size = new Size(736, 617);
        rightPanelHost.TabIndex = 0;
        // 
        // rightLayout
        // 
        rightLayout.AutoSize = true;
        rightLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        rightLayout.ColumnCount = 1;
        rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightLayout.Controls.Add(groupPresets, 0, 0);
        rightLayout.Controls.Add(groupTrim, 0, 1);
        rightLayout.Controls.Add(groupOutput, 0, 2);
        rightLayout.Controls.Add(groupPriority, 0, 3);
        rightLayout.Controls.Add(groupStatus, 0, 4);
        rightLayout.Dock = DockStyle.Top;
        rightLayout.Location = new Point(8, 8);
        rightLayout.Name = "rightLayout";
        rightLayout.RowCount = 5;
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.Size = new Size(703, 687);
        rightLayout.TabIndex = 0;
        // 
        // groupPresets
        // 
        groupPresets.AutoSize = true;
        groupPresets.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupPresets.Controls.Add(presetsLayout);
        groupPresets.Dock = DockStyle.Fill;
        groupPresets.Location = new Point(0, 0);
        groupPresets.Margin = new Padding(0, 0, 0, 8);
        groupPresets.Name = "groupPresets";
        groupPresets.Size = new Size(703, 117);
        groupPresets.TabIndex = 0;
        groupPresets.TabStop = false;
        groupPresets.Text = "Presets";
        // 
        // presetsLayout
        // 
        presetsLayout.AutoSize = true;
        presetsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        presetsLayout.ColumnCount = 1;
        presetsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        presetsLayout.Controls.Add(comboActivePreset, 0, 0);
        presetsLayout.Controls.Add(btnApplyPreset, 0, 1);
        presetsLayout.Controls.Add(presetButtons, 0, 2);
        presetsLayout.Dock = DockStyle.Fill;
        presetsLayout.Location = new Point(3, 19);
        presetsLayout.Name = "presetsLayout";
        presetsLayout.RowCount = 3;
        presetsLayout.RowStyles.Add(new RowStyle());
        presetsLayout.RowStyles.Add(new RowStyle());
        presetsLayout.RowStyles.Add(new RowStyle());
        presetsLayout.Size = new Size(697, 95);
        presetsLayout.TabIndex = 0;
        // 
        // comboActivePreset
        // 
        comboActivePreset.Dock = DockStyle.Fill;
        comboActivePreset.DropDownStyle = ComboBoxStyle.DropDownList;
        comboActivePreset.Location = new Point(3, 3);
        comboActivePreset.Name = "comboActivePreset";
        comboActivePreset.Size = new Size(691, 23);
        comboActivePreset.TabIndex = 0;
        // 
        // btnApplyPreset
        // 
        btnApplyPreset.AutoSize = true;
        btnApplyPreset.Dock = DockStyle.Top;
        btnApplyPreset.Location = new Point(3, 32);
        btnApplyPreset.Name = "btnApplyPreset";
        btnApplyPreset.Size = new Size(691, 25);
        btnApplyPreset.TabIndex = 1;
        btnApplyPreset.Text = "Apply to Selected";
        // 
        // presetButtons
        // 
        presetButtons.AutoSize = true;
        presetButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        presetButtons.Controls.Add(btnNewPreset);
        presetButtons.Controls.Add(btnEditPreset);
        presetButtons.Controls.Add(btnDuplicatePreset);
        presetButtons.Controls.Add(btnDeletePreset);
        presetButtons.Location = new Point(3, 63);
        presetButtons.Name = "presetButtons";
        presetButtons.Size = new Size(324, 29);
        presetButtons.TabIndex = 2;
        // 
        // btnNewPreset
        // 
        btnNewPreset.Location = new Point(3, 3);
        btnNewPreset.Name = "btnNewPreset";
        btnNewPreset.Size = new Size(75, 23);
        btnNewPreset.TabIndex = 0;
        btnNewPreset.Text = "New";
        // 
        // btnEditPreset
        // 
        btnEditPreset.Location = new Point(84, 3);
        btnEditPreset.Name = "btnEditPreset";
        btnEditPreset.Size = new Size(75, 23);
        btnEditPreset.TabIndex = 1;
        btnEditPreset.Text = "Edit";
        // 
        // btnDuplicatePreset
        // 
        btnDuplicatePreset.Location = new Point(165, 3);
        btnDuplicatePreset.Name = "btnDuplicatePreset";
        btnDuplicatePreset.Size = new Size(75, 23);
        btnDuplicatePreset.TabIndex = 2;
        btnDuplicatePreset.Text = "Duplicate";
        // 
        // btnDeletePreset
        // 
        btnDeletePreset.Location = new Point(246, 3);
        btnDeletePreset.Name = "btnDeletePreset";
        btnDeletePreset.Size = new Size(75, 23);
        btnDeletePreset.TabIndex = 3;
        btnDeletePreset.Text = "Delete";
        // 
        // groupTrim
        // 
        groupTrim.AutoSize = true;
        groupTrim.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupTrim.Controls.Add(trimPanel);
        groupTrim.Dock = DockStyle.Fill;
        groupTrim.Location = new Point(0, 125);
        groupTrim.Margin = new Padding(0, 0, 0, 8);
        groupTrim.Name = "groupTrim";
        groupTrim.Size = new Size(703, 216);
        groupTrim.TabIndex = 1;
        groupTrim.TabStop = false;
        groupTrim.Text = "Trim";
        // 
        // trimPanel
        // 
        trimPanel.AutoSize = true;
        trimPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        trimPanel.Controls.Add(trimLayout);
        trimPanel.Dock = DockStyle.Top;
        trimPanel.Location = new Point(3, 19);
        trimPanel.Name = "trimPanel";
        trimPanel.Padding = new Padding(6);
        trimPanel.Size = new Size(697, 194);
        trimPanel.TabIndex = 0;
        // 
        // trimLayout
        // 
        trimLayout.AutoSize = true;
        trimLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        trimLayout.ColumnCount = 2;
        trimLayout.ColumnStyles.Add(new ColumnStyle());
        trimLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        trimLayout.Controls.Add(checkEnableTrim, 0, 0);
        trimLayout.Controls.Add(labelTrimHint, 0, 1);
        trimLayout.Controls.Add(trimTimeline, 0, 2);
        trimLayout.Controls.Add(labelTrimStart, 0, 3);
        trimLayout.Controls.Add(textTrimStart, 1, 3);
        trimLayout.Controls.Add(labelTrimEnd, 0, 4);
        trimLayout.Controls.Add(textTrimEnd, 1, 4);
        trimLayout.Controls.Add(labelTrimError, 0, 5);
        trimLayout.Controls.Add(btnResetTrim, 0, 6);
        trimLayout.Dock = DockStyle.Fill;
        trimLayout.Location = new Point(6, 6);
        trimLayout.Name = "trimLayout";
        trimLayout.RowCount = 7;
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.RowStyles.Add(new RowStyle());
        trimLayout.Size = new Size(685, 182);
        trimLayout.TabIndex = 0;
        // 
        // checkEnableTrim
        // 
        checkEnableTrim.AutoSize = true;
        trimLayout.SetColumnSpan(checkEnableTrim, 2);
        checkEnableTrim.Location = new Point(3, 3);
        checkEnableTrim.Name = "checkEnableTrim";
        checkEnableTrim.Size = new Size(70, 19);
        checkEnableTrim.TabIndex = 0;
        checkEnableTrim.Text = "Use trim";
        // 
        // labelTrimHint
        // 
        labelTrimHint.AutoSize = true;
        trimLayout.SetColumnSpan(labelTrimHint, 2);
        labelTrimHint.ForeColor = SystemColors.GrayText;
        labelTrimHint.Location = new Point(3, 25);
        labelTrimHint.Name = "labelTrimHint";
        labelTrimHint.Size = new Size(144, 15);
        labelTrimHint.TabIndex = 1;
        labelTrimHint.Text = "Select one file to edit Trim";
        // 
        // trimTimeline
        // 
        trimLayout.SetColumnSpan(trimTimeline, 2);
        trimTimeline.Dock = DockStyle.Top;
        trimTimeline.DurationSeconds = 0D;
        trimTimeline.EndSeconds = 0D;
        trimTimeline.Location = new Point(3, 43);
        trimTimeline.MinimumSize = new Size(140, 28);
        trimTimeline.Name = "trimTimeline";
        trimTimeline.Size = new Size(679, 32);
        trimTimeline.StartSeconds = 0D;
        trimTimeline.TabIndex = 2;
        // 
        // labelTrimStart
        // 
        labelTrimStart.AutoSize = true;
        labelTrimStart.Location = new Point(3, 78);
        labelTrimStart.Name = "labelTrimStart";
        labelTrimStart.Size = new Size(100, 15);
        labelTrimStart.TabIndex = 3;
        labelTrimStart.Text = "Start (HH:MM:SS)";
        // 
        // textTrimStart
        // 
        textTrimStart.Dock = DockStyle.Fill;
        textTrimStart.Location = new Point(109, 81);
        textTrimStart.Name = "textTrimStart";
        textTrimStart.Size = new Size(573, 23);
        textTrimStart.TabIndex = 4;
        // 
        // labelTrimEnd
        // 
        labelTrimEnd.AutoSize = true;
        labelTrimEnd.Location = new Point(3, 107);
        labelTrimEnd.Name = "labelTrimEnd";
        labelTrimEnd.Size = new Size(96, 15);
        labelTrimEnd.TabIndex = 5;
        labelTrimEnd.Text = "End (HH:MM:SS)";
        // 
        // textTrimEnd
        // 
        textTrimEnd.Dock = DockStyle.Fill;
        textTrimEnd.Location = new Point(109, 110);
        textTrimEnd.Name = "textTrimEnd";
        textTrimEnd.Size = new Size(573, 23);
        textTrimEnd.TabIndex = 6;
        // 
        // labelTrimError
        // 
        labelTrimError.AutoSize = true;
        trimLayout.SetColumnSpan(labelTrimError, 2);
        labelTrimError.ForeColor = Color.DarkRed;
        labelTrimError.Location = new Point(3, 136);
        labelTrimError.Name = "labelTrimError";
        labelTrimError.Size = new Size(111, 15);
        labelTrimError.TabIndex = 7;
        labelTrimError.Text = "Start must be < End";
        labelTrimError.Visible = false;
        // 
        // btnResetTrim
        // 
        btnResetTrim.AutoSize = true;
        btnResetTrim.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        trimLayout.SetColumnSpan(btnResetTrim, 2);
        btnResetTrim.Location = new Point(3, 154);
        btnResetTrim.Name = "btnResetTrim";
        btnResetTrim.Size = new Size(72, 25);
        btnResetTrim.TabIndex = 8;
        btnResetTrim.Text = "Reset Trim";
        // 
        // groupOutput
        // 
        groupOutput.AutoSize = true;
        groupOutput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupOutput.Controls.Add(outputLayout);
        groupOutput.Dock = DockStyle.Fill;
        groupOutput.Location = new Point(0, 349);
        groupOutput.Margin = new Padding(0, 0, 0, 8);
        groupOutput.Name = "groupOutput";
        groupOutput.Size = new Size(703, 107);
        groupOutput.TabIndex = 2;
        groupOutput.TabStop = false;
        groupOutput.Text = "Output";
        // 
        // outputLayout
        // 
        outputLayout.AutoSize = true;
        outputLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        outputLayout.ColumnCount = 3;
        outputLayout.ColumnStyles.Add(new ColumnStyle());
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        outputLayout.ColumnStyles.Add(new ColumnStyle());
        outputLayout.Controls.Add(checkSameAsSource, 0, 0);
        outputLayout.Controls.Add(labelOutputFolder, 0, 1);
        outputLayout.Controls.Add(textOutputFolder, 1, 1);
        outputLayout.Controls.Add(btnBrowseOutput, 2, 1);
        outputLayout.Controls.Add(labelRecentOutputs, 0, 2);
        outputLayout.Controls.Add(comboRecentOutputs, 1, 2);
        outputLayout.Dock = DockStyle.Fill;
        outputLayout.Location = new Point(3, 19);
        outputLayout.Name = "outputLayout";
        outputLayout.RowCount = 3;
        outputLayout.RowStyles.Add(new RowStyle());
        outputLayout.RowStyles.Add(new RowStyle());
        outputLayout.RowStyles.Add(new RowStyle());
        outputLayout.Size = new Size(697, 85);
        outputLayout.TabIndex = 0;
        // 
        // checkSameAsSource
        // 
        checkSameAsSource.AutoSize = true;
        outputLayout.SetColumnSpan(checkSameAsSource, 3);
        checkSameAsSource.Location = new Point(3, 3);
        checkSameAsSource.Name = "checkSameAsSource";
        checkSameAsSource.Size = new Size(107, 19);
        checkSameAsSource.TabIndex = 0;
        checkSameAsSource.Text = "Same as source";
        // 
        // labelOutputFolder
        // 
        labelOutputFolder.AutoSize = true;
        labelOutputFolder.Location = new Point(3, 25);
        labelOutputFolder.Name = "labelOutputFolder";
        labelOutputFolder.Size = new Size(79, 15);
        labelOutputFolder.TabIndex = 1;
        labelOutputFolder.Text = "Output folder";
        // 
        // textOutputFolder
        // 
        textOutputFolder.Dock = DockStyle.Fill;
        textOutputFolder.Location = new Point(130, 28);
        textOutputFolder.Name = "textOutputFolder";
        textOutputFolder.Size = new Size(503, 23);
        textOutputFolder.TabIndex = 2;
        // 
        // btnBrowseOutput
        // 
        btnBrowseOutput.AutoSize = true;
        btnBrowseOutput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnBrowseOutput.Location = new Point(639, 28);
        btnBrowseOutput.Name = "btnBrowseOutput";
        btnBrowseOutput.Size = new Size(55, 25);
        btnBrowseOutput.TabIndex = 3;
        btnBrowseOutput.Text = "Browse";
        // 
        // labelRecentOutputs
        // 
        labelRecentOutputs.AutoSize = true;
        labelRecentOutputs.Location = new Point(3, 56);
        labelRecentOutputs.Name = "labelRecentOutputs";
        labelRecentOutputs.Size = new Size(121, 15);
        labelRecentOutputs.TabIndex = 4;
        labelRecentOutputs.Text = "Recent output folders";
        // 
        // comboRecentOutputs
        // 
        outputLayout.SetColumnSpan(comboRecentOutputs, 2);
        comboRecentOutputs.Dock = DockStyle.Fill;
        comboRecentOutputs.DropDownStyle = ComboBoxStyle.DropDownList;
        comboRecentOutputs.Location = new Point(130, 59);
        comboRecentOutputs.Name = "comboRecentOutputs";
        comboRecentOutputs.Size = new Size(564, 23);
        comboRecentOutputs.TabIndex = 5;
        // 
        // groupPriority
        // 
        groupPriority.AutoSize = true;
        groupPriority.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupPriority.Controls.Add(priorityLayout);
        groupPriority.Dock = DockStyle.Fill;
        groupPriority.Location = new Point(0, 464);
        groupPriority.Margin = new Padding(0, 0, 0, 8);
        groupPriority.Name = "groupPriority";
        groupPriority.Size = new Size(703, 118);
        groupPriority.TabIndex = 3;
        groupPriority.TabStop = false;
        groupPriority.Text = "Encoder priority";
        // 
        // priorityLayout
        // 
        priorityLayout.AutoSize = true;
        priorityLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        priorityLayout.ColumnCount = 2;
        priorityLayout.ColumnStyles.Add(new ColumnStyle());
        priorityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        priorityLayout.Controls.Add(labelPriority, 0, 0);
        priorityLayout.Controls.Add(comboPriority, 1, 0);
        priorityLayout.Controls.Add(labelSpeedQuality, 0, 1);
        priorityLayout.Controls.Add(trackSpeedQuality, 1, 1);
        priorityLayout.Controls.Add(btnFfmpegSettings, 0, 2);
        priorityLayout.Dock = DockStyle.Fill;
        priorityLayout.Location = new Point(3, 19);
        priorityLayout.Name = "priorityLayout";
        priorityLayout.RowCount = 3;
        priorityLayout.RowStyles.Add(new RowStyle());
        priorityLayout.RowStyles.Add(new RowStyle());
        priorityLayout.RowStyles.Add(new RowStyle());
        priorityLayout.Size = new Size(697, 96);
        priorityLayout.TabIndex = 0;
        // 
        // labelPriority
        // 
        labelPriority.AutoSize = true;
        labelPriority.Location = new Point(3, 0);
        labelPriority.Name = "labelPriority";
        labelPriority.Size = new Size(45, 15);
        labelPriority.TabIndex = 0;
        labelPriority.Text = "Priority";
        // 
        // comboPriority
        // 
        comboPriority.Dock = DockStyle.Fill;
        comboPriority.DropDownStyle = ComboBoxStyle.DropDownList;
        comboPriority.Location = new Point(97, 3);
        comboPriority.Name = "comboPriority";
        comboPriority.Size = new Size(597, 23);
        comboPriority.TabIndex = 1;
        // 
        // labelSpeedQuality
        // 
        labelSpeedQuality.AutoSize = true;
        labelSpeedQuality.Location = new Point(3, 29);
        labelSpeedQuality.Name = "labelSpeedQuality";
        labelSpeedQuality.Size = new Size(88, 15);
        labelSpeedQuality.TabIndex = 2;
        labelSpeedQuality.Text = "Speed / Quality";
        toolTip.SetToolTip(labelSpeedQuality, "Controls encoding speed vs quality.\r\nApplies globally and depends on selected encoder (NVENC / AMF / QSV / CPU).");
        // 
        // trackSpeedQuality
        // 
        trackSpeedQuality.AutoSize = false;
        trackSpeedQuality.Dock = DockStyle.Fill;
        trackSpeedQuality.Location = new Point(97, 32);
        trackSpeedQuality.Maximum = 9;
        trackSpeedQuality.Minimum = 1;
        trackSpeedQuality.Name = "trackSpeedQuality";
        trackSpeedQuality.Size = new Size(597, 30);
        trackSpeedQuality.TabIndex = 3;
        toolTip.SetToolTip(trackSpeedQuality, "Controls encoding speed vs quality.\r\nApplies globally and depends on selected encoder (NVENC / AMF / QSV / CPU).");
        trackSpeedQuality.Value = 5;
        // 
        // btnFfmpegSettings
        // 
        btnFfmpegSettings.AutoSize = true;
        btnFfmpegSettings.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        priorityLayout.SetColumnSpan(btnFfmpegSettings, 2);
        btnFfmpegSettings.Dock = DockStyle.Top;
        btnFfmpegSettings.Location = new Point(3, 68);
        btnFfmpegSettings.Name = "btnFfmpegSettings";
        btnFfmpegSettings.Size = new Size(691, 25);
        btnFfmpegSettings.TabIndex = 4;
        btnFfmpegSettings.Text = "FFmpeg...";
        // 
        // groupStatus
        // 
        groupStatus.AutoSize = true;
        groupStatus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupStatus.Controls.Add(statusLayout);
        groupStatus.Dock = DockStyle.Fill;
        groupStatus.Location = new Point(0, 590);
        groupStatus.Margin = new Padding(0);
        groupStatus.Name = "groupStatus";
        groupStatus.Size = new Size(703, 97);
        groupStatus.TabIndex = 4;
        groupStatus.TabStop = false;
        groupStatus.Text = "Status";
        // 
        // statusLayout
        // 
        statusLayout.AutoSize = true;
        statusLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        statusLayout.ColumnCount = 1;
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusLayout.Controls.Add(labelFfmpegStatus, 0, 0);
        statusLayout.Controls.Add(labelOutputStatus, 0, 1);
        statusLayout.Controls.Add(labelBitrateStatus, 0, 2);
        statusLayout.Controls.Add(labelTrimStatus, 0, 3);
        statusLayout.Controls.Add(labelFfmpegDownloadStatus, 0, 4);
        statusLayout.Dock = DockStyle.Fill;
        statusLayout.Location = new Point(3, 19);
        statusLayout.Name = "statusLayout";
        statusLayout.RowCount = 5;
        statusLayout.RowStyles.Add(new RowStyle());
        statusLayout.RowStyles.Add(new RowStyle());
        statusLayout.RowStyles.Add(new RowStyle());
        statusLayout.RowStyles.Add(new RowStyle());
        statusLayout.RowStyles.Add(new RowStyle());
        statusLayout.Size = new Size(697, 75);
        statusLayout.TabIndex = 0;
        // 
        // labelFfmpegStatus
        // 
        labelFfmpegStatus.AutoSize = true;
        labelFfmpegStatus.Location = new Point(3, 0);
        labelFfmpegStatus.Name = "labelFfmpegStatus";
        labelFfmpegStatus.Size = new Size(0, 15);
        labelFfmpegStatus.TabIndex = 0;
        // 
        // labelOutputStatus
        // 
        labelOutputStatus.AutoSize = true;
        labelOutputStatus.Location = new Point(3, 15);
        labelOutputStatus.Name = "labelOutputStatus";
        labelOutputStatus.Size = new Size(0, 15);
        labelOutputStatus.TabIndex = 1;
        // 
        // labelBitrateStatus
        // 
        labelBitrateStatus.AutoSize = true;
        labelBitrateStatus.Location = new Point(3, 30);
        labelBitrateStatus.Name = "labelBitrateStatus";
        labelBitrateStatus.Size = new Size(0, 15);
        labelBitrateStatus.TabIndex = 2;
        // 
        // labelTrimStatus
        // 
        labelTrimStatus.AutoSize = true;
        labelTrimStatus.Location = new Point(3, 45);
        labelTrimStatus.Name = "labelTrimStatus";
        labelTrimStatus.Size = new Size(0, 15);
        labelTrimStatus.TabIndex = 3;
        // 
        // labelFfmpegDownloadStatus
        // 
        labelFfmpegDownloadStatus.AutoSize = true;
        labelFfmpegDownloadStatus.Location = new Point(3, 60);
        labelFfmpegDownloadStatus.Name = "labelFfmpegDownloadStatus";
        labelFfmpegDownloadStatus.Size = new Size(0, 15);
        labelFfmpegDownloadStatus.TabIndex = 4;
        // 
        // tabLogs
        // 
        tabLogs.Controls.Add(logsRootPanel);
        tabLogs.Location = new Point(4, 24);
        tabLogs.Name = "tabLogs";
        tabLogs.Padding = new Padding(8);
        tabLogs.Size = new Size(1076, 633);
        tabLogs.TabIndex = 1;
        tabLogs.Text = "Logs";
        // 
        // logsRootPanel
        // 
        logsRootPanel.Controls.Add(logsLayout);
        logsRootPanel.Dock = DockStyle.Fill;
        logsRootPanel.Location = new Point(8, 8);
        logsRootPanel.Name = "logsRootPanel";
        logsRootPanel.Size = new Size(1060, 617);
        logsRootPanel.TabIndex = 0;
        // 
        // logsLayout
        // 
        logsLayout.ColumnCount = 1;
        logsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        logsLayout.Controls.Add(logsTopPanel, 0, 0);
        logsLayout.Controls.Add(richLogs, 0, 1);
        logsLayout.Dock = DockStyle.Fill;
        logsLayout.Location = new Point(0, 0);
        logsLayout.Name = "logsLayout";
        logsLayout.RowCount = 2;
        logsLayout.RowStyles.Add(new RowStyle());
        logsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        logsLayout.Size = new Size(1060, 617);
        logsLayout.TabIndex = 0;
        // 
        // logsTopPanel
        // 
        logsTopPanel.AutoSize = true;
        logsTopPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        logsTopPanel.Controls.Add(btnCopySelected);
        logsTopPanel.Controls.Add(btnCopyDiagnostics);
        logsTopPanel.Controls.Add(btnClearLog);
        logsTopPanel.Controls.Add(labelLogFilter);
        logsTopPanel.Controls.Add(comboLogFilter);
        logsTopPanel.Controls.Add(checkAutoScroll);
        logsTopPanel.Dock = DockStyle.Fill;
        logsTopPanel.Location = new Point(3, 3);
        logsTopPanel.Name = "logsTopPanel";
        logsTopPanel.Padding = new Padding(6);
        logsTopPanel.Size = new Size(1054, 51);
        logsTopPanel.TabIndex = 0;
        // 
        // btnCopySelected
        // 
        btnCopySelected.AutoSize = true;
        btnCopySelected.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnCopySelected.Location = new Point(9, 9);
        btnCopySelected.MinimumSize = new Size(120, 0);
        btnCopySelected.Name = "btnCopySelected";
        btnCopySelected.Padding = new Padding(8, 4, 8, 4);
        btnCopySelected.Size = new Size(120, 33);
        btnCopySelected.TabIndex = 0;
        btnCopySelected.Text = "Copy Selected";
        // 
        // btnCopyDiagnostics
        // 
        btnCopyDiagnostics.AutoSize = true;
        btnCopyDiagnostics.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnCopyDiagnostics.Location = new Point(135, 9);
        btnCopyDiagnostics.MinimumSize = new Size(120, 0);
        btnCopyDiagnostics.Name = "btnCopyDiagnostics";
        btnCopyDiagnostics.Padding = new Padding(8, 4, 8, 4);
        btnCopyDiagnostics.Size = new Size(120, 33);
        btnCopyDiagnostics.TabIndex = 1;
        btnCopyDiagnostics.Text = "Copy All";
        // 
        // btnClearLog
        // 
        btnClearLog.AutoSize = true;
        btnClearLog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnClearLog.Location = new Point(261, 9);
        btnClearLog.MinimumSize = new Size(90, 0);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Padding = new Padding(8, 4, 8, 4);
        btnClearLog.Size = new Size(90, 33);
        btnClearLog.TabIndex = 2;
        btnClearLog.Text = "Clear";
        // 
        // labelLogFilter
        // 
        labelLogFilter.AutoSize = true;
        labelLogFilter.Location = new Point(360, 14);
        labelLogFilter.Margin = new Padding(6, 8, 3, 0);
        labelLogFilter.Name = "labelLogFilter";
        labelLogFilter.Size = new Size(36, 15);
        labelLogFilter.TabIndex = 3;
        labelLogFilter.Text = "Filter:";
        // 
        // comboLogFilter
        // 
        comboLogFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        comboLogFilter.Items.AddRange(new object[] { "All", "Errors", "Warnings", "Commands" });
        comboLogFilter.Location = new Point(402, 9);
        comboLogFilter.MinimumSize = new Size(120, 0);
        comboLogFilter.Name = "comboLogFilter";
        comboLogFilter.Size = new Size(121, 23);
        comboLogFilter.TabIndex = 4;
        // 
        // checkAutoScroll
        // 
        checkAutoScroll.AutoSize = true;
        checkAutoScroll.Checked = true;
        checkAutoScroll.CheckState = CheckState.Checked;
        checkAutoScroll.Location = new Point(529, 9);
        checkAutoScroll.Name = "checkAutoScroll";
        checkAutoScroll.Size = new Size(85, 19);
        checkAutoScroll.TabIndex = 5;
        checkAutoScroll.Text = "Auto-scroll";
        // 
        // richLogs
        // 
        richLogs.Dock = DockStyle.Fill;
        richLogs.Font = new Font("Consolas", 9F);
        richLogs.Location = new Point(3, 60);
        richLogs.Name = "richLogs";
        richLogs.ReadOnly = true;
        richLogs.ScrollBars = RichTextBoxScrollBars.Vertical;
        richLogs.Size = new Size(1054, 554);
        richLogs.TabIndex = 1;
        richLogs.Text = "";
        richLogs.WordWrap = false;
        // 
        // notifyIcon
        // 
        notifyIcon.Text = "Aplysia AV1 Transcoder";
        notifyIcon.Visible = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1084, 661);
        Controls.Add(tabControl);
        Font = new Font("Segoe UI", 9F);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        Text = "Aplysia AV1 Transcoder";
        tabControl.ResumeLayout(false);
        tabQueue.ResumeLayout(false);
        queueRootPanel.ResumeLayout(false);
        splitQueue.Panel1.ResumeLayout(false);
        splitQueue.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitQueue).EndInit();
        splitQueue.ResumeLayout(false);
        queueLeftLayout.ResumeLayout(false);
        queueLeftLayout.PerformLayout();
        queueButtonsPanel.ResumeLayout(false);
        queueBottomPanel.ResumeLayout(false);
        queueBottomPanel.PerformLayout();
        rightPanelHost.ResumeLayout(false);
        rightPanelHost.PerformLayout();
        rightLayout.ResumeLayout(false);
        rightLayout.PerformLayout();
        groupPresets.ResumeLayout(false);
        groupPresets.PerformLayout();
        presetsLayout.ResumeLayout(false);
        presetsLayout.PerformLayout();
        presetButtons.ResumeLayout(false);
        groupTrim.ResumeLayout(false);
        groupTrim.PerformLayout();
        trimPanel.ResumeLayout(false);
        trimPanel.PerformLayout();
        trimLayout.ResumeLayout(false);
        trimLayout.PerformLayout();
        groupOutput.ResumeLayout(false);
        groupOutput.PerformLayout();
        outputLayout.ResumeLayout(false);
        outputLayout.PerformLayout();
        groupPriority.ResumeLayout(false);
        groupPriority.PerformLayout();
        priorityLayout.ResumeLayout(false);
        priorityLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)trackSpeedQuality).EndInit();
        groupStatus.ResumeLayout(false);
        groupStatus.PerformLayout();
        statusLayout.ResumeLayout(false);
        statusLayout.PerformLayout();
        tabLogs.ResumeLayout(false);
        logsRootPanel.ResumeLayout(false);
        logsLayout.ResumeLayout(false);
        logsLayout.PerformLayout();
        logsTopPanel.ResumeLayout(false);
        logsTopPanel.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel presetsLayout;
    private FlowLayoutPanel presetButtons;
    private TableLayoutPanel trimLayout;
    private TableLayoutPanel outputLayout;
    private TableLayoutPanel priorityLayout;
    private TableLayoutPanel statusLayout;
}
