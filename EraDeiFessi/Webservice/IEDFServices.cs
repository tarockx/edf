using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace EraDeiFessi.Webservice
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEDFServices" in both code and config file together.
    [ServiceContract]
    public interface IEDFServices
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        string UnblockURL(string url);

    }
}
