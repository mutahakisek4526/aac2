using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private AppSettings _settings = new();

    public SettingsViewModel(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public InputModeKind InputModeKind
    {
        get => _settings.InputModeKind;
        set
        {
            if (_settings.InputModeKind != value)
            {
                _settings.InputModeKind = value;
                RaisePropertyChanged();
                _ = SaveAsync();
            }
        }
    }

    public int DwellTimeMs
    {
        get => _settings.DwellTimeMs;
        set
        {
            if (_settings.DwellTimeMs != value)
            {
                _settings.DwellTimeMs = value;
                RaisePropertyChanged();
                _ = SaveAsync();
            }
        }
    }

    public double FontScale
    {
        get => _settings.FontScale;
        set
        {
            if (Math.Abs(_settings.FontScale - value) > 0.001)
            {
                _settings.FontScale = value;
                RaisePropertyChanged();
                _ = SaveAsync();
            }
        }
    }

    public double ButtonScale
    {
        get => _settings.ButtonScale;
        set
        {
            if (Math.Abs(_settings.ButtonScale - value) > 0.001)
            {
                _settings.ButtonScale = value;
                RaisePropertyChanged();
                _ = SaveAsync();
            }
        }
    }

    public async Task LoadAsync(AppSettings settings)
    {
        _settings = settings;
        RaisePropertyChanged(nameof(InputModeKind));
        RaisePropertyChanged(nameof(DwellTimeMs));
        RaisePropertyChanged(nameof(FontScale));
        RaisePropertyChanged(nameof(ButtonScale));
        await Task.CompletedTask;
    }

    private async Task SaveAsync() => await _storageService.SaveSettings(_settings);
}
