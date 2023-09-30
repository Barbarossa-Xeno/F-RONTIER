using UnityEngine;
using FRONTIER.Utility;
using FRONTIER.Utility.Easing;
using System;

namespace FRONTIER.Menu.Window
{
    /// <summary>
    /// メニュー画面と曲リストのボーダーを曲線で描く。
    /// </summary>
    public class WindowBorderCurve : WindowBorder
    {
        /// <summary>
        /// 曲線を描くためのラインレンダラー。
        /// </summary>
        private LineRenderer curve;

        /// <summary>
        /// 曲線を描く領域を取得する。
        /// </summary>
        private CurvePosition curvePosition = null;

        /// <summary>
        /// グラデーション
        /// </summary>
        [SerializeField] private Gradient gradient;

        /// <summary>
        /// グラデーションの変化速度。
        /// </summary>
        [SerializeField][Range(0.1f, 1f)] private float gradientChangeSpeed;

        /// <summary>
        /// 曲線の幅。
        /// </summary>
        [SerializeField] private float curveWidth;

        /// <summary>
        /// 曲線の精密さの値。
        /// </summary>
        [SerializeField][Header("曲線の精密さ")] private int curveDetailed;

        /// <summary>
        /// グラデーションのオプションを適用するために、値を書き込むインスタンス。
        /// </summary>
        private GradientOption gradientOption = null;

        /// <summary>
        /// グラデーションにするカラー。
        /// </summary>
        [SerializeField] private GradientColor gradientColor = default;

        /// <summary>
        /// 自分のRectTransform。
        /// </summary>
        private RectTransform rectTransform => this.transform.GetComponent<RectTransform>();

        /// <summary>
        /// 曲線を描くエリア（曲線の各点の位置を計算するための最大幅・高さ）を保持するクラス。
        /// </summary>
        private class CurvePosition : ParentRect
        {
            public float maxWidth;
            public float maxHeight;
            public CurvePosition(float widthFactor)
            {
                // 最大幅は親の幅に対してある割合 =>  ある割合は、このゲームオブジェクトのRectTransformに指定されている AnchorMax の xの値
                this.maxWidth = parentRect.width * widthFactor;
                this.maxHeight = parentRect.height;
            }
        }

        /// <summary>
        /// グラデーションのオプション
        /// </summary>
        private class GradientOption
        {
            /// <summary>
            /// グラデーションのカラーキー。
            /// </summary>
            public GradientColorKey[] colorKey;

            /// <summary>
            /// グラデーションのアルファキー。
            /// </summary>
            public GradientAlphaKey[] alphaKey;

            public GradientOption()
            {
                colorKey = new GradientColorKey[3];
                alphaKey = new GradientAlphaKey[3];
            }
        }

        [Serializable]
        private struct GradientColor
        {
            /// <summary>
            /// 端（<c>0</c>と<c>1</c>）のカラー。
            /// </summary>
            public Color32 edgeColor;

            /// <summary>
            /// 位置が移動するキーのカラー。
            /// </summary>
            public Color32 activeColor;
        }

        void Start()
        {
            // グラデーション初期化
            SetGradient();
        }

        void OnValidate() => Initialize();

        /// <summary>
        /// 増分を記録する為の変数
        /// </summary>
        float delta;

        void Update()
        {
            // デルタタイムをもとに変化量を増やす
            delta += Time.deltaTime * gradientChangeSpeed;
            // 0.1のオフセットを設けて、移動の始まりと終わりに余裕を持たす
            if (delta > 1f + 0.1f) { delta = 0 - 0.1f; }
            // グラデーションの適用
            MoveGradient(delta);
        }

        /// <summary>
        /// グラデーションの設定をする。
        /// </summary>
        /// <param name="key">カラーキー及びアルファキーの数</param>
        private void SetGradient(int key = 3)
        {
            gradientOption = new();
            // カラーキーの設定
            gradientOption.colorKey[0] = new(gradientColor.edgeColor, 0f);
            gradientOption.colorKey[1] = new(gradientColor.activeColor, 0.01f);
            gradientOption.colorKey[2] = new(gradientColor.edgeColor, 1f);
            // 透明度は適当に埋める
            for (int i = 0; i < key; i++) { gradientOption.alphaKey[i] = new(1f, Mathf.Clamp01(i)); }
            // 適用
            gradient.mode = GradientMode.Blend;
            gradient.SetKeys(gradientOption.colorKey, gradientOption.alphaKey);
        }

