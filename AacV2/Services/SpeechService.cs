using System.Speech.Synthesis;

namespace AacV2.Services;

public sealed class SpeechService : ISpeechService
{
    private readonly SpeechSynthesizer _synthesizer = new();

    public Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.CompletedTask;
        }

        Stop();
        return Task.Run(() => _synthesizer.Speak(text));
    }

    public void Stop()
    {
        _synthesizer.SpeakAsyncCancelAll();
    }
}
