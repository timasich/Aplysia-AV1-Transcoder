using System.Windows.Forms;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder;

public sealed class PresetEditorForm : Form
{
    private readonly TextBox _nameTextBox;
    private readonly ComboBox _codecCombo;
    private readonly ComboBox _priorityCombo;
    private readonly NumericUpDown _bitrateUpDown;
    private readonly ComboBox _nvencPresetCombo;
    private readonly TextBox _pixelFormatTextBox;
    private readonly ComboBox _audioModeCombo;
    private readonly CheckBox _forceDav1dCheck;

    public Preset? ResultPreset { get; private set; }

    public PresetEditorForm(Preset preset, string title)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Width = 420;
        Height = 390;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < 8; i++)
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

        layout.Controls.Add(new Label { Text = "Encoder priority", AutoSize = true }, 0, 2);
        _priorityCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _priorityCombo.Items.AddRange(Enum.GetNames<EncoderPriority>());
        _priorityCombo.SelectedItem = preset.EncoderPriority.ToString();
        if (_priorityCombo.SelectedIndex < 0)
        {
            _priorityCombo.SelectedIndex = 0;
        }
        layout.Controls.Add(_priorityCombo, 1, 2);

        layout.Controls.Add(new Label { Text = "Bitrate (kbps)", AutoSize = true }, 0, 3);
        _bitrateUpDown = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = 1,
            Maximum = 200000,
            Value = Math.Max(1, preset.BitrateKbps)
        };
        layout.Controls.Add(_bitrateUpDown, 1, 3);

        layout.Controls.Add(new Label { Text = "NVENC preset", AutoSize = true }, 0, 4);
        _nvencPresetCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _nvencPresetCombo.Items.AddRange(new object[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7" });
        _nvencPresetCombo.SelectedItem = preset.NvencPreset;
        if (_nvencPresetCombo.SelectedIndex < 0)
        {
            _nvencPresetCombo.SelectedItem = "p5";
        }
        layout.Controls.Add(_nvencPresetCombo, 1, 4);

        layout.Controls.Add(new Label { Text = "Pixel format", AutoSize = true }, 0, 5);
        _pixelFormatTextBox = new TextBox { Dock = DockStyle.Fill, Text = preset.PixelFormat };
        layout.Controls.Add(_pixelFormatTextBox, 1, 5);

        layout.Controls.Add(new Label { Text = "Audio mode", AutoSize = true }, 0, 6);
        _audioModeCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _audioModeCombo.Items.AddRange(Enum.GetNames<AudioMode>());
        _audioModeCombo.SelectedItem = preset.AudioMode.ToString();
        if (_audioModeCombo.SelectedIndex < 0)
        {
            _audioModeCombo.SelectedIndex = 0;
        }
        layout.Controls.Add(_audioModeCombo, 1, 6);

        _forceDav1dCheck = new CheckBox { Text = "Force dav1d decoder", Checked = preset.ForceDav1d, AutoSize = true };
        layout.SetColumnSpan(_forceDav1dCheck, 2);
        layout.Controls.Add(_forceDav1dCheck, 0, 7);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        var okButton = new Button { Text = "OK", AutoSize = true };
        okButton.Click += OkButtonOnClick;
        var cancelButton = new Button { Text = "Cancel", AutoSize = true };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(okButton);
        buttons.Controls.Add(cancelButton);
        layout.SetColumnSpan(buttons, 2);
        layout.Controls.Add(buttons, 0, 8);

        Controls.Add(layout);
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
            Name = name,
            TargetCodec = Enum.Parse<TargetCodec>(_codecCombo.SelectedItem!.ToString()!, true),
            EncoderPriority = Enum.Parse<EncoderPriority>(_priorityCombo.SelectedItem!.ToString()!, true),
            BitrateKbps = (int)_bitrateUpDown.Value,
            NvencPreset = _nvencPresetCombo.SelectedItem?.ToString() ?? "p5",
            PixelFormat = _pixelFormatTextBox.Text.Trim(),
            AudioMode = Enum.Parse<AudioMode>(_audioModeCombo.SelectedItem!.ToString()!, true),
            ForceDav1d = _forceDav1dCheck.Checked
        };

        DialogResult = DialogResult.OK;
    }
}
