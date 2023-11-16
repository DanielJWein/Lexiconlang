using System.Data.SQLite;

using Lexiconlang.Data.Dict;

using DictionaryEntry = Lexiconlang.Data.Dict.DictionaryEntry;
using Lexicon = Lexiconlang.Data.Dict.Lexicon;

namespace Lexiconlang.Data.Database;

public sealed class LexiconlangDatabase : Database {

    /// <summary>
    /// The prefix for all table names
    /// </summary>
    private const string PREFIX = "lexicon_";

    /// <summary>
    /// The prefix for the definition table name
    /// </summary>
    private const string PREFIX_DEFS = PREFIX + "defs_";

    ///<summary>
    /// The prefix for the words table name
    /// </summary>
    private const string PREFIX_WORDS = PREFIX + "words_";

    /// <summary>
    /// The schema for the definition table
    /// </summary>
    private const string SCHEMA_DEF = "wordid INT, defnum INT, deftext TEXT, partofspeech TEXT, example TEXT, exampletranslation TEXT, notes TEXT, PRIMARY KEY (wordid, defnum)";

    /// <summary>
    /// The schema for the word table
    /// </summary>
    private const string SCHEMA_WORD = "id INT PRIMARY KEY, word TEXT";

    /// <summary>
    /// The table for language registrations
    /// </summary>
    private const string TABLE_LANGUAGES = PREFIX + "languages";

    private SQLiteCommand c_deleteDefinitions;

    private SQLiteCommand c_deleteWord;

    /// <summary>
    /// Cached command to insert a definition
    /// </summary>
    private SQLiteCommand c_insertDef;

    /// <summary>
    /// Cached command to insert a word
    /// </summary>
    private SQLiteCommand c_insertWord;

    /// <summary>
    /// Locks its related c_ field
    /// </summary>
    private object lck_deleteDefinitions = new( ), lck_deleteWord = new( ), lck_insertDef = new( ), lck_insertWord = new( );

    public void DropLexicon( string pairName ) {
        SQLiteCommand c_deleteTable;

        c_deleteTable = new( $"DROP TABLE {PREFIX_DEFS}{pairName};", dCon );
        c_deleteTable.ExecuteNonQuery( );
        c_deleteTable.Dispose( );

        c_deleteTable = new( $"DROP TABLE {PREFIX_WORDS}{pairName};", dCon );
        c_deleteTable.ExecuteNonQuery( );
        c_deleteTable.Dispose( );

        c_deleteTable = new( $"DELETE FROM {TABLE_LANGUAGES} WHERE internalname = '{pairName}';", dCon );
        c_deleteTable.ExecuteNonQuery( );
        c_deleteTable.Dispose( );
    }

    /// <summary>
    /// Gets the input and output names for a language by the pair name
    /// </summary>
    /// <param name="pairName"> The name to search for </param>
    /// <returns> The input name and output name </returns>
    /// <exception cref="Exception">
    /// Thrown if <paramref name="pairName" /> could not be found in the languages table
    /// </exception>
    public IEnumerable<Tuple<string, string>> GetAllLanguageNames( ) {
        //Find language names
        SQLiteCommand c_findInputOutput = new($"SELECT inputname, outputname FROM {TABLE_LANGUAGES};", dCon);
        var r_findInputOutput = c_findInputOutput.ExecuteReader();
        if ( !r_findInputOutput.HasRows ) {
            throw new Exception( "There was no row to read!" );
        }
        List<Tuple<string,string>> help = new();
        while ( r_findInputOutput.HasRows ) {
            bool succ = r_findInputOutput.Read( );
            if ( !succ )
                break;
            string inputName = (string) r_findInputOutput[ 0 ];
            string outputName  = (string) r_findInputOutput[ 1 ];
            Tuple<string, string> names = Tuple.Create( inputName, outputName );
            help.Add( names );
        }
        r_findInputOutput.Close( );
        c_findInputOutput.Dispose( );
        return help;
    }
    /// <summary>
    /// Gets the input and output names for a language by the pair name
    /// </summary>
    /// <param name="pairName"> The name to search for </param>
    /// <returns> The input name and output name </returns>
    /// <exception cref="Exception">
    /// Thrown if <paramref name="pairName" /> could not be found in the languages table
    /// </exception>
    public (string inputName, string outputName) GetLanguageNames( string pairName ) {
        //Find language names
        SQLiteCommand c_findInputOutput = new($"SELECT inputname, outputname FROM {TABLE_LANGUAGES} WHERE internalname=@pp;", dCon);
        c_findInputOutput.Parameters.AddWithValue( "@pp", pairName.ToLower( ) );
        var r_findInputOutput = c_findInputOutput.ExecuteReader();
        if ( !r_findInputOutput.HasRows ) {
            throw new Exception( "There was no row to read!" );
        }
        r_findInputOutput.Read( );
        string inputName = (string) r_findInputOutput[ 0 ];
        string outputName  = (string) r_findInputOutput[ 1 ];
        r_findInputOutput.Close( );
        c_findInputOutput.Dispose( );
        return (inputName, outputName);
    }

