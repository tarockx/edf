using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    [System.Runtime.Serialization.DataContract]
    public class HistoryContainer
    {
        public SerializableDictionary<string, HistoryEntry> History;
    }
}
