using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Exceptions;
using WordMasterMind.Models;

namespace WordMasterMind;

[TestClass]
public class WordMasterMindTest
{
    private const int StandardLength = 5;

    private static ScrabbleDictionary GetScrabbleDictionary()
    {
        return new ScrabbleDictionary(pathToDictionaryJson: GetTestRoot(fileName: "scrabble-dictionary.json"));
    }

    /// <summary>
    ///     Gets the path to the test project's root directory.
    /// </summary>
    private static string GetTestRoot(string? fileName = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var startupPath = Path.GetDirectoryName(path: assembly.Location);
        Debug.Assert(condition: startupPath != null,
            message: nameof(startupPath) + " != null");
        var pathItems = startupPath.Split(separator: Path.DirectorySeparatorChar);
        var pos = pathItems.Reverse().ToList().FindIndex(match: x => string.Equals(a: "bin",
            b: x));
        var basePath = string.Join(separator: Path.DirectorySeparatorChar.ToString(),
            values: pathItems.Take(count: pathItems.Length - pos - 1));
        return fileName is null
            ? basePath
            : Path.Combine(path1: basePath,
                path2: fileName);
    }

    /// <summary>
    ///     Inspects an attempt result and makes sure it is valid.
    /// </summary>
    /// <param name="knownSecretWord"></param>
    /// <param name="attemptDetails"></param>
    private static void TestAttempt(string knownSecretWord, IEnumerable<AttemptDetail> attemptDetails)
    {
        attemptDetails = attemptDetails.ToArray();
        var positionIndex = 0;
        foreach (var position in attemptDetails)
        {
            var correspondingSecretLetter = knownSecretWord[index: positionIndex++];
            var letterMatch = knownSecretWord.Contains(value: position.Letter);
            Assert.AreEqual(
                expected: letterMatch,
                actual: position.LetterCorrect);
            var positionMatch = position.Letter.Equals(obj: correspondingSecretLetter);
            Assert.AreEqual(
                expected: positionMatch,
                actual: position.PositionCorrect);
        }
    }

    [TestMethod]
    public void TestWordMasterMindWordTooShort()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            new Models.WordMasterMind(
                minLength: StandardLength,
                maxLength: StandardLength,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                // secretWord is valid, but not long enough
                secretWord: scrabbleDictionary.GetRandomWord(minLength: 3,
                    maxLength: StandardLength - 1)));
        Assert.AreEqual(expected: "Secret word must be between minLength and maxLength",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordTooLong()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            new Models.WordMasterMind(
                minLength: StandardLength,
                maxLength: StandardLength,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                // secretWord is valid, but too long
                secretWord: scrabbleDictionary.GetRandomWord(minLength: StandardLength + 1,
                    maxLength: StandardLength + 1)));
        Assert.AreEqual(expected: "Secret word must be between minLength and maxLength",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordNotInDictionary()
    {
        // secretWord is made up word not in dictionary
        const string expectedSecretWord = "fizzbuzz";
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            new Models.WordMasterMind(
                minLength: 8,
                maxLength: 8,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                secretWord: expectedSecretWord));
        Assert.AreEqual(expected: "Secret word must be a valid word in the Scrabble dictionary",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptLengthMismatch()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: StandardLength,
            maxLength: StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: scrabbleDictionary.GetRandomWord(minLength: StandardLength,
                maxLength: StandardLength));
        Assert.AreEqual(
            expected: StandardLength,
            actual: mastermind.WordLength);
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            mastermind.Attempt(wordAttempt: "invalid"));
        Assert.AreEqual(expected: "Word length does not match secret word length",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptCorrect()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var scrabbleDictionary = GetScrabbleDictionary();
        var secretWord = scrabbleDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var mastermind = new Models.WordMasterMind(
            minLength: length,
            maxLength: length,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: secretWord);
        Assert.AreEqual(
            expected: length,
            actual: mastermind.WordLength);
        var attempt = mastermind.Attempt(wordAttempt: secretWord);
        Assert.IsTrue(condition: mastermind.Solved);
        TestAttempt(knownSecretWord: secretWord,
            attemptDetails: attempt);
    }

    [TestMethod]
    public void TestWordMasterMindTooManyAttempts()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var scrabbleDictionary = GetScrabbleDictionary();
        var secretWord = scrabbleDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var incorrectWord = secretWord;
        while (incorrectWord.Equals(value: secretWord))
            incorrectWord = scrabbleDictionary.GetRandomWord(minLength: length,
                maxLength: length);
        var mastermind = new Models.WordMasterMind(
            minLength: length,
            maxLength: length,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: secretWord);
        Assert.AreEqual(
            expected: length,
            actual: mastermind.WordLength);
        for (var i = 0; i < Models.WordMasterMind.GetMaxAttemptsForLength(length: length); i++)
        {
            var attempt = mastermind.Attempt(wordAttempt: incorrectWord);
            TestAttempt(knownSecretWord: secretWord,
                attemptDetails: attempt);
        }

        Assert.IsFalse(condition: mastermind.Solved);
        var thrownException = Assert.ThrowsException<GameOverException>(action: () => mastermind.Attempt(wordAttempt: "wrong"));
        Assert.IsFalse(condition: thrownException.Solved);
        Assert.AreEqual(
            expected: GameOverException.GameOverText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWithProvidedRandomWordAndInvalidAttempt()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: StandardLength,
            maxLength: StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
        Assert.AreEqual(
            expected: StandardLength,
            actual: mastermind.WordLength);
        var secretWord = mastermind.SecretWord.ToUpperInvariant();
        Assert.AreEqual(
            expected: StandardLength,
            actual: secretWord.Length);
        var invalidSecretWord = "".PadLeft(totalWidth: StandardLength,
            paddingChar: 'z');
        var thrownAssertion = Assert.ThrowsException<NotInDictionaryException>(action: () =>
            mastermind.Attempt(wordAttempt: invalidSecretWord));
        Assert.AreEqual(
            expected: NotInDictionaryException.MessageText,
            actual: thrownAssertion.Message);
    }

    [TestMethod]
    public void TestWordMasterMindHardMode()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: StandardLength,
            maxLength: StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
        Assert.AreEqual(
            expected: StandardLength,
            actual: mastermind.WordLength);
    }

    [TestMethod]
    public void TestAttemptsToString()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: StandardLength,
            maxLength: StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
        Assert.AreEqual(
            expected: StandardLength,
            actual: mastermind.WordLength);
        WordMasterMindPlayer.ComputerGuess(mastermind: mastermind,
            turns: 1);
    }
}