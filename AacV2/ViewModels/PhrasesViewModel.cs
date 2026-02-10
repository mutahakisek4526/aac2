using System.Collections.ObjectModel;
using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class PhrasesViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private readonly ObservableCollection<AacPhraseItem> _phrases;

    private string _newText = string.Empty;
    private string _newCategory = "一般";

    public PhrasesViewModel(IStorageService storageService, ObservableCollection<AacPhraseItem> phrases)
    {
        _storageService = storageService;
        _phrases = phrases;
        AddPhraseCommand = new RelayCommand(async () => await AddPhraseAsync());
        DeletePhraseCommand = new RelayCommand<AacPhraseItem>(async item => await DeletePhraseAsync(item));
    }

    public ObservableCollection<AacPhraseItem> Phrases => _phrases;
    public RelayCommand AddPhraseCommand { get; }
    public RelayCommand<AacPhraseItem> DeletePhraseCommand { get; }

    public string NewText
    {
        get => _newText;
        set => SetProperty(ref _newText, value);
    }

    public string NewCategory
    {
        get => _newCategory;
        set => SetProperty(ref _newCategory, value);
    }

    private async Task AddPhraseAsync()
    {
        if (string.IsNullOrWhiteSpace(NewText))
        {
            return;
        }

        _phrases.Add(new AacPhraseItem { Text = NewText.Trim(), Category = NewCategory.Trim() });
        await _storageService.SavePhrases(_phrases);
        NewText = string.Empty;
    }

    private async Task DeletePhraseAsync(AacPhraseItem? item)
    {
        if (item is null)
        {
            return;
        }

        _phrases.Remove(item);
        await _storageService.SavePhrases(_phrases);
    }
}
