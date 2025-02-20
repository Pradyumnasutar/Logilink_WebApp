using LeS_LogiLink_WebApp.Interface;
using Logistic_Management_Lib;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Repo
{
    public class BuyerOutbound : IOutbound
    {
        private readonly ApiCallRoutine _apiroutine;
        public BuyerOutbound(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string GetOutboundShipmentList(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetShipmentListByCustomerId", strBody);
           
        }
        public string GetCustomersList(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetCustomerInfoListByCustomerId", null, null, QueryParam);
       
        }
        public string SaveCreatedOutboundShipmentDetails(CreateShipmentOrderModal Modal,int customerid=0,int companyid=0)
        {
            Modal.shipment_Info.companyid = Modal.shipment_Info.receiverid;
            Modal.shipment_Info.receiverid = customerid;
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
            //return _apiroutine.PostAPI("Logistic", "GetNewShipmentWithDOList", strBody);
            return "";
        }
        public string SaveOutboundShipmentDetails(UpdateShipmentOrderModal Modal ,int custmerid=0,int companyid=0)
        {
            Modal.shipment_Info.companyid = Modal.shipment_Info.receiverid;
            Modal.shipment_Info.receiverid = custmerid;
            string Modified = JsonConvert.SerializeObject(Modal);
            return _apiroutine.PostAPI("Logistic", "UpdateShipmentDetails", Modified);
        }
        public string UnassignedDeliveryOrder(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UpdateUnAssignShipmentToDeliveryOrder", strBody);
        }
    }
}
