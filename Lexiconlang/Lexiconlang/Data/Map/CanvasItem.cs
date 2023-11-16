using System.Drawing;

namespace Lexiconlang.Data.Map;

public class CanvasItem {
    public static int id_last;

    public int ID;

    public int Layer = 1;

    public Coordinate Location;

    public string Map = "";

    public double Scale = 1.0;

    protected CanvasItem( string map, Coordinate location = default( Coordinate ), int layer = 1, int id = -1 ) {
        ID = id == -1 ? id_last++ : id;
        Layer = layer;
        Map = map;
        Location = new( 0, 0 );
        Scale = 1.0;
    }

    public RectangleF Bounds { get; }
}
