using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Interface
{
    public interface IMasters
    {
        string GetCustomersList(IDictionary<string, string> strBody);
    }
}
