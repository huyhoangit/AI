# fast_intelligent_tts_server.py

from flask import Flask, request, send_file
import pyttsx3
import time


app = Flask(__name__)
tts_engine = pyttsx3.init()
tts_engine.setProperty('rate', 200)


@app.route('/fast_intelligent_tts', methods=['POST'])
def fast_intelligent_tts():
    try:
        data = request.get_json()
        user_input = data.get('text', '')
        if not user_input or not user_input.strip():
            print("[FastTTS] Error: Input text is empty.")
            return {"error": "Input text is empty."}, 400
        print(f"[FastTTS] Speaking input: {user_input}")
        audio_file = generate_tts_fast(user_input)
        return send_file(audio_file, mimetype='audio/wav')
    except Exception as e:
        print(f"[FastTTS] Error: {str(e)}")
        audio_file = generate_tts_fast("Xin lỗi, có lỗi xảy ra.")
        return send_file(audio_file, mimetype='audio/wav')



def generate_tts_fast(text):
    """Generate TTS audio from input text only"""
    output_file = f"fast_response_{int(time.time())}.wav"
    try:
        tts_engine.save_to_file(text, output_file)
        tts_engine.runAndWait()
        return output_file
    except Exception as e:
        print(f"[FastTTS] TTS Error: {str(e)}")
        return create_emergency_audio()

def create_emergency_audio():
    """Create minimal audio file for emergencies"""
    emergency_file = "emergency.wav"
    tts_engine.save_to_file("Có lỗi âm thanh", emergency_file)
    tts_engine.runAndWait()
    return emergency_file


# Health check endpoint
@app.route('/health', methods=['GET'])
def health_check():
    return {"status": "healthy", "timestamp": time.time()}

if __name__ == '__main__':
    print("[FastTTS] Starting Fast Intelligent TTS Server...")
    print("[FastTTS] Endpoints:")
    print("  POST /fast_intelligent_tts - Main TTS endpoint (reads input text)")
    print("  GET  /health - Health check")
    tts_engine.say("")
    tts_engine.runAndWait()
    app.run(host='0.0.0.0', port=8001, debug=False, threaded=True)