        /// <summary>
        /// 難易度の変更に応じてグラデーションのカラーキーを変更する。
        /// </summary>
        /// <param name="difficulty">曲の難易度</param>
        public override void SetColorTrigger(Reference.DifficultyRank difficulty)
        {
            gradientOption.colorKey[1].color = MenuInfo.menuInfo.DifficultyTo(difficulty).Item2;
            gradient.SetKeys(gradientOption.colorKey, gradientOption.alphaKey);
        }

        private void MoveGradient(float time)
        {
            // グラデーションをきれいに見せるための振り分け
            // timeが0以下の時、始端をアクティブなカラーと同色にする
            if (time <= 0)
            {
                gradientOption.colorKey[0].color = gradientColor.activeColor;
                gradientOption.colorKey[2].color = gradientColor.edgeColor;
            }
            // それ以降は元々の始端・終端の色に準ずる
            else if (time > 0 && time < 1)
            {
                gradientOption.colorKey[0].color = gradientColor.edgeColor;
                gradientOption.colorKey[2].color = gradientColor.edgeColor;
            }
            // timeが1以上のとき、終端をアクティブなカラーと同色にする
            else if (time >= 1)
            {
                gradientOption.colorKey[2].color = gradientColor.activeColor;
            }
            // 移動するキーにはイージングを適用しながら移動させる（Clamp01で1を超えないように）
            gradientOption.colorKey[1].time = Mathf.Clamp01(time.EaseInOutCirc());

            gradient.SetKeys(gradientOption.colorKey, gradientOption.alphaKey);
            curve.colorGradient = gradient;
        }

        public override void Initialize()
        {
            // ラインレンダラーの初期化
            curve = GetComponentInChildren<LineRenderer>();
            curve.useWorldSpace = false;
            curve.startWidth = curveWidth;
            curve.endWidth = curveWidth;

            try
            {
                // トランスフォームを計算する
                origin = new(parentRect.width / 2, parentRect.height / 2);
                curvePosition = new(rectTransform.anchorMax.x);
            }
            catch (NullReferenceException)
            {
                #if UNITY_EDITOR
                Debug.LogWarning($"トランスフォームの取得に失敗しました。プレビューをやり直すにはインスペクター上の何かの値に変更を加えてください。この警告は無視して実行できます。");
                #endif
                return;
            }

            // ボーダー生成
            curveLocalPositions = GenerateCurve(curveDetailed);
            SetCurve(curveLocalPositions);
        }

        public override void ReScaleObject()
        {
            transform.localScale = new(1, canvasSize.y / parentRect.height, 1);
        }

        const float CURVE_HEIGHT_SCALE = 1.05f;
        /// <summary>
        /// ベジェ曲線を計算する。
        /// </summary>
        /// <param name="split">分割数</param>
        /// <returns>曲線の点の配列</returns>
        private Vector3[] GenerateCurve(int split)
        {
            Vector3 start = new(0, curvePosition.maxHeight * CURVE_HEIGHT_SCALE / 2, 0);
            Vector3 control = new(curvePosition.maxWidth * 2, 0, 0);
            Vector3 end = new(0, curvePosition.maxHeight * CURVE_HEIGHT_SCALE / -2, 0); ;

            // 点の数は分割数より1大きい（当然）
            Vector3[] v = new Vector3[split + 1];
            for (int i = 0; i <= split; i++)
            {
                float t = (float)i / split;
                Vector3 v1 = Vector3.Lerp(start, control, t);
                Vector3 v2 = Vector3.Lerp(control, end, t);
                v[i] = Vector3.Lerp(v1, v2, t);
            }
            return v;
        }

        /// <summary>
        /// ラインレンダラーに点を設定する。
        /// </summary>
        /// <param name="positions">曲線の点の配列</param>
        private void SetCurve(Vector3[] positions)
        {
            curve.positionCount = positions.Length;
            curve.SetPositions(positions);
        }
    }
}