    /// <summary>
    /// Initializes this DatabaseConnection
    /// </summary>
    public override void Init( ) {
        base.Init( );
        checkLexiconTablesExist( );
    }

    /// <summary>
    /// Reads all definitions for a word
    /// </summary>
    /// <param name="pairName"> The table to read from </param>
    /// <param name="wordId">   The ID of the word to read definitions for </param>
    /// <returns> All definitions for that word </returns>
    public IEnumerable<DictionaryDefinition> ReadDefinitions( string pairName, int wordId ) {
        string? gs( object a ) => a is DBNull ? null : (string) a;

        List<DictionaryDefinition> defs = new();

        SQLiteCommand c_readDefs = new($"SELECT wordid, defnum, deftext, partofspeech, example, exampletranslation, notes FROM {PREFIX_DEFS}{pairName} WHERE wordid={wordId};", dCon);
        //Read in definitions
        var r_readDefs = c_readDefs.ExecuteReader();
        while ( r_readDefs.HasRows ) {
            bool l_def = r_readDefs.Read( );
            if ( !l_def )
                break;

            DictionaryDefinition o_definition = new((int)r_readDefs[1], gs(r_readDefs[3]), gs(r_readDefs[2]), gs(r_readDefs[4]), gs(r_readDefs[5]), gs(r_readDefs[6]));
            defs.Add( o_definition );
        }
        r_readDefs.Close( );
        c_readDefs.Dispose( );
        return defs;
    }

    /// <summary>
    /// Reads an entire lexicon from a pair name
    /// </summary>
    /// <param name="pairName"> The internal name of the lexicon to open </param>
    /// <returns> The lexicon that was read </returns>
    /// <exception cref="Exception"> Thrown if the lexicon could not be found </exception>
    public Lexicon ReadEntireLexicon( string pairName ) {
        string inputName, outputName;

        (inputName, outputName) = GetLanguageNames( pairName );

        Lexicon o_lexicon = new(inputName, outputName);

        var entries = ReadWords(pairName);

        foreach ( var entry in entries )
            o_lexicon.AddEntry( entry );

        return o_lexicon;
    }

    /// <summary>
    /// Reads all words in a dictionary
    /// </summary>
    /// <param name="pairName"> The pair name of the dictionary </param>
    /// <returns> An enumerable with all words </returns>
    public IEnumerable<DictionaryEntry> ReadWords( string pairName ) {
        List<DictionaryEntry> entries = new();
        //Read in words
        SQLiteCommand c_readWords = new($"SELECT id, word FROM {PREFIX_WORDS}{pairName}", dCon);
        var r_readWords = c_readWords.ExecuteReader();
        while ( r_readWords.HasRows ) {
            bool l_word = r_readWords.Read( );
            if ( !l_word )
                break;

            int wordID = (int) r_readWords[ 0 ];
            DictionaryEntry o_word = new( (string) r_readWords[ 1 ] ) {
                WordID = wordID
            };

            var defs = ReadDefinitions(pairName, wordID);
            foreach ( var def in defs )
                o_word.AddDefinition( def );

            entries.Add( o_word );
        }
        r_readWords.Close( );
        c_readWords.Dispose( );

        return entries;
    }

