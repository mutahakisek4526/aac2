namespace AacV2.Models;

public sealed class AppSettings
{
    public InputModeKind InputModeKind { get; set; } = InputModeKind.Mouse;
    public int DwellTimeMs { get; set; } = 800;
    public double FontScale { get; set; } = 1.0;
    public double ButtonScale { get; set; } = 1.0;
}
