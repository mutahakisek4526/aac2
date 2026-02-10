using System.IO;
using System.Text.Json;
using AacV2.Models;

namespace AacV2.Services;

public sealed class StorageService : IStorageService
{
    private readonly string _baseDir;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public StorageService()
    {
        _baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AacV2");
        Directory.CreateDirectory(_baseDir);
    }

    public Task<IReadOnlyList<AacHistoryItem>> LoadHistory() => LoadFile("history.json", new List<AacHistoryItem>());
    public Task SaveHistory(IReadOnlyCollection<AacHistoryItem> items) => SaveFile("history.json", items);

    public Task<IReadOnlyList<AacPhraseItem>> LoadPhrases() => LoadFile("phrases.json", new List<AacPhraseItem>());
    public Task SavePhrases(IReadOnlyCollection<AacPhraseItem> items) => SaveFile("phrases.json", items);

    public Task<IReadOnlyList<string>> LoadLearnedWords() => LoadFile("learnedWords.json", new List<string>());
    public Task SaveLearnedWords(IReadOnlyCollection<string> words) => SaveFile("learnedWords.json", words);

    public Task<IReadOnlyList<AacEnvironmentAction>> LoadEnvironmentActions() => LoadFile("environmentActions.json", GetDefaultActions());
    public Task SaveEnvironmentActions(IReadOnlyCollection<AacEnvironmentAction> actions) => SaveFile("environmentActions.json", actions);

    public async Task<AppSettings> LoadSettings()
    {
        var path = ResolvePath("settings.json");
        if (!File.Exists(path))
        {
            var settings = new AppSettings();
            await SaveSettings(settings);
            return settings;
        }

        await using var stream = File.OpenRead(path);
        var loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _options);
        return loaded ?? new AppSettings();
    }

    public async Task SaveSettings(AppSettings settings)
    {
        var path = ResolvePath("settings.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, settings, _options);
    }

    private async Task<IReadOnlyList<T>> LoadFile<T>(string fileName, IReadOnlyList<T> fallback)
    {
        var path = ResolvePath(fileName);
        if (!File.Exists(path))
        {
            await SaveFile(fileName, fallback);
            return fallback;
        }

        await using var stream = File.OpenRead(path);
        var loaded = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options);
        return loaded ?? fallback;
    }

    private async Task SaveFile<T>(string fileName, IReadOnlyCollection<T> items)
    {
        var path = ResolvePath(fileName);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, items, _options);
    }

    private string ResolvePath(string fileName) => Path.Combine(_baseDir, fileName);

    private static IReadOnlyList<AacEnvironmentAction> GetDefaultActions() =>
        new List<AacEnvironmentAction>
        {
            new() { Name = "Googleを開く", Category = "Web", ActionType = EnvironmentActionType.OpenUrl, Payload = "https://www.google.com" },
            new() { Name = "メモ帳", Category = "PC", ActionType = EnvironmentActionType.PcCommand, Payload = "notepad" }
        };
}
