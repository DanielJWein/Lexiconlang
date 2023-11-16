namespace Lexiconlang.Data.Map;

public struct Coordinate {
    public double x = 0, y = 0;

    public Coordinate( double x, double y ) {
        this.x = x;
        this.y = y;
    }

    public static Coordinate Parse( string val ) {
        string[] tokens = val.Split(',');
        return tokens.Length == 2
            ? new Coordinate( double.Parse( tokens[ 0 ] ), double.Parse( tokens[ 1 ] ) )
            : throw new Exception( "The coordinate was not in a correct format!" );
    }

    public double[ ] ToDoublePair( ) {
        return new[ ] { x, y };
    }

    public override string ToString( ) => $"{x}, {y}";
}
