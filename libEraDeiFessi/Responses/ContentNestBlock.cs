using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class ContentNestBlock : NestBlock
    {
        public List<Link> Links { get; set; }

        private DateTime? _LastAccess = null;
        public DateTime? LastAccess
        {
            get { return _LastAccess; }
            set { _LastAccess = value; OnPropertyChanged("WasAccessed"); OnPropertyChanged("LastAccess"); } 
        }


        public ContentNestBlock() { Links = new List<Link>(); }
        
        public override int LinkCount()
        {
            int c = 0;
            foreach (var item in Children)
            {
                c += item.LinkCount();
            }
            if (Links.Count > 0)
                c++;
            return c;
        }

        public bool ContainsLink(string link)
        {
            foreach (var item in Links)
            {
                if (item.Equals(link))
                    return true;
            }
            return false;
        }

        public bool WasAccessed
        {
            get { return LastAccess.HasValue; }
        }
    }
}
