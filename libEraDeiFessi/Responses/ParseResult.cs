using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class ParseResult
    {
        public enum ParseResultType { HtmlContent, NestedContent, TorrentContent}

        public bool HasError { get; set; }
        public string ErrMsg { get; set; }

        public object Result { get; set; }
        public ParseResultType ResultType { get; set; }

        public ParseResult(String error)
        {
            HasError = true;
            ErrMsg = error;
            Result = null;
        }

        public ParseResult(HtmlContent res)
        {
            HasError = false;
            Result = res;
            ResultType = ParseResultType.HtmlContent;
        }

        public ParseResult(NestedContent res)
        {
            HasError = false;
            Result = res;
            ResultType = ParseResultType.NestedContent;
        }

        public ParseResult(TorrentContent res)
        {
            HasError = false;
            Result = res;
            ResultType = ParseResultType.TorrentContent;
        }
    }
}
