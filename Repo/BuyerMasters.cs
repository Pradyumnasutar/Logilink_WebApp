using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Repo
{
    public class BuyerMasters:IMasters
    {
        private readonly ApiCallRoutine _apiroutine;
        public BuyerMasters(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string GetCustomersList(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetCustomerInfoListByCustomerId", null, null, QueryParam);

        }
    }
}
