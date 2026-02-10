using AacV2.Models;

namespace AacV2.Services;

public interface IStorageService
{
    Task<IReadOnlyList<AacHistoryItem>> LoadHistory();
    Task SaveHistory(IReadOnlyCollection<AacHistoryItem> items);
    Task<IReadOnlyList<AacPhraseItem>> LoadPhrases();
    Task SavePhrases(IReadOnlyCollection<AacPhraseItem> items);
    Task<IReadOnlyList<string>> LoadLearnedWords();
    Task SaveLearnedWords(IReadOnlyCollection<string> words);
    Task<IReadOnlyList<AacEnvironmentAction>> LoadEnvironmentActions();
    Task SaveEnvironmentActions(IReadOnlyCollection<AacEnvironmentAction> actions);
    Task<AppSettings> LoadSettings();
    Task SaveSettings(AppSettings settings);
}
