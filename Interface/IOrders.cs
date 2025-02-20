using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Interface
{
    public interface IOrders
    {
        string hello();
        string GetDeliveryOrderList(string strBody);
        string AssignToShipment(string strBody);
        string DeliveryOrderDetails(IDictionary<string, string> strBody);
    }
}