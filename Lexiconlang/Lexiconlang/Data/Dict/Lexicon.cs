namespace Lexiconlang.Data.Dict;

/// <summary>
/// Holds the dictionary entries
/// </summary>
public class Lexicon {

    /// <summary>
    /// Creates a new Lexicon
    /// </summary>
    /// <param name="inputLanguage">  The understandable language for this lexicon </param>
    /// <param name="outputLanguage"> The not-so-understandable language for this lexicon </param>
    /// <param name="entries">        Represents entries in the dictionary </param>
    public Lexicon( string inputLanguage, string outputLanguage, List<DictionaryEntry>? entries = null ) {
        Entries = entries ?? new( );
        InputLanguage = inputLanguage;
        OutputLanguage = outputLanguage;
    }

    /// <summary>
    /// The understandable language for this lexicon
    /// </summary>
    public string InputLanguage { get; set; }

    /// <summary>
    /// The not-so-understandable language for this lexicon
    /// </summary>
    public string OutputLanguage { get; set; }

    internal string InternalDictionaryName =>
            InputLanguage.ToLowerInvariant( ).Replace( " ", "" )
          + "_"
          + OutputLanguage.ToLowerInvariant( ).Replace( " ", "" );

    /// <summary>
    /// Represents entries in the dictionary
    /// </summary>
    protected List<DictionaryEntry> Entries { get; set; }

    /// <summary>
    /// Adds an entry
    /// </summary>
    /// <param name="entry"> The entry to add </param>
    /// <exception cref="ArgumentNullException"> Thrown if <paramref name="entry" /> is null </exception>
    public void AddEntry( DictionaryEntry entry ) {
        if ( entry is null )
            throw new ArgumentNullException( nameof( entry ), "The entry specified was null!" );
        if ( entry.WordID == -1 )
            entry.WordID = Entries.Any( ) ? Entries.Max( x => x.WordID ) + 1 : 1;

        Entries.Add( entry );
    }

    /// <summary>
    /// Gets all entries in the lexicon
    /// </summary>
    /// <returns> All entries in the lexicon </returns>
    public IEnumerable<DictionaryEntry> GetEntries( )
        => Entries.ToArray( );

    /// <summary>
    /// Gets a specific entry in the lexicon
    /// </summary>
    /// <param name="Word"> The entry to search for </param>
    /// <returns> The entry, if found. Null otherwise. </returns>
    public DictionaryEntry? GetEntry( string Word )
        => Entries.FirstOrDefault( x => x.Word == Word );

    /// <summary>
    /// Removes an entry
    /// </summary>
    /// <param name="entry"> The entry to remove </param>
    /// <exception cref="ArgumentNullException"> Thrown if <paramref name="entry" /> is null </exception>
    public void RemoveEntry( DictionaryEntry entry ) {
        if ( entry is null )
            throw new ArgumentNullException( nameof( entry ), "The entry specified was null!" );
        Entries.Remove( entry );
    }
}
