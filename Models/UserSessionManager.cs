using LES_USER_ADMINISTRATION_LIB.Model;
using LeS_LogiLink_WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using LeSDataMain;
using System;

namespace LeS_LogiLink_WebApp.Models
{
    #region UserDefaultData
    public class UserDefaultData : IUserDefaultData
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public UserDefaultData(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public Dictionary<int, Dictionary<string, int>> companiesaccess { get { return JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, int>>>(_contextAccessor.HttpContext.Session.GetString("UserAccessRights")); } }
        public List<V_COMPANY_DETAILS_DATA> company_detail_data { get { return JsonConvert.DeserializeObject<List<V_COMPANY_DETAILS_DATA>>(_contextAccessor.HttpContext.Session.GetString("CompanyList")); } }

        public int companyid { get { return convert.ToInt(_contextAccessor.HttpContext.Session.GetString("CompanyId")); } }
        public string username { get { return convert.ToString(_contextAccessor.HttpContext.Session.GetString("UserName")); } }
        public string emailid { get { return convert.ToString(_contextAccessor.HttpContext.Session.GetString("UserEmail")); } }
        public string currcompanydesc { get { return convert.ToString(_contextAccessor.HttpContext.Session.GetString("Company")); } }
        public string projectversion { get { return convert.ToString(_contextAccessor.HttpContext.Session.GetString("Projectversion")); } }
        public bool isbuyer { get
            {
                string addressType = _contextAccessor.HttpContext.Session.GetString("AddressType");
                return string.Equals(addressType.ToLower(), "buyer", StringComparison.OrdinalIgnoreCase);
            } }
    }
    public class EpodUserDefaultData : IEpodUserDefaultData
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public EpodUserDefaultData(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public VesselAuthenticationModalViews EpodUser { get 
            {
                if (_contextAccessor.HttpContext.Session.GetString("EPOD_USER_INFO") != null)
                {
                    return JsonConvert.DeserializeObject<VesselAuthenticationModalViews>(_contextAccessor.HttpContext.Session.GetString("EPOD_USER_INFO"));

                }
                else
                {
                    var emptymodal = new VesselAuthenticationModalViews();
                    return emptymodal;

                }
            } 
        }
        
        public int companyid { get { return convert.ToInt(_contextAccessor.HttpContext.Session.GetString("EPOD_CompanyId")); } }
        public int logoflag { get { return convert.ToInt(_contextAccessor.HttpContext.Session.GetString("EPOD_logoflag")); } }
        public string projectversion { get { return convert.ToString(_contextAccessor.HttpContext.Session.GetString("Projectversion")); } }

    }
    #endregion
}
