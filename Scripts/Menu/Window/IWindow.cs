namespace Game.Menu.Window
{
    public interface IWindow
    {
        /// <summary>
        /// ステート<see cref="ButtonState"/>の状況に応じて、ある項目が選択されているとき処理を行う。
        /// </summary>
        /// <remarks>
        /// AnimationEvent上のみで発火
        /// </remarks>
        /// <param name="state"></param>
        public void OnChangeButtonState(ButtonState state) { }
    }
    
    /// <summary>
    /// ボタンに対して、その項目選択されているか、されていないかの条件を自由に設定して、確認するためのステート
    /// </summary>
    public enum ButtonState { Selected, Unselected }
}