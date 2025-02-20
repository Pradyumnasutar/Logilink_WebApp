using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Interface
{
    public interface IePOD
    {
        string GetEpodListTable(string strBody);
        string SaveEpodRemark(string strBody);
        string EpodDetailsByUrl(string strBody);
        string GetDeliveryOrderListByShipmentId(IDictionary<string, string> QueryParam);
        string UpdateEpodDeliveryOrderLines(string strBody);
    }
}
