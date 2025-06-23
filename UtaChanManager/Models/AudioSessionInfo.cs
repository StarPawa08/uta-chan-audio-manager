using AudioSwitcher.AudioApi.Session;

namespace UtaChanManager.Models;

public class AudioSessionInfo
{
    public int Id { get; set; } = 0;
    public int ParentProcessId { get; set; } = 0;
    public string Title { get; set; } = string.Empty;
    public bool IsMuted { get; set; }
    public IAudioSession Session { get; init; } = null!;
}