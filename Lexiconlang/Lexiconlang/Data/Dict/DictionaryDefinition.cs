namespace Lexiconlang.Data.Dict;

/// <summary>
/// Represents the definition of a word in the dictionary
/// </summary>
public class DictionaryDefinition {

    public DictionaryDefinition( int definitionNumber, string partOfSpeech, string definition, string? exampleSentence = null, string? exampleTranslation = null, string? notes = null ) {
        DefinitionNumber = definitionNumber;
        PartOfSpeech = partOfSpeech;
        Definition = definition;
        ExampleSentence = exampleSentence;
        ExampleTranslation = exampleTranslation;
        Notes = notes;
    }

    /// <summary>
    /// The actual definition text
    /// </summary>
    public string Definition { get; set; }

    /// <summary>
    /// The number next to the definition
    /// </summary>
    public int DefinitionNumber { get; set; }

    /// <summary>
    /// An example sentence for the definition
    /// </summary>
    public string? ExampleSentence { get; set; }

    /// <summary>
    /// An example translation for the definition
    /// </summary>
    public string? ExampleTranslation { get; set; }

    /// <summary>
    /// Any notes about this definition
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// The part of speech this definition defines
    /// </summary>
    public string PartOfSpeech { get; set; }
}
