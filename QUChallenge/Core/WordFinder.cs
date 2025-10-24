using System.Collections.Concurrent;
using System.Diagnostics;

namespace QUChallenge.Core;

/// <summary>
/// High-performance word finder that searches for words in a character matrix.
/// Supports horizontal (left-to-right) and vertical (top-to-bottom) word searches.
/// Optimized for large word streams with O(1) lookup time after preprocessing.
/// </summary>
public class WordFinder
{
    private readonly HashSet<string> _horizontalWords;
    private readonly HashSet<string> _verticalWords;
    private readonly int _matrixSize;
    private readonly string[] _matrix;

    /// <summary>
    /// Initializes a new instance of the WordFinder class.
    /// </summary>
    /// <param name="matrix">The character matrix to search in. Must be square and not exceed 64x64.</param>
    /// <exception cref="ArgumentNullException">Thrown when matrix is null.</exception>
    /// <exception cref="ArgumentException">Thrown when matrix is invalid or exceeds size limits.</exception>
    public WordFinder(IEnumerable<string> matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        _matrix = matrix.ToArray();
        ValidateMatrix(_matrix);

        _matrixSize = _matrix.Length;
        _horizontalWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _verticalWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        PreprocessMatrix();
    }

    /// <summary>
    /// Asynchronous version of Find method for better performance with large streams.
    /// </summary>
    /// <param name="wordstream">The stream of words to search for.</param>
    /// <returns>Top 10 most repeated words found in the matrix, ordered by frequency.</returns>
    public async Task<IEnumerable<string>> FindAsync(IEnumerable<string> wordstream)
    {
        ArgumentNullException.ThrowIfNull(wordstream);

        var wordFrequency = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var foundWords = new ConcurrentBag<string>();
        var processedWords = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            Parallel.ForEach(wordstream, word =>
            {
                if (string.IsNullOrEmpty(word)) return;

                wordFrequency.AddOrUpdate(word, 1, (key, value) => value + 1);

                if (IsWordInMatrix(word) && processedWords.TryAdd(word, 0))
                {
                    foundWords.Add(word);
                }
            });
        });

        return foundWords
            .Where(word => wordFrequency.ContainsKey(word))
            .OrderByDescending(word => wordFrequency[word])
            .ThenBy(word => word)
            .Take(10);
    }

    /// <summary>
    /// Checks if a word exists in the matrix (horizontal or vertical).
    /// </summary>
    /// <param name="word">The word to search for.</param>
    /// <returns>True if the word is found, false otherwise.</returns>
    private bool IsWordInMatrix(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;

        return _horizontalWords.Contains(word) || _verticalWords.Contains(word);
    }

    /// <summary>
    /// Preprocesses the matrix to extract all possible words in all four directions:
    /// - Horizontal: Left-to-right and Right-to-left
    /// - Vertical: Top-to-bottom and Bottom-to-top
    /// This optimization allows for O(1) lookup time during word searches.
    /// </summary>
    private void PreprocessMatrix()
    {
        // Extract horizontal words (left to right AND right to left)
        for (int row = 0; row < _matrixSize; row++)
        {
            var rowText = _matrix[row];
            for (int start = 0; start < rowText.Length; start++)
            {
                for (int end = start + 1; end <= rowText.Length; end++)
                {
                    var word = rowText.Substring(start, end - start);
                    if (word.Length > 0)
                    {
                        _horizontalWords.Add(word);

                        var reversedWord = new string(word.Reverse().ToArray());
                        if (reversedWord != word)
                        {
                            _horizontalWords.Add(reversedWord);
                        }
                    }
                }
            }
        }

        for (int col = 0; col < _matrixSize; col++)
        {
            for (int start = 0; start < _matrixSize; start++)
            {
                for (int end = start + 1; end <= _matrixSize; end++)
                {
                    var word = ExtractVerticalWord(start, end, col);
                    if (word.Length > 0)
                    {
                        _verticalWords.Add(word);

                        var reversedWord = new string(word.Reverse().ToArray());
                        if (reversedWord != word)
                        {
                            _verticalWords.Add(reversedWord);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extracts a vertical word from the matrix.
    /// </summary>
    /// <param name="startRow">Starting row index.</param>
    /// <param name="endRow">Ending row index (exclusive).</param>
    /// <param name="col">Column index.</param>
    /// <returns>The extracted vertical word.</returns>
    private string ExtractVerticalWord(int startRow, int endRow, int col)
    {
        var chars = new char[endRow - startRow];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = _matrix[startRow + i][col];
        }
        return new string(chars);
    }

    /// <summary>
    /// Validates the input matrix for correctness and constraints.
    /// </summary>
    /// <param name="matrix">The matrix to validate.</param>
    /// <exception cref="ArgumentException">Thrown when matrix is invalid.</exception>
    private static void ValidateMatrix(string[] matrix)
    {
        if (matrix.Length == 0)
            throw new ArgumentException("Matrix cannot be empty.", nameof(matrix));

        if (matrix.Length > 64)
            throw new ArgumentException("Matrix size cannot exceed 64x64.", nameof(matrix));

        var firstRowLength = matrix[0].Length;
        if (firstRowLength > 64)
            throw new ArgumentException("Matrix row length cannot exceed 64.", nameof(matrix));

        for (int i = 0; i < matrix.Length; i++)
        {
            if (matrix[i].Length != firstRowLength)
                throw new ArgumentException("All matrix rows must have the same length.", nameof(matrix));
        }

        if (matrix.Length != firstRowLength)
            throw new ArgumentException("Matrix must be square.", nameof(matrix));
    }
}
