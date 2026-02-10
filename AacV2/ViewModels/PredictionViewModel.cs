using System.Collections.ObjectModel;
using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class PredictionViewModel : ViewModelBase
{
    private readonly IPredictionService _predictionService;
    private readonly ObservableCollection<AacHistoryItem> _history;
    private readonly ObservableCollection<AacPhraseItem> _phrases;

    private string _query = string.Empty;

    public PredictionViewModel(
        IPredictionService predictionService,
        ObservableCollection<AacHistoryItem> history,
        ObservableCollection<AacPhraseItem> phrases)
    {
        _predictionService = predictionService;
        _history = history;
        _phrases = phrases;
        Predictions = new ObservableCollection<string>();
        RefreshCommand = new RelayCommand(async () => await RefreshAsync(Query));
    }

    public ObservableCollection<string> Predictions { get; }
    public RelayCommand RefreshCommand { get; }

    public string Query
    {
        get => _query;
        set => SetProperty(ref _query, value);
    }

    public async Task RefreshAsync(string text)
    {
        var results = await _predictionService.GetPredictions(text, _history, _phrases);
        Predictions.Clear();
        foreach (var item in results)
        {
            Predictions.Add(item);
        }
    }
}
