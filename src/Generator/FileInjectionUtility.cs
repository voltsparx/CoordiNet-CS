using System.Text;

namespace CoordiNet.Generator;

public static class FileInjectionUtility
{
    public static void InjectBeforeLocator(
        string inputFilePath,
        string outputFilePath,
        string targetLocator,
        string payload)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            throw new ArgumentException("Input file path is required.", nameof(inputFilePath));
        }

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new ArgumentException("Output file path is required.", nameof(outputFilePath));
        }

        if (string.IsNullOrEmpty(targetLocator))
        {
            throw new ArgumentException("Target locator is required.", nameof(targetLocator));
        }

        if (string.IsNullOrEmpty(payload))
        {
            throw new ArgumentException("Payload is required.", nameof(payload));
        }

        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException("Input file was not found.", inputFilePath);
        }

        var directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var inputBytes = File.ReadAllBytes(inputFilePath);
        var encoding = DetectEncoding(inputBytes) ?? Encoding.UTF8;
        var originalText = encoding.GetString(inputBytes);

        var markerIndex = originalText.IndexOf(targetLocator, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            throw new InvalidOperationException($"Target locator '{targetLocator}' was not found in the input file.");
        }

        var updatedText = originalText.Insert(markerIndex, payload);
        var outputBytes = encoding.GetBytes(updatedText);

        File.WriteAllBytes(outputFilePath, outputBytes);
    }

    private static Encoding? DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return Encoding.Unicode;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        return null;
    }
}