    /// <summary>
    /// Adds definitions to the table
    /// </summary>
    /// <param name="pairName"> The table to modify (only pair name part) </param>
    /// <param name="wordId">   The ID of the word we are modifying definitions for </param>
    /// <param name="def">      The definition we are adding </param>
    /// <exception cref="LexiconlangDatabaseNoTableException">
    /// Thrown if the table for <paramref name="pairName" /> does not exist.
    /// </exception>
    public void RegisterDefinitions( string pairName, int wordId, params DictionaryDefinition[ ] defs ) {
        if ( !checkIfTableExists( $"{PREFIX_DEFS}{pairName}" ) )
            throw new LexiconlangDatabaseNoTableException( );

        //Lock the command so other users can't override it
        lock ( lck_insertDef ) {
            if ( c_insertDef is null || !c_insertDef.CommandText.Contains( PREFIX_DEFS + pairName ) )
                c_insertDef = new( $"INSERT INTO {PREFIX_DEFS}{pairName} (wordid, defnum, deftext, partofspeech, example, exampletranslation, notes) VALUES (@wid, @dnum, @dtext, @dpos, @dex, @dext, @dnot);", dCon );

            if ( c_insertDef.Parameters.Count == 0 ) {
                c_insertDef.Parameters.AddWithValue( "@wid", 0 );
                c_insertDef.Parameters.AddWithValue( "@dnum", 0 );
                c_insertDef.Parameters.AddWithValue( "@dtext", "" );
                c_insertDef.Parameters.AddWithValue( "@dpos", "" );
                c_insertDef.Parameters.AddWithValue( "@dex", "" );
                c_insertDef.Parameters.AddWithValue( "@dext", "" );
                c_insertDef.Parameters.AddWithValue( "@dnot", "" );
            }
            foreach ( var def in defs ) {
                c_insertDef.Parameters[ 0 ].Value = wordId;
                c_insertDef.Parameters[ 1 ].Value = def.DefinitionNumber;
                c_insertDef.Parameters[ 2 ].Value = def.Definition;
                c_insertDef.Parameters[ 3 ].Value = def.PartOfSpeech;
                c_insertDef.Parameters[ 4 ].Value = def.ExampleSentence;
                c_insertDef.Parameters[ 5 ].Value = def.ExampleTranslation;
                c_insertDef.Parameters[ 6 ].Value = def.Notes;
                c_insertDef.ExecuteNonQuery( );
            }
        }
        //c_insertDef.Dispose( );
    }

    /// <summary>
    /// Registers a language in the Language Table
    /// </summary>
    /// <param name="input">        The input language (e.g. English) </param>
    /// <param name="output">       The output language (e.g. Blixter) </param>
    /// <param name="internalName"> The name for the language pair. See remarks. </param>
    /// <exception cref="LanguageAlreadyRegisteredException">
    /// Thrown if any entry already exists with the same <paramref name="input" /> and
    /// <paramref name="output" /> languages, or with the same <paramref name="internalName" />.
    /// </exception>
    ///<remarks>If <paramref name="internalName"/> is null (or unspecified), the pair will be saved with the name <c>"<paramref name="input"/>_<paramref name="output"/>".ToLowerInvariant()</c>.</remarks>
    public void RegisterLanguage( string input, string output, string? internalName = null ) {
        string pairName = internalName ?? $"{input}_{output}".ToLowerInvariant();

        //The language is already registered
        if ( IsLanguageRegistered( input, output, null ) != null
          || IsLanguageRegistered( null, null, pairName ) != null )
            throw new LanguageAlreadyRegisteredException( );

        SQLiteCommand c_insertLanguage = new($"INSERT INTO {TABLE_LANGUAGES} (internalname, inputname, outputname) VALUES (@pair, @inp, @outp);", dCon);
        c_insertLanguage.Parameters.AddWithValue( "@pair", pairName );
        c_insertLanguage.Parameters.AddWithValue( "@inp", input );
        c_insertLanguage.Parameters.AddWithValue( "@outp", output );
        c_insertLanguage.ExecuteNonQuery( );
        c_insertLanguage.Dispose( );

        createTableIfNotExists( $"{PREFIX_DEFS}{pairName}", SCHEMA_DEF );
        createTableIfNotExists( $"{PREFIX_WORDS}{pairName}", SCHEMA_WORD );
    }

