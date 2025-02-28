﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Raylib_cs;

namespace Potesara;

public class ExoParser
{
    /// <summary>
    /// exeditのプロパティ
    /// </summary>
    private int Width { get; set; }
    private int Height { get; set; }
    private int Rate { get; set; }
    private int Scale { get; set; }
    private int Length { get; set; }
    private bool IsLoop { get; set; }
    private bool _isPlaying { get; set; }

    private List<ExoImageObject> imageObjects = new(); // 画像オブジェクトのリスト
    private List<ExoGroupObject> groupObjects = new(); // グループ制御オブジェクトのリスト
    private List<string> textureFileNames = new(); // テクスチャ名のリスト

    private Counter counter;


    /// <summary>
    /// exoファイルを読み込む
    /// </summary>
    /// <param name="exoPath">exoのファイルパス</param>
    /// <param name="isLoop">ループするかどうか</param>
    /// <param name="isUseAntialiasing">アンチエイリアスをかけるかどうか</param>
    public ExoParser(string exoPath, bool isLoop, bool isUseAntialiasing)
    {
        IsLoop = isLoop;
        ExoObject currentObject = null;
        ExoFilterType currentFilter = ExoFilterType.None;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        IEnumerable<string> lines = File.ReadLines(exoPath, Encoding.GetEncoding("shift_jis"));

        foreach (string line in lines)
        {
            #region [exeditのパース] 
            if (currentObject == null)
            {
                if (line.StartsWith("width="))
                {
                    Width = int.Parse(line.Split('=')[1]);
                }
                else if (line.StartsWith("height="))
                {
                    Height = int.Parse(line.Split('=')[1]);
                }
                else if (line.StartsWith("rate="))
                {
                    Rate = int.Parse(line.Split('=')[1]);
                }
                else if (line.StartsWith("scale="))
                {
                    Scale = int.Parse(line.Split('=')[1]);
                }
                else if (line.StartsWith("length="))
                {
                    Length = int.Parse(line.Split('=')[1]);
                }
            }
            #endregion

            #region [ オブジェクトのパース]

            // [0]のような小数点なしの行
            if (line.StartsWith("[") && line.Contains("]") && !line.StartsWith("[exedit]") && !line.Contains("."))
            {
                string indexString = line.Trim('[', ']');
                if (int.TryParse(indexString, out int index))
                {
                    // オブジェクトの作成
                    ExoObject exoObject = new();
                    currentObject = exoObject;
                }
            }

            if (currentObject != null)
            {
                // [0]のような小数点なしの行
                if (line.StartsWith("start="))
                {
                    currentObject.StartFrame = int.Parse(line.Split('=')[1]);
                }
                else if (line.StartsWith("end="))
                {
                    currentObject.EndFrame = int.Parse(line.Split('=')[1]);
                }
                else if (line.StartsWith("layer="))
                {
                    currentObject.Layer = int.Parse(line.Split('=')[1]);
                }


                if (line.StartsWith("_name="))
                {
                    if (line.Split('=')[1] == "グループ制御")
                    {
                        currentFilter = ExoFilterType.None;
                        ExoGroupObject groupObject = new(currentObject);
                        currentObject = groupObject;

                        groupObjects.Add(groupObject);
                    }
                    else if (line.Split('=')[1] == "画像ファイル")
                    {
                        currentFilter = ExoFilterType.None;
                        ExoImageObject imageObject = new(currentObject);
                        currentObject = imageObject;

                        imageObjects.Add(imageObject);
                    }
                    else if (line.Split('=')[1] == "リサイズ" || line.Split('=')[1] == "拡大率")
                    {
                        currentFilter = ExoFilterType.Scale;
                    }
                    else if (line.Split('=')[1] == "回転")
                    {
                        currentFilter = ExoFilterType.Rotation;

                    }
                    else if (line.Split('=')[1] == "透明度")
                    {
                        currentFilter = ExoFilterType.Opacity;
                    }
                    else if (line.Split('=')[1] == "反転")
                    {
                        currentFilter = ExoFilterType.Reverse;
                    }
                }

                #region [フィルター以外の場合]
                if (currentFilter == ExoFilterType.None)
                {
                    if (line.StartsWith("X="))
                    {
                        // "X=" を削除し、カンマで分割
                        string[] parts = line.Substring(2).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        if (currentObject is ExoGroupObject groupObject)
                        {
                            groupObject = groupObjects.Last();
                            groupObject.Position.StartPosition = new Vector2(numbers[0], groupObject.Position.StartPosition.Y);

                            // EndPositionがある場合は設定
                            if (line.Contains(","))
                            {
                                groupObject.Position.EndPosition = new Vector2(numbers[1], groupObject.Position.EndPosition.Y);
                            }
                            // EndPositionがない場合はStartPositionと同じにする
                            else
                            {
                                groupObject.Position.EndPosition = new Vector2(groupObject.Position.StartPosition.X, groupObject.Position.StartPosition.Y);
                            }
                        }
                        else if (currentObject is ExoImageObject imageObject)
                        {
                            imageObject = imageObjects.Last();
                            imageObject.Position.StartPosition = new Vector2(numbers[0], imageObject.Position.StartPosition.Y);

                            // EndPositionがある場合は設定
                            if (line.Contains(","))
                            {
                                imageObject.Position.EndPosition = new Vector2(numbers[1], imageObject.Position.EndPosition.Y);
                            }
                            // EndPositionがない場合はStartPositionと同じにする
                            else
                            {
                                imageObject.Position.EndPosition = new Vector2(imageObject.Position.StartPosition.X, imageObject.Position.StartPosition.Y);
                            }
                        }

                    }
                    else if (line.StartsWith("Y="))
                    {
                        // "Y=" を削除し、カンマで分割
                        string[] parts = line.Substring(2).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        if (currentObject is ExoGroupObject groupObject)
                        {
                            groupObject = groupObjects.Last();
                            groupObject.Position.StartPosition = new Vector2(groupObject.Position.StartPosition.X, numbers[0]);

                            // EndPositionがある場合は設定
                            if (line.Contains(","))
                            {
                                groupObject.Position.EndPosition = new Vector2(groupObject.Position.EndPosition.X, numbers[1]);
                            }
                            // EndPositionがない場合はStartPositionと同じにする
                            else
                            {
                                groupObject.Position.EndPosition = new Vector2(groupObject.Position.StartPosition.X, groupObject.Position.StartPosition.Y);
                            }
                        }
                        else if (currentObject is ExoImageObject imageObject)
                        {
                            imageObject = imageObjects.Last();
                            imageObject.Position.StartPosition = new Vector2(imageObject.Position.StartPosition.X, numbers[0]);

                            // EndPositionがある場合は設定
                            if (line.Contains(","))
                            {
                                imageObject.Position.EndPosition = new Vector2(imageObject.Position.EndPosition.X, numbers[1]);
                            }
                            // EndPositionがない場合はStartPositionと同じにする
                            else
                            {
                                imageObject.Position.EndPosition = new Vector2(imageObject.Position.StartPosition.X, imageObject.Position.StartPosition.Y);
                            }
                        }
                    }
                    else if (line.StartsWith("拡大率="))
                    {
                        // "拡大率=" を削除し、カンマで分割
                        string[] parts = line.Substring(4).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        if (currentObject is ExoGroupObject groupObject)
                        {
                            groupObject = groupObjects.Last();
                            groupObject.Scale.StartScale = numbers[0] / 100.0f;

                            // EndScaleがある場合は設定
                            if (line.Contains(","))
                            {
                                groupObject.Scale.EndScale = numbers[1] / 100.0f;
                            }
                            // EndScaleがない場合はStartScaleと同じにする
                            else
                            {
                                groupObject.Scale.EndScale = groupObject.Scale.StartScale;
                            }
                        }
                        else if (currentObject is ExoImageObject imageObject)
                        {
                            imageObject = imageObjects.Last();
                            imageObject.Scale.StartScale = numbers[0] / 100.0f;

                            // EndScaleがある場合は設定
                            if (line.Contains(","))
                            {
                                imageObject.Scale.EndScale = numbers[1] / 100.0f;
                            }
                            // EndScaleがない場合はStartScaleと同じにする
                            else
                            {
                                imageObject.Scale.EndScale = imageObject.Scale.StartScale;
                            }
                        }
                    }
                    else if (line.StartsWith("上位グループ制御の影響を受ける="))
                    {
                        // "上位グループ制御の影響を受ける=" を削除
                        string str = line.Substring(16);

                        if (currentObject is ExoGroupObject groupObject)
                        {
                            groupObject = groupObjects.Last();
                            groupObject.AffectUpperGroup = Convert.ToBoolean(int.Parse(str));
                        }
                    }
                    else if (line.StartsWith("range="))
                    {
                        // "range=" を削除
                        string str = line.Substring(6);

                        if (currentObject is ExoGroupObject groupObject)
                        {
                            groupObject = groupObjects.Last();
                            groupObject.Range = int.Parse(str);
                        }
                    }
                    else if (line.StartsWith("file="))
                    {
                        // "file=" を削除して、ファイル名を取得
                        string fileName = Path.GetFileName(line.Substring(5));

                        if (currentObject is ExoImageObject imageObject)
                        {
                            imageObject = imageObjects.Last();

                            // 画像の読み込み
                            if (!textureFileNames.Contains(fileName))
                                imageObject.Texture = new Texture(Path.GetDirectoryName(exoPath) + @"\" + fileName);
                            else
                                imageObject.Texture = imageObjects[textureFileNames.IndexOf(fileName)].Texture;

                            // アンチエイリアスをかける
                            if (isUseAntialiasing)
                                Raylib.SetTextureFilter(imageObject.Texture.RayTexture, TextureFilter.Bilinear); // テクスチャフィルターを設定

                            textureFileNames.Add(fileName);
                        }
                    }
                    else if (line.StartsWith("透明度="))
                    {
                        // "透明度=" を削除し、カンマで分割
                        string[] parts = line.Substring(4).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        if (currentObject is ExoImageObject imageObject)
                        {
                            imageObject = imageObjects.Last();
                            imageObject.Opacity.StartOpacity = 1 - numbers[0] / 100.0f;

                            // EndOpacityがある場合は設定
                            if (line.Contains(","))
                            {
                                imageObject.Opacity.EndOpacity = 1 - numbers[1] / 100.0f;
                            }
                            // EndOpacityがない場合はStartOpacityと同じにする
                            else
                            {
                                imageObject.Opacity.EndOpacity = imageObject.Opacity.StartOpacity;
                            }
                        }
                    }
                    else if (line.StartsWith("回転="))
                    {
                        // "回転=" を削除し、カンマで分割
                        string[] parts = line.Substring(3).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        if (currentObject is ExoImageObject imageObject)
                        {
                            imageObject = imageObjects.Last();
                            imageObject.Rotation.StartRotation = numbers[0];

                            // EndPositionがある場合は設定
                            if (line.Contains(","))
                            {
                                imageObject.Rotation.EndRotation = numbers[1];
                            }
                            // EndPositionがない場合はStartPositionと同じにする
                            else
                            {
                                imageObject.Rotation.EndRotation = imageObject.Rotation.StartRotation;
                            }
                        }
                    }

                }
                #endregion

                #region [リサイズフィルター]
                else if (currentFilter == ExoFilterType.Scale)
                {
                    if (line.StartsWith("拡大率="))
                    {
                        ScaleFilter scaleFilter = new();
                        currentObject.Filters.Add(scaleFilter);

                        // "拡大率=" を削除し、カンマで分割
                        string[] parts = line.Substring(4).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        ScaleFilter FilterObject = (ScaleFilter)currentObject.Filters.Last();
                        FilterObject.StartBaseScale = numbers[0] / 100.0f;

                        // EndScaleがある場合は設定
                        if (line.Contains(","))
                        {
                            FilterObject.EndBaseScale = numbers[1] / 100.0f;
                        }
                        // EndScaleがない場合はStartScaleと同じにする
                        else
                        {
                            FilterObject.EndBaseScale = FilterObject.StartBaseScale;
                        }
                    }
                    else if (line.StartsWith("X="))
                    {
                        // "X=" を削除し、カンマで分割
                        string[] parts = line.Substring(2).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        ScaleFilter FilterObject = (ScaleFilter)currentObject.Filters.Last();

                        FilterObject.StartScale = new Vector2(numbers[0] / 100.0f, FilterObject.StartScale.Y);

                        // EndScaleがある場合は設定
                        if (line.Contains(","))
                        {
                            FilterObject.EndScale = new Vector2(numbers[1] / 100.0f, FilterObject.EndScale.Y);
                        }
                        // EndScaleがない場合はStartScaleと同じにする
                        else
                        {
                            FilterObject.EndScale = new Vector2(FilterObject.StartScale.X, FilterObject.StartScale.Y);
                        }
                    }
                    else if (line.StartsWith("Y="))
                    {
                        // "Y=" を削除し、カンマで分割
                        string[] parts = line.Substring(2).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        ScaleFilter FilterObject = (ScaleFilter)currentObject.Filters.Last();

                        FilterObject.StartScale = new Vector2(FilterObject.StartScale.X, numbers[0] / 100.0f);

                        // EndScaleがある場合は設定
                        if (line.Contains(","))
                        {
                            FilterObject.EndScale = new Vector2(FilterObject.EndScale.X, numbers[1] / 100.0f);
                        }
                        // EndScaleがない場合はStartScaleと同じにする
                        else
                        {
                            FilterObject.EndScale = new Vector2(FilterObject.StartScale.X, FilterObject.StartScale.Y);
                        }

                        // フィルターの終了
                        currentFilter = ExoFilterType.None;
                    }

                }

                #endregion
                #region [回転フィルター]
                else if (currentFilter == ExoFilterType.Rotation)
                {
                    if (line.StartsWith("Z="))
                    {
                        // フィルターの作成
                        RotationFilter rotationFilter = new();
                        currentObject.Filters.Add(rotationFilter);

                        // "Z=" を削除し、カンマで分割
                        string[] parts = line.Substring(2).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        RotationFilter FilterObject = (RotationFilter)currentObject.Filters.Last();

                        FilterObject.Rotation.StartRotation = numbers[0];

                        // EndRotationがある場合は設定
                        if (line.Contains(","))
                        {
                            FilterObject.Rotation.EndRotation = numbers[1];
                        }
                        // EndRotationがない場合はStartRotationと同じにする
                        else
                        {
                            FilterObject.Rotation.EndRotation = FilterObject.Rotation.StartRotation;
                        }

                        // フィルターの終了
                        currentFilter = ExoFilterType.None;
                    }

                }

                #endregion
                #region [透明度フィルター]
                else if (currentFilter == ExoFilterType.Opacity)
                {
                    if (line.StartsWith("透明度="))
                    {
                        // フィルターの作成
                        OpacityFilter opacityFilter = new();
                        currentObject.Filters.Add(opacityFilter);

                        // "透明度=" を削除し、カンマで分割
                        string[] parts = line.Substring(4).Split(',');

                        // 数値に変換
                        float[] numbers = parts.Select(float.Parse).ToArray();

                        OpacityFilter FilterObject = (OpacityFilter)currentObject.Filters.Last();

                        FilterObject.Opacity.StartOpacity = 1 - numbers[0] / 100.0f;

                        // EndOpacityがある場合は設定
                        if (line.Contains(","))
                        {
                            FilterObject.Opacity.EndOpacity = 1 - numbers[1] / 100.0f;
                        }
                        // EndOpacityがない場合はStartOpacityと同じにする
                        else
                        {
                            FilterObject.Opacity.EndOpacity = FilterObject.Opacity.StartOpacity;
                        }

                        // フィルターの終了
                        currentFilter = ExoFilterType.None;
                    }

                }

                #endregion
                #region [反転フィルター]
                else if (currentFilter == ExoFilterType.Reverse)
                {
                    if (line.StartsWith("上下反転="))
                    {
                        // フィルターの作成
                        ReverseFilter reverseFilter = new();
                        currentObject.Filters.Add(reverseFilter);

                        ReverseFilter FilterObject = (ReverseFilter)currentObject.Filters.Last();

                        FilterObject.ReverseY = Convert.ToBoolean(int.Parse(line.Split('=')[1]));
                    }
                    else if (line.StartsWith("左右反転="))
                    {
                        ReverseFilter FilterObject = (ReverseFilter)currentObject.Filters.Last();

                        FilterObject.ReverseX = Convert.ToBoolean(int.Parse(line.Split('=')[1]));

                        // フィルターの終了
                        currentFilter = ExoFilterType.None;
                    }

                }

                #endregion

            }

            #endregion
        }

        #region [画像オブジェクトとグループ制御オブジェクトの関連付け]
        foreach (var imageObject in imageObjects)
        {
            foreach (var groupObject in groupObjects)
            {
                // グループ制御の適応範囲内の場合
                if (imageObject.Layer <= groupObject.Layer + groupObject.Range || groupObject.Range == 0)
                {
                    // グループ制御のレイヤーが画像オブジェクトのレイヤーより下の場合はスキップ
                    if (imageObject.Layer < groupObject.Layer)
                        continue;

                    // グループ制御のフレーム内に画像オブジェクトがない場合はスキップ
                    if (groupObject.StartFrame > imageObject.EndFrame || groupObject.EndFrame < imageObject.StartFrame)
                        continue;

                    imageObject.GroupObjects.Add(groupObject);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// カウンターをスタートする
    /// </summary>
    public void Start()
    {
        counter = new Counter(1, Length, 1000.0 / Rate, IsLoop);
        counter.Start();
        _isPlaying = true;
    }

    /// <summary>
    /// カウンターをストップする
    /// </summary>
    public void Stop()
    {
        if (counter != null)
        {
            counter.Stop();
            _isPlaying = false;
        }
    }

    /// <summary>
    /// カウンターを再開する
    /// </summary>
    public void Resume()
    {
        if (counter != null)
        {
            counter.Start();
            _isPlaying = true;
        }
    }

    /// <summary>
    /// 再生中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return _isPlaying;
    }

    /// <summary>
    /// 現在何フレーム目かを取得する
    /// </summary>
    /// <returns></returns>
    public int GetNowFrame()
    {
        return (int)counter.Value;
    }

    /// <summary>
    /// Exoを描画する関数
    /// </summary>
    /// <param name="offsetX">OffsetX</param>
    /// <param name="offsetY">OffsetY</param>
    public void Draw(float offsetX, float offsetY)
    {
        if (counter != null)
        {
            counter.Tick();

            foreach (var imageObject in imageObjects)
            {
                if (counter.Value >= imageObject.StartFrame && counter.Value <= imageObject.EndFrame)
                {
                    UpdateTransform(imageObject); // Transformを更新
                    ApplyFilter(imageObject); // フィルターを適用
                    ApplyGroupObject(imageObject); // グループ制御オブジェクトを適用
                    imageObject.Draw(offsetX, offsetY);
                }
            }

            Raylib.DrawText(counter.Value.ToString(), 10, 200, 20, Color.White);
        }
    }


    #region [Private]

    /// <summary>
    /// 画像オブジェクトのTransformを更新する関数
    /// </summary>
    /// <param name="imageObject"></param>
    private void UpdateTransform(ExoImageObject imageObject)
    {
        // 0.0～1.0の進行度
        float t = (float)(counter.Value - imageObject.StartFrame) / (imageObject.EndFrame - imageObject.StartFrame);

        // StartFrameとEndFrameが同じ場合(1フレームの場合)1.0固定
        if (imageObject.StartFrame == imageObject.EndFrame)
        {
            t = 0.0f;
        }

        // 補間を行う
        Vector2 interpolatedPosition = imageObject.Position.StartPosition + (imageObject.Position.EndPosition - imageObject.Position.StartPosition) * t;
        float interpolatedScale = imageObject.Scale.StartScale + (imageObject.Scale.EndScale - imageObject.Scale.StartScale) * t;
        float interpolatedRotation = imageObject.Rotation.StartRotation + (imageObject.Rotation.EndRotation - imageObject.Rotation.StartRotation) * t;
        float interpolatedOpacity = imageObject.Opacity.StartOpacity + (imageObject.Opacity.EndOpacity - imageObject.Opacity.StartOpacity) * t;

        // Transformの更新
        imageObject.Transfrom.Position = interpolatedPosition;
        imageObject.Transfrom.Scale = new Vector2(interpolatedScale, interpolatedScale);
        imageObject.Transfrom.Rotation = interpolatedRotation;
        imageObject.Transfrom.Opacity = interpolatedOpacity;
        imageObject.Transfrom.ReverseX = false; // 画像オブジェクトは反転がないので、初期値false
        imageObject.Transfrom.ReverseY = false; // 画像オブジェクトは反転がないので、初期値false
    }

    /// <summary>
    /// グループ制御オブジェクトを適用する関数
    /// </summary>
    /// <param name="imageObject"></param>
    private void ApplyGroupObject(ExoImageObject imageObject)
    {
        List<ExoGroupObject> nowFrameGroupObjects = new(); // 今のフレームに存在するグループ制御オブジェクトのリスト

        // 今のフレームに存在するグループ制御オブジェクトをリストに追加
        foreach (var groupObject in imageObject.GroupObjects)
        {
            if (counter.Value >= groupObject.StartFrame && counter.Value <= groupObject.EndFrame)
            {
                nowFrameGroupObjects.Add(groupObject);
            }
        }

        // 今のフレームに存在するグループ制御オブジェクトを適用
        foreach (var nowFrameGroupObject in Enumerable.Reverse(nowFrameGroupObjects))
        {
            //Console.WriteLine(nowFrameGroupObjects.Count);

            if (imageObject.IsAffectUpperGroup || nowFrameGroupObject == nowFrameGroupObjects.Last())
            {
                // 0.0～1.0の進行度
                float t = (float)(counter.Value - nowFrameGroupObject.StartFrame) / (nowFrameGroupObject.EndFrame - nowFrameGroupObject.StartFrame);

                // StartFrameとEndFrameが同じ場合(1フレームの場合)1.0固定
                if (nowFrameGroupObject.StartFrame == nowFrameGroupObject.EndFrame)
                {
                    t = 0.0f;
                }

                // 補間を行う
                Vector2 interpolatedPosition = nowFrameGroupObject.Position.StartPosition + (nowFrameGroupObject.Position.EndPosition - nowFrameGroupObject.Position.StartPosition) * t;
                float interpolatedScale = nowFrameGroupObject.Scale.StartScale + (nowFrameGroupObject.Scale.EndScale - nowFrameGroupObject.Scale.StartScale) * t;
                float interpolatedRotation = nowFrameGroupObject.Rotation.StartRotation + (nowFrameGroupObject.Rotation.EndRotation - nowFrameGroupObject.Rotation.StartRotation) * t;

                // グループ制御オブジェクトのTransformの更新
                nowFrameGroupObject.Transfrom.Position = interpolatedPosition;
                nowFrameGroupObject.Transfrom.Scale = new Vector2(interpolatedScale, interpolatedScale);
                nowFrameGroupObject.Transfrom.Rotation = interpolatedRotation;
                nowFrameGroupObject.Transfrom.Opacity = 1.0f; // グループ制御は透明度がないので、初期値1.0
                nowFrameGroupObject.Transfrom.ReverseX = false; // グループ制御は反転がないので、初期値false
                nowFrameGroupObject.Transfrom.ReverseY = false; // グループ制御は反転がないので、初期値false

                // グループ制御オブジェクトのフィルターを適用
                ApplyFilter(nowFrameGroupObject);

                // 画像オブジェクトのTransformにグループ制御オブジェクトのTransformを適用
                imageObject.Transfrom.Position += nowFrameGroupObject.Transfrom.Position;
                imageObject.Transfrom.Position *= interpolatedScale; // グループ制御の拡大率で補正
                imageObject.Transfrom.Scale *= nowFrameGroupObject.Transfrom.Scale;
                imageObject.Transfrom.Rotation += nowFrameGroupObject.Transfrom.Rotation;
                imageObject.Transfrom.Opacity *= nowFrameGroupObject.Transfrom.Opacity;

                // 反転の適用
                if (nowFrameGroupObject.Transfrom.ReverseX) imageObject.Transfrom.ReverseX = !imageObject.Transfrom.ReverseX;
                if (nowFrameGroupObject.Transfrom.ReverseY) imageObject.Transfrom.ReverseY = !imageObject.Transfrom.ReverseY;

                imageObject.IsAffectUpperGroup = nowFrameGroupObject.AffectUpperGroup;
            }
        }
    }

    /// <summary>
    /// フィルターを適用する関数
    /// </summary>
    /// <param name="exoObject"></param>
    private void ApplyFilter(ExoObject exoObject)
    {
        foreach (var filter in exoObject.Filters)
        {
            // 0.0～1.0の進行度
            float t = (float)(counter.Value - exoObject.StartFrame) / (exoObject.EndFrame - exoObject.StartFrame);

            // StartFrameとEndFrameが同じ場合(1フレームの場合)1.0固定
            if (exoObject.StartFrame == exoObject.EndFrame)
            {
                t = 0.0f;
            }

            // リサイズフィルター
            if (filter is ScaleFilter scaleFilter)
            {
                // 補間を行う
                float interpolatedBaseScale = scaleFilter.StartBaseScale + (scaleFilter.EndBaseScale - scaleFilter.StartBaseScale) * t;
                Vector2 interpolatedScale = scaleFilter.StartScale + (scaleFilter.EndScale - scaleFilter.StartScale) * t;

                // Transformの更新
                exoObject.Transfrom.Scale *= interpolatedBaseScale;
                exoObject.Transfrom.Scale *= interpolatedScale;
            }
            // 回転フィルター
            else if (filter is RotationFilter rotationFilter)
            {
                // 補間を行う
                float interpolatedRotation = rotationFilter.Rotation.StartRotation + (rotationFilter.Rotation.EndRotation - rotationFilter.Rotation.StartRotation) * t;

                // Transformの更新
                exoObject.Transfrom.Rotation += interpolatedRotation;
            }
            // 透明度フィルター
            else if (filter is OpacityFilter opacityFilter)
            {
                // 補間を行う
                float interpolatedOpacity = opacityFilter.Opacity.StartOpacity + (opacityFilter.Opacity.EndOpacity - opacityFilter.Opacity.StartOpacity) * t;

                // Transformの更新
                exoObject.Transfrom.Opacity *= interpolatedOpacity;
            }
            // 反転フィルター
            else if (filter is ReverseFilter reverseFilter)
            {
                if (reverseFilter.ReverseX) exoObject.Transfrom.ReverseX = !exoObject.Transfrom.ReverseX;
                if (reverseFilter.ReverseY) exoObject.Transfrom.ReverseY = !exoObject.Transfrom.ReverseY;
            }

        }
    }

    #endregion
}
