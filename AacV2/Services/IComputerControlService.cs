using AacV2.Models;

namespace AacV2.Services;

public interface IComputerControlService
{
    void SendKey(AacKeyCode key);
    void KeyCombo(params AacKeyCode[] keys);
    void MouseMove(int dx, int dy);
    void LeftClick();
    void RightClick();
    void DoubleClick();
}
