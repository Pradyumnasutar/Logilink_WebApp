using LeS_LogiLink_WebApp.Interface;
using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Repo
{
    public class SupplierMasters: IMasters
    {
        private readonly ApiCallRoutine _apiroutine;
        public SupplierMasters(ApiCallRoutine Routine)
        {
            _apiroutine = Routine;
        }
        public string GetCustomersList(IDictionary<string, string> QueryParam)
        {
            return _apiroutine.PostAPI("Logistic", "GetCustomerInfoListByCompanyId", null, null, QueryParam);
        }
    }
}
