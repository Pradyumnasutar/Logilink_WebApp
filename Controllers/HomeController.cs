using System;
using System.Text;
using LeSDataMain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using LeS_LogiLink_WebApp.Data;
using Newtonsoft.Json;
using LES_USER_ADMINISTRATION_LIB.Model;
using LeS_LogiLink_WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using LeS_LogiLink_WebApp.Interface;
using LeS_LogiLink_WebApp.Repo;
namespace LeS_LogiLink_WebApp.Controllers
{

    public class HomeController : Controller
    {
        //DataAccess _dataAccess;
        private IConfiguration Configuration;
        private IUserDefaultData UserDefaultData;
        public HomeController(IConfiguration _configuration, IUserDefaultData _userdefaultdata)
        {
            
            Configuration = _configuration;
            UserDefaultData = _userdefaultdata;
        }
        
        //}
        public IActionResult Index()
        {

            return View();
        }
        public IActionResult WorkInprogress()
        {
            
            return View();

        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error(int? statuscode = null)  // Added by sadanand on 17 MAY 23
        {
            ViewBag.Module = "Error";
            if (statuscode.HasValue)
            {
                string errorMessage = GetErrorMessage(statuscode.Value);

                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "An error occured";
                }
                ViewBag.ErrorMessage = errorMessage;
            }
            else
            {
                ViewBag.ErrorMessage = "An Error Occured";
            }
            return View("Error");

        }
        public IActionResult GetCompanyList()
        {
            List<V_COMPANY_DETAILS> Model = new();
            JsonResult _result = Json(new { Data = Model, selctedId = 0 });

            var CompanyList = HttpContext.Session.GetString("CompanyList");
            var CompanyId = HttpContext.Session.GetString("CompanyId");

            if (CompanyList != null && CompanyList != "")
            {
                Model = JsonConvert.DeserializeObject<List<V_COMPANY_DETAILS>>(CompanyList);
                _result = Json(new { Data = Model, selectedId = CompanyId });
            }
            return _result;


        }
        public IActionResult RedirectToEPOD()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            string url = Configuration.GetValue("AppSettings:EPOD_URL", "");
            return Redirect(url);
        }
        public IActionResult SwitchCompany(int companyId)
        {
            JsonResult _result = Json(new { result = false, msg = "Unable to switch company, please contact support !" });
            try
            {
                if (companyId > 0)
                {
                    List<V_COMPANY_DETAILS_DATA> list_company_details = new();
                    var CompanyList = HttpContext.Session.GetString("CompanyList");
                    var username = HttpContext.Session.GetString("UserName");
                    var userid = HttpContext.Session.GetString("UserID");
                    var oldcompany = HttpContext.Session.GetString("Company");
                    if (CompanyList != null && CompanyList != "")
                    {
                        list_company_details = JsonConvert.DeserializeObject<List<V_COMPANY_DETAILS_DATA>>(CompanyList);
                        var CompanyData = list_company_details.Where(x => x.CompanyId == companyId).FirstOrDefault();

                        HttpContext.Session.SetString("CompanyId", convert.ToString(companyId));
                        HttpContext.Session.SetString("Company", CompanyData.Company_Description);
                        HttpContext.Session.SetString("CompanyCode", CompanyData.Company_Code);
                        HttpContext.Session.SetString("AddressType", CompanyData.AddressType.ToLower());
                        HttpContext.Session.SetString("CustomerID", convert.ToString(CompanyData.CustomerId));
                        LeSDM.AddLog("User : " + username + " switched company from " + oldcompany + " to " + CompanyData.Company_Description);

                        _result = Json(new { result = true, msg = "Company switched successfully !" });
                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetModuleStatuses - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        private string GetErrorMessage(int statusCode)
        {
            switch (statusCode)
            {
                case 401:
                    return $"401 Unauthorized Error" + "\n" + "Sorry, your request could not be proceed";
                case 403:
                    return "403 Forbidden Error" + "\n" + "User Access Denied";
                case 404:
                    return "404 Page not found" + "\n" + "The requested URL was not found on this server";
                case 500:
                    return "500 Internal server error" + "\n" + "The server encountered an error and could not complete your request";
                default:
                    return "An error occured" + "\n" + "Please try again later";
            }
        }



    }
    public class UserHome
    {
        public string UserName { get; set; }
        public string Company { get; set; }

    }
}
