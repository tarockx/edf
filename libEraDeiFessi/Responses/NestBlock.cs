using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace libEraDeiFessi
{
    public class NestBlock : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public List<NestBlock> Children { get; set; }

        public NestBlock() { Children = new List<NestBlock>();}

        public virtual int LinkCount()
        {
            int c = 0;
            foreach (var item in Children)
            {
                c += item.LinkCount();
            }
            return c;
        }

        public List<ContentNestBlock> GetContentBlocks() {
            List<ContentNestBlock> cnbs = new List<ContentNestBlock>();
            foreach (var item in Children)
            {
                if (item is ContentNestBlock)
                    cnbs.Add(item as ContentNestBlock);
                else
                    cnbs.AddRange(item.GetContentBlocks());
            }
            return cnbs;
        }

        public List<NestBlock> GetNestBlocksWithContent()
        {
            List<NestBlock> nbs = new List<NestBlock>();
            if (this is ContentNestBlock)
                return nbs;

            foreach (var item in Children)
            {
                if (item is ContentNestBlock)
                {
                    if (!nbs.Contains(this))
                        nbs.Add(this);
                }
                else
                {
                    nbs.AddRange(item.GetNestBlocksWithContent());
                }
            }

            return nbs;
        }


        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; OnPropertyChanged("IsSelected"); }
        }

        private bool _IsExpanded;

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { _IsExpanded = value; OnPropertyChanged("IsExpanded"); }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
