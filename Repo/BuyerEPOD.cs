using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Repo
{
    public class BuyerEPOD : IePOD
    {
        private readonly ApiCallRoutine _apiroutine;
        public BuyerEPOD(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string GetEpodListTable(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetEpodShipmentListByCustomerId", strBody);
            
        }
        public string SaveEpodRemark(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UpdateEPODShipmentDetails", strBody);
        }
        public string EpodDetailsByUrl(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "EpodLoginByVesselAuthenticationCrewEncryptData", strBody);
   ;
        }
        public string GetDeliveryOrderListByShipmentId(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, QueryParam);
        }
        public string UpdateEpodDeliveryOrderLines(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UpdateEpodDeliveryOrderLines", strBody);
        }
    }
}
