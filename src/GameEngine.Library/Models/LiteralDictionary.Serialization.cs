using System.Text.Json;
using System.Text.Json.Nodes;
using GameEngine.Library.Enumerations;
using GameEngine.Library.Exceptions;

namespace GameEngine.Library.Models;

public partial class LiteralDictionary
{
    public static Stream OpenFileForRead(string fileName)
    {
        if (!File.Exists(path: fileName))
            throw new FileNotFoundException(message: "File not found",
                fileName: fileName);

        return File.Open(
            path: fileName,
            mode: FileMode.Open,
            access: FileAccess.Read);
    }

    /// <summary>
    ///     Read a binary encoded file and re-create a sorted dictionary from it
    ///     TODO: re-serialize dictionaries with Description included, add to deserialize
    /// </summary>
    /// <param name="sourceType"></param>
    /// <param name="inputStream"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static Dictionary<int, IEnumerable<string>>
        DeserializeToDictionary(Stream inputStream)
    {
        using var reader = new BinaryReader(input: inputStream);

        var count = reader.ReadInt32();
        var dictionary = new Dictionary<int, IEnumerable<string>>(capacity: count);
        for (var n = 0; n < count; n++)
        {
            var key = reader.ReadInt32();
            var wordCount = reader.ReadInt32();
            var words = new List<string>(capacity: wordCount);
            for (var i = 0; i < wordCount; i++)
            {
                var value = reader.ReadString();
                words.Add(item: value);
            }

            dictionary.Add(key: key,
                value: words);
        }

        reader.Close();

        return dictionary;
    }

    /// <summary>
    ///     Read a binary encoded file and re-create a sorted dictionary from it
    ///     TODO: re-serialize dictionaries with Description included, add to deserialize
    /// </summary>
    /// <param name="sourceType"></param>
    /// <param name="inputStream"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static LiteralDictionary Deserialize(
        LiteralDictionarySourceType sourceType,
        Stream inputStream,
        string? description = null)
    {
        return new LiteralDictionary(
            dictionary: DeserializeToDictionary(
                inputStream: inputStream),
            sourceType: sourceType,
            description: description);
    }

    /// <summary>
    ///     Save the dictionary to a binary encoded file
    /// </summary>
    /// <param name="outputFilename"></param>
    /// <param name="forLength"></param>
    /// <exception cref="FileAlreadyExistsException"></exception>
    public int Serialize(string outputFilename, int forLength = -1)
    {
        if (File.Exists(path: outputFilename))
            throw new FileAlreadyExistsException(message: $"File already exists: {outputFilename}");

        if (forLength != -1 && !this._wordsByLength.ContainsKey(key: forLength))
            throw new ArgumentException(message: $"No words of length {forLength}");

        var wordCount = 0;
        using var stream = new StreamWriter(path: outputFilename);
        var writer = new BinaryWriter(output: stream.BaseStream);
        // write count of keys
        writer.Write(value: forLength > 0 ? 1 : this._wordsByLength.Count);
        foreach (var (key, value) in this._wordsByLength)
        {
            if (forLength > 0 && key != forLength)
                continue;
            // write key name
            writer.Write(value: key);
            var words = value.ToArray();
            // write count of words
            writer.Write(value: words.Length);
            foreach (var word in words)
            {
                wordCount++;
                // write word
                writer.Write(value: word);
            }
        }

        writer.Flush();

        return wordCount;
    }

    public int SplitSerialize(string outputFilename)
    {
        var lengths = this._wordsByLength.Keys.ToArray();
        var totalWordCount = (from key in lengths
            let lengthFilename = $"{key}-{outputFilename}"
            select this.Serialize(outputFilename: lengthFilename,
                forLength: key)).Sum();

        // write a table of contents file
        var jsonFilename = $"{outputFilename}-lengths.json";
        var jsonText = JsonSerializer.Serialize(value: lengths);

        File.WriteAllText(
            path: jsonFilename,
            contents: jsonText);

        return totalWordCount;
    }

    public JsonObject ToJson()
    {
        throw new NotImplementedException();
    }
}