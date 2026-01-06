using System.Drawing;
using System.Windows.Forms;

namespace AplysiaAv1Transcoder;

public enum FfmpegSetupMode
{
    None,
    Browse,
    Download
}

public sealed class FfmpegSetupForm : Form
{
    private readonly TextBox _pathTextBox;
    private readonly TextBox _urlTextBox;

    public FfmpegSetupMode Mode { get; private set; }

    public string? SelectedFfmpegPath => _pathTextBox.Text;

    public string DownloadUrl => _urlTextBox.Text.Trim();

    public FfmpegSetupForm(string defaultUrl)
    {
        Text = "Locate FFmpeg";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        Width = 520;
        Height = 240;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 5,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var intro = new Label
        {
            Text = "FFmpeg is required. Choose an existing ffmpeg.exe or download a portable build.",
            AutoSize = true
        };
        layout.SetColumnSpan(intro, 3);
        layout.Controls.Add(intro, 0, 0);

        _pathTextBox = new TextBox { Dock = DockStyle.Fill };
        var browseButton = new Button { Text = "Browse...", AutoSize = true };
        browseButton.Click += BrowseButtonOnClick;
        layout.Controls.Add(_pathTextBox, 0, 1);
        layout.Controls.Add(browseButton, 1, 1);

        var urlLabel = new Label { Text = "Download page:", AutoSize = true };
        _urlTextBox = new TextBox { Dock = DockStyle.Fill, Text = defaultUrl, ReadOnly = true };
        layout.Controls.Add(urlLabel, 0, 2);
        layout.SetColumnSpan(_urlTextBox, 3);
        layout.Controls.Add(_urlTextBox, 0, 3);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        var downloadButton = new Button { Text = "Download && Install (Portable)", AutoSize = true };
        downloadButton.Click += DownloadButtonOnClick;
        var cancelButton = new Button { Text = "Cancel", AutoSize = true };
        cancelButton.Click += (_, _) => { Mode = FfmpegSetupMode.None; DialogResult = DialogResult.Cancel; };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(downloadButton);
        layout.SetColumnSpan(buttons, 3);
        layout.Controls.Add(buttons, 0, 4);

        Controls.Add(layout);
    }

    private void BrowseButtonOnClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "ffmpeg.exe|ffmpeg.exe|All files|*.*",
            Title = "Select ffmpeg.exe"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _pathTextBox.Text = dialog.FileName;
        Mode = FfmpegSetupMode.Browse;
        DialogResult = DialogResult.OK;
    }

    private void DownloadButtonOnClick(object? sender, EventArgs e)
    {
        Mode = FfmpegSetupMode.Download;
        DialogResult = DialogResult.OK;
    }
}
