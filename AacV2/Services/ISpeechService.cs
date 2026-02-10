namespace AacV2.Services;

public interface ISpeechService
{
    Task SpeakAsync(string text);
    void Stop();
}
