using System;
using System.Drawing;
using System.Windows.Forms;
using AplysiaAv1Transcoder.Models;
using AplysiaAv1Transcoder.Services;

namespace AplysiaAv1Transcoder;

public sealed class PresetEditorForm : Form
{
    private readonly TextBox _nameTextBox;
    private readonly ComboBox _codecCombo;
    private readonly ComboBox _bitrateModeCombo;
    private readonly Label _bitrateLabel;
    private readonly NumericUpDown _bitrateUpDown;
    private readonly Label _multiplierLabel;
    private readonly TrackBar _multiplierTrack;
    private readonly Label _multiplierValueLabel;
    private readonly FlowLayoutPanel _multiplierPanel;
    private readonly Label _multiplierHelpLabel;
    private readonly Label _multiplierPreviewLabel;
    private readonly TextBox _pixelFormatTextBox;
    private readonly ComboBox _audioModeCombo;
    private readonly CheckBox _forceDav1dCheck;
    private readonly Func<ProbeInfo?>? _probeProvider;
    private readonly Guid _presetId;

    public Preset? ResultPreset { get; private set; }

    public PresetEditorForm(Preset preset, string title, Func<ProbeInfo?>? probeProvider = null)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        Width = 480;
        Height = 520;
        _probeProvider = probeProvider;
        _presetId = preset.Id == Guid.Empty ? Guid.NewGuid() : preset.Id;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 11,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < 10; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "Name", AutoSize = true }, 0, 0);
        _nameTextBox = new TextBox { Dock = DockStyle.Fill, Text = preset.Name };
        layout.Controls.Add(_nameTextBox, 1, 0);

        layout.Controls.Add(new Label { Text = "Target codec", AutoSize = true }, 0, 1);
        _codecCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _codecCombo.Items.AddRange(Enum.GetNames<TargetCodec>());
        _codecCombo.SelectedItem = preset.TargetCodec.ToString();
        if (_codecCombo.SelectedIndex < 0)
        {
            _codecCombo.SelectedIndex = 0;
        }
        layout.Controls.Add(_codecCombo, 1, 1);

        layout.Controls.Add(new Label { Text = "Bitrate mode", AutoSize = true }, 0, 2);
        _bitrateModeCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _bitrateModeCombo.Items.AddRange(new object[] { "Fixed bitrate (kbps)", "Multiplier from source bitrate" });
        _bitrateModeCombo.SelectedIndex = preset.BitrateMode == BitrateMode.MultiplierFromSource ? 1 : 0;
        layout.Controls.Add(_bitrateModeCombo, 1, 2);

        _bitrateLabel = new Label { Text = "Bitrate (kbps)", AutoSize = true };
        layout.Controls.Add(_bitrateLabel, 0, 3);
        _bitrateUpDown = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = 1,
            Maximum = 200000,
            Value = Math.Max(1, preset.BitrateKbps)
        };
        layout.Controls.Add(_bitrateUpDown, 1, 3);

        _multiplierLabel = new Label { Text = "Multiplier", AutoSize = true };
        _multiplierTrack = new TrackBar
        {
            Minimum = 100,
            Maximum = 300,
            TickFrequency = 10,
            LargeChange = 10,
            SmallChange = 5,
            Value = (int)Math.Round(Math.Clamp(preset.Multiplier, 1.0, 3.0) * 100)
        };
        _multiplierTrack.AutoSize = false;
        _multiplierTrack.Width = 220;
        _multiplierValueLabel = new Label { AutoSize = true };
        _multiplierPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        _multiplierPanel.Controls.Add(_multiplierTrack);
        _multiplierPanel.Controls.Add(_multiplierValueLabel);
        layout.Controls.Add(_multiplierLabel, 0, 4);
        layout.Controls.Add(_multiplierPanel, 1, 4);

        _multiplierHelpLabel = new Label
        {
            Text = "Auto match target bitrate from AV1 source bitrate (approximate)",
            AutoSize = true
        };
        layout.SetColumnSpan(_multiplierHelpLabel, 2);
        layout.Controls.Add(_multiplierHelpLabel, 0, 5);

        _multiplierPreviewLabel = new Label { AutoSize = true };
        layout.SetColumnSpan(_multiplierPreviewLabel, 2);
        layout.Controls.Add(_multiplierPreviewLabel, 0, 6);

        layout.Controls.Add(new Label { Text = "Pixel format", AutoSize = true }, 0, 7);
        _pixelFormatTextBox = new TextBox { Dock = DockStyle.Fill, Text = preset.PixelFormat };
        layout.Controls.Add(_pixelFormatTextBox, 1, 7);

        layout.Controls.Add(new Label { Text = "Audio mode", AutoSize = true }, 0, 8);
        _audioModeCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _audioModeCombo.Items.AddRange(Enum.GetNames<AudioMode>());
        _audioModeCombo.SelectedItem = preset.AudioMode.ToString();
        if (_audioModeCombo.SelectedIndex < 0)
        {
            _audioModeCombo.SelectedIndex = 0;
        }
        layout.Controls.Add(_audioModeCombo, 1, 8);

        _forceDav1dCheck = new CheckBox { Text = "Force dav1d decoder", Checked = preset.ForceDav1d, AutoSize = true };
        layout.SetColumnSpan(_forceDav1dCheck, 2);
        layout.Controls.Add(_forceDav1dCheck, 0, 9);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        var okButton = new Button { Text = "OK", AutoSize = true };
        okButton.Click += OkButtonOnClick;
        var cancelButton = new Button { Text = "Cancel", AutoSize = true };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(okButton);
        buttons.Controls.Add(cancelButton);
        layout.SetColumnSpan(buttons, 2);
        layout.Controls.Add(buttons, 0, 10);

        Controls.Add(layout);

        _bitrateModeCombo.SelectedIndexChanged += (_, _) => UpdateBitrateModeUi();
        _multiplierTrack.ValueChanged += (_, _) => UpdateMultiplierPreview();
        UpdateBitrateModeUi();
    }

    private void OkButtonOnClick(object? sender, EventArgs e)
    {
        var name = _nameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Preset name is required.", "Preset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ResultPreset = new Preset
        {
            Id = _presetId,
            Name = name,
            TargetCodec = Enum.Parse<TargetCodec>(_codecCombo.SelectedItem!.ToString()!, true),
            BitrateMode = _bitrateModeCombo.SelectedIndex == 1 ? BitrateMode.MultiplierFromSource : BitrateMode.FixedKbps,
            BitrateKbps = (int)_bitrateUpDown.Value,
            Multiplier = GetMultiplierValue(),
            PixelFormat = _pixelFormatTextBox.Text.Trim(),
            AudioMode = Enum.Parse<AudioMode>(_audioModeCombo.SelectedItem!.ToString()!, true),
            ForceDav1d = _forceDav1dCheck.Checked
        };

        DialogResult = DialogResult.OK;
    }

    private void UpdateBitrateModeUi()
    {
        var isFixed = _bitrateModeCombo.SelectedIndex <= 0;
        _bitrateLabel.Visible = isFixed;
        _bitrateUpDown.Visible = isFixed;
        _multiplierLabel.Visible = !isFixed;
        _multiplierPanel.Visible = !isFixed;
        _multiplierHelpLabel.Visible = !isFixed;
        _multiplierPreviewLabel.Visible = !isFixed;

        UpdateMultiplierPreview();
    }

    private void UpdateMultiplierPreview()
    {
        if (_bitrateModeCombo.SelectedIndex != 1)
        {
            return;
        }

        var multiplier = GetMultiplierValue();
        _multiplierValueLabel.Text = $"x{multiplier:0.00}";

        var info = _probeProvider?.Invoke();
        if (info == null)
        {
            _multiplierPreviewLabel.Text = "Estimated target bitrate: — (select a file)";
            return;
        }

        var sourceKbps = info.VideoBitrateKbps ?? 0;
        if (sourceKbps <= 0)
        {
            sourceKbps = TranscodeService.GetFallbackSourceKbps(info);
        }

        var targetKbps = (int)Math.Round(sourceKbps * multiplier);
        targetKbps = Math.Clamp(targetKbps, 500, 200000);
        var sourceMbps = sourceKbps / 1000.0;
        var targetMbps = targetKbps / 1000.0;
        _multiplierPreviewLabel.Text =
            $"Estimated target bitrate: {targetMbps:0.0} Mbps (from source {sourceMbps:0.0} Mbps, x{multiplier:0.00})";
    }

    private double GetMultiplierValue()
    {
        return Math.Clamp(_multiplierTrack.Value / 100.0, 1.0, 3.0);
    }
}
