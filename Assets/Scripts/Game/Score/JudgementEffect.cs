using UnityEngine;
using JudgementRank = FRONTIER.Utility.Reference.JudgementRank;

namespace FRONTIER.Game.Score
{
    /// <summary>
    /// 判定エフェクトを制御するクラス。
    /// </summary>
    /// <remarks>
    /// <list>
    ///     <item><i>Prefabs/JudgementEffects</i> 内のエフェクトオブジェクトにアタッチされている。</item>
    ///     <item>エフェクトオブジェクトは、<see cref = "JudgementEffectPool"/> で管理されている。</item>
    /// </list>
    /// </remarks>
    public class JudgementEffect : MonoBehaviour
    {
        [SerializeField] private JudgementRank type;

        /// <summary>
        /// このエフェクトが入っているオブジェクトプール。
        /// 生成時に参照を渡すようにすること！
        /// </summary>
        public JudgementEffectPool Effect { get; set; }

        /// <summary>
        /// このエフェクトをリリースするときに発火させる処理。
        /// </summary>
        /// <remarks>
        /// このコンポーネントと一緒にアタッチされている Animation Clip の最後のフレームで呼び出されるようにする。
        /// </remarks>
        public void OnRelease()
        {
            Debug.Assert(Effect != null, "オブジェクトプールが設定されていません。");
            switch (type)
            {
                case JudgementRank.Perfect:
                {
                    Effect.Perfect.Release(this.gameObject);
                    break;
                }
                case JudgementRank.Great:
                {
                    Effect.Great.Release(this.gameObject);
                    break;
                }
                case JudgementRank.Good:
                {
                    Effect.Good.Release(this.gameObject);
                    break;
                }
                case JudgementRank.Bad:
                {
                    Effect.Bad.Release(this.gameObject);
                    break;
                }
                case JudgementRank.Miss:
                {
                    Effect.Miss.Release(this.gameObject);
                    break;
                }
                default: return;
            }
        }
    }
}
