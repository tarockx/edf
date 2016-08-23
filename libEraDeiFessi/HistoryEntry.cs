using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public enum HistoryAction { Unblocked, FailedToUnblock, OpenedInBrowser}

    [System.Runtime.Serialization.DataContract]
    public class HistoryEntry
    {
        public string user { get; set; }
        public string link { get; set; }
        public DateTime timestamp { get; set; }
        public HistoryAction action { get; set; }

        public HistoryEntry() { }
        public HistoryEntry(string user, string link, DateTime timestamp, HistoryAction action)
        {
            this.user = user;
            this.link = link;
            this.action = action;
            this.timestamp = timestamp;
        }
    }
}
