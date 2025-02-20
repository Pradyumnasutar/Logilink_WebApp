using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Repo
{
    public class SupplierOrders : IOrders
    {
        private readonly ApiCallRoutine _apiroutine;
        public SupplierOrders(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string hello()
        {
            return "hello from supplier!";
        }
        public string GetDeliveryOrderList(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByCompanyId", strBody);
        }

        public string AssignToShipment(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UpdateAssignShipmentToDeliveryOrder", strBody);
        }

        public string DeliveryOrderDetails(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersInfoByCompanyId", null, null, QueryParam);
        }
    }
}
