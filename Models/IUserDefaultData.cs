using LeS_LogiLink_WebApp.Controllers;
using LES_USER_ADMINISTRATION_LIB.Model;
using System.Collections.Generic;

namespace LeS_LogiLink_WebApp.Models
{
    public interface IUserDefaultData
    {
        List<V_COMPANY_DETAILS_DATA> company_detail_data { get; }
        Dictionary<int, Dictionary<string, int>> companiesaccess { get; }
        int companyid {get;}
        string username { get; }
        string emailid {  get; }
        string currcompanydesc {  get; }
        string projectversion {  get; }
        bool isbuyer {  get; }
    }
    public interface IEpodUserDefaultData
    {
        VesselAuthenticationModalViews EpodUser { get; }
        int companyid { get; }
        string projectversion { get; }
        int logoflag { get; }

    }
}