using LeS_LogiLink_WebApp.Interface;
using Logistic_Management_Lib;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Repo
{
    public class SupplierOutbound : IOutbound
    {
        private readonly ApiCallRoutine _apiroutine;
        public SupplierOutbound(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }

        public string GetOutboundShipmentList(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetShipmentListByCompanyId", strBody);
        }
        public string GetCustomersList(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetCustomerInfoListByCompanyId", null, null, QueryParam);
        }
        public string SaveCreatedOutboundShipmentDetails(CreateShipmentOrderModal Modal,int customerid=0,int companyid=0)
        {
            Modal.shipment_Info.companyid = companyid;
            string Modified = JsonConvert.SerializeObject(Modal);
            return _apiroutine.PostAPI("Logistic", "CreateShipmentDetails", Modified);

        }
        public string EditOutboundShipmentDetails(bool isShipInfo, IDictionary<string, string> strBody)
        {
            if (isShipInfo)
            {
                return _apiroutine.PostAPI("Logistic", "GetShipmentInfo",null,null, strBody);
            }
            else
            {
                return _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId",null,null, strBody);
            }
        }
        public string ValidateDOswithNewShipment(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetNewShipmentWithDOList", strBody);
        }
        public string SaveOutboundShipmentDetails(UpdateShipmentOrderModal Modal, int custmerid = 0, int companyid = 0)
        {
            Modal.shipment_Info.companyid = companyid;
            string Modified = JsonConvert.SerializeObject(Modal);
            return _apiroutine.PostAPI("Logistic", "UpdateShipmentDetails", Modified);
          
        }
        public string UnassignedDeliveryOrder(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UpdateUnAssignShipmentToDeliveryOrder", strBody);
           
        }
    }
}
