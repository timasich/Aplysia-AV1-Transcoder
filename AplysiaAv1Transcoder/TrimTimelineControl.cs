using System.Drawing;
using System.Windows.Forms;

namespace AplysiaAv1Transcoder;

public sealed class TrimTimelineControl : Control
{
    private const int HandleWidth = 8;
    private const int TrackHeight = 10;
    private const int PaddingX = 6;

    private enum DragHandle
    {
        None,
        Start,
        End
    }

    private DragHandle _dragHandle;
    private double _durationSeconds;
    private double _startSeconds;
    private double _endSeconds;

    public event EventHandler? RangeChanged;

    public TrimTimelineControl()
    {
        DoubleBuffered = true;
        MinimumSize = new Size(140, 28);
        Size = new Size(200, 32);
    }

    public double DurationSeconds
    {
        get => _durationSeconds;
        set
        {
            var next = Math.Max(0, value);
            if (Math.Abs(next - _durationSeconds) < 0.001)
            {
                return;
            }

            _durationSeconds = next;
            ClampRange();
            Invalidate();
        }
    }

    public double StartSeconds
    {
        get => _startSeconds;
        set
        {
            var next = Math.Max(0, value);
            if (Math.Abs(next - _startSeconds) < 0.001)
            {
                return;
            }

            _startSeconds = next;
            ClampRange();
            Invalidate();
        }
    }

    public double EndSeconds
    {
        get => _endSeconds;
        set
        {
            var next = Math.Max(0, value);
            if (Math.Abs(next - _endSeconds) < 0.001)
            {
                return;
            }

            _endSeconds = next;
            ClampRange();
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        var trackRect = GetTrackRect();
        using var trackBrush = new SolidBrush(Color.Gainsboro);
        using var rangeBrush = new SolidBrush(Color.SteelBlue);
        using var handleBrush = new SolidBrush(Color.DodgerBlue);
        using var borderPen = new Pen(Color.DarkGray);

        g.FillRectangle(trackBrush, trackRect);

        if (_durationSeconds > 0)
        {
            var startX = SecondsToX(_startSeconds);
            var endX = SecondsToX(_endSeconds);
            var rangeRect = Rectangle.FromLTRB(Math.Min(startX, endX), trackRect.Top, Math.Max(startX, endX), trackRect.Bottom);
            g.FillRectangle(rangeBrush, rangeRect);

            var handleRectStart = new Rectangle(startX - HandleWidth / 2, trackRect.Top - 6, HandleWidth, trackRect.Height + 12);
            var handleRectEnd = new Rectangle(endX - HandleWidth / 2, trackRect.Top - 6, HandleWidth, trackRect.Height + 12);
            g.FillRectangle(handleBrush, handleRectStart);
            g.FillRectangle(handleBrush, handleRectEnd);
        }

        g.DrawRectangle(borderPen, trackRect);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        _dragHandle = GetHandleAtPoint(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragHandle == DragHandle.None || _durationSeconds <= 0)
        {
            return;
        }

        var seconds = XToSeconds(e.X);
        var changed = false;
        if (_dragHandle == DragHandle.Start)
        {
            var next = Math.Min(seconds, _endSeconds);
            if (Math.Abs(next - _startSeconds) > 0.001)
            {
                _startSeconds = next;
                changed = true;
            }
        }
        else if (_dragHandle == DragHandle.End)
        {
            var next = Math.Max(seconds, _startSeconds);
            if (Math.Abs(next - _endSeconds) > 0.001)
            {
                _endSeconds = next;
                changed = true;
            }
        }

        if (changed)
        {
            Invalidate();
            RangeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragHandle = DragHandle.None;
    }

    private void ClampRange()
    {
        if (_durationSeconds <= 0)
        {
            _startSeconds = 0;
            _endSeconds = 0;
            return;
        }

        _startSeconds = Math.Clamp(_startSeconds, 0, _durationSeconds);
        _endSeconds = Math.Clamp(_endSeconds, 0, _durationSeconds);
        if (_startSeconds > _endSeconds)
        {
            _startSeconds = _endSeconds;
        }
    }

    private Rectangle GetTrackRect()
    {
        var width = Math.Max(40, Width - PaddingX * 2);
        var x = PaddingX;
        var y = (Height - TrackHeight) / 2;
        return new Rectangle(x, y, width, TrackHeight);
    }

    private int SecondsToX(double seconds)
    {
        var rect = GetTrackRect();
        if (_durationSeconds <= 0)
        {
            return rect.Left;
        }

        var ratio = seconds / _durationSeconds;
        return rect.Left + (int)Math.Round(rect.Width * ratio);
    }

    private double XToSeconds(int x)
    {
        var rect = GetTrackRect();
        if (rect.Width <= 0 || _durationSeconds <= 0)
        {
            return 0;
        }

        var clamped = Math.Clamp(x, rect.Left, rect.Right);
        var ratio = (clamped - rect.Left) / (double)rect.Width;
        return ratio * _durationSeconds;
    }

    private DragHandle GetHandleAtPoint(Point point)
    {
        if (_durationSeconds <= 0)
        {
            return DragHandle.None;
        }

        var startX = SecondsToX(_startSeconds);
        var endX = SecondsToX(_endSeconds);
        var startRect = new Rectangle(startX - HandleWidth, 0, HandleWidth * 2, Height);
        var endRect = new Rectangle(endX - HandleWidth, 0, HandleWidth * 2, Height);

        if (startRect.Contains(point))
        {
            return DragHandle.Start;
        }

        if (endRect.Contains(point))
        {
            return DragHandle.End;
        }

        return DragHandle.None;
    }
}
