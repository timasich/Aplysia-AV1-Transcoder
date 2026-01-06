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
    private NumericUpDown numTrimStartHours;
    private NumericUpDown numTrimStartMinutes;
    private NumericUpDown numTrimStartSeconds;
    private NumericUpDown numTrimStartMilliseconds;
    private NumericUpDown numTrimEndHours;
    private NumericUpDown numTrimEndMinutes;
    private NumericUpDown numTrimEndSeconds;
    private NumericUpDown numTrimEndMilliseconds;
    private Label labelTrimStart;
    private Label labelTrimEnd;
    private FlowLayoutPanel trimStartPanel;
    private FlowLayoutPanel trimEndPanel;
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
    private Label labelTrimStatus;

    private TableLayoutPanel logsLayout;
    private FlowLayoutPanel logsTopPanel;
    private Button btnCopySelected;
    private Button btnCopyDiagnostics;
    private Button btnClearLog;
    private Label labelLogFilter;
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
        labelTrimStart = new Label();
        labelTrimEnd = new Label();
        numTrimStartHours = new NumericUpDown();
        numTrimStartMinutes = new NumericUpDown();
        numTrimStartSeconds = new NumericUpDown();
        numTrimStartMilliseconds = new NumericUpDown();
        numTrimEndHours = new NumericUpDown();
        numTrimEndMinutes = new NumericUpDown();
        numTrimEndSeconds = new NumericUpDown();
        numTrimEndMilliseconds = new NumericUpDown();
        trimStartPanel = BuildTrimPanel(numTrimStartHours, numTrimStartMinutes, numTrimStartSeconds, numTrimStartMilliseconds);
        trimEndPanel = BuildTrimPanel(numTrimEndHours, numTrimEndMinutes, numTrimEndSeconds, numTrimEndMilliseconds);
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
        btnFfmpegSettings = new Button();
        groupStatus = new GroupBox();
        statusLayout = new TableLayoutPanel();
        labelFfmpegStatus = new Label();
        labelOutputStatus = new Label();
        labelBitrateStatus = new Label();
        labelTrimStatus = new Label();
        tabLogs = new TabPage();
        logsRootPanel = new Panel();
        logsLayout = new TableLayoutPanel();
        logsTopPanel = new FlowLayoutPanel();
        btnCopySelected = new Button();
        btnClearLog = new Button();
        btnCopyDiagnostics = new Button();
        labelLogFilter = new Label();
        comboLogFilter = new ComboBox();
        checkAutoScroll = new CheckBox();
        richLogs = new RichTextBox();
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
        groupStatus.SuspendLayout();
        statusLayout.SuspendLayout();
        tabLogs.SuspendLayout();
        logsRootPanel.SuspendLayout();
        logsLayout.SuspendLayout();
        logsTopPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numTrimStartHours).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimStartMinutes).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimStartSeconds).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimStartMilliseconds).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndHours).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndMinutes).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndSeconds).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndMilliseconds).BeginInit();
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
        splitQueue.Panel1MinSize = 320;
        splitQueue.Panel2MinSize = 320;
        // 
        // splitQueue.Panel1
        // 
        splitQueue.Panel1.Controls.Add(queueLeftLayout);
        // 
        // splitQueue.Panel2
        // 
        splitQueue.Panel2.Controls.Add(rightPanelHost);
        splitQueue.Size = new Size(1060, 617);
        splitQueue.SplitterDistance = 960;
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
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        queueLeftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        queueLeftLayout.Size = new Size(960, 617);
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
        queueButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
        queueButtonsPanel.Location = new Point(3, 3);
        queueButtonsPanel.Margin = new Padding(0, 0, 0, 6);
        queueButtonsPanel.Name = "queueButtonsPanel";
        queueButtonsPanel.Size = new Size(960, 29);
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
        listQueue.Size = new Size(954, 541);
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
        queueBottomPanel.Dock = DockStyle.Fill;
        queueBottomPanel.FlowDirection = FlowDirection.LeftToRight;
        queueBottomPanel.Location = new Point(3, 585);
        queueBottomPanel.Margin = new Padding(0, 6, 0, 0);
        queueBottomPanel.Name = "queueBottomPanel";
        queueBottomPanel.Size = new Size(960, 29);
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
        // rightPanelHost
        // 
        rightPanelHost.AutoScroll = true;
        rightPanelHost.Controls.Add(rightLayout);
        rightPanelHost.Dock = DockStyle.Fill;
        rightPanelHost.Location = new Point(0, 0);
        rightPanelHost.Name = "rightPanelHost";
        rightPanelHost.Padding = new Padding(8);
        rightPanelHost.Size = new Size(96, 617);
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
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.Size = new Size(0, 0);
        rightLayout.TabIndex = 0;
        // 
        // groupPresets
        // 
        groupPresets.AutoSize = true;
        groupPresets.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupPresets.Controls.Add(presetsLayout);
        groupPresets.Dock = DockStyle.Fill;
        groupPresets.Location = new Point(8, 8);
        groupPresets.Margin = new Padding(0, 0, 0, 8);
        groupPresets.Name = "groupPresets";
        groupPresets.Size = new Size(206, 122);
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
        presetsLayout.Size = new Size(200, 100);
        presetsLayout.TabIndex = 0;
        // 
        // comboActivePreset
        // 
        comboActivePreset.Dock = DockStyle.Fill;
        comboActivePreset.DropDownStyle = ComboBoxStyle.DropDownList;
        comboActivePreset.Location = new Point(3, 3);
        comboActivePreset.Name = "comboActivePreset";
        comboActivePreset.Size = new Size(194, 23);
        comboActivePreset.TabIndex = 0;
        // 
        // btnApplyPreset
        // 
        btnApplyPreset.AutoSize = true;
        btnApplyPreset.Dock = DockStyle.Top;
        btnApplyPreset.Location = new Point(3, 32);
        btnApplyPreset.Name = "btnApplyPreset";
        btnApplyPreset.Size = new Size(194, 25);
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
        presetButtons.Location = new Point(3, 61);
        presetButtons.Name = "presetButtons";
        presetButtons.Size = new Size(194, 58);
        presetButtons.TabIndex = 2;
        presetButtons.WrapContents = true;
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
        btnDuplicatePreset.Location = new Point(3, 32);
        btnDuplicatePreset.Name = "btnDuplicatePreset";
        btnDuplicatePreset.Size = new Size(75, 23);
        btnDuplicatePreset.TabIndex = 2;
        btnDuplicatePreset.Text = "Duplicate";
        // 
        // btnDeletePreset
        // 
        btnDeletePreset.Location = new Point(84, 32);
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
        groupTrim.Location = new Point(8, 146);
        groupTrim.Margin = new Padding(0, 0, 0, 8);
        groupTrim.Name = "groupTrim";
        groupTrim.Size = new Size(206, 122);
        groupTrim.TabIndex = 1;
        groupTrim.TabStop = false;
        groupTrim.Text = "Trim";
        // 
        // trimPanel
        // 
        trimPanel.Controls.Add(trimLayout);
        trimPanel.Dock = DockStyle.Fill;
        trimPanel.Location = new Point(3, 19);
        trimPanel.Name = "trimPanel";
        trimPanel.Padding = new Padding(6);
        trimPanel.Size = new Size(200, 100);
        trimPanel.TabIndex = 0;
        // 
        // trimLayout
        // 
        trimLayout.AutoSize = true;
        trimLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        trimLayout.ColumnCount = 2;
        trimLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        trimLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        trimLayout.Controls.Add(checkEnableTrim, 0, 0);
        trimLayout.Controls.Add(labelTrimStart, 0, 1);
        trimLayout.Controls.Add(labelTrimEnd, 0, 2);
        trimLayout.Controls.Add(trimStartPanel, 1, 1);
        trimLayout.Controls.Add(trimEndPanel, 1, 2);
        trimLayout.Dock = DockStyle.Top;
        trimLayout.Location = new Point(6, 6);
        trimLayout.Name = "trimLayout";
        trimLayout.RowCount = 3;
        trimLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        trimLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        trimLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        trimLayout.Size = new Size(188, 74);
        trimLayout.TabIndex = 0;
        // 
        // checkEnableTrim
        // 
        trimLayout.SetColumnSpan(checkEnableTrim, 2);
        checkEnableTrim.AutoSize = true;
        checkEnableTrim.Location = new Point(3, 3);
        checkEnableTrim.Name = "checkEnableTrim";
        checkEnableTrim.Size = new Size(86, 19);
        checkEnableTrim.TabIndex = 0;
        checkEnableTrim.Text = "Enable trim";
        // 
        // labelTrimStart
        // 
        labelTrimStart.AutoSize = true;
        labelTrimStart.Location = new Point(3, 20);
        labelTrimStart.Name = "labelTrimStart";
        labelTrimStart.Size = new Size(31, 15);
        labelTrimStart.TabIndex = 1;
        labelTrimStart.Text = "Start";
        // 
        // labelTrimEnd
        // 
        labelTrimEnd.AutoSize = true;
        labelTrimEnd.Location = new Point(3, 40);
        labelTrimEnd.Name = "labelTrimEnd";
        labelTrimEnd.Size = new Size(27, 15);
        labelTrimEnd.TabIndex = 2;
        labelTrimEnd.Text = "End";
        // 
        // groupOutput
        // 
        groupOutput.AutoSize = true;
        groupOutput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupOutput.Controls.Add(outputLayout);
        groupOutput.Dock = DockStyle.Fill;
        groupOutput.Location = new Point(8, 184);
        groupOutput.Margin = new Padding(0, 0, 0, 8);
        groupOutput.Name = "groupOutput";
        groupOutput.Size = new Size(206, 122);
        groupOutput.TabIndex = 2;
        groupOutput.TabStop = false;
        groupOutput.Text = "Output";
        // 
        // outputLayout
        // 
        outputLayout.AutoSize = true;
        outputLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        outputLayout.ColumnCount = 3;
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
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
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outputLayout.Size = new Size(200, 100);
        outputLayout.TabIndex = 0;
        // 
        // checkSameAsSource
        // 
        outputLayout.SetColumnSpan(checkSameAsSource, 3);
        checkSameAsSource.AutoSize = true;
        checkSameAsSource.Location = new Point(3, 3);
        checkSameAsSource.Name = "checkSameAsSource";
        checkSameAsSource.Size = new Size(115, 19);
        checkSameAsSource.TabIndex = 0;
        checkSameAsSource.Text = "Same as source";
        // 
        // labelOutputFolder
        // 
        labelOutputFolder.AutoSize = true;
        labelOutputFolder.Location = new Point(3, 20);
        labelOutputFolder.Name = "labelOutputFolder";
        labelOutputFolder.Size = new Size(79, 15);
        labelOutputFolder.TabIndex = 1;
        labelOutputFolder.Text = "Output folder";
        // 
        // textOutputFolder
        // 
        textOutputFolder.Dock = DockStyle.Fill;
        textOutputFolder.Location = new Point(130, 23);
        textOutputFolder.Name = "textOutputFolder";
        textOutputFolder.Size = new Size(1, 23);
        textOutputFolder.TabIndex = 2;
        // 
        // btnBrowseOutput
        // 
        btnBrowseOutput.AutoSize = true;
        btnBrowseOutput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnBrowseOutput.Location = new Point(122, 23);
        btnBrowseOutput.Name = "btnBrowseOutput";
        btnBrowseOutput.Size = new Size(67, 25);
        btnBrowseOutput.TabIndex = 3;
        btnBrowseOutput.Text = "Browse";
        // 
        // labelRecentOutputs
        // 
        labelRecentOutputs.AutoSize = true;
        labelRecentOutputs.Location = new Point(3, 40);
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
        comboRecentOutputs.Location = new Point(130, 43);
        comboRecentOutputs.Name = "comboRecentOutputs";
        comboRecentOutputs.Size = new Size(67, 23);
        comboRecentOutputs.TabIndex = 5;
        // 
        // groupPriority
        // 
        groupPriority.AutoSize = true;
        groupPriority.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupPriority.Controls.Add(priorityLayout);
        groupPriority.Dock = DockStyle.Fill;
        groupPriority.Location = new Point(8, 322);
        groupPriority.Margin = new Padding(0, 0, 0, 8);
        groupPriority.Name = "groupPriority";
        groupPriority.Size = new Size(206, 122);
        groupPriority.TabIndex = 3;
        groupPriority.TabStop = false;
        groupPriority.Text = "Encoder priority";
        // 
        // priorityLayout
        // 
        priorityLayout.AutoSize = true;
        priorityLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        priorityLayout.ColumnCount = 2;
        priorityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        priorityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        priorityLayout.Controls.Add(labelPriority, 0, 0);
        priorityLayout.Controls.Add(comboPriority, 1, 0);
        priorityLayout.Controls.Add(btnFfmpegSettings, 0, 1);
        priorityLayout.Dock = DockStyle.Fill;
        priorityLayout.Location = new Point(3, 19);
        priorityLayout.Name = "priorityLayout";
        priorityLayout.RowCount = 2;
        priorityLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        priorityLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        priorityLayout.Size = new Size(200, 100);
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
        comboPriority.Location = new Point(54, 3);
        comboPriority.Name = "comboPriority";
        comboPriority.Size = new Size(143, 23);
        comboPriority.TabIndex = 1;
        // 
        // btnFfmpegSettings
        // 
        priorityLayout.SetColumnSpan(btnFfmpegSettings, 2);
        btnFfmpegSettings.AutoSize = true;
        btnFfmpegSettings.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnFfmpegSettings.Dock = DockStyle.Top;
        btnFfmpegSettings.Location = new Point(3, 29);
        btnFfmpegSettings.Name = "btnFfmpegSettings";
        btnFfmpegSettings.Size = new Size(194, 25);
        btnFfmpegSettings.TabIndex = 2;
        btnFfmpegSettings.Text = "FFmpeg...";
        // 
        // groupStatus
        // 
        groupStatus.AutoSize = true;
        groupStatus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupStatus.Controls.Add(statusLayout);
        groupStatus.Dock = DockStyle.Fill;
        groupStatus.Location = new Point(8, 460);
        groupStatus.Margin = new Padding(0);
        groupStatus.Name = "groupStatus";
        groupStatus.Size = new Size(206, 122);
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
        statusLayout.Dock = DockStyle.Fill;
        statusLayout.Location = new Point(3, 19);
        statusLayout.Name = "statusLayout";
        statusLayout.RowCount = 4;
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusLayout.Size = new Size(200, 100);
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
        labelOutputStatus.Location = new Point(3, 20);
        labelOutputStatus.Name = "labelOutputStatus";
        labelOutputStatus.Size = new Size(0, 15);
        labelOutputStatus.TabIndex = 1;
        // 
        // labelBitrateStatus
        // 
        labelBitrateStatus.AutoSize = true;
        labelBitrateStatus.Location = new Point(3, 40);
        labelBitrateStatus.Name = "labelBitrateStatus";
        labelBitrateStatus.Size = new Size(0, 15);
        labelBitrateStatus.TabIndex = 2;
        // 
        // labelTrimStatus
        // 
        labelTrimStatus.AutoSize = true;
        labelTrimStatus.Location = new Point(3, 60);
        labelTrimStatus.Name = "labelTrimStatus";
        labelTrimStatus.Size = new Size(0, 15);
        labelTrimStatus.TabIndex = 3;
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
        logsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        logsTopPanel.FlowDirection = FlowDirection.LeftToRight;
        logsTopPanel.Location = new Point(3, 3);
        logsTopPanel.Name = "logsTopPanel";
        logsTopPanel.Padding = new Padding(6);
        logsTopPanel.Size = new Size(523, 38);
        logsTopPanel.TabIndex = 0;
        logsTopPanel.WrapContents = true;
        // 
        // btnCopySelected
        // 
        btnCopySelected.AutoSize = true;
        btnCopySelected.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnCopySelected.Location = new Point(3, 3);
        btnCopySelected.MinimumSize = new Size(120, 0);
        btnCopySelected.Name = "btnCopySelected";
        btnCopySelected.Padding = new Padding(8, 4, 8, 4);
        btnCopySelected.Size = new Size(120, 27);
        btnCopySelected.TabIndex = 0;
        btnCopySelected.Text = "Copy Selected";
        // 
        // btnClearLog
        // 
        btnClearLog.AutoSize = true;
        btnClearLog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnClearLog.Location = new Point(129, 3);
        btnClearLog.MinimumSize = new Size(90, 0);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Padding = new Padding(8, 4, 8, 4);
        btnClearLog.Size = new Size(90, 27);
        btnClearLog.TabIndex = 2;
        btnClearLog.Text = "Clear";
        // 
        // btnCopyDiagnostics
        // 
        btnCopyDiagnostics.AutoSize = true;
        btnCopyDiagnostics.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnCopyDiagnostics.Location = new Point(225, 3);
        btnCopyDiagnostics.MinimumSize = new Size(120, 0);
        btnCopyDiagnostics.Name = "btnCopyDiagnostics";
        btnCopyDiagnostics.Padding = new Padding(8, 4, 8, 4);
        btnCopyDiagnostics.Size = new Size(120, 27);
        btnCopyDiagnostics.TabIndex = 1;
        btnCopyDiagnostics.Text = "Copy All";
        // 
        // labelLogFilter
        // 
        labelLogFilter.AutoSize = true;
        labelLogFilter.Location = new Point(351, 11);
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
        comboLogFilter.Location = new Point(393, 8);
        comboLogFilter.MinimumSize = new Size(120, 0);
        comboLogFilter.Name = "comboLogFilter";
        comboLogFilter.Size = new Size(121, 23);
        comboLogFilter.TabIndex = 4;
        // 
        // checkAutoScroll
        // 
        checkAutoScroll.Checked = true;
        checkAutoScroll.CheckState = CheckState.Checked;
        checkAutoScroll.AutoSize = true;
        checkAutoScroll.Location = new Point(520, 8);
        checkAutoScroll.Name = "checkAutoScroll";
        checkAutoScroll.Size = new Size(84, 19);
        checkAutoScroll.TabIndex = 5;
        checkAutoScroll.Text = "Auto-scroll";
        // 
        // richLogs
        // 
        richLogs.Dock = DockStyle.Fill;
        richLogs.Font = new Font("Consolas", 9F);
        richLogs.Location = new Point(3, 39);
        richLogs.Name = "richLogs";
        richLogs.ReadOnly = true;
        richLogs.ScrollBars = RichTextBoxScrollBars.Vertical;
        richLogs.Size = new Size(1054, 575);
        richLogs.TabIndex = 1;
        richLogs.Text = "";
        richLogs.WordWrap = false;
        // 
        // numTrimStartHours
        // 
        numTrimStartHours.Location = new Point(0, 0);
        numTrimStartHours.Name = "numTrimStartHours";
        numTrimStartHours.Size = new Size(46, 23);
        numTrimStartHours.TabIndex = 0;
        // 
        // numTrimStartMinutes
        // 
        numTrimStartMinutes.Location = new Point(0, 0);
        numTrimStartMinutes.Name = "numTrimStartMinutes";
        numTrimStartMinutes.Size = new Size(40, 23);
        numTrimStartMinutes.TabIndex = 0;
        // 
        // numTrimStartSeconds
        // 
        numTrimStartSeconds.Location = new Point(0, 0);
        numTrimStartSeconds.Name = "numTrimStartSeconds";
        numTrimStartSeconds.Size = new Size(40, 23);
        numTrimStartSeconds.TabIndex = 0;
        // 
        // numTrimStartMilliseconds
        // 
        numTrimStartMilliseconds.Location = new Point(0, 0);
        numTrimStartMilliseconds.Name = "numTrimStartMilliseconds";
        numTrimStartMilliseconds.Size = new Size(56, 23);
        numTrimStartMilliseconds.TabIndex = 0;
        // 
        // numTrimEndHours
        // 
        numTrimEndHours.Location = new Point(0, 0);
        numTrimEndHours.Name = "numTrimEndHours";
        numTrimEndHours.Size = new Size(46, 23);
        numTrimEndHours.TabIndex = 0;
        // 
        // numTrimEndMinutes
        // 
        numTrimEndMinutes.Location = new Point(0, 0);
        numTrimEndMinutes.Name = "numTrimEndMinutes";
        numTrimEndMinutes.Size = new Size(40, 23);
        numTrimEndMinutes.TabIndex = 0;
        // 
        // numTrimEndSeconds
        // 
        numTrimEndSeconds.Location = new Point(0, 0);
        numTrimEndSeconds.Name = "numTrimEndSeconds";
        numTrimEndSeconds.Size = new Size(40, 23);
        numTrimEndSeconds.TabIndex = 0;
        // 
        // numTrimEndMilliseconds
        // 
        numTrimEndMilliseconds.Location = new Point(0, 0);
        numTrimEndMilliseconds.Name = "numTrimEndMilliseconds";
        numTrimEndMilliseconds.Size = new Size(56, 23);
        numTrimEndMilliseconds.TabIndex = 0;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1084, 661);
        Controls.Add(tabControl);
        Font = new Font("Segoe UI", 9F);
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
        rightPanelHost.ResumeLayout(false);
        rightPanelHost.PerformLayout();
        rightLayout.ResumeLayout(false);
        rightLayout.PerformLayout();
        groupPresets.ResumeLayout(false);
        presetsLayout.ResumeLayout(false);
        presetsLayout.PerformLayout();
        presetButtons.ResumeLayout(false);
        presetButtons.PerformLayout();
        groupTrim.ResumeLayout(false);
        trimPanel.ResumeLayout(false);
        trimLayout.ResumeLayout(false);
        trimLayout.PerformLayout();
        groupOutput.ResumeLayout(false);
        outputLayout.ResumeLayout(false);
        outputLayout.PerformLayout();
        groupPriority.ResumeLayout(false);
        priorityLayout.ResumeLayout(false);
        priorityLayout.PerformLayout();
        groupStatus.ResumeLayout(false);
        statusLayout.ResumeLayout(false);
        statusLayout.PerformLayout();
        tabLogs.ResumeLayout(false);
        logsRootPanel.ResumeLayout(false);
        logsLayout.ResumeLayout(false);
        logsLayout.PerformLayout();
        logsTopPanel.ResumeLayout(false);
        logsTopPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numTrimStartHours).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimStartMinutes).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimStartSeconds).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimStartMilliseconds).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndHours).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndMinutes).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndSeconds).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTrimEndMilliseconds).EndInit();
        ResumeLayout(false);
    }

    private static FlowLayoutPanel BuildTrimPanel(NumericUpDown hours, NumericUpDown minutes, NumericUpDown seconds, NumericUpDown milliseconds)
    {
        ConfigureTrimField(hours, 0, 99, 46);
        ConfigureTrimField(minutes, 0, 59, 40);
        ConfigureTrimField(seconds, 0, 59, 40);
        ConfigureTrimField(milliseconds, 0, 999, 56);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        panel.Controls.Add(hours);
        panel.Controls.Add(new Label { Text = ":", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(minutes);
        panel.Controls.Add(new Label { Text = ":", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(seconds);
        panel.Controls.Add(new Label { Text = ".", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(milliseconds);

        return panel;
    }

    private static void ConfigureTrimField(NumericUpDown field, int min, int max, int width)
    {
        field.Minimum = min;
        field.Maximum = max;
        field.Width = width;
        field.TextAlign = HorizontalAlignment.Right;
    }

    private TableLayoutPanel presetsLayout;
    private FlowLayoutPanel presetButtons;
    private TableLayoutPanel trimLayout;
    private TableLayoutPanel outputLayout;
    private TableLayoutPanel priorityLayout;
    private TableLayoutPanel statusLayout;
}
