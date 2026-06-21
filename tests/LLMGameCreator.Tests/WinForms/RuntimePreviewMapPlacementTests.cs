using System.Drawing;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Runtime;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class RuntimePreviewMapPlacementTests
{
    [Fact]
    public void RuntimeMapCanvas_RendersGeneratedMarkersAndPlayerTogether()
    {
        var package = GeneratedMapPlacementPreviewServiceTests.CreatePackage();
        var state = new GameState
        {
            CurrentMapId = "map/start",
            PlayerPosition = new Position2D(1, 1)
        };
        var markers = new[]
        {
            new GeneratedRuntimeMapMarker
            {
                MarkerId = "generated-marker/npc/npc/guide",
                Type = GeneratedRuntimeMapMarkerType.Npc,
                SourceId = "npc/guide",
                MapId = "map/start",
                Position = new Position2D(2, 1)
            },
            new GeneratedRuntimeMapMarker
            {
                MarkerId = "generated-marker/encounter/encounter/road",
                Type = GeneratedRuntimeMapMarkerType.Encounter,
                SourceId = "encounter/road",
                MapId = "map/start",
                Position = new Position2D(3, 1)
            }
        };

        using var canvas = new RuntimeMapCanvas { Size = new Size(128, 128) };
        canvas.SetGeneratedMarkers(markers);
        canvas.SetState(package, state);
        using var bitmap = new Bitmap(canvas.Width, canvas.Height);

        canvas.DrawToBitmap(bitmap, canvas.ClientRectangle);

        Assert.Equal(Color.DeepSkyBlue.ToArgb(), bitmap.GetPixel(48, 48).ToArgb());
        Assert.NotEqual(Color.Gray.ToArgb(), bitmap.GetPixel(80, 48).ToArgb());
        Assert.NotEqual(Color.Gray.ToArgb(), bitmap.GetPixel(112, 48).ToArgb());
    }
}
