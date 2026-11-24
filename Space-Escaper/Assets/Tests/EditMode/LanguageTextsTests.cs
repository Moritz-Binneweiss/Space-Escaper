using System;
using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    // The rules of the texts of a language, with languages of the test, so a change of the real texts does not
    // change what it checks.
    public class LanguageTextsTests
    {
        [Test]
        public void ReadsTheTextsUnderTheirKeys()
        {
            LanguageTexts texts = new LanguageTexts("{\n  \"menu.title\": \"MENU\",\n  \"menu.play\": \"PLAY\"\n}\n");

            Assert.That(texts.Keys, Is.EquivalentTo(new[] { "menu.title", "menu.play" }));
            Assert.That(texts.Get("menu.title"), Is.EqualTo("MENU"));
            Assert.That(texts.Get("menu.play"), Is.EqualTo("PLAY"));
            Assert.That(texts.Contains("menu.play"), Is.True);
        }

        // Some editors write a byte order mark at the start of a file.
        [TestCase("{}")]
        [TestCase(" \r\n{\r\n}\r\n")]
        [TestCase("\uFEFF{}")]
        public void ReadsAnEmptyLanguage(string json)
        {
            Assert.That(new LanguageTexts(json).Keys, Is.Empty);
        }

        // The escapes of JSON, so a text can hold quotes, line breaks and any character.
        [Test]
        public void ReadsEveryEscape()
        {
            LanguageTexts texts = new LanguageTexts("{\"quote\": \"say \\\"hi\\\"\", \"lines\": \"one\\ntwo\", " +
                "\"rest\": \"\\\\ \\/ \\b\\f\\r\\t\", \"unicode\": \"\\u00df \\u1E9E \\u00b7\"}");

            Assert.That(texts.Get("quote"), Is.EqualTo("say \"hi\""));
            Assert.That(texts.Get("lines"), Is.EqualTo("one\ntwo"));
            Assert.That(texts.Get("rest"), Is.EqualTo("\\ / \b\f\r\t"));
            Assert.That(texts.Get("unicode"), Is.EqualTo("ß ẞ ·"));
        }

        // The files are UTF-8, so everything beyond ASCII can be written as it is.
        [Test]
        public void ReadsTextBeyondAsciiAsItIs()
        {
            LanguageTexts texts = new LanguageTexts("{\"größe\": \"„GRÖSSE“ ×1,4 · …\"}");

            Assert.That(texts.Get("größe"), Is.EqualTo("„GRÖSSE“ ×1,4 · …"));
        }

        // A mistake in a file names its line and column, so it is quick to find.
        [TestCase("", 1, 1, "expected '{'")]
        [TestCase("[]", 1, 1, "expected '{'")]
        [TestCase("{\"a\": 1}", 1, 7, "expected a string in double quotes")]
        [TestCase("{\"a\": {\"b\": \"c\"}}", 1, 7, "expected a string in double quotes")]
        [TestCase("{\"a\": \"b\",}", 1, 11, "expected a string in double quotes")]
        [TestCase("{\"a\" \"b\"}", 1, 6, "expected ':'")]
        [TestCase("{\"a\": \"b\" \"c\": \"d\"}", 1, 11, "expected '}'")]
        [TestCase("{\"a\": \"b\"} x", 1, 12, "text after the object")]
        [TestCase("{\"a\": \"b}", 1, 7, "a string that never ends")]
        [TestCase("{\"a\": \"line\nbreak\"}", 1, 12,
            "a line break or control character in a string, write \\n for a new line")]
        [TestCase("{\"a\": \"\\x\"}", 1, 8, "an unknown escape in a string")]
        [TestCase("{\"a\": \"\\u12\"}", 1, 8, "\\u needs four hex digits")]
        [TestCase("{\n  \"a\": \"b\",\n  \"a\": \"c\"\n}", 3, 3, "the key \"a\" comes twice")]
        public void AMistakeNamesItsLine(string json, int line, int column, string problem)
        {
            FormatException exception = Assert.Throws<FormatException>(() => new LanguageTexts(json));

            Assert.That(exception.Message, Is.EqualTo($"Line {line}, column {column}: {problem}"));
        }

        [Test]
        public void AKeyALanguageLacksComesFromTheFallback()
        {
            LanguageTexts english = new LanguageTexts("{\"menu.title\": \"MENU\", \"menu.play\": \"PLAY\"}");
            LanguageTexts german = new LanguageTexts("{\"menu.play\": \"SPIELEN\"}", english);

            Assert.That(german.Get("menu.play"), Is.EqualTo("SPIELEN"));
            Assert.That(german.Get("menu.title"), Is.EqualTo("MENU"));
            Assert.That(german.Keys, Is.EquivalentTo(new[] { "menu.play" }), "The keys hold those of the fallback");
            Assert.That(german.Contains("menu.title"), Is.False);
        }

        // So a missing text shows where it is missing.
        [Test]
        public void AKeyOfNoTextGivesTheKey()
        {
            LanguageTexts english = new LanguageTexts("{\"menu.title\": \"MENU\"}");
            LanguageTexts german = new LanguageTexts("{}", english);

            Assert.That(german.Get("menu.missing"), Is.EqualTo("menu.missing"));
            Assert.That(english.Get(""), Is.Empty);
            Assert.That(english.Get(null), Is.Null);
            Assert.That(english.Contains(null), Is.False);
        }
    }
}
