import flask
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
        
        print(f'[TTS] Converting text: "{text}" (lang: {lang})')
        
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
