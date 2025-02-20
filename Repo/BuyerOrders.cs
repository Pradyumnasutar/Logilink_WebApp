using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;
namespace LeS_LogiLink_WebApp.Repo
{
    public class BuyerOrders : IOrders
    {
        private readonly ApiCallRoutine _apiroutine;
        public BuyerOrders(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string hello()
        {
            return "hello from buyer!";
        }
        public string GetDeliveryOrderList(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByCustomerId", strBody);
            
        }
        public string AssignToShipment(string strBody)
        {
            return _apiroutine.PostAPI("Logistic", "UpdateAssignShipmentToDeliveryOrder", strBody);
        }

        public string DeliveryOrderDetails(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersInfoByCustomerId", null, null, QueryParam);
          
        }

    }
}
