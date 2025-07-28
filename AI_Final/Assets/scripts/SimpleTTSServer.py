#!/usr/bin/env python3
"""
Simple TTS Server for Unity Testing
Run this script to start a basic TTS server on localhost:8001
"""

from flask import Flask, request, send_file, jsonify
import pyttsx3
import io
import os

app = Flask(__name__)

@app.route('/tts', methods=['POST'])
def tts():
    try:
        data = request.get_json()
        text = data.get('text', '')
        lang = data.get('lang', 'vi')
        if not text:
            return jsonify({'error': 'No text provided'}), 400

        print(f'[TTS] Converting text: "{text}" (lang: {lang})')

        engine = pyttsx3.init()
        engine.setProperty('rate', 180)
        engine.setProperty('volume', 1.0)

        # Tự động chọn voice tiếng Việt nếu có
        vi_voice = None
        print('[TTS] Available voices:')
        for voice in engine.getProperty('voices'):
            print(voice.id, voice.name)
            if 'vi_' in voice.id or 'viVN' in voice.id or 'Vietnam' in voice.name or 'An' in voice.name:
                vi_voice = voice.id
                break
        if vi_voice:
            engine.setProperty('voice', vi_voice)
            print(f'[TTS] Using Vietnamese voice: {vi_voice}')
        else:
            print('[TTS] Vietnamese voice not found, using default voice.')

        temp_wav = 'temp_tts.wav'
        engine.save_to_file(text, temp_wav)
        engine.runAndWait()

        with open(temp_wav, 'rb') as f:
            wav_data = f.read()
        wav_buffer = io.BytesIO(wav_data)
        wav_buffer.seek(0)
        os.remove(temp_wav)

        print(f'[TTS] Generated audio: {len(wav_data)} bytes')

        return send_file(wav_buffer, mimetype='audio/wav', as_attachment=False, download_name='tts.wav')
    except Exception as e:
        print(f'[TTS] Error: {e}')
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8001) 