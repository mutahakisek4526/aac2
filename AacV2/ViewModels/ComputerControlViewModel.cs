using AacV2.Models;
using AacV2.Services;

namespace AacV2.ViewModels;

public sealed class ComputerControlViewModel : ViewModelBase
{
    private readonly IComputerControlService _computer;

    public ComputerControlViewModel(IComputerControlService computer)
    {
        _computer = computer;
        EnterCommand = new RelayCommand(() => _computer.SendKey(AacKeyCode.Enter));
        SpaceCommand = new RelayCommand(() => _computer.SendKey(AacKeyCode.Space));
        CtrlCCommand = new RelayCommand(() => _computer.KeyCombo(AacKeyCode.Ctrl, AacKeyCode.C));
        LeftClickCommand = new RelayCommand(_computer.LeftClick);
        RightClickCommand = new RelayCommand(_computer.RightClick);
        DoubleClickCommand = new RelayCommand(_computer.DoubleClick);
        MoveUpCommand = new RelayCommand(() => _computer.MouseMove(0, -20));
        MoveDownCommand = new RelayCommand(() => _computer.MouseMove(0, 20));
        MoveLeftCommand = new RelayCommand(() => _computer.MouseMove(-20, 0));
        MoveRightCommand = new RelayCommand(() => _computer.MouseMove(20, 0));
    }

    public RelayCommand EnterCommand { get; }
    public RelayCommand SpaceCommand { get; }
    public RelayCommand CtrlCCommand { get; }
    public RelayCommand LeftClickCommand { get; }
    public RelayCommand RightClickCommand { get; }
    public RelayCommand DoubleClickCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand MoveLeftCommand { get; }
    public RelayCommand MoveRightCommand { get; }
}
