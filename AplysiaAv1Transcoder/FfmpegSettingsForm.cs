using System.Drawing;
using System.Windows.Forms;

namespace AplysiaAv1Transcoder;

public sealed class FfmpegSettingsForm : Form
{
    private readonly TextBox _pathTextBox;

    public string FfmpegPath => _pathTextBox.Text.Trim();

    public FfmpegSettingsForm(string? initialPath)
    {
        Text = "FFmpeg Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        Width = 520;
        Height = 170;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label { Text = "ffmpeg.exe path:", AutoSize = true };
        layout.SetColumnSpan(label, 3);
        layout.Controls.Add(label, 0, 0);

        _pathTextBox = new TextBox { Dock = DockStyle.Fill, Text = initialPath ?? string.Empty };
        var browseButton = new Button { Text = "Browse...", AutoSize = true };
        browseButton.Click += BrowseButtonOnClick;
        layout.Controls.Add(_pathTextBox, 0, 1);
        layout.Controls.Add(browseButton, 1, 1);

        var okButton = new Button { Text = "OK", AutoSize = true };
        okButton.Click += (_, _) => DialogResult = DialogResult.OK;
        var cancelButton = new Button { Text = "Cancel", AutoSize = true };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        layout.Controls.Add(okButton, 1, 2);
        layout.Controls.Add(cancelButton, 2, 2);

        Controls.Add(layout);
    }

    private void BrowseButtonOnClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "ffmpeg.exe|ffmpeg.exe|All files|*.*",
            Title = "Select ffmpeg.exe"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _pathTextBox.Text = dialog.FileName;
        }
    }
}
