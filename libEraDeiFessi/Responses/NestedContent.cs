using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libEraDeiFessi
{
    public class NestedContent
    {
        public List<NestBlock> Children { get; set; }

        public string Description { get; set; }
        public string CoverImageUrl { get; set; }

        public NestedContent() { Children = new List<NestBlock>(); }

        public List<ContentNestBlock> GetContentBlocks()
        {
            List<ContentNestBlock> cnbs = new List<ContentNestBlock>();
            foreach (var item in Children)
            {
                cnbs.AddRange(item.GetContentBlocks());
            }
            return cnbs;
        }

        public List<NestBlock> GetNestBlocksWithDirectContent()
        {
            List<NestBlock> nbs = new List<NestBlock>();
            foreach (var item in Children)
            {
                nbs.AddRange(item.GetNestBlocksWithContent());
            }
            return nbs;
        }
    }
}
