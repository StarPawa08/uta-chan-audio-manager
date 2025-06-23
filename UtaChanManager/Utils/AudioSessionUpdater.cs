using System.Collections.ObjectModel;
using AudioSwitcher.AudioApi.Session;
using UtaChanManager.Models;

namespace UtaChanManager.Utils;

public class AudioSessionUpdater(ref List<AudioSessionInfo> audioSessionInfos) : IObserver<IAudioSession>
{
    List<AudioSessionInfo> _audioSessionInfos = audioSessionInfos;
    
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(IAudioSession value)
    {
        var existingSession = _audioSessionInfos.FirstOrDefault(x => x.Id == value.ProcessId);

        if (existingSession != null) return;
        var audioSessionInfo = new AudioSessionInfo
        {
            Id = value.ProcessId,
            ParentProcessId = value.ProcessId,
            Title = value.DisplayName ?? string.Empty,
            IsMuted = value.IsMuted,
            Session = value
        };
            
        _audioSessionInfos.Add(audioSessionInfo);
    }
}