using System.Collections.ObjectModel;
using System.Diagnostics;
using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class EnvironmentControlViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;

    public EnvironmentControlViewModel(IStorageService storageService, ObservableCollection<AacEnvironmentAction> actions)
    {
        _storageService = storageService;
        Actions = actions;
        RunActionCommand = new RelayCommand<AacEnvironmentAction>(RunAction);
        SaveCommand = new RelayCommand(async () => await _storageService.SaveEnvironmentActions(Actions));
    }

    public ObservableCollection<AacEnvironmentAction> Actions { get; }
    public RelayCommand<AacEnvironmentAction> RunActionCommand { get; }
    public RelayCommand SaveCommand { get; }

    private void RunAction(AacEnvironmentAction? action)
    {
        if (action is null)
        {
            return;
        }

        if (action.ActionType == EnvironmentActionType.OpenUrl)
        {
            Process.Start(new ProcessStartInfo(action.Payload) { UseShellExecute = true });
            return;
        }

        Process.Start(new ProcessStartInfo(action.Payload) { UseShellExecute = true });
    }
}
