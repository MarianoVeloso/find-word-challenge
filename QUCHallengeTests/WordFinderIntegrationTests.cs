using FluentAssertions;
using QUChallenge.Core;
using Xunit;

namespace QUCHallengeTests;

public class WordFinderIntegrationTests
{
    [Fact]
    public void Constructor_ValidMatrix_ShouldInitialize()
    {
        // Arrange
        var matrix = new[] { "abc", "def", "ghi" };

        // Act
        var finder = new WordFinder(matrix);

        // Assert
        finder.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullMatrix_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new WordFinder(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyMatrix_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = Array.Empty<string>();

        // Act & Assert
        var action = () => new WordFinder(matrix);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_MatrixTooLarge_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = new string[65];
        for (int i = 0; i < 65; i++)
        {
            matrix[i] = new string('a', 65);
        }

        // Act & Assert
        var action = () => new WordFinder(matrix);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NonSquareMatrix_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = new[] { "abc", "de" };

        // Act & Assert
        var action = () => new WordFinder(matrix);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_UnequalRowLengths_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = new[] { "abc", "defg" };

        // Act & Assert
        var action = () => new WordFinder(matrix);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task FindAsync_RealWordSearchPuzzle_ShouldFindCorrectWords()
    {
        // Arrange - Real word search puzzle (like the one in the image)
        var wordSearchMatrix = new[]
        {
            "RAICNEREFIDTM",
            "CCZERTSDVQBMO",
            "ASTIZEZKLDMAC",
            "JNLQCZFAJFUFO",
            "TICTRATIDEALN",
            "RPULCJRPRECIO",
            "ARECCCHORIZOI",
            "THNCUIUMCUUNM",
            "ATTWETLBKBTEM",
            "RPIHROUADAOAN",
            "UUSRDINTQGWCT",
            "NITSOPHORIZON",
            "QYAKSFCARDIOK"
        };

        var puzzleWords = new[]
        {
            "DIFERENCIAR", "TRATAR", "IDEAL", "PRECIO", "HORIZON",
            "CARDIO", "FORTE", "MUSCULO", "EJERCICIO", "SALUD",
            "NOTFOUND", "MISSING", "ABSENT"
        };

        var finder = new WordFinder(wordSearchMatrix);

        // Act
        var result = await finder.FindAsync(puzzleWords);

        // Assert
        result.Should().Contain("DIFERENCIAR"); // First row, horizontal
        result.Should().Contain("TRATAR");     // Found in matrix
        result.Should().Contain("IDEAL");      // Found in matrix
        result.Should().Contain("PRECIO");     // Found in matrix
        result.Should().Contain("HORIZON");    // Found in matrix
        result.Should().Contain("CARDIO");     // Last row, horizontal
        result.Should().NotContain("NOTFOUND");
        result.Should().NotContain("MISSING");
        result.Should().NotContain("ABSENT");
    }

    [Fact]
    public async Task FindAsync_LargeRealWorldScenario_ShouldPerformWell()
    {
        // Arrange - Simulate a real-world word search with many words
        var matrix = GenerateRealWorldMatrix();
        var wordStream = GenerateRealWorldWordStream();
        var finder = new WordFinder(matrix);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await finder.FindAsync(wordStream);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "Real-world scenario should complete quickly");
        result.Should().NotBeEmpty("Should find some words in the matrix");

        Console.WriteLine($"Found {result.Count()} words in {stopwatch.ElapsedMilliseconds}ms");
    }


    [Fact]
    public async Task FindAsync_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var matrix = GenerateLargeMatrix(10);
        var wordStream = GenerateLargeWordStream(1000);
        var finder = new WordFinder(matrix);

        // Act - Run multiple concurrent searches
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => finder.FindAsync(wordStream))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(result => result.Should().NotBeNull());

