using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Interface
{
    public interface IInbound
    {
        string GetInboundShipmentList(string strbody);

        string GetCustomersList(IDictionary<string, string> strBody);

        string UnassignedDeliveryOrder(string strBody);
    }
}
