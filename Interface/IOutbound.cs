using Logistic_Management_Lib;
using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Interface
{
    public interface IOutbound
    {
        string GetOutboundShipmentList(string strBody);
        string GetCustomersList(IDictionary<string, string> strBody);
        string SaveCreatedOutboundShipmentDetails(CreateShipmentOrderModal Modal, int customerid=0,int companyid =0);
        string EditOutboundShipmentDetails(bool isShipInfo, IDictionary<string, string> strBody);
        string ValidateDOswithNewShipment(string strBody);
        string SaveOutboundShipmentDetails(UpdateShipmentOrderModal Modal, int custmerid = 0, int companyid = 0);
        string UnassignedDeliveryOrder(string strBody);
    }
}
