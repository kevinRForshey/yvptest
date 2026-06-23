#region  usings
using Platform.API.Models;
using YouVersion.UsfmReferences;
#endregion

namespace Platform.API.Clients;

/// <summary>
/// Static catalog of the 66 canonical Bible books, keyed by USFM book code.
/// Extracted from <see cref="BibleClient"/> so client classes stay focused on
/// HTTP concerns (Single Responsibility Principle).
/// </summary>
internal static class BibleBookCatalog
{
    /// <summary>
    /// Returns a <see cref="Book"/> for the given USFM code, using catalog metadata
    /// for the human-readable name and chapter count.  Falls back to the raw USFM code
    /// as the name when the code is unrecognised (e.g. deuterocanonical books).
    /// </summary>
    internal static Book FromUsfm(string usfm)
    {
        // Validate code against package-provided canonical list
        var isKnown = BookCatalog.IsKnownBook(usfm);
        var human = isKnown && s_books.TryGetValue(usfm, out var meta) 
            ? meta.Human 
            : usfm;
        var chapters = isKnown && s_books.TryGetValue(usfm, out var meta2) 
            ? meta2.Chapters 
            : 0;
        
        return new Book { Usfm = usfm, Human = human, ChapterCount = chapters };
    }

    // Standard chapter counts keyed by USFM book code (66 canonical books).
    private static readonly IReadOnlyDictionary<string, (string Human, int Chapters)> s_books =
        new Dictionary<string, (string Human, int Chapters)>(StringComparer.OrdinalIgnoreCase)
        {
            { "GEN", ("Genesis", 50) },        { "EXO", ("Exodus", 40) },
            { "LEV", ("Leviticus", 27) },       { "NUM", ("Numbers", 36) },
            { "DEU", ("Deuteronomy", 34) },     { "JOS", ("Joshua", 24) },
            { "JDG", ("Judges", 21) },          { "RUT", ("Ruth", 4) },
            { "1SA", ("1 Samuel", 31) },        { "2SA", ("2 Samuel", 24) },
            { "1KI", ("1 Kings", 22) },         { "2KI", ("2 Kings", 25) },
            { "1CH", ("1 Chronicles", 29) },    { "2CH", ("2 Chronicles", 36) },
            { "EZR", ("Ezra", 10) },            { "NEH", ("Nehemiah", 13) },
            { "EST", ("Esther", 10) },          { "JOB", ("Job", 42) },
            { "PSA", ("Psalms", 150) },         { "PRO", ("Proverbs", 31) },
            { "ECC", ("Ecclesiastes", 12) },    { "SNG", ("Song of Solomon", 8) },
            { "ISA", ("Isaiah", 66) },          { "JER", ("Jeremiah", 52) },
            { "LAM", ("Lamentations", 5) },     { "EZK", ("Ezekiel", 48) },
            { "DAN", ("Daniel", 12) },          { "HOS", ("Hosea", 14) },
            { "JOL", ("Joel", 3) },             { "AMO", ("Amos", 9) },
            { "OBA", ("Obadiah", 1) },          { "JON", ("Jonah", 4) },
            { "MIC", ("Micah", 7) },            { "NAM", ("Nahum", 3) },
            { "HAB", ("Habakkuk", 3) },         { "ZEP", ("Zephaniah", 3) },
            { "HAG", ("Haggai", 2) },           { "ZEC", ("Zechariah", 14) },
            { "MAL", ("Malachi", 4) },          { "MAT", ("Matthew", 28) },
            { "MRK", ("Mark", 16) },            { "LUK", ("Luke", 24) },
            { "JHN", ("John", 21) },            { "ACT", ("Acts", 28) },
            { "ROM", ("Romans", 16) },          { "1CO", ("1 Corinthians", 16) },
            { "2CO", ("2 Corinthians", 13) },   { "GAL", ("Galatians", 6) },
            { "EPH", ("Ephesians", 6) },        { "PHP", ("Philippians", 4) },
            { "COL", ("Colossians", 4) },       { "1TH", ("1 Thessalonians", 5) },
            { "2TH", ("2 Thessalonians", 3) },  { "1TI", ("1 Timothy", 6) },
            { "2TI", ("2 Timothy", 4) },        { "TIT", ("Titus", 3) },
            { "PHM", ("Philemon", 1) },         { "HEB", ("Hebrews", 13) },
            { "JAS", ("James", 5) },            { "1PE", ("1 Peter", 5) },
            { "2PE", ("2 Peter", 3) },          { "1JN", ("1 John", 5) },
            { "2JN", ("2 John", 1) },           { "3JN", ("3 John", 1) },
            { "JUD", ("Jude", 1) },             { "REV", ("Revelation", 22) },
        };
}
