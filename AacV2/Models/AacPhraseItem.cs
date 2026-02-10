namespace AacV2.Models;

public sealed class AacPhraseItem
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = "一般";
    public string Name => Text;
}
