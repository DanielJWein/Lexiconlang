using System.Runtime.CompilerServices;

using Lexiconlang.Data.Map;

namespace Lexiconlang.Data.Database;

public enum ScaleFrom {
    TopLeft,

    Center
};

public static class MapHelper {

    public static IEnumerable<Coordinate> ScaleCoordinates( IEnumerable<Coordinate> coords, double scaleFactor, ScaleFrom where = ScaleFrom.TopLeft ) {
        List<Coordinate> coordinates = new();
        double minX = coords.Min( x => x.x );
        double minY = coords.Min( x => x.y );
        double maxX = coords.Max( x => x.x );
        double maxY = coords.Max( x => x.y );
        Coordinate scaleAnchor = where switch {
            ScaleFrom.TopLeft => new Coordinate(minX, minY),
            ScaleFrom.Center => new Coordinate(( maxX + minX ) / 2, ( maxY + minY ) / 2),
            _ => new Coordinate(minX, minY)
        };

        foreach ( Coordinate coord in coords ) {
            double dX = coord.x - scaleAnchor.x;
            double dY = coord.y - scaleAnchor.y;

            dX *= scaleFactor;
            dY *= scaleFactor;

            Coordinate final = new (scaleAnchor.x + dX, scaleAnchor.y + dY);
            coordinates.Add( final );
        }
        return coordinates;
    }

    public static string ToJSColor( this System.Drawing.Color c ) {
        return $"#{c.R:X}{c.G:X}{c.B:X}{c.A:X}";
    }

    public static IEnumerable<Coordinate> TranslateCoordinates( IEnumerable<Coordinate> coords, Coordinate offset ) {
        List<Coordinate> coordinates = new();
        foreach ( Coordinate coord in coords ) {
            Coordinate final = new (offset.x + coord.x, offset.y + coord.y);
            coordinates.Add( final );
        }
        return coordinates;
    }
}
