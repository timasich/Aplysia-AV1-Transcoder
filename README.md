# Aplysia AV1 Transcoder

A compact Windows Forms AV1 -> H.264/H.265 transcoder with per-file presets, auto-match quality for new files, recent output folders, and optional FFmpeg download.

## FFmpeg setup

The app requires `ffmpeg.exe` and `ffprobe.exe`.

- **Automatic (recommended):** On first launch the app prompts to download a portable FFmpeg build. It extracts into a local `ffmpeg` folder next to the app if writable; otherwise it uses `%LOCALAPPDATA%\AplysiaAv1Transcoder`.
- **Manual:** Place FFmpeg in one of these locations:
  - `ffmpeg\bin\ffmpeg.exe` relative to the app folder (portable)
  - Any folder of your choice, then set the path via the **FFmpeg...** button in the Queue tab.

## Presets

- Presets only contain encoding settings (codec, bitrate, encoder priority, pixel format, audio mode, dav1d toggle). Trim is per-file and not stored in presets.
- Four built-in presets are always available:
  - **H264 Safe (x1.6 Bitrate) & H264 Balance (x1.3 Bitrate)**
  - **H265 Safe (x1.25 Bitrate) & H265 Balance (x1.05 Bitrate)**
- You can create new presets and change them.

## Settings and files

- `settings.json` and `presets.json` are stored in the app folder if writable. If not, they are stored in `%LOCALAPPDATA%\AplysiaAv1Transcoder`.
- Recent output folders are kept (up to 10).

## Usage

1. Add files via **Add...** or drag-drop.
2. Choose a preset and click **Apply to Selected**, or rely on auto-match.
3. Set output folder (or use **Same as source**).
4. Click **Start** to process checked items.

Logs are captured live in the **Logs** tab.
