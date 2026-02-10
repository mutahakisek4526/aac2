using System.Collections.ObjectModel;
using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class CaregiverViewModel : ViewModelBase
{
    private readonly IStorageService _storage;
    private readonly ObservableCollection<AacHistoryItem> _history;
    private readonly PredictionService _prediction;

    public CaregiverViewModel(
        IStorageService storage,
        ObservableCollection<AacHistoryItem> history,
        PredictionService prediction)
    {
        _storage = storage;
        _history = history;
        _prediction = prediction;
        ResetDataCommand = new RelayCommand(async () => await ResetDataAsync());
    }

    public string HelpText =>
        "支援者モード\n" +
        "・視線入力はマウスカーソル位置で注視判定します。\n" +
        "・GazeOnlyは滞留で自動クリックします。\n" +
        "・GazeSwitchは注視後にEnter/Spaceで決定します。\n" +
        "・SwitchOnlyはスキャン＋スイッチ利用を想定します。";

    public RelayCommand ResetDataCommand { get; }

    private async Task ResetDataAsync()
    {
        _history.Clear();
        await _storage.SaveHistory(_history);
        _prediction.SeedLearnedWords(Array.Empty<string>());
        await _storage.SaveLearnedWords(Array.Empty<string>());
    }
}
