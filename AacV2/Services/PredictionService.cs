using AacV2.Models;

namespace AacV2.Services;

public sealed class PredictionService : IPredictionService
{
    private readonly HashSet<string> _learned = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<string>> GetPredictions(
        string text,
        IReadOnlyCollection<AacHistoryItem> history,
        IReadOnlyCollection<AacPhraseItem> phrases)
    {
        IEnumerable<string> source = history.Select(h => h.Text)
            .Concat(phrases.Select(p => p.Text))
            .Concat(_learned)
            .Where(s => !string.IsNullOrWhiteSpace(s));

        if (!string.IsNullOrWhiteSpace(text))
        {
            source = source.Where(s => s.StartsWith(text, StringComparison.OrdinalIgnoreCase) ||
                                       s.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        var results = source
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s.Length)
            .ThenBy(s => s)
            .Take(10)
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(results);
    }

    public Task LearnWord(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            _learned.Add(text.Trim());
        }

        return Task.CompletedTask;
    }

    public void SeedLearnedWords(IEnumerable<string> words)
    {
        _learned.Clear();
        foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)))
        {
            _learned.Add(word.Trim());
        }
    }

    public IReadOnlyList<string> SnapshotLearnedWords() => _learned.OrderBy(x => x).ToArray();
}
