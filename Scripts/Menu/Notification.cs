namespace Game.Menu
{
    [System.Serializable]
    public class Notification
    {
        public static Notification instance = new Notification();
        public News[] news;
        [System.Serializable]
        public class News
        {
            public string title;
            public string p;
        }
    }
}