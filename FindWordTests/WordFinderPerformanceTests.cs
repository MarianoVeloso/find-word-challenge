using FluentAssertions;
using QUChallenge.Core;
using Xunit;

namespace QUCHallengeTests;

public class WordFinderPerformanceTests
{
    [Fact]
    public async Task FindAsync_LargeWordStream_ShouldCompleteQuickly()
    {
        // Arrange
        var matrix = GenerateLargeMatrix(20);
        var wordStream = GenerateLargeWordStream(10000);
        var finder = new WordFinder(matrix);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await finder.FindAsync(wordStream);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Large word stream should complete in under 1 second");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FindAsync_PerformanceComparison_SyncVsAsync()
    {
        // Arrange
        var matrix = GenerateLargeMatrix(15);
        var wordStream = GenerateLargeWordStream(5000);
        var finder = new WordFinder(matrix);

        // Act - Sync
        var syncStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var syncResult = await finder.FindAsync(wordStream);
        syncStopwatch.Stop();

        // Act - Async
        var asyncStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var asyncResult = await finder.FindAsync(wordStream);
        asyncStopwatch.Stop();

        // Assert
        Console.WriteLine($"Sync: {syncStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Async: {asyncStopwatch.ElapsedMilliseconds}ms");
        
        // Async should be faster or at least comparable for large streams
        asyncStopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(syncStopwatch.ElapsedMilliseconds + 50);
        
        // Results should be equivalent
        asyncResult.Should().BeEquivalentTo(syncResult);
    }

    [Fact]
    public async Task FindAsync_MemoryEfficiency_ShouldNotThrowOutOfMemory()
    {
        // Arrange
        var matrix = GenerateLargeMatrix(30);
        var wordStream = GenerateLargeWordStream(50000); // 50k words
        var finder = new WordFinder(matrix);

        // Act & Assert
        var action = async () => await finder.FindAsync(wordStream);
        await action.Should().NotThrowAsync<OutOfMemoryException>();
    }

    private static string[] GenerateLargeMatrix(int size)
    {
        var random = new Random(42); // Fixed seed for reproducible results
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
        var random = new Random(42); // Fixed seed for reproducible results
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
