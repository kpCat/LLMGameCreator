using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class RuntimeMapCanvas : Control
{
    private GamePackageDefinition? _package;
    private GameState? _state;
    private IReadOnlyList<GeneratedRuntimeMapMarker> _generatedMarkers = Array.Empty<GeneratedRuntimeMapMarker>();
    private const int CellSize = 32;

    public RuntimeMapCanvas()
    {
        InitializeComponent();
    }

    public event Action<PlayerCommand>? CommandRequested;

    public void SetState(GamePackageDefinition package, GameState state)
    {
        _package = package;
        _state = state;
        Invalidate();
        Focus();
    }

    public void SetGeneratedMarkers(IReadOnlyList<GeneratedRuntimeMapMarker> markers)
    {
        _generatedMarkers = markers ?? Array.Empty<GeneratedRuntimeMapMarker>();
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData is Keys.W or Keys.A or Keys.S or Keys.D or Keys.Enter || base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.W) CommandRequested?.Invoke(PlayerCommand.Move(Direction2D.Up));
        if (e.KeyCode == Keys.S) CommandRequested?.Invoke(PlayerCommand.Move(Direction2D.Down));
        if (e.KeyCode == Keys.A) CommandRequested?.Invoke(PlayerCommand.Move(Direction2D.Left));
        if (e.KeyCode == Keys.D) CommandRequested?.Invoke(PlayerCommand.Move(Direction2D.Right));
        if (e.KeyCode == Keys.Enter) CommandRequested?.Invoke(PlayerCommand.Interact());
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_package == null || _state == null)
        {
            DrawCenteredText(e.Graphics, "Нет запущенного runtime. Нажми Старт.");
            return;
        }

        var map = _package.Game.Maps.FirstOrDefault(m => m.Id == _state.CurrentMapId);
        if (map == null)
        {
            DrawCenteredText(e.Graphics, "Карта не найдена.");
            return;
        }

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tileId = map.Tiles.FirstOrDefault(t => t.X == x && t.Y == y)?.TileId ?? map.DefaultTileId;
                var tile = _package.Game.TilePrototypes.FirstOrDefault(t => t.Id == tileId);
                using var brush = new SolidBrush(GetTileColor(tile));
                e.Graphics.FillRectangle(brush, x * CellSize, y * CellSize, CellSize, CellSize);
                e.Graphics.DrawRectangle(Pens.DimGray, x * CellSize, y * CellSize, CellSize, CellSize);
            }
        }

        foreach (var entity in map.Entities)
        {
            using var brush = new SolidBrush(Color.Goldenrod);
            e.Graphics.FillEllipse(brush, entity.Position.X * CellSize + 6, entity.Position.Y * CellSize + 6, CellSize - 12, CellSize - 12);
        }

        foreach (var marker in _generatedMarkers.Where(marker =>
                     string.Equals(marker.MapId, map.Id, StringComparison.OrdinalIgnoreCase)))
        {
            DrawGeneratedMarker(e.Graphics, marker);
        }

        using var playerBrush = new SolidBrush(Color.DeepSkyBlue);
        e.Graphics.FillRectangle(playerBrush, _state.PlayerPosition.X * CellSize + 5, _state.PlayerPosition.Y * CellSize + 5, CellSize - 10, CellSize - 10);
    }

    private void DrawGeneratedMarker(Graphics graphics, GeneratedRuntimeMapMarker marker)
    {
        var left = marker.Position.X * CellSize + 7;
        var top = marker.Position.Y * CellSize + 7;
        var size = CellSize - 14;
        var label = marker.Type == GeneratedRuntimeMapMarkerType.Npc ? "N" : "!";
        var color = marker.Type == GeneratedRuntimeMapMarkerType.Npc ? Color.MediumSeaGreen : Color.OrangeRed;

        using var brush = new SolidBrush(color);
        using var textBrush = new SolidBrush(Color.White);
        if (marker.Type == GeneratedRuntimeMapMarkerType.Npc)
        {
            graphics.FillEllipse(brush, left, top, size, size);
        }
        else
        {
            graphics.FillPolygon(brush, new[]
            {
                new Point(left + size / 2, top),
                new Point(left + size, top + size / 2),
                new Point(left + size / 2, top + size),
                new Point(left, top + size / 2)
            });
        }

        var labelSize = graphics.MeasureString(label, Font);
        graphics.DrawString(
            label,
            Font,
            textBrush,
            marker.Position.X * CellSize + (CellSize - labelSize.Width) / 2,
            marker.Position.Y * CellSize + (CellSize - labelSize.Height) / 2);
    }

    private static Color GetTileColor(TilePrototypeDefinition? tile)
    {
        if (tile == null) return Color.Magenta;
        if (!tile.Walkable) return Color.DarkSlateGray;
        if (tile.Id.Contains("grass", StringComparison.OrdinalIgnoreCase)) return Color.ForestGreen;
        if (tile.Id.Contains("water", StringComparison.OrdinalIgnoreCase)) return Color.DarkBlue;
        if (tile.Id.Contains("road", StringComparison.OrdinalIgnoreCase)) return Color.SaddleBrown;
        return Color.Gray;
    }

    private void DrawCenteredText(Graphics graphics, string text)
    {
        using var brush = new SolidBrush(Color.White);
        var size = graphics.MeasureString(text, Font);
        graphics.DrawString(text, Font, brush, (Width - size.Width) / 2, (Height - size.Height) / 2);
    }
}
