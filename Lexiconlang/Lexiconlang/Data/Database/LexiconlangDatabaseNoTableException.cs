namespace Lexiconlang.Data.Database;

[Serializable]
public class LexiconlangDatabaseNoTableException : LexiconlangDatabaseException {

    protected LexiconlangDatabaseNoTableException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }

    public LexiconlangDatabaseNoTableException( ) : base( "There was no table to write to!" ) {
    }

    public LexiconlangDatabaseNoTableException( string message ) : base( message ) {
    }

    public LexiconlangDatabaseNoTableException( string message, Exception inner ) : base( message, inner ) {
    }
}
