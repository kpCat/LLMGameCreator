using LLMGameCreator.Application.Play.GeneratedCampaign;

namespace LLMGameCreator.WinForms.Pages;

public sealed class GeneratedCampaignMapControl : Control
{
    private const int MinimumCellSize = 28;
    private readonly ToolTip _toolTip = new();
    private GeneratedCampaignMapProjection? _projection;
    private (int X, int Y)? _lastTooltipCell;

    public GeneratedCampaignMapControl()
    {
        DoubleBuffered = true;
        TabStop = false;
    }

    public GeneratedCampaignMapProjection? Projection
    {
        get => _projection;
        set
        {
            _projection = value;
            _lastTooltipCell = null;
            if (value is { Width: > 0, Height: > 0 })
                Size = new Size(value.Width * MinimumCellSize + 1, value.Height * MinimumCellSize + 1);
            Invalidate();
        }
    }

    public event EventHandler<(int X, int Y)>? CellClicked;

    protected override void Dispose(bool disposing)
    {
        if (disposing) _toolTip.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);
        if (Projection is not { Width: > 0, Height: > 0 }) return;
        var cellSize = CellSize();
        foreach (var cell in Projection.Cells)
        {
            var rectangle = new Rectangle(cell.X * cellSize, cell.Y * cellSize, cellSize, cellSize);
            eventArgs.Graphics.FillRectangle(cell.Blocked ? Brushes.DimGray : Brushes.WhiteSmoke, rectangle);
            if (cell.InteractionAvailable)
                eventArgs.Graphics.FillRectangle(Brushes.LightGoldenrodYellow,
                    Rectangle.Inflate(rectangle, -3, -3));
            eventArgs.Graphics.DrawRectangle(Pens.Silver, rectangle);
            TextRenderer.DrawText(
                eventArgs.Graphics,
                cell.PrimarySymbol,
                Font,
                rectangle,
                cell.PlayerPresent ? Color.DarkBlue : Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    protected override void OnMouseMove(MouseEventArgs eventArgs)
    {
        base.OnMouseMove(eventArgs);
        var cell = CellAt(eventArgs.Location);
        if (cell == _lastTooltipCell) return;
        _lastTooltipCell = cell;
        var projection = Projection?.Cells.FirstOrDefault(item => cell is not null
            && item.X == cell.Value.X && item.Y == cell.Value.Y);
        _toolTip.SetToolTip(this, projection?.PrimaryTitle ?? string.Empty);
    }

    protected override void OnMouseClick(MouseEventArgs eventArgs)
    {
        base.OnMouseClick(eventArgs);
        var cell = CellAt(eventArgs.Location);
        if (cell is not null) CellClicked?.Invoke(this, cell.Value);
    }

    private (int X, int Y)? CellAt(Point point)
    {
        if (Projection is not { Width: > 0, Height: > 0 }) return null;
        var cellSize = CellSize();
        var x = point.X / cellSize;
        var y = point.Y / cellSize;
        return x >= 0 && y >= 0 && x < Projection.Width && y < Projection.Height ? (x, y) : null;
    }

    private int CellSize() => Projection is null
        ? MinimumCellSize
        : Math.Max(MinimumCellSize, Math.Min(
            Math.Max(MinimumCellSize, ClientSize.Width) / Projection.Width,
            Math.Max(MinimumCellSize, ClientSize.Height) / Projection.Height));
}
