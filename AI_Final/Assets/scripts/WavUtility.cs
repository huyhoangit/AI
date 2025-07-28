using UnityEngine;
using System;

public static class WavUtility
{
    // Chuyển byte[] WAV PCM 16bit thành AudioClip
    public static AudioClip ToAudioClip(byte[] wavFile, int offsetSamples = 0, string name = "wav")
    {
        // Đọc header WAV
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int bitDepth = BitConverter.ToInt16(wavFile, 34);

        // Tìm vị trí chunk 'data'
        int pos = 12;
        while (!(wavFile[pos] == 'd' && wavFile[pos + 1] == 'a' && wavFile[pos + 2] == 't' && wavFile[pos + 3] == 'a'))
        {
            pos += 4;
            int chunkSize = BitConverter.ToInt32(wavFile, pos);
            pos += 4 + chunkSize;
        }
        pos += 8; // bỏ qua 'data' và size

        int samples = (wavFile.Length - pos) / 2;
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            short value = BitConverter.ToInt16(wavFile, pos + i * 2);
            data[i] = value / 32768f;
        }

        AudioClip audioClip = AudioClip.Create(name, samples / channels, channels, sampleRate, false);
        audioClip.SetData(data, offsetSamples);
        return audioClip;
    }
} 