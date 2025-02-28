using System.Numerics;
using Raylib_cs;

namespace Potesara;

public class Texture : IDisposable
{
    public Texture()
    {
        Rotation = 0.0f;
        Scale = new Vector2(1.0f, 1.0f);
        Opacity = 1.0f;
        ReferencePoint = ReferencePoint.TopLeft;
        BlendMode = BlendMode.Alpha; // デフォルトはアルファブレンド
    }

    public Texture(string fileName) : this()
    {
        RayTexture = Raylib.LoadTexture(fileName);
        if (RayTexture.Id != 0)
        {
            IsEnable = true;
        }
        FileName = fileName;
    }

    public void Dispose()
    {
        if (IsEnable)
        {
            Raylib.UnloadTexture(RayTexture);
            IsEnable = false;
        }
    }

    /// <summary>
    /// テクスチャを描画する (左上が X0, Y0 の座標系)
    /// </summary>
    public void Draw(float x, float y, Rectangle? sourceRect = null, Vector2? drawOrigin = null, bool reverseX = false, bool reverseY = false)
    {
        if (!IsEnable) return;

        Rectangle source = sourceRect ?? new Rectangle(0, 0, RayTexture.Width, RayTexture.Height);
        // 反転処理
        if (reverseX) source.Width = -source.Width;
        if (reverseY) source.Height = -source.Height;
        Vector2 origin = drawOrigin ?? GetReferencePoint(source);
        // スケールを考慮したオリジン調整
        origin.X *= (float)Scale.X;
        origin.Y *= (float)Scale.Y;
        Vector2 position = new Vector2(x, y);
        Vector2 scale = new Vector2(Scale.X, Scale.Y);
        Color color = new Color(255, 255, 255, (int)(Opacity * 255));

        Raylib.BeginBlendMode(BlendMode);
        Raylib.DrawTexturePro(RayTexture, source, new Rectangle(x, y, source.Width * scale.X, source.Height * scale.Y), origin, Rotation, color);
        Raylib.EndBlendMode();
    }

    /// <summary>
    /// 画面中央を基準に描画 (AviutlやYMM4の座標系)
    /// </summary>
    public void DrawCenteredCoords(float x, float y, Rectangle? sourceRect = null, Vector2? drawOrigin = null, bool reverseX = false, bool reverseY = false)
    {
        Rectangle source = sourceRect ?? new Rectangle(0, 0, RayTexture.Width, RayTexture.Height);
        Vector2 origin = drawOrigin ?? GetReferencePoint(source);

        Draw(1920 / 2 + x, 1080 / 2 + y, sourceRect, origin, reverseX, reverseY);
    }

    private Vector2 GetReferencePoint(Rectangle rect)
    {
        return ReferencePoint switch
        {
            ReferencePoint.TopLeft => new Vector2(0, 0),
            ReferencePoint.TopCenter => new Vector2(rect.Width / 2, 0),
            ReferencePoint.TopRight => new Vector2(rect.Width, 0),
            ReferencePoint.CenterLeft => new Vector2(0, rect.Height / 2),
            ReferencePoint.Center => new Vector2(rect.Width / 2, rect.Height / 2),
            ReferencePoint.CenterRight => new Vector2(rect.Width, rect.Height / 2),
            ReferencePoint.BottomLeft => new Vector2(0, rect.Height),
            ReferencePoint.BottomCenter => new Vector2(rect.Width / 2, rect.Height),
            ReferencePoint.BottomRight => new Vector2(rect.Width, rect.Height),
            _ => new Vector2(0, 0),
        };
    }

    /// <summary>
    /// 有効かどうか
    /// </summary>
    public bool IsEnable { get; private set; }
    /// <summary>
    /// ファイル名
    /// </summary>
    public string FileName { get; private set; }
    /// <summary>
    /// 不透明度
    /// </summary>
    public float Opacity { get; set; }
    /// <summary>
    /// 回転角度
    /// </summary>
    public float Rotation { get; set; }
    /// <summary>
    /// 描画する基準点
    /// </summary>
    public ReferencePoint ReferencePoint { get; set; }
    /// <summary>
    /// 拡大率
    /// </summary>
    public Vector2 Scale { get; set; }
    /// <summary>
    /// ブレンドモード
    /// </summary>
    public BlendMode BlendMode { get; set; }
    /// <summary>
    /// Raylibのテクスチャ2D
    /// </summary>
    public Texture2D RayTexture { get; private set; }
}

/// <summary>
/// 描画する基準点
/// </summary>
public enum ReferencePoint
{
    TopLeft, TopCenter, TopRight,
    CenterLeft, Center, CenterRight,
    BottomLeft, BottomCenter, BottomRight
}
