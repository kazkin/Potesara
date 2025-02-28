using System.Numerics;

namespace Potesara;

/// <summary>
/// 拡大縮小フィルター
/// </summary>
class ScaleFilter : ExoFilter
{
    public float StartBaseScale { get; set; }
    public float EndBaseScale { get; set; }

    public Vector2 StartScale { get; set; } = new Vector2(1.0f, 1.0f);
    public Vector2 EndScale { get; set; } = new Vector2(1.0f, 1.0f);
}
