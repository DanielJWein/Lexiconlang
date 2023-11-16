namespace Lexiconlang.Data.Database;

/// <summary>
/// Base exception for Lexiconlang Database errors
/// </summary>
[Serializable]
public class LexiconlangDatabaseException : Exception {

    protected LexiconlangDatabaseException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }

    public LexiconlangDatabaseException( ) {
    }

    public LexiconlangDatabaseException( string message ) : base( message ) {
    }

    public LexiconlangDatabaseException( string message, Exception inner ) : base( message, inner ) {
    }
}