    /// <summary>
    /// Registers a word to the dictionary
    /// </summary>
    /// <param name="pairName"> The pair name for the table </param>
    /// <param name="word">     The word to write </param>
    /// <exception cref="LexiconlangDatabaseNoTableException">
    /// Thrown if the table for <paramref name="pairName" /> does not exist.
    /// </exception>
    public void RegisterWords( string pairName, params DictionaryEntry[ ] words ) {
        if ( !checkIfTableExists( $"{PREFIX_WORDS}{pairName}" ) )
            throw new LexiconlangDatabaseNoTableException( );

        //Lock the command so others can't override it
        lock ( lck_insertWord ) {
            if ( c_insertWord is null || !c_insertWord.CommandText.Contains( PREFIX_WORDS + pairName ) )
                c_insertWord = new( $"INSERT INTO {PREFIX_WORDS}{pairName} (id, word) VALUES (@wid, @wword);", dCon );

            if ( c_insertWord.Parameters.Count == 0 ) {
                c_insertWord.Parameters.AddWithValue( "@wid", 0 );
                c_insertWord.Parameters.AddWithValue( "@wword", "" );
            }

            foreach ( var word in words ) {
                UnregisterWord( pairName, word, true );

                c_insertWord.Parameters[ 0 ].Value = word.WordID;
                c_insertWord.Parameters[ 1 ].Value = word.Word;

                c_insertWord.ExecuteNonQuery( );

                RegisterDefinitions( pairName, word.WordID, word.GetAllDefinitions( ).ToArray( ) );
            }
        }
        // c_insertWord.Dispose( );
    }

    /// <summary>
    /// Saves an entire lexicon
    /// </summary>
    /// <param name="lex"> The lexicon to save </param>
    [Obsolete( "Using this method will cause the database to drop all words and definitions and rewrite them with the user's understanding of the lexicon. This method should be avoided in production code." )]
    public void SaveLexicon( Lexicon lex ) {
        try {
            RegisterLanguage( lex.InputLanguage, lex.OutputLanguage, lex.InternalDictionaryName );
        }
        catch ( LanguageAlreadyRegisteredException ) {
            //Ignore; expected. We're simply updating the record for this lexicon.
        }

        //This is going to be absurdly slow as it is going to regenerate the command and execute it for each word in the lexicon.
        foreach ( var v in lex.GetEntries( ) ) {
            RegisterWords( lex.InternalDictionaryName, v );
        }
    }

    /// <summary>
    /// Deletes a definition from the table
    /// </summary>
    /// <param name="pairName"> The table to modify (only pair name part) </param>
    /// <param name="wordId">   The ID of the word we are modifying definitions for </param>
    /// <param name="def">      The definition number we are deleting </param>
    /// <exception cref="LexiconlangDatabaseNoTableException">
    /// Thrown if the table for <paramref name="pairName" /> does not exist.
    /// </exception>
    public void UnregisterDefinition( string pairName, int wordId, DictionaryDefinition def ) {
        if ( !checkIfTableExists( $"{PREFIX_DEFS}{pairName}" ) )
            throw new LexiconlangDatabaseNoTableException( );

        SQLiteCommand c_removeDef = new ($"DELETE FROM {PREFIX_DEFS}{pairName} WHERE wordid=@wid AND defnum=@dnum;", dCon);
        c_removeDef.Parameters.AddWithValue( "@wid", wordId );
        c_removeDef.Parameters.AddWithValue( "@dnum", def.DefinitionNumber );
        c_removeDef.ExecuteNonQuery( );
        c_removeDef.Dispose( );
    }

