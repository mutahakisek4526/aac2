using AacV2.Models;

namespace AacV2.Services;

public interface IPredictionService
{
    Task<IReadOnlyList<string>> GetPredictions(
        string text,
        IReadOnlyCollection<AacHistoryItem> history,
        IReadOnlyCollection<AacPhraseItem> phrases);

    Task LearnWord(string text);
}
