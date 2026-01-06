# Aplysia AV1 Transcoder

A lightweight, portable Windows tool for transcoding **AV1 video** to **H.264 or H.265** using FFmpeg.

Designed to be simple, reliable, and transparent:
- no installers
- no background services
- no bundled FFmpeg
- full control over encoding parameters

---

## Features

- Transcode **AV1 → H.264 / H.265**
- Hardware acceleration support (NVENC, AMD AMF, Intel QSV, CPU fallback)
- Preset-based workflow (Safe / Balanced, user presets)
- Per-file Trim (non-destructive)
- Batch processing
- Drag & Drop support
- Clear FFmpeg command logging
- Portable (single `.exe`)

---

## Requirements

- **Windows 10 or Windows 11 (x64)**
- **.NET Desktop Runtime 8**
- **FFmpeg for Windows (x64) with AV1 support (dav1d)**

FFmpeg is **not bundled** and must be provided by the user.

---

## Getting FFmpeg

Download a Windows x64 build with AV1 support (dav1d), for example:

- https://www.gyan.dev/ffmpeg/builds/

Recommended:
- **Full build**
- Windows x64
- Contains `ffmpeg.exe` and `ffprobe.exe`

You can:
- place FFmpeg next to the program, or
- select its path inside the application

The app will guide you if FFmpeg is missing.

---

## Quick Start

1. Download and extract `AplysiaAV1Transcoder.exe`
2. Launch the application
3. Add AV1 video files (Drag & Drop or Add Files)
4. Choose or create a preset
5. Select output folder
6. Click **Start**

Non-AV1 files are automatically skipped.

---

## Notes

- This tool is intended **only for AV1 input**.
- Files without readable duration may have limited trim functionality.
- FFmpeg commands used for encoding are fully visible in Logs.

---

## License

MIT License

This project uses FFmpeg, which is licensed under LGPL/GPL.
FFmpeg is not distributed with this application.