        // All results should be identical
        var firstResult = results[0];
        results.Should().AllSatisfy(result => result.Should().BeEquivalentTo(firstResult));
    }

    private static string[] GenerateRealWorldMatrix()
    {
        // Generate a realistic word search matrix
        var random = new Random(42);
        var size = 15;
        var matrix = new string[size];

        for (int i = 0; i < size; i++)
        {
            var row = new char[size];
            for (int j = 0; j < size; j++)
            {
                // Use more common letters to increase word probability
                var commonLetters = "ETAOINSHRDLCUMWFGYPBVKJXQZ";
                row[j] = commonLetters[random.Next(commonLetters.Length)];
            }
            matrix[i] = new string(row);
        }

        return matrix;
    }

    private static string[] GenerateRealWorldWordStream()
    {
        // Generate a realistic word stream with common English words
        var commonWords = new[]
        {
            "THE", "AND", "FOR", "ARE", "BUT", "NOT", "YOU", "ALL", "CAN", "HER",
            "WAS", "ONE", "OUR", "HAD", "BY", "WORD", "BUT", "WHAT", "SOME", "WE",
            "IT", "SAY", "SHE", "USE", "HIS", "AN", "EACH", "WHICH", "DO", "HOW",
            "THEIR", "IF", "WILL", "UP", "OTHER", "ABOUT", "OUT", "MANY", "THEN",
            "THEM", "THESE", "SO", "SOME", "HER", "WOULD", "MAKE", "LIKE", "INTO",
            "HIM", "TIME", "HAS", "TWO", "MORE", "GO", "NO", "WAY", "COULD", "MY",
            "THAN", "FIRST", "BEEN", "CALL", "WHO", "ITS", "NOW", "FIND", "LONG",
            "DOWN", "DAY", "DID", "GET", "COME", "MADE", "MAY", "PART", "OVER",
            "NEW", "SOUND", "TAKE", "ONLY", "LITTLE", "WORK", "KNOW", "PLACE",
            "YEARS", "LIVE", "ME", "BACK", "GIVE", "MOST", "VERY", "GOOD", "MAN",
            "OLD", "SEE", "HIM", "TWO", "MORE", "GO", "NO", "WAY", "COULD", "MY"
        };

        var random = new Random(42);
        var wordStream = new string[2000]; // 2000 words total

        for (int i = 0; i < wordStream.Length; i++)
        {
            if (i < commonWords.Length)
            {
                wordStream[i] = commonWords[i];
            }
            else
            {
                // Repeat some common words with higher frequency
                wordStream[i] = commonWords[random.Next(Math.Min(20, commonWords.Length))];
            }
        }

        return wordStream;
    }

    private static string[] GenerateLargeMatrix(int size)
    {
        var random = new Random(42);
        var matrix = new string[size];

        for (int i = 0; i < size; i++)
        {
            var row = new char[size];
            for (int j = 0; j < size; j++)
            {
                row[j] = (char)('A' + random.Next(26));
            }
            matrix[i] = new string(row);
        }

        return matrix;
    }

    private static string[] GenerateLargeWordStream(int count)
    {
        var random = new Random(42);
        var words = new string[count];
        var commonWords = new[]
        {
            "THE", "AND", "FOR", "ARE", "BUT", "NOT", "YOU", "ALL", "CAN", "HER",
            "WAS", "ONE", "OUR", "HAD", "BY", "WORD", "WHAT", "SOME", "WE", "IT",
            "SAY", "SHE", "USE", "HIS", "AN", "EACH", "WHICH", "DO", "HOW", "THEIR",
            "IF", "WILL", "UP", "OTHER", "ABOUT", "OUT", "MANY", "THEN", "THEM",
            "THESE", "SO", "HER", "WOULD", "MAKE", "LIKE", "INTO", "HIM", "TIME",
            "HAS", "TWO", "MORE", "GO", "NO", "WAY", "COULD", "MY", "THAN", "FIRST",
            "BEEN", "CALL", "WHO", "ITS", "NOW", "FIND", "LONG", "DOWN", "DAY",
            "DID", "GET", "COME", "MADE", "MAY", "PART"
        };

        for (int i = 0; i < count; i++)
        {
            if (i < commonWords.Length)
            {
                words[i] = commonWords[i];
            }
            else
            {
                var length = random.Next(3, 8);
                var word = new char[length];
                for (int j = 0; j < length; j++)
                {
                    word[j] = (char)('A' + random.Next(26));
                }
                words[i] = new string(word);
            }
        }

        return words;
    }
}
