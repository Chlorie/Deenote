#nullable enable

using Cysharp.Threading.Tasks;
using NAudio.Wave;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Deenote.Library
{
    // Copied from Chlorie's version
    public static class AudioUtils
    {
        public static async UniTask<AudioClip?> TryLoadAsync(Stream stream, string audioType,
            CancellationToken cancellationToken = default)
        {
            var result =
                await UniTask.RunOnThreadPool(Load, configureAwait: true, cancellationToken: cancellationToken);
            if (result is not var (data, reader))
                return null;

            // AudioClip.Create should run on main thread
            AudioClip newClip = AudioClip.Create("SongAudioClip", data.Length / reader.WaveFormat.Channels,
                reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, false);
            newClip.SetData(data, 0);
            return newClip;

            (float[], WaveStream)? Load()
            {
                WaveStream wave;
                int initialSampleCount;
                switch (audioType) {
                    case ".wav" or ".WAV":
                        wave = new WaveFileReader(stream);
                        initialSampleCount = 0;
                        break;
                    case ".mp3" or ".MP3":
                        var id3TagLength = SkipID3Tag(stream);
                        wave = new Mp3FileReader(stream);
                        Mp3Frame frame = Mp3Frame.LoadFromStream(stream);
                        initialSampleCount = frame.SampleCount * wave.WaveFormat.Channels;
                        stream.Seek(id3TagLength, SeekOrigin.Begin);
                        break;
                    default:
                        return null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                ISampleProvider provider = wave.ToSampleProvider();
                long length = wave.Length / (wave.WaveFormat.BitsPerSample / 8);
                float[] raw = ArrayPool<float>.Shared.Rent((int)length);
                try {
                    try {
                        provider.Read(raw, 0, (int)length);
                    } catch (NAudio.MmException) {
                        return null;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    float[] samples = new float[length + initialSampleCount];
                    raw.CopyTo(samples, initialSampleCount);
                    return (samples, wave);
                } finally {
                    ArrayPool<float>.Shared.Return(raw);
                }
            }

            /// <return>Length of ID3 tag</return>
            int SkipID3Tag(Stream stream)
            {
                // byte[3] identifier
                // byte version
                // byte revision
                // byte flags
                // byte[4] size, Big Endian, ignore the leading 1 bit (always 0) of every byte

                var resetPos = stream.Position;
                Span<byte> id3 = stackalloc byte[3];
                stream.Read(id3);
                if (id3[0] == 'I' && id3[1] == 'D' && id3[2] == '3') {
                    stream.Position += 3;
                    Span<byte> sizeData = stackalloc byte[4];
                    stream.Read(sizeData);
                    uint size = 0;
                    size |= (uint)sizeData[0] << 21;
                    size |= (uint)sizeData[1] << 14;
                    size |= (uint)sizeData[2] << 7;
                    size |= sizeData[3];
                    stream.Position += size;
                    return 10 + checked((int)size);
                }
                else {
                    stream.Position = resetPos;
                    return 0;
                }
            }
        }

        public static void EncodeToWav(int channel, int frequency, int length, float[] sampleData, out byte[] wavData)
            => wavData = new WavEncoder(channel, frequency, length, sampleData).EncodeToWav();

        private readonly struct WavEncoder
        {
            public WavEncoder(int channel, int frequency, int length, float[] sampleData)
            {
                _channel = channel;
                _frequency = frequency;
                _length = length;
                _sampleData = sampleData;
            }

            public byte[] EncodeToWav()
            {
                using MemoryStream memStream = CreateEmpty();
                ConvertAndWrite(memStream);
                WriteHeader(memStream);
                return memStream.GetBuffer();
            }

            private readonly float[] _sampleData;
            private readonly int _frequency;
            private readonly int _channel;
            private readonly int _length; // Length is in samples
            private const int HeaderSize = 44;

            private MemoryStream CreateEmpty()
            {
                MemoryStream memStream = new();
                memStream.Write(stackalloc byte[HeaderSize]);
                return memStream;
            }

            private void ConvertAndWrite(MemoryStream memStream)
            {
                BinaryWriter writer = new(memStream);
                const int rescaleFactor = 32767; // To convert float to short
                for (int i = 0; i < _sampleData.Length; i++)
                    writer.Write((short)(_sampleData[i] * rescaleFactor));
            }

            private void WriteHeader(MemoryStream memStream)
            {
                // Unity's support of C# 11 is incomplete thus I can't use UTF8 literals
                var utf8 = System.Text.Encoding.UTF8;
                BinaryWriter writer = new(memStream);
                writer.Seek(0, SeekOrigin.Begin);
                // Master RIFF chunk
                writer.Write(utf8.GetBytes("RIFF"));
                writer.Write((uint)memStream.Length - 8);
                writer.Write(utf8.GetBytes("WAVE"));
                // Data format description
                writer.Write(utf8.GetBytes("fmt "));
                writer.Write(16); // 16 bytes = 2 ushorts + 2 ints + 2 ushorts
                writer.Write((ushort)1); // 1 for PCM integer
                writer.Write((ushort)_channel);
                writer.Write(_frequency);
                writer.Write(_frequency * _channel * sizeof(short)); // Bytes per second
                writer.Write((ushort)_channel * sizeof(short)); // Bytes per block
                writer.Write((ushort)sizeof(short)); // Bits per sample
                // Data chunk
                writer.Write(utf8.GetBytes("data"));
                writer.Write(_length * _channel * 2); // Length of data
            }
        }
    }
}