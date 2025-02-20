using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Repo
{
    public class SupplierInbound:IInbound
    {
        private readonly ApiCallRoutine _apiroutine;
        public SupplierInbound(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string GetInboundShipmentList(string strbody)
        {
            return _apiroutine.PostAPI("Logistic", "GetInboundShipmentListByCompanyId", strbody);
        }

        public string GetCustomersList(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetCustomerInfoListByCompanyId", null, null, QueryParam);

        }

        public string UnassignedDeliveryOrder(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UnAssignPOToDeliveryOrder", strBody);

        }

    }
}
