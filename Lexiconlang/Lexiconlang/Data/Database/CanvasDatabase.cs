using System.Data.SQLite;
using System.Drawing;
using System.Globalization;

using Lexiconlang.Data.Map;

namespace Lexiconlang.Data.Database;

public class CanvasDatabase : Database {
    private const string PREFIX = "canvas_";

    private const string PREFIX_IMAGES = "images_";

    private const string PREFIX_PATHS = "paths_";

    /// <summary>
    /// The schema for the definition table
    /// </summary>
    private const string SCHEMA_IMAGES = "imageid INT, location TEXT, imagedata BLOB, scale REAL, layer INT, map TEXT, PRIMARY KEY (imageid)";

    private const string SCHEMA_PATHS = "pathid INT, location TEXT, data TEXT, weight REAL, color TEXT, scale REAL, layer INT, map TEXT, PRIMARY KEY (pathid)";

    protected override string SqliteDatabaseName => "maps.db";

    public void DeregisterImage( ImageData iData ) {
        string command = "DELETE FROM " + PREFIX + PREFIX_IMAGES+ $" WHERE imageid={iData.ID}";
        SQLiteCommand cmd = new( command, dCon);
        cmd.ExecuteNonQuery( );
        cmd.Dispose( );
    }

    public void DeregisterImages( params ImageData[ ] iDatas ) {
        foreach ( var v in iDatas ) {
            DeregisterImage( v );
        }
    }

    public void DeregisterPath( PathData pData ) {
        string command = "DELETE FROM " + PREFIX + PREFIX_PATHS + $" WHERE pathid={pData.ID}";
        SQLiteCommand cmd = new( command, dCon);
        cmd.ExecuteNonQuery( );
        cmd.Dispose( );
    }

    public void DeregisterPaths( params PathData[ ] pDatas ) {
        foreach ( var v in pDatas ) {
            DeregisterPath( v );
        }
    }

    public override void Init( ) {
        base.Init( );
        checkCanvasTablesExist( );
    }

    public ImageData ReadImage( int imageId ) {
        string cmd = "SELECT (imageid, location, imagedata, scale, layer, map) FROM " + PREFIX + PREFIX_IMAGES + $" WHERE imageid = {imageId}";
        SQLiteCommand command = new( cmd, dCon );

        SQLiteDataReader dReader = command.ExecuteReader();
        if ( dReader.HasRows ) {
            dReader.Read( );
            int idHazu = (int)dReader[0];
            if ( idHazu != imageId ) {
                throw new Exception( "The expected ID did not match the ID" );
            }
            Coordinate coordinate = Coordinate.Parse((string)dReader[1]);
            byte[] data = (byte[])dReader[2];
            MemoryStream ms = new();
            ms.Write( data );
            ms.Position = 0;
            Image i = Image.FromStream(ms);
            double scale = (double)dReader[3];
            int layer = (int)dReader[4];
            string map = (string)dReader[5];

            ImageData iData = new(map,  i, coordinate, layer, idHazu) {
                Scale = scale
            };
            return iData;
        }
        throw new Exception( "The image was not found." );
    }

    public CanvasMap ReadMap( string map, int layer ) {
        CanvasMap canvasMap = new CanvasMap();
        canvasMap.items = new( );
        //Images
        SQLiteCommand command = new SQLiteCommand("SELECT imageid, location, imagedata, scale, layer, map FROM " + PREFIX + PREFIX_IMAGES + $" WHERE map = @m AND layer = {layer}", dCon);
        command.Parameters.AddWithValue( "@m", map );
        var r = command.ExecuteReader( );
        while ( r.HasRows ) {
            if ( !r.Read( ) )
                break;

            ImageData  id = ReadImage((int)r[0]);
            canvasMap.items.Add( id );
        }
        r.Close( );
        command.Dispose( );

        //Paths
        command = new SQLiteCommand( "SELECT pathid, location, data, weight, color, scale, map, layer FROM " + PREFIX + PREFIX_PATHS + $" WHERE map = @m AND layer = {layer}", dCon );
        command.Parameters.AddWithValue( "@m", map );
        r = command.ExecuteReader( );
        while ( r.HasRows ) {
            if ( !r.Read( ) )
                break;

            PathData  id = ReadPath((int)r[0]);
            canvasMap.items.Add( id );
        }
        r.Close( );
        command.Dispose( );

        return canvasMap;
    }

