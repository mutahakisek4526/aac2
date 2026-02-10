using System.Collections.ObjectModel;
using AacV2.Models;

namespace AacV2.ViewModels;

public sealed class HistoryViewModel : ViewModelBase
{
    private readonly KeyboardViewModel _keyboardViewModel;

    public HistoryViewModel(ObservableCollection<AacHistoryItem> history, KeyboardViewModel keyboardViewModel)
    {
        History = history;
        _keyboardViewModel = keyboardViewModel;
        UseHistoryCommand = new RelayCommand<AacHistoryItem>(UseHistory);
    }

    public ObservableCollection<AacHistoryItem> History { get; }
    public RelayCommand<AacHistoryItem> UseHistoryCommand { get; }

    private void UseHistory(AacHistoryItem? item)
    {
        if (item is not null)
        {
            _keyboardViewModel.CurrentText = item.Text;
        }
    }
}
