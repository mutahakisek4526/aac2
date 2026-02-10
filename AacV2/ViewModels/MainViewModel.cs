using System.Collections.ObjectModel;
using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private readonly PredictionService _predictionService;
    private readonly ISpeechService _speechService;
    private readonly IComputerControlService _computerControlService;

    private ViewModelBase? _currentPage;
    private AppSettings _settings = new();

    public MainViewModel()
    {
        _storageService = new StorageService();
        _predictionService = new PredictionService();
        _speechService = new SpeechService();
        _computerControlService = new ComputerControlService();

        History = new ObservableCollection<AacHistoryItem>();
        Phrases = new ObservableCollection<AacPhraseItem>();
        EnvironmentActions = new ObservableCollection<AacEnvironmentAction>();

        HomePage = new HomeViewModel();
        KeyboardPage = new KeyboardViewModel(_predictionService, _speechService, _storageService, History, Phrases);
        PhrasesPage = new PhrasesViewModel(_storageService, Phrases);
        HistoryPage = new HistoryViewModel(History, KeyboardPage);
        PredictionPage = new PredictionViewModel(_predictionService, History, Phrases);
        ComputerPage = new ComputerControlViewModel(_computerControlService);
        EnvironmentPage = new EnvironmentControlViewModel(_storageService, EnvironmentActions);
        SettingsPage = new SettingsViewModel(_storageService);
        CaregiverPage = new CaregiverViewModel(_storageService, History, _predictionService);

        NavigateHomeCommand = new RelayCommand(() => CurrentPage = HomePage);
        NavigateKeyboardCommand = new RelayCommand(() => CurrentPage = KeyboardPage);
        NavigatePhrasesCommand = new RelayCommand(() => CurrentPage = PhrasesPage);
        NavigateHistoryCommand = new RelayCommand(() => CurrentPage = HistoryPage);
        NavigatePredictionCommand = new RelayCommand(() => CurrentPage = PredictionPage);
        NavigateComputerCommand = new RelayCommand(() => CurrentPage = ComputerPage);
        NavigateEnvironmentCommand = new RelayCommand(() => CurrentPage = EnvironmentPage);
        NavigateSettingsCommand = new RelayCommand(() => CurrentPage = SettingsPage);
        NavigateCaregiverCommand = new RelayCommand(() => CurrentPage = CaregiverPage);

        CurrentPage = HomePage;
    }

    public ObservableCollection<AacHistoryItem> History { get; }
    public ObservableCollection<AacPhraseItem> Phrases { get; }
    public ObservableCollection<AacEnvironmentAction> EnvironmentActions { get; }

    public HomeViewModel HomePage { get; }
    public KeyboardViewModel KeyboardPage { get; }
    public PhrasesViewModel PhrasesPage { get; }
    public HistoryViewModel HistoryPage { get; }
    public PredictionViewModel PredictionPage { get; }
    public ComputerControlViewModel ComputerPage { get; }
    public EnvironmentControlViewModel EnvironmentPage { get; }
    public SettingsViewModel SettingsPage { get; }
    public CaregiverViewModel CaregiverPage { get; }

    public RelayCommand NavigateHomeCommand { get; }
    public RelayCommand NavigateKeyboardCommand { get; }
    public RelayCommand NavigatePhrasesCommand { get; }
    public RelayCommand NavigateHistoryCommand { get; }
    public RelayCommand NavigatePredictionCommand { get; }
    public RelayCommand NavigateComputerCommand { get; }
    public RelayCommand NavigateEnvironmentCommand { get; }
    public RelayCommand NavigateSettingsCommand { get; }
    public RelayCommand NavigateCaregiverCommand { get; }

    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public AppSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    public async Task InitializeAsync()
    {
        Settings = await _storageService.LoadSettings();
        await SettingsPage.LoadAsync(Settings);

        History.Clear();
        foreach (var item in await _storageService.LoadHistory())
        {
            History.Add(item);
        }

        Phrases.Clear();
        foreach (var phrase in await _storageService.LoadPhrases())
        {
            Phrases.Add(phrase);
        }

        EnvironmentActions.Clear();
        foreach (var action in await _storageService.LoadEnvironmentActions())
        {
            EnvironmentActions.Add(action);
        }

        var learnedWords = await _storageService.LoadLearnedWords();
        _predictionService.SeedLearnedWords(learnedWords);

        await KeyboardPage.InitializeAsync(Settings);
        await PredictionPage.RefreshAsync(string.Empty);
    }
}
