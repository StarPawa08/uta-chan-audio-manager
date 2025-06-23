using System.Diagnostics;
using System.Management;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Session;
using UtaChanManager.Models;
using UtaChanManager.Utils;

namespace UtaChanManager.Services;

public static class AudioManager
{
    private static readonly CoreAudioController AudioController = new();

    private static List<AudioSessionInfo> _activeAudioSessions;

    static AudioManager()
    {
        _activeAudioSessions = [];
        var task = GetActiveAudioSessionAsync();
        _activeAudioSessions = task.Result.ToList();
    }
    
    private static async Task<IEnumerable<AudioSessionInfo>> GetActiveAudioSessionAsync()
    {
        if(!_activeAudioSessions.IsEmpty()) return _activeAudioSessions;
        
        var defaultPlaybackDevice = AudioController.GetDefaultDevice(DeviceType.Playback, Role.Multimedia);
        
        var sessionController = defaultPlaybackDevice.GetCapability<IAudioSessionController>();
        if (sessionController == null)
            return [];

        var sessions = await sessionController.AllAsync();

        var filtered = sessions
            .Where(s => s.IsSystemSession == false && !string.IsNullOrWhiteSpace(s.DisplayName));

        _activeAudioSessions.AddRange(filtered.Select(s => new AudioSessionInfo
        {
            Id = s.ProcessId,
            Title = s.DisplayName,
            IsMuted = s.IsMuted,
            Session = s
        }).ToList());
        
        sessionController.SessionCreated.Subscribe(new AudioSessionUpdater(ref _activeAudioSessions));

        return _activeAudioSessions;
    }

    public static async Task MuteAllExceptAsync(int windowProcessId)
    {
        // Obtener el nombre del proceso de la ventana
        string? targetProcessName = null;
        try
        {
            var windowProcess = Process.GetProcessById(windowProcessId);
            targetProcessName = windowProcess.ProcessName;
        }
        catch
        {
            return; // Si no se puede obtener el proceso, no hacer nada
        }

        foreach (var audioSession in _activeAudioSessions)
        {
            var shouldMute = true; // Por defecto mutear
            if (audioSession.ParentProcessId == windowProcessId)
            {
                shouldMute = false; // No mutear si es el mismo proceso
                await audioSession.Session.SetMuteAsync(shouldMute);
                continue;
            }

            if (audioSession.ParentProcessId != 0 && audioSession.ParentProcessId != windowProcessId)
            {
                if(audioSession.Session.IsMuted) continue;
                await audioSession.Session.SetMuteAsync(shouldMute);
                continue;
            }
            
            try
            {
                // Obtener el proceso de la sesión de audio
                var audioProcess = Process.GetProcessById(audioSession.Id);
                
                // Comparar nombres de proceso (sin extensión)
                if (string.Equals(audioProcess.ProcessName, targetProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    shouldMute = false; // No mutear si es el mismo proceso
                    audioSession.ParentProcessId = windowProcessId; // Guardar el proceso padre
                }
                
                // También verificar procesos padre/hijo o relacionados
                if (shouldMute && AreRelatedProcesses(audioProcess, targetProcessName))
                {
                    shouldMute = false;
                    audioSession.ParentProcessId = windowProcessId;
                }
            }
            catch
            {
                // Si no se puede obtener el proceso de audio, mantener muted = true
            }
            
            await audioSession.Session.SetMuteAsync(shouldMute);
        }
    }
    
    private static bool AreRelatedProcesses(Process audioProcess, string targetProcessName)
    {
        try
        {
            var parentProcess = GetParentProcess(audioProcess);
            if (parentProcess != null && 
                string.Equals(parentProcess.ProcessName, targetProcessName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            return audioProcess.ProcessName.StartsWith(targetProcessName, StringComparison.OrdinalIgnoreCase) ||
                   targetProcessName.StartsWith(audioProcess.ProcessName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    
    // Método auxiliar para obtener el proceso padre
    private static Process? GetParentProcess(Process process)
    {
        try
        {
            using var query = new ManagementObjectSearcher(
                $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}");
            using var results = query.Get();
            
            foreach (var result in results)
            {
                var parentId = (uint)result["ParentProcessId"];
                return Process.GetProcessById((int)parentId);
            }
        }
        catch
        {
            // Ignorar errores
        }
        
        return null;
    }
    
    public static async Task UnMuteAllAsync()
    {
        foreach (var audioSession in _activeAudioSessions)
        {
            await audioSession.Session.SetMuteAsync(false);
        }
    }
}