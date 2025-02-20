using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Repo
{
    public class BuyerInbound:IInbound
    {
        private readonly ApiCallRoutine _apiroutine;
        public BuyerInbound(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string GetInboundShipmentList(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetInboundShipmentListByCustomerId", strBody);
        }

        public string GetCustomersList(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetCustomerInfoListByCustomerId", null, null, QueryParam);

        }
        public string UnassignedDeliveryOrder(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "", strBody);

        }
    }
}
