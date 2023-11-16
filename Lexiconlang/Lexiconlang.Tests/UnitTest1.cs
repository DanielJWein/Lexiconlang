using Lexiconlang.Data.Dict;
using Lexiconlang.Data;
using Lexiconlang.Data.Database;
using System.Diagnostics;

namespace Lexiconlang.Tests;

[TestClass]
public class TestLexicon {
    private Random r = new( );

    private string GetSlop( int len ) {
        char[] chars = new char[len];
        for ( int i = 0; i < len; i++ ) {
            chars[ i ] = (char) ( 'a' + r.Next( 0, 'z' - 'a' ) );
        }

        string s = new string( chars );
        chars = null;
        return s;
    }

    [TestMethod]
    public void AddWord( ) {
        Lexicon l = new("English", "Testish");
        DictionaryEntry entry = new ("gbyhu");
        DictionaryDefinition def = new(1, "Noun", "The test word");
        entry.AddDefinition( def );
        l.AddEntry( entry );

        var x = l.GetEntry( "gbyhu" );

        Assert.IsTrue( x.Word == "gbyhu" );
        Assert.IsTrue( x.NumDefinitions == 1 );
        Assert.IsTrue( x.GetDefinition( 1 ).PartOfSpeech == "Noun" );
        Assert.IsTrue( x.GetDefinition( 1 ).Definition == "The test word" );
    }

    [TestMethod]
    public void TestDatabase( ) {
        File.Delete( "database.db" );
        LexiconlangDatabase d = new();
        d.Init( );

        Lexicon l = new("INPUT", "OUTPUT");
        l.AddEntry( new DictionaryEntry( "Test" ) );
        l.GetEntry( "Test" ).AddDefinition( new DictionaryDefinition( 1, "Noun", "A set of checks to ensure that something is working correctly", "He ran a few tests to make sure it was working all right.", null, null ) );
        l.GetEntry( "Test" ).AddDefinition( new DictionaryDefinition( 2, "Verb", "To run a set of checks to ensure that something is working correctly", "He tested to make sure it was working all right.", null, null ) );

        d.SaveLexicon( l );

        l.GetEntry( "Test" ).AddDefinition( new( 3, "Test", "This is to test that even though we loaded an extra definition, we didn't break anything!" ) );

        d.SaveLexicon( l );

        Lexicon l2 = d.ReadEntireLexicon($"{l.InputLanguage}_{l.OutputLanguage}".ToLowerInvariant());

        Assert.AreEqual( "A set of checks to ensure that something is working correctly", l2.GetEntry( "Test" ).GetDefinition( 1 ).Definition );
        Assert.AreEqual( "To run a set of checks to ensure that something is working correctly", l2.GetEntry( "Test" ).GetDefinition( 2 ).Definition );
        Assert.AreEqual( "This is to test that even though we loaded an extra definition, we didn't break anything!", l2.GetEntry( "Test" ).GetDefinition( 3 ).Definition );

        d.Unit( );
    }

    [TestMethod]
    public void TestDatabasePerformance( ) {
        //HACK: Word 1 isn't being saved
        File.Delete( "database.db" );
        Lexicon l = new("TEST", "PERF");
        Debug.WriteLine( "Setting up lexicon" );
        const int max = 1000;
        for ( int i = 0; i < max; i++ ) {
            DictionaryEntry entry = new( GetSlop( 16 ) ) {
                WordID = i + 1
            };
            for ( int j = 0; j < 5; j++ ) {
                DictionaryDefinition def = new(j + 1, GetSlop(6), GetSlop(52), GetSlop(250), GetSlop(250), GetSlop(16));
                entry.AddDefinition( def );
            }
            l.AddEntry( entry );
        }
        LexiconlangDatabase d = new();
        d.Init( );
        Debug.WriteLine( "Lexicon ready! Saving..." );
        DateTime start = DateTime.Now;
        d.SaveLexicon( l );
        DateTime end = DateTime.Now;
        Debug.WriteLine( $"Finished! Took {( end - start ).TotalSeconds:n3}s" );
        Debug.WriteLine( $"Approximately {( ( 6.0 * max ) / ( end - start ).TotalSeconds ):n3} operations / second" );
    }
}
