using UnityEngine;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

/// <summary>
/// TTS Server Starter - Starts Python TTS server from Unity
/// </summary>
public class TTSServerStarter : MonoBehaviour
{
    [Header("TTS Server Settings")]
    public string pythonPath = "py"; // Đổi mặc định thành 'py' cho Windows
    public string ttsScriptPath = "SimpleTTSServer.py";
    public int ttsPort = 8001;
    public bool autoStartOnPlay = true;
    public bool verboseLogging = true;
    
    private Process ttsProcess;
    private bool isServerRunning = false;
    
    void Start()
    {
        if (autoStartOnPlay)
        {
            StartTTSServer();
        }
    }
    
    void OnDestroy()
    {
        StopTTSServer();
    }
    
    void OnApplicationQuit()
    {
        StopTTSServer();
    }
    
    [ContextMenu("Start TTS Server")]
    public void StartTTSServer()
    {
        if (isServerRunning)
        {
            Debug.LogWarning("⚠️ TTS Server is already running!");
            return;
        }
        
        Debug.Log("🚀 Starting TTS Server...");
        
        // Check if Python script exists
        string scriptFullPath = Path.Combine(Application.dataPath, "..", ttsScriptPath);
        if (!File.Exists(scriptFullPath))
        {
            Debug.LogError($"❌ TTS script not found at: {scriptFullPath}");
            Debug.Log("Creating default TTS script...");
            CreateDefaultTTSScript();
        }
        
        try
        {
            // Start Python process
            ttsProcess = new Process();
            ttsProcess.StartInfo.FileName = pythonPath;
            ttsProcess.StartInfo.Arguments = $"\"{scriptFullPath}\"";
            ttsProcess.StartInfo.UseShellExecute = false;
            ttsProcess.StartInfo.RedirectStandardOutput = true;
            ttsProcess.StartInfo.RedirectStandardError = true;
            ttsProcess.StartInfo.CreateNoWindow = false;
            ttsProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(scriptFullPath);
            
            // Set up output handling
            ttsProcess.OutputDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data) && verboseLogging)
                {
                    UnityEngine.Debug.Log($"[TTS Server] {e.Data}");
                }
            };
            
            ttsProcess.ErrorDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.LogError($"[TTS Server Error] {e.Data}");
                }
            };
            
            // Start the process
            ttsProcess.Start();
            ttsProcess.BeginOutputReadLine();
            ttsProcess.BeginErrorReadLine();
            
            isServerRunning = true;
            
            if (verboseLogging)
                Debug.Log($"✅ TTS Server started on port {ttsPort}");
            
            // Wait a bit for server to start
            Invoke(nameof(TestTTSServer), 2f);
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to start TTS Server: {e.Message}");
            isServerRunning = false;
        }
    }
    
    [ContextMenu("Stop TTS Server")]
    public void StopTTSServer()
    {
        if (!isServerRunning || ttsProcess == null)
        {
            return;
        }
        
        Debug.Log("🛑 Stopping TTS Server...");
        
        try
        {
            if (!ttsProcess.HasExited)
            {
                ttsProcess.Kill();
                ttsProcess.WaitForExit(3000); // Wait up to 3 seconds
            }
            
            ttsProcess.Dispose();
            ttsProcess = null;
            isServerRunning = false;
            
            if (verboseLogging)
                Debug.Log("✅ TTS Server stopped");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error stopping TTS Server: {e.Message}");
        }
    }
    
    [ContextMenu("Test TTS Server")]
    public void TestTTSServer()
    {
        if (!isServerRunning)
        {
            Debug.LogWarning("⚠️ TTS Server is not running!");
            return;
        }
        
        Debug.Log("🧪 Testing TTS Server...");
        
        // Create a simple test request
        StartCoroutine(TestTTSCoroutine());
    }
    
    System.Collections.IEnumerator TestTTSCoroutine()
    {
        string testUrl = $"http://localhost:{ttsPort}/tts";
        string testJson = "{\"text\":\"Hello World\",\"lang\":\"en\"}";
        
        using (UnityEngine.Networking.UnityWebRequest req = new UnityEngine.Networking.UnityWebRequest(testUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(testJson);
            req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            
            yield return req.SendWebRequest();
            
            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ TTS Server test successful! Received {req.downloadHandler.data.Length} bytes");
            }
            else
            {
                Debug.LogError($"❌ TTS Server test failed: {req.error}");
            }
        }
    }
    
    void CreateDefaultTTSScript()
    {
        string scriptContent = @"import flask
from flask import request, jsonify, send_file
import io
import wave
import numpy as np
import threading
import time

app = flask.Flask(__name__)

@app.route('/tts', methods=['POST'])
def text_to_speech():
    try:
        data = request.get_json()
        text = data.get('text', 'Hello World')
        lang = data.get('lang', 'en')
        
        print(f'[TTS] Converting text: ""{text}"" (lang: {lang})')
        
        # Create a simple beep sound as demo
        sample_rate = 22050
        duration = 1.0  # seconds
        frequency = 440  # Hz (A note)
        
        # Generate sine wave
        t = np.linspace(0, duration, int(sample_rate * duration), False)
        audio_data = np.sin(2 * np.pi * frequency * t)
        
        # Convert to 16-bit PCM
        audio_data = (audio_data * 32767).astype(np.int16)
        
        # Create WAV file in memory
        wav_buffer = io.BytesIO()
        with wave.open(wav_buffer, 'wb') as wav_file:
            wav_file.setnchannels(1)  # Mono
            wav_file.setsampwidth(2)  # 16-bit
            wav_file.setframerate(sample_rate)
            wav_file.writeframes(audio_data.tobytes())
        
        wav_buffer.seek(0)
        
        print(f'[TTS] Generated audio: {len(wav_buffer.getvalue())} bytes')
        
        return send_file(
            wav_buffer,
            mimetype='audio/wav',
            as_attachment=True,
            download_name='tts_output.wav'
        )
        
    except Exception as e:
        print(f'[TTS] Error: {e}')
        return jsonify({'error': str(e)}), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({'status': 'ok', 'service': 'TTS Server'})

if __name__ == '__main__':
    print('[TTS] Starting TTS Server on port 8001...')
    print('[TTS] Endpoints:')
    print('[TTS]   POST /tts - Convert text to speech')
    print('[TTS]   GET  /health - Health check')
    app.run(host='0.0.0.0', port=8001, debug=False)
";
        
        string scriptPath = Path.Combine(Application.dataPath, "..", ttsScriptPath);
        string scriptDir = Path.GetDirectoryName(scriptPath);
        
        if (!Directory.Exists(scriptDir))
        {
            Directory.CreateDirectory(scriptDir);
        }
        
        File.WriteAllText(scriptPath, scriptContent);
        Debug.Log($"✅ Created default TTS script at: {scriptPath}");
    }
    
    public bool IsServerRunning()
    {
        return isServerRunning && ttsProcess != null && !ttsProcess.HasExited;
    }
    
    void OnGUI()
    {
        if (verboseLogging)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label($"TTS Server: {(IsServerRunning() ? "🟢 Running" : "🔴 Stopped")}");
            
            if (GUILayout.Button("Start TTS Server"))
            {
                StartTTSServer();
            }
            
            if (GUILayout.Button("Stop TTS Server"))
            {
                StopTTSServer();
            }
            
            if (GUILayout.Button("Test TTS Server"))
            {
                TestTTSServer();
            }
            
            GUILayout.EndArea();
        }
    }
} 