    /// <summary>
    /// Deletes a word from the dictionary
    /// </summary>
    /// <param name="pairName">             The pair name for the table </param>
    /// <param name="word">                 The word to delete </param>
    /// <param name="deleteDefinitionsToo"> If true, will delete all definitions too </param>
    /// <exception cref="LexiconlangDatabaseNoTableException">
    /// Thrown if the table for <paramref name="pairName" /> does not exist.
    /// </exception>
    public void UnregisterWord( string pairName, DictionaryEntry word, bool deleteDefinitionsToo = false ) {
        if ( !checkIfTableExists( $"{PREFIX_WORDS}{pairName}" ) )
            throw new LexiconlangDatabaseNoTableException( );
        lock ( lck_deleteWord ) {
            if ( c_deleteWord is null || !c_deleteWord.CommandText.Contains( PREFIX_WORDS + pairName ) )
                c_deleteWord = new( $"DELETE FROM {PREFIX_WORDS}{pairName} WHERE id=@wid AND word=@wword;", dCon );

            if ( c_deleteWord.Parameters.Count == 0 ) {
                c_deleteWord.Parameters.AddWithValue( "@wid", word.WordID );
                c_deleteWord.Parameters.AddWithValue( "@wword", word.Word );
            }

            c_deleteWord.Parameters[ 0 ].Value = word.WordID;
            c_deleteWord.Parameters[ 1 ].Value = word.Word;

            c_deleteWord.ExecuteNonQuery( );
        }
        //c_deleteWord.Dispose( );

        if ( deleteDefinitionsToo ) {
            lock ( lck_deleteDefinitions ) {
                if ( c_deleteDefinitions is null || !c_deleteDefinitions.CommandText.Contains( PREFIX_DEFS + pairName ) )
                    c_deleteDefinitions = new( $"DELETE FROM {PREFIX_DEFS}{pairName} WHERE wordid=@wid;", dCon );

                if ( c_deleteDefinitions.Parameters.Count == 0 )
                    c_deleteDefinitions.Parameters.AddWithValue( "@wid", word.WordID );

                c_deleteDefinitions.Parameters[ 0 ].Value = word.WordID;

                c_deleteDefinitions.ExecuteNonQuery( );
            }
        }
    }

    /// <summary>
    /// Creates and initializes missing Lexicon tables.
    /// </summary>
    private void checkLexiconTablesExist( ) {
        SQLiteTransaction st = dCon.BeginTransaction();
        SQLiteCommand s;
        /*
        if ( createTableIfNotExists( PREFIX_WORDS + "default", SCHEMA_WORD ) ) {
            s = new( $"INSERT INTO {PREFIX_WORDS}default( id, word) VALUES (1, 'Setup Successful');", dCon );
            s.ExecuteNonQuery( );
            s.Dispose( );
        }
        if ( createTableIfNotExists( PREFIX_DEFS + "default", SCHEMA_DEF ) ) {
            s = new( $"INSERT INTO {PREFIX_DEFS}default (wordid, defnum, deftext, partofspeech, example, exampletranslation, notes) VALUES (1, 1, 'Setup Was Successful', 'Message', 'We hope you enjoy!', '', '');", dCon );
            s.ExecuteNonQuery( );
            s.Dispose( );
        }
        */
        if ( createTableIfNotExists( TABLE_LANGUAGES, "internalname TEXT PRIMARY KEY, inputname TEXT, outputname TEXT" ) ) {
            //s = new( $"INSERT INTO {TABLE_LANGUAGES}( internalname, inputname, outputname) VALUES ('default', 'English', 'Default');", dCon );
            //s.ExecuteNonQuery( );
            //s.Dispose( );
        }


        st.Commit( );
    }

    /// <summary>
    /// Determines if a language is registered in the Language Table
    /// </summary>
    /// <param name="input">        The input language name (e.g. English) </param>
    /// <param name="output">       The output language name (e.g. Blixter) </param>
    /// <param name="internalName"> The internal pair name (e.g. English_Blixter) </param>
    /// <returns> </returns>
    private string? IsLanguageRegistered( string? input = null, string? output = null, string? internalName = null ) {
        string? name;
        if ( internalName is not null ) {
            SQLiteCommand c_findTablebyInternalName = new($"SELECT internalname FROM {TABLE_LANGUAGES} WHERE internalname=@Name", dCon);
            c_findTablebyInternalName.Parameters.AddWithValue( "@Name", internalName );
            name = findNameFromCommand( c_findTablebyInternalName );
            if ( name is not null )
                return name;
        }
        SQLiteCommand c_findTableByInputOutput = new($"SELECT internalname, inputname, outputname FROM {TABLE_LANGUAGES} WHERE inputname=@inp AND outputname=@outp;".ToLowerInvariant(), dCon);

        c_findTableByInputOutput.Parameters.AddWithValue( "@inp", input );
        c_findTableByInputOutput.Parameters.AddWithValue( "@outp", output );
        name = findNameFromCommand( c_findTableByInputOutput );
        return name;

        static string? findNameFromCommand( SQLiteCommand c_command ) {
            var r_reader = c_command.ExecuteReader( );
            if ( r_reader.HasRows )
                r_reader.Read( );
            string? o_langName = r_reader.HasRows ? (string)r_reader[0] : null;
            r_reader.Close( );
            c_command.Dispose( );
            return o_langName;
        }
    }
}
