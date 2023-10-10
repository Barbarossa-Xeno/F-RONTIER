namespace FRONTIER.Save
{
    /// <summary>
    /// 通知（お知らせやニュース）を読み込んで情報を保持する。
    /// </summary>
    [System.Serializable]
    public class NotificationData : SaveManager<NotificationData>
    {
        /// <summary>
        /// 通知内容。
        /// </summary>
        public Notification[] notification;

        [System.Serializable]
        public class Notification
        {
            /// <summary>
            /// 通知のタイトル。
            /// </summary>
            public string title;

            /// <summary>
            /// 通知の内容と文章。
            /// </summary>
            public string p;
        }

        public override void Load() => base.Load(DataMode.NOTIFICATION);

        public override void Save() => throw new System.Exception("セーブできません");
    }
}