    public PathData ReadPath( int pathId ) {
        string cmd = "SELECT pathid, location, data, weight, color, scale, layer, map FROM " + PREFIX + PREFIX_PATHS + $" WHERE pathid = {pathId}";
        SQLiteCommand command = new( cmd, dCon );

        SQLiteDataReader dReader = command.ExecuteReader();
        if ( dReader.HasRows ) {
            dReader.Read( );
            int idHazu = (int)dReader[0];
            if ( idHazu != pathId ) {
                throw new Exception( "The expected ID did not match the ID" );
            }
            Coordinate coordinate = Coordinate.Parse((string)dReader[1]);
            List<Coordinate> points = ((string)dReader[2]).Split("|").Select(x=>Coordinate.Parse(x)).ToList();
            double weight = (double)dReader[3];
            string[] channels = ((string)dReader[4]).Split(' ');
            Color col = Color.FromArgb(int.Parse(channels[0], NumberStyles.HexNumber), int.Parse(channels[1], NumberStyles.HexNumber), int.Parse(channels[2], NumberStyles.HexNumber), int.Parse(channels[3], NumberStyles.HexNumber));
            double scale = (double)dReader[5];
            int layer = (int)dReader[6];
            string map = (string)dReader[7];

            PathData pData = new(map, coordinate, layer, idHazu) {
                Weight = (float)weight,
                Scale = scale,
                Points = points
            };
            return pData;
        }
        throw new Exception( "The path could not be found!" );
    }

    public void RegisterImage( ImageData iData ) {
        int imageId = iData.ID;
        string location = iData.Location.ToString( );

        MemoryStream ms = new();
        iData.imageData.Save( ms, System.Drawing.Imaging.ImageFormat.Png );
        ms.Position = 0;

        byte[] data = ms.ToArray();

        string map = iData.Map;
        int layer = iData.Layer;

        string command = "INSERT INTO " + PREFIX + PREFIX_IMAGES
            + $"(imageid, location, imagedata, scale, layer, map) VALUES ({imageId}, @loc, @dat, {iData.Scale}, @map, {layer});";

        SQLiteCommand sQLiteCommand = new  ( command , dCon);

        sQLiteCommand.Parameters.AddWithValue( "@loc", location );
        sQLiteCommand.Parameters.AddWithValue( "@dat", data );
        sQLiteCommand.Parameters.AddWithValue( "@map", map );

        sQLiteCommand.ExecuteNonQuery( );
        sQLiteCommand.Dispose( );
    }

    public void RegisterImages( params ImageData[ ] images ) {
        foreach ( var image in images ) {
            RegisterImage( image );
        }
    }

    public void RegisterPath( PathData pData ) {
        if ( !pData.Points.Any( ) )
            return;
        int pathid = pData.ID;
        int layer = pData.Layer;
        string location = pData.Location.ToString( );
        string data = string.Join( " | ", pData.Points.Select( x => x.ToString( ) ) );
        string color = $"{pData.Color.R:X} {pData.Color.G:X} {pData.Color.B:X} {pData.Color.A:X}";
        string map = pData.Map;
        double weight = (double) pData.Weight;
        double scale = pData.Scale;

        string command = "INSERT INTO " + PREFIX + PREFIX_PATHS
            + $"(pathid, location, data, weight, color, scale, map, layer) " +
            $"VALUES ({pathid}, @loc, @dat, {weight}, @col, {scale}, @map, {layer});";

        SQLiteCommand sQLiteCommand = new  ( command , dCon);

        sQLiteCommand.Parameters.AddWithValue( "@loc", location );
        sQLiteCommand.Parameters.AddWithValue( "@dat", data );
        sQLiteCommand.Parameters.AddWithValue( "@col", color );
        sQLiteCommand.Parameters.AddWithValue( "@map", map );

        sQLiteCommand.ExecuteNonQuery( );
        sQLiteCommand.Dispose( );
    }

    public void RegisterPaths( params PathData[ ] paths ) {
        foreach ( var path in paths ) {
            RegisterPath( path );
        }
    }

    public void DestroyAndAnnihilateEverything( ) {
        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM "+ PREFIX + PREFIX_PATHS +"; DELETE FROM " + PREFIX + PREFIX_IMAGES+";" , dCon);
        cmd.ExecuteNonQuery( );
    }
    /// <summary>
    /// Creates and initializes missing Lexicon tables.
    /// </summary>
    private void checkCanvasTablesExist( ) {
        SQLiteTransaction st = dCon.BeginTransaction();

        createTableIfNotExists( PREFIX + PREFIX_PATHS, SCHEMA_PATHS );
        createTableIfNotExists( PREFIX + PREFIX_IMAGES, SCHEMA_IMAGES );

        st.Commit( );
    }
}
