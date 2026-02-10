using System.Collections.ObjectModel;
using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class KeyboardViewModel : ViewModelBase
{
    private readonly IPredictionService _predictionService;
    private readonly ISpeechService _speechService;
    private readonly IStorageService _storageService;
    private readonly ObservableCollection<AacHistoryItem> _history;
    private readonly ObservableCollection<AacPhraseItem> _phrases;

    private string _currentText = string.Empty;
    private string _selectedGroup = "あ";

    private readonly Dictionary<string, string[]> _kana = new()
    {
        ["あ"] = ["あ", "い", "う", "え", "お"],
        ["か"] = ["か", "き", "く", "け", "こ"],
        ["さ"] = ["さ", "し", "す", "せ", "そ"],
        ["た"] = ["た", "ち", "つ", "て", "と"],
        ["な"] = ["な", "に", "ぬ", "ね", "の"],
        ["は"] = ["は", "ひ", "ふ", "へ", "ほ"],
        ["ま"] = ["ま", "み", "む", "め", "も"],
        ["や"] = ["や", "ゆ", "よ"],
        ["ら"] = ["ら", "り", "る", "れ", "ろ"],
        ["わ"] = ["わ", "を", "ん"]
    };

    public KeyboardViewModel(
        IPredictionService predictionService,
        ISpeechService speechService,
        IStorageService storageService,
        ObservableCollection<AacHistoryItem> history,
        ObservableCollection<AacPhraseItem> phrases)
    {
        _predictionService = predictionService;
        _speechService = speechService;
        _storageService = storageService;
        _history = history;
        _phrases = phrases;

        KanaGroups = new ObservableCollection<string>(_kana.Keys);
        CurrentCharacters = new ObservableCollection<string>(_kana[_selectedGroup]);
        Predictions = new ObservableCollection<string>();

        SelectGroupCommand = new RelayCommand<string>(SelectGroup);
        AddCharacterCommand = new RelayCommand<string>(AddCharacter);
        DeleteCharCommand = new RelayCommand(DeleteChar);
        ClearCommand = new RelayCommand(Clear);
        ConfirmCommand = new RelayCommand(async () => await ConfirmAsync());
        SpeakCommand = new RelayCommand(async () => await _speechService.SpeakAsync(CurrentText));
        StopSpeakCommand = new RelayCommand(_speechService.Stop);
        ApplyPredictionCommand = new RelayCommand<string>(ApplyPrediction);
    }

    public ObservableCollection<string> KanaGroups { get; }
    public ObservableCollection<string> CurrentCharacters { get; }
    public ObservableCollection<string> Predictions { get; }

    public RelayCommand<string> SelectGroupCommand { get; }
    public RelayCommand<string> AddCharacterCommand { get; }
    public RelayCommand DeleteCharCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand ConfirmCommand { get; }
    public RelayCommand SpeakCommand { get; }
    public RelayCommand StopSpeakCommand { get; }
    public RelayCommand<string> ApplyPredictionCommand { get; }

    public string CurrentText
    {
        get => _currentText;
        set
        {
            if (SetProperty(ref _currentText, value))
            {
                _ = UpdatePredictionsAsync();
            }
        }
    }

    public string SelectedGroup
    {
        get => _selectedGroup;
        set => SetProperty(ref _selectedGroup, value);
    }

    public async Task InitializeAsync(AppSettings settings)
    {
        await UpdatePredictionsAsync();
    }

    private void SelectGroup(string? group)
    {
        if (string.IsNullOrWhiteSpace(group) || !_kana.TryGetValue(group, out var chars))
        {
            return;
        }

        SelectedGroup = group;
        CurrentCharacters.Clear();
        foreach (var c in chars)
        {
            CurrentCharacters.Add(c);
        }
    }

    private void AddCharacter(string? c)
    {
        if (string.IsNullOrWhiteSpace(c))
        {
            return;
        }

        CurrentText += c;
    }

    private void DeleteChar()
    {
        if (CurrentText.Length > 0)
        {
            CurrentText = CurrentText[..^1];
        }
    }

    private void Clear()
    {
        CurrentText = string.Empty;
    }

    private async Task ConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentText))
        {
            return;
        }

        var text = CurrentText.Trim();
        _history.Insert(0, new AacHistoryItem { Timestamp = DateTime.Now, Text = text });
        await _storageService.SaveHistory(_history);

        await _predictionService.LearnWord(text);
        if (_predictionService is PredictionService concrete)
        {
            await _storageService.SaveLearnedWords(concrete.SnapshotLearnedWords());
        }

        CurrentText = string.Empty;
    }

    private void ApplyPrediction(string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            CurrentText = text;
        }
    }

    private async Task UpdatePredictionsAsync()
    {
        var items = await _predictionService.GetPredictions(CurrentText, _history, _phrases);
        Predictions.Clear();
        foreach (var item in items.Take(10))
        {
            Predictions.Add(item);
        }
    }
}
