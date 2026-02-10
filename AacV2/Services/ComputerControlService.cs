using System.Runtime.InteropServices;
using AacV2.Models;

namespace AacV2.Services;

public sealed class ComputerControlService : IComputerControlService
{
    public void SendKey(AacKeyCode key)
    {
        KeyDown((ushort)key);
        KeyUp((ushort)key);
    }

    public void KeyCombo(params AacKeyCode[] keys)
    {
        foreach (var key in keys)
        {
            KeyDown((ushort)key);
        }

        for (var i = keys.Length - 1; i >= 0; i--)
        {
            KeyUp((ushort)keys[i]);
        }
    }

    public void MouseMove(int dx, int dy)
    {
        var input = new INPUT
        {
            type = 0,
            U = new INPUTUNION
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = 0,
                    dwFlags = MouseEventFlags.MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Send(new[] { input });
    }

    public void LeftClick()
    {
        SendMouse(MouseEventFlags.LEFTDOWN);
        SendMouse(MouseEventFlags.LEFTUP);
    }

    public void RightClick()
    {
        SendMouse(MouseEventFlags.RIGHTDOWN);
        SendMouse(MouseEventFlags.RIGHTUP);
    }

    public void DoubleClick()
    {
        LeftClick();
        LeftClick();
    }

    private static void SendMouse(uint flags)
    {
        var input = new INPUT
        {
            type = 0,
            U = new INPUTUNION
            {
                mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Send(new[] { input });
    }

    private static void KeyDown(ushort key)
    {
        var input = new INPUT
        {
            type = 1,
            U = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = key,
                    wScan = 0,
                    dwFlags = KeyEventFlags.KEYDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Send(new[] { input });
    }

    private static void KeyUp(ushort key)
    {
        var input = new INPUT
        {
            type = 1,
            U = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = key,
                    wScan = 0,
                    dwFlags = KeyEventFlags.KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Send(new[] { input });
    }

    private static void Send(INPUT[] inputs)
    {
        _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static class MouseEventFlags
    {
        public const uint MOVE = 0x0001;
        public const uint LEFTDOWN = 0x0002;
        public const uint LEFTUP = 0x0004;
        public const uint RIGHTDOWN = 0x0008;
        public const uint RIGHTUP = 0x0010;
    }

    private static class KeyEventFlags
    {
        public const uint KEYDOWN = 0x0000;
        public const uint KEYUP = 0x0002;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
