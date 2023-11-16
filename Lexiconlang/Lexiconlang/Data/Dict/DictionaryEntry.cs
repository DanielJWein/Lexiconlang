namespace Lexiconlang.Data.Dict;

/// <summary>
/// Represents a word in the dictionary
/// </summary>
public class DictionaryEntry {

    /// <summary>
    /// Creates a new Dictionary Entry
    /// </summary>
    /// <param name="word">        The word as written in the output language </param>
    /// <param name="definitions">
    /// A list of definitions for this word. If null, a new list is created.
    /// </param>
    /// <exception cref="ArgumentNullException"> Thrown if <paramref name="word" /> is null </exception>
    public DictionaryEntry( string word, List<DictionaryDefinition>? definitions = null ) {
        Word = word ?? throw new ArgumentNullException( nameof( word ) );
        Definitions = definitions ?? new( );
    }

    /// <summary>
    /// The definitions of this word
    /// </summary>
    protected List<DictionaryDefinition> Definitions { get; set; }

    /// <summary>
    /// Holds the number of definitions
    /// </summary>
    public int NumDefinitions => Definitions.Count;

    /// <summary>
    /// The actual writing of the word
    /// </summary>
    public string Word { get; set; }

    public int WordID { get; set; } = -1;

    /// <summary>
    /// Adds an definition
    /// </summary>
    /// <param name="definition"> The definition to add </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="definition" /> is null
    /// </exception>
    public void AddDefinition( DictionaryDefinition definition ) {
        if ( definition is null )
            throw new ArgumentNullException( nameof( definition ), "The definition specified was null!" );
        Definitions.Add( definition );
    }

    /// <summary>
    /// Gets all definitions for this word
    /// </summary>
    /// <returns> The definitions found </returns>
    public IEnumerable<DictionaryDefinition> GetAllDefinitions( ) => Definitions.ToArray( );

    /// <summary>
    /// Gets a specified definition
    /// </summary>
    /// <param name="DefinitionNumber"> The Definition Number to get </param>
    /// <returns> The definition </returns>
    public DictionaryDefinition GetDefinition( int DefinitionNumber ) {
        //if ( DefinitionNumber >= Definitions.Count )
        return Definitions.FirstOrDefault( x => x.DefinitionNumber == DefinitionNumber );
        //return Definitions[ DefinitionNumber ];
    }

    /// <summary>
    /// Removes an definition
    /// </summary>
    /// <param name="definition"> The definition to remove </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="definition" /> is null
    /// </exception>
    public void RemoveDefinition( DictionaryDefinition definition ) {
        if ( definition is null )
            throw new ArgumentNullException( nameof( definition ), "The definition specified was null!" );
        Definitions.Remove( definition );
    }
}
