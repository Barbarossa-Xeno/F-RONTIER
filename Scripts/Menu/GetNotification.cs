namespace Game{
    [System.Serializable]
    public class Notification{
        public static Notification notification = new Notification();
        public News[] news;
        [System.Serializable]
        public class News{
            public string title;
            public string p;
        }
    }
}