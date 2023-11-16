using System.Drawing;

namespace Lexiconlang.Data.Map;

public class PathData : CanvasItem {
    public Color Color;

    public List<Coordinate> Points;

    public float Weight;

    public PathData( string map, Coordinate location = default( Coordinate ), int layer = 1, int id = -1 ) : base( map, location, layer, id ) {
        Color = Color.Black;
        Points = new( );
        Weight = 1.0f;
    }
}
