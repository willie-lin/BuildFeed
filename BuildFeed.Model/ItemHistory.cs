using System;

namespace BuildFeed.Model
{
    public class ItemHistory<T>
    {
        public ItemHistoryType Type { get; set; }
        public DateTime Time { get; set; }
        public string UserName { get; set; }

        public T Item { get; set; }
    }
}