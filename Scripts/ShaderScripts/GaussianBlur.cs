// [reference] https://edom18.hateblo.jp/entry/2018/12/30/171214

using UnityEngine;

namespace Game.ShaderScripts
{
    public class GaussianBlur : MonoBehaviour
    {
        /// <summary>
        /// ブラーさせるテクスチャ。
        /// </summary>
        [SerializeField]
        private Texture _texture;

        /// <summary>
        /// ガウシアンブラーを行うシェーダー。
        /// </summary>
        [SerializeField]
        private Shader _shader;

        /// <summary>
        /// オフセット。
        /// </summary>
        [SerializeField, Range(1f, 10f)]
        private float _offset = 1f;

        /// <summary>
        /// ブラーの強さ。=> 標準偏差。
        /// </summary>
        [SerializeField, Range(10f, 1000f)]
        private float _blur = 100f;

        /// <summary>
        /// 作成するマテリアル。
        /// </summary>
        private Material _material;

        /// <summary>
        /// 使用するレンダラー。
        /// </summary>
        private Renderer _renderer;

        /// <summary>
        /// 縦横でブラーを適用するためにダブルバッファで処理する。
        /// </summary>
        private RenderTexture[] _rt = new RenderTexture[2];

        /// <summary>
        /// 重み。
        /// </summary>
        private float[] _weights = new float[10];

        /// <summary>
        /// 初期化したかどうか。パラメーターが変わった時だけブラーを計算すればいい。
        /// </summary>
        private bool _isInitialized = false;

        #region ### MonoBehaviour ###
        private void Awake()
        {
            Initialize();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateWeights();

            Blur();
        }
        #endregion ### MonoBehaviour ###

        /// <summary>
        /// Initialize (setup)
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _material = new Material(_shader);
            _material.hideFlags = HideFlags.HideAndDontSave;

            // Down scale.
            _rt[0] = new RenderTexture(_texture.width / 2, _texture.height / 2, 0, RenderTextureFormat.ARGB32);
            _rt[1] = new RenderTexture(_texture.width / 2, _texture.height / 2, 0, RenderTextureFormat.ARGB32);

            _renderer = GetComponent<Renderer>();

            UpdateWeights();

            _isInitialized = true;
        }

        /// <summary>
        /// テクスチャにブラーをかける。
        /// </summary>
        public void Blur()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            Graphics.Blit(_texture, _rt[0]);

            _material.SetFloatArray("_Weights", _weights);

            float x = _offset / _rt[0].width;
            float y = _offset / _rt[0].height;

            // for horizontal blur.
            _material.SetVector("_Offsets", new Vector4(x, 0, 0, 0));

            Graphics.Blit(_rt[0], _rt[1], _material);

            // for vertical blur.
            _material.SetVector("_Offsets", new Vector4(0, y, 0, 0));

            Graphics.Blit(_rt[1], _rt[0], _material);

            _renderer.material.mainTexture = _rt[0];
        }

        /// <summary>
        /// ガウス関数をつかって重みを計算する。
        /// </summary>
        private void UpdateWeights()
        {
            // 重みの総量
            float total = 0;
            // 分散
            float sigma2 = _blur * _blur;
            // 0.001はオフセット的な係数（？）
            float d = sigma2 * 0.001f;
            // 各地点（各テクセル）での重みの計算
            for (int i = 0; i < _weights.Length; i++)
            {
                // x に若干のオフセットをかけて値をばらつかせる
                float x = i * 2f;
                // ガウス関数の出番
                /* 今回使うガウス関数
                 *  w = exp(-x^2 / 2d)
                 *  d は分散に係数をかけたもの 
                 * ちなみに、ガウス関数の定義
                 *  y = a exp(-(x - u)^2 / 2c^2)
                 */
                float w = Mathf.Exp(-(x * x) / 2f * d);
                _weights[i] = w;

                if (i > 0)
                {
                    w *= 2.0f;
                }

                total += w;
            }

            // 重みの総和が1になるように、総量で割って正規化する
            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] /= total;
            }
        }
    }
}