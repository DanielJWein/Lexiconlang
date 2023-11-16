namespace Lexiconlang.Data.Database;

[Serializable]
public class LanguageAlreadyRegisteredException : LexiconlangDatabaseException {

    protected LanguageAlreadyRegisteredException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }

    public LanguageAlreadyRegisteredException( ) : base( "The language was already registered!" ) {
    }

    public LanguageAlreadyRegisteredException( string message ) : base( message ) {
    }

    public LanguageAlreadyRegisteredException( string message, Exception inner ) : base( message, inner ) {
    }
}
