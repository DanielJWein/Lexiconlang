using System.Data.SQLite;

namespace Lexiconlang.Data.Database;

public class Database {
    internal readonly SQLiteConnection dCon = new( );

    protected  virtual string SqliteDatabaseName => "database.db";

    /// <summary>
    /// Checks if a table exists
    /// </summary>
    /// <param name="tableName"> The table to search for </param>
    /// <returns> True, if the table was found. False otherwise. </returns>
    protected bool checkIfTableExists( string tableName ) {
        //Setup; create objects and get ready to execute
        SQLiteParameter p = parameterizeUnsafeInput(tableName);
        SQLiteCommand cmd = new( dCon ) {
            CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name=@A;"
        };
        cmd.Parameters.Add( p );

        //Actually execute the query
        SQLiteDataReader scl = cmd.ExecuteReader();

        //Copy HasRows (which will be true if the table existed) into bool so we can delete objects
        bool wasExist = scl.HasRows;

        //Delete objects and return
        scl.Close( );
        cmd.Dispose( );
        return wasExist;
    }

    /// <summary>
    /// Creates a table if it doesn't already exist. By NO MEANS should user input enter this function!!
    /// </summary>
    /// <param name="tableName">   The name of the table. </param>
    /// <param name="tableSchema"> The schema of the table. See remarks for examples. </param>
    /// <returns> true, if the table was created. </returns>
    /// <remarks> Example <paramref name="tableSchema" />: "name VARCHAR(20), score INT" </remarks>
    protected bool createTableIfNotExists( string tableName, string tableSchema ) {
        if ( checkIfTableExists( tableName ) )
            return false;
        SQLiteCommand cmd = new($"CREATE TABLE {tableName} ({tableSchema});", dCon);
        cmd.ExecuteNonQuery( );
        return true;
    }

    /// <summary>
    /// The magic bullet to kill any SQLI attacks.
    /// </summary>
    /// <param name="input"> The input that we're parameterizing </param>
    /// <param name="name">  The name of the variable. "@A" if undefined. </param>
    /// <returns> </returns>
    protected SQLiteParameter parameterizeUnsafeInput( object input, string name = "@A" )
        => new( name, input ?? "" );

    /// <summary>
    /// Opens the connection and creates the database if it does not exist.
    /// </summary>
    public virtual void Init( ) {
        //Create the connection string
        SQLiteConnectionStringBuilder builder = new( ) {
            DataSource = SqliteDatabaseName,
            Version = 3
        };

        //If the database does not exist already, create it.
        if ( !File.Exists( SqliteDatabaseName ) )
            SQLiteConnection.CreateFile( SqliteDatabaseName );

        dCon.ConnectionString = builder.ConnectionString;
        //Open the actual connection
        dCon.Open( );

        SQLiteCommand cmd2 = new("PRAGMA journal_mode = WAL;", dCon);
        cmd2.ExecuteNonQuery( );
        cmd2.Dispose( );
    }

    /// <summary>
    /// Closes the sqlite connection
    /// </summary>
    public virtual void Unit( ) {
        SQLiteCommand cmd2 = new("VACUUM;", dCon);
        cmd2.ExecuteNonQuery( );
        cmd2.Dispose( );
        dCon.Close( );
        dCon.Dispose( );
    }
}
