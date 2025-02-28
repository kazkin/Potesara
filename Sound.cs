using Raylib_cs;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace Potesara;

/// <summary>
/// 非ストリーミング再生用のクラス (BassAudio).
/// </summary>
public class Sound : IDisposable
{
    private int sample;
    private int channel;
    private bool isLoaded = false;

    public Sound(string fileName, float volume = 1.0f)
    {
        if (Raylib.IsFileExtension(fileName, ".wav") || Raylib.IsFileExtension(fileName, ".mp3") || Raylib.IsFileExtension(fileName, ".ogg"))
        {
            // サウンドサンプルの作成
            sample = Bass.BASS_SampleLoad(fileName, 0, 0, 1, BASSFlag.BASS_SAMPLE_SOFTWARE | BASSFlag.BASS_SAMPLE_FLOAT); // 1は同時再生数
            if (sample == 0)
            {
                throw new Exception("音声サンプルの作成失敗");
            }

            // サンプルからチャンネルを取得
            channel = Bass.BASS_SampleGetChannel(sample, BASSFlag.BASS_SAMPLE_OVER_POS); // BASS_SAMPLE_OVER_POSは最長の再生チャネルがオーバーライドされます。
            if (channel == 0)
            {
                throw new Exception("サンプルからチャンネルを取得できませんでした");
            }

            isLoaded = true;
            FileName = fileName;
            SetVolume(volume);
        }
    }

    public void Dispose()
    {
        if (isLoaded)
        {
            Bass.BASS_SampleFree(sample);
            isLoaded = false;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// サウンドを再生します。
    /// </summary>
    public void Play()
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelPlay(channel, true);
        }
    }

    /// <summary>
    /// サウンドを停止します。
    /// </summary>
    public void Stop()
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelStop(channel);
        }
    }

    /// <summary>
    /// サウンドが再生中かどうかを取得します。
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return isLoaded && Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PLAYING;
    }

    /// <summary>
    /// サウンドの音量を設定します。
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolume(float volume)
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, Math.Clamp(volume, 0.0f, 1.0f));
        }
    }

    /// <summary>
    /// サウンドのピッチを設定します。
    /// </summary>
    /// <param name="pitch"></param>
    public void SetPitch(float pitch)
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, pitch);
        }
    }

    /// <summary>
    /// サウンドのパンを設定します。
    /// </summary>
    /// <param name="pan"></param>
    public void SetPan(float pan)
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_PAN, Math.Clamp(pan, -1.0f, 1.0f));
        }
    }

    /// <summary>
    /// 音量を取得します。
    /// </summary>
    public float Volume { get; set; }

    /// <summary>
    /// ファイルのパスを取得します。
    /// </summary>
    public string FileName { get; private set; }
}
