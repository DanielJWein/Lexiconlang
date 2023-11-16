using System.Drawing;

namespace Lexiconlang.Data.Map;

public struct CanvasMap {
    public List<CanvasItem> items;

    public string Name;

    public CanvasMap( ) {
        items = new( );
    }

    public void AddImage( ImageData d ) {
        items.Add( d );
    }

    public void AddPath( PathData d ) {
        items.Add( d );
    }

    public IEnumerable<CanvasItem> GetVisibleItems( RectangleF viewRange ) {
        return items.Where( x =>
            viewRange.Contains( x.Bounds.Location )
            || viewRange.Contains( x.Bounds.X + x.Bounds.Width, x.Bounds.Y )
            || viewRange.Contains( x.Bounds.X, x.Bounds.Y + x.Bounds.Height )
            || viewRange.Contains( x.Bounds.X + x.Bounds.Width, x.Bounds.Y + x.Bounds.Height ) );
    }

    public void RemoveImage( ImageData d ) {
        items.Remove( d );
    }

    public void RemovePath( PathData d ) {
        items.Remove( d );
    }
}
