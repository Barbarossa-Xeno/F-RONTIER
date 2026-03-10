using UnityEngine;

namespace FRONTIER.Menu.Window.Setting
{
    /// <summary>
    /// 各設定項目の値を決定するための基底抽象クラス。
    /// </summary>
    public abstract class AnyTypeValue
    {
        /// <summary>
        /// 項目を初期化する。
        /// </summary>
        /// <param name="element">そのオブジェクトで何の項目を設定するか。</param>
        public abstract void Init(ElementTypes element);

        /// <summary>
        /// 設定の値を初期化する。
        /// </summary>
        /// <param name="element">そのオブジェクトで何の項目を設定するか。</param>
        protected abstract void ValueInit(ElementTypes element);

        /// <summary>
        /// 値を表示する。
        /// </summary>
        /// <param name="targetValue">設定された値</param>
        protected abstract void ShowValue(in float targetValue);

        /// <summary>
        /// 設定値を更新する。
        /// </summary>
        /// <param name="targetValue">設定値</param>
        /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
        protected abstract void UpdateValue(ref float targetValue, float increment);

        /// <summary>
        /// 設定値がintの場合floatにキャストして反映する。
        /// </summary>
        /// <param name="targetValue">設定値</param>
        /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
        protected virtual void UpdateValue(ref int targetValue, float increment)
        {
            float casted = (float)targetValue;
            UpdateValue(ref casted, increment);
            targetValue = (int)casted;
        }

        /// <summary>
        /// 設定値が<c>bool</c>の場合、入力された値を０か１にして反映させる。
        /// </summary>
        /// <param name="targetValue">設定値</param>
        /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
        protected virtual void UpdateValue(ref bool targetValue, float increment)
        {
            float clamped = Mathf.Clamp01(increment);
            targetValue = clamped == 0 ? false : clamped == 1 ? true : false;
        }

        /// <summary>
        /// 設定する項目に合わせて実行するメソッドやアクションを選択させる。
        /// </summary>
        /// <param name="element">設定する項目</param>
        /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
        protected abstract void SelectAction(ElementTypes element, float increment);
    }

}