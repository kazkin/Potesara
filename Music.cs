using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace Potesara;

/// <summary>
/// ストリーミング再生用のクラス (BassAudio).
/// </summary>
public class Music : IDisposable
{
    private int stream;
    private bool isLoaded = false;
    private bool isPlaying = false;


    public Music(string fileName, bool isLoop, float volume = 1.0f)
    {
        if (fileName.EndsWith(".wav") || fileName.EndsWith(".mp3") || fileName.EndsWith(".ogg"))
        {
            // ストリームの作成
            stream = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_DEFAULT);
            if (stream == 0)
            {
                throw new Exception("音声ストリーム作成失敗");
            }

            isLoaded = true;
            FileName = fileName;
            SetVolume(volume);

            // ループ設定
            if (isLoop)
            {
                Bass.BASS_ChannelFlags(stream, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
            }
        }
    }

    public void Dispose()
    {
        if (isLoaded)
        {
            Bass.BASS_StreamFree(stream);

            isLoaded = false;
        }
    }

    /// <summary>
    /// 曲を再生します。
    /// </summary>
    public void Play()
    {
        if (isLoaded)
        {
            // 再生中の場合は停止してから再生
            if (IsPlaying())
            {
                Stop();
            }

            Bass.BASS_ChannelPlay(stream, true);
            Bass.BASS_ChannelUpdate(stream, 0); // Bassライブラリの内部状態を更新
            isPlaying = true;
        }
    }

    /// <summary>
    /// 曲を停止します。
    /// </summary>
    public void Stop()
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelStop(stream);
            isPlaying = false;
        }
    }

    /// <summary>
    /// 曲を一時停止します。
    /// </summary>
    public void Pause()
    {

        if (isLoaded)
        {
            Bass.BASS_ChannelPause(stream);
            isPlaying = false;
        }
    }

    /// <summary>
    /// 一時停止した曲を再開します。
    /// </summary>
    public void Resume()
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelPlay(stream, false);
            Bass.BASS_ChannelUpdate(stream, 0); // Bassライブラリの内部状態を更新
            isPlaying = true;
        }
    }

    /// <summary>
    /// 曲が再生中かどうかを取得します。
    /// </summary>
    public bool IsPlaying()
    {
        return isLoaded && Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING;
    }

    /// <summary>
    /// 音量を設定します。
    /// </summary>
    public void SetVolume(float volume)
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Math.Clamp(volume, 0.0f, 1.0f));
        }
    }

    /// <summary>
    /// ピッチを設定します。
    /// </summary>
    public void SetPitch(float pitch)
    {
        Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, pitch);

    }

    /// <summary>
    /// パンを設定します。
    /// </summary>
    public void SetPan(float pan)
    {
        Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_PAN, Math.Clamp(pan, -1.0f, 1.0f));
    }

    /// <summary>
    /// 現在の再生位置を取得します。
    /// </summary>
    public double GetPosition()
    {
        return Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));
    }

    /// <summary>
    /// 曲の長さを取得します。
    /// </summary>
    public double GetLength()
    {
        return Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
    }

    /// <summary>
    /// 指定した時間に移動します。
    /// </summary>
    public void Seek(float time)
    {
        if (isLoaded)
        {
            Bass.BASS_ChannelSetPosition(stream, Bass.BASS_ChannelSeconds2Bytes(stream, Math.Clamp(time, 0.0f, (float)GetLength())));
        }
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    public void Update()
    {
        Bass.BASS_ChannelUpdate(stream, 16); // Bassライブラリの内部状態を更新

        // 再生が終了したら再生フラグを下げる
        if (isPlaying && !IsPlaying())
        {
            isPlaying = false;
        }
    }

    public string FileName { get; private set; }
}
