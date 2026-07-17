using LLMGameCreator.Application.Play.GeneratedCampaign;

namespace LLMGameCreator.WinForms.Pages;

public sealed class GeneratedCampaignMapControl : Control
{
    public GeneratedCampaignMapProjection? Projection { get; set; }
    public event EventHandler<(int X, int Y)>? CellClicked;
    public GeneratedCampaignMapControl() { DoubleBuffered = true; TabStop = false; }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); if (Projection is null || Projection.Width == 0 || Projection.Height == 0) return; var size=Math.Max(16,Math.Min(ClientSize.Width/Projection.Width,ClientSize.Height/Projection.Height)); foreach(var cell in Projection.Cells){var rect=new Rectangle(cell.X*size,cell.Y*size,size,size); e.Graphics.FillRectangle(cell.Blocked?Brushes.DimGray:Brushes.WhiteSmoke,rect);e.Graphics.DrawRectangle(Pens.LightGray,rect);TextRenderer.DrawText(e.Graphics,cell.PrimarySymbol,Font,rect,cell.PlayerPresent?Color.DarkBlue:Color.Black,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);} }
    protected override void OnMouseClick(MouseEventArgs e) { if(Projection is null)return;var size=Math.Max(16,Math.Min(ClientSize.Width/Projection.Width,ClientSize.Height/Projection.Height));CellClicked?.Invoke(this,(e.X/size,e.Y/size)); }
}
