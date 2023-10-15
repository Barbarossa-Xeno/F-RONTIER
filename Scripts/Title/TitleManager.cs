using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FRONTIER.Utility;

namespace FRONTIER.Title
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private Button screen;

        [SerializeField] private TextMeshProUGUI versionInfomation;
        
        [SerializeField] private AnimationElement animationElement;

        [System.Serializable]
        private class AnimationElement
        {
            [SerializeField] private Shadow logo;
            [SerializeField] private TextMeshProUGUI text;

            /// <summary>
            /// ロゴの影部分をアニメーションする。
            /// </summary>
            /// <returns></returns>
            public IEnumerator FadeInOutLogoShadow()
            {
                // 角度
                float angle = 0;
                // 透明度
                float alpha = 0;
                // 変化させるカラー
                Color changingColor = Reference.DifficultyUtilities.Colors.Restricted;
                // 周期カウント
                int periodCount = 0;

                // 初期化
                logo.effectDistance = new(0, 0);
                logo.effectColor = new(changingColor.r, changingColor.g, changingColor.b, 0.5f);

                // 影の移動アニメーション
                while (logo.effectDistance.x > -5f)
                {
                    // 0.02秒ごとに影を移動
                    // x, y座標が-5になるまで
                    yield return new WaitForSeconds(0.02f);
                    
                    logo.effectDistance -= new Vector2(0.1f, 0.1f);
                }

                // 0.2秒小休止
                yield return new WaitForSeconds(0.2f);

                // １周期ごとに色を変えながら影をアニメーションする
                while (true)
                {
                    // 0.04秒ごとに変化
                    yield return new WaitForSeconds(0.04f);

                    // 角度と透明度の変更
                    angle = angle <= Mathf.PI ? angle + 0.05f : 0;
                    // (0 <= 角度 <= 3.14...)の範囲で(|-1| <= 透明度 <= 1)をとるように
                    // sinをcosとして扱い、0.5倍する
                    alpha = 0.5f * Mathf.Abs(Mathf.Sin(angle - Mathf.PI));

                    // 角度が0のとき、１周したと判定する
                    periodCount = angle == 0 ? periodCount + 1 : periodCount;
                    // 周回が4回になったらリセットする
                    periodCount = periodCount >= 4 ? 0 : periodCount;

                    // 剰余で色を決定する
                    changingColor = (periodCount % 4) switch
                            {
                                0 => Reference.DifficultyUtilities.Colors.Restricted,
                                1 => Reference.DifficultyUtilities.Colors.Ecstasy,
                                2 => Reference.DifficultyUtilities.Colors.Hard,
                                3 => Reference.DifficultyUtilities.Colors.Lite,
                                _ => Reference.DifficultyUtilities.Colors.Restricted
                            };
                    
                    // 色を変える
                    logo.effectColor = new(changingColor.r, changingColor.g, changingColor.b, alpha);
                }
            }

            /// <summary>
            /// テキストをフェードインアウトする。
            /// </summary>
            /// <returns></returns>
            public IEnumerator FadeInOutText()
            {
                float angle = 0;
                float alpha = 0;
                text.color = new(0, 0, 0, 0);
                yield return new WaitForSeconds(0.5f);

                while (true)
                {
                    yield return new WaitForSeconds(0.05f);

                    alpha = Mathf.Sin(angle);
                    text.color = new(text.color.r, text.color.g, text.color.b, alpha);
                    angle = angle < Mathf.PI ? angle + 0.1f : 0;
                }
            }
        }

        void Start()
        {
            StartCoroutine(animationElement.FadeInOutLogoShadow());
            StartCoroutine(animationElement.FadeInOutText());

            screen.onClick.AddListener(GameManager.instance.scene.menu.Invoke);

            versionInfomation.text = $"ver. {Application.version}";
        }
    }
}