using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LeSDataMain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LeS_LogiLink_WebApp.Data;
using Microsoft.AspNetCore.Authorization;
using LeSEncryptionHelperr = LeSEncryptionHelper.LeSEncryptionHelper;
using GlobalTools = LeS_LogiLink_WebApp.Data.GlobalTools;
using LeS_LogiLink_WebApp.Models;
using LES_USER_ADMINISTRATION_LIB.Model;
using System.IO;
using Logistic_Management_Lib.Model;
using Logistic_Management_Lib;
using System.Web;
using System.Security.Claims;
using LES_USER_ADMINISTRATION_LIB;
using Aspose.Cells;
using Aspose.Cells.Drawing;
using System.Globalization;
using System.Drawing;
using Aspose.Cells.Rendering;
using Aspose.Pdf;

namespace LeS_LogiLink_WebApp.Controllers
{
    public class AuthenticateController :Controller
    {
        private IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;      
        //DataAccess _dataAccess;
        public string cErrorMessage = "";
        public AuthenticateController( IConfiguration _configuration, ApiCallRoutine routine)
        {      
            Configuration = _configuration;
            _apiroutine = routine;
        }
        public ActionResult GetSessionValues()
        {

            try
            {
                
                int companyid = convert.ToInt(  HttpContext.Session.GetString("CompanyId"));
                Dictionary<int, Dictionary<string, int>> CompaniesAccess = new Dictionary<int, Dictionary<string, int>>();
                Dictionary<string, int> EachCompanyAccess = new Dictionary<string, int>();

                CompaniesAccess = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, int>>>(HttpContext.Session.GetString("UserAccessRights"));
                EachCompanyAccess = CompaniesAccess[companyid];
                
                return new JsonStringResult(JsonConvert.SerializeObject(EachCompanyAccess));

               
            }   
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetSessionValues " + e.GetBaseException().ToString());
                throw ;
            }
        }
        public void LoadProjectVersion()
        {
           
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                HttpContext.Session.SetString("Projectversion", fvi.FileVersion.ToString()) ;
            }
            catch (Exception ex)
            {
                LeSDataMain.LeSDM.AddLog("Error on Get_Version : " + ex.Message);
            }
            
        }
        public ActionResult Get_Version()
        {
            string version = "";
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                version = fvi.FileVersion;
            }
            catch (Exception ex)
            {
                LeSDataMain.LeSDM.AddLog("Error on Get_Version : " + ex.Message);
            }
            return Json(version);
        }
        public ActionResult Login()
        {
            try
            {
                
                ViewBag.ShowPwdExpiryDialog = 0;

                LoadProjectVersion();
                if (HttpContext.Session.GetString("UserID") != null)
                {
                    int USERID = convert.ToInt(HttpContext.Session.GetString("UserID"));

                    string _errorMessage = convert.ToString(HttpContext.Session.GetString("LOGIN_FAIL_MESSAGE"));
                    if (_errorMessage.Length > 0)
                    {
                        LeSDM.AddLog("_errorMessage - " + _errorMessage);
                    }
                    string _errorNotice = convert.ToString(HttpContext.Session.GetString("LOGIN_FAIL_NOTICE"));
                    if (_errorNotice.Length > 0)
                    {
                        LeSDM.AddLog("_errorNotice - " + _errorNotice);
                    }
                    ViewBag.Notice = _errorNotice;
                    ViewBag.Message = _errorMessage;
                    HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "");
                    HttpContext.Session.SetString("LOGIN_FAIL_NOTICE", "");
                }
                return View();
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception - " + ex.Message);
                ViewBag.Message = ex.Message;
                return View();
            }
        }
        public JsonResult GetLogo()
        {
            List<V_COMPANY_DETAILS_DATA> list_company_details = new();
            V_COMPANY_DETAILS_DATA Modal = new();
            string companyname = HttpContext.Session.GetString("Company");
            JsonResult _result = Json(new { result = false,company= companyname });
            try
            {
                int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    var CompanyList = HttpContext.Session.GetString("CompanyList");
                    if (CompanyList != null && CompanyList != "")
                    {
                        list_company_details = JsonConvert.DeserializeObject<List<V_COMPANY_DETAILS_DATA>>(CompanyList);
                        Modal = list_company_details.Where(x => x.CompanyId == companyId).FirstOrDefault();
                        _result = Json(new { result = true,logo= Modal.base64Logo,minlogo = Modal.base64minLogo, company = Modal.Company_Description });
                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetLogo - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
      
        public ActionResult LoginInByCredentials(string username,string password)
        {
           
            try
            {
                ViewBag.ShowPwdExpiryDialog = 0;
                HttpContext.Session.SetString("UserID", "0");
                HttpContext.Session.SetString("UserName", "");
                HttpContext.Session.SetString("UserEmail", "");
                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "");
                
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                     _apiroutine.RefreshTokenAsync();
                    if (HttpContext.Session.Keys.Any(x=>x=="ApplicationID")) {}
                    else
                    {
                        //string Data =  _apiroutine.GetAPI("UserAdmin", "GetApplicationList");
                        //List<LES_APPLICATIONS> Apps = JsonConvert.DeserializeObject<List<LES_APPLICATIONS>>(Data);
                        //int applicationId = Apps.Where(x => x.application_name.ToLower() == "logistic management").FirstOrDefault().applicationid;
                        //HttpContext.Session.SetString("AppicationID", applicationId.ToString());
                        
                        HttpContext.Session.SetString("AppicationID", Configuration.GetValue("AppSettings:HostURL", ""));
                        int applicationId = convert.ToInt(Configuration.GetValue("AppSettings:HostURL", ""));
                    }
                    string body = $@"
                    {{
                        ""username"": ""{username}"",
                        ""password"": ""{password}""
                    }}";
                    string UserData =  _apiroutine.PostAPI("UserAdmin", "UserLogIn", body);
                    LoadProjectVersion();
                    if (!string.IsNullOrEmpty(UserData) && !string.IsNullOrWhiteSpace(UserData))
                    {
                        if(CommonRoutine.IsValidJson(UserData))
                        {
                           
                            LoginModalViews User = JsonConvert.DeserializeObject<LoginModalViews>(UserData);
                            ApiResponse Response = JsonConvert.DeserializeObject<ApiResponse>(UserData);
                            if(Response.Message == null)
                            {
                                var DefaultCompany = convert.ToInt(User.DefaultCompanyID);
                                if (DefaultCompany != 0) //if no default company id found then showing first company
                                {
                                    SetModuleAccess(User);
                                    var CompanyData = User.list_company_details.Where(x => x.CompanyId == DefaultCompany).FirstOrDefault();

                                    HttpContext.Session.SetString("CompanyId", convert.ToString(DefaultCompany));
                                    HttpContext.Session.SetString("Company", CompanyData.Company_Description);
                                    HttpContext.Session.SetString("CompanyCode", CompanyData.Company_Code);
                                    HttpContext.Session.SetString("UserTypeDesc", CompanyData.Usertypedescr);
                                    HttpContext.Session.SetString("CompanyList", JsonConvert.SerializeObject(User.list_company_details));
                                    HttpContext.Session.SetString("UserID", convert.ToString(User.Userid).Trim());
                                    HttpContext.Session.SetString("CustomerID", convert.ToString(CompanyData.CustomerId));
                                    //HttpContext.Session.SetString("UserTypeID", convert.ToString(User.uset));
                                    HttpContext.Session.SetString("UserEmail", User.UserEmail);
                                    HttpContext.Session.SetString("UserName", User.UserName);
                                    HttpContext.Session.SetString("AddressType", CompanyData.AddressType.ToLower());
                                    LeSDM.AddLog("User Logged in : " + User.UserName + " (" + User.Userid + ") for the company : " + CompanyData.Company_Description);
                                    if(SaveModuleStatuses(1)&& SaveModuleStatuses(2))//1==Outbound shipment 2==delivery orders
                                    {
                                        
                                        return RedirectPreserveMethod(GetDefaltPage());
                                    }
                                    else
                                    {
                                        
                                        ViewBag.Message = "Internal Error";
                                        HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Something went wrong, It's not you but us!");
                                    }

                                    
                                }
                                else
                                {
                                    SetModuleAccess(User);

                                    HttpContext.Session.SetString("CompanyId", convert.ToString(User.list_company_details[0].CompanyId));
                                    HttpContext.Session.SetString("Company", User.list_company_details[0].Company_Description);
                                    HttpContext.Session.SetString("CompanyCode", User.list_company_details[0].Company_Code);
                                    HttpContext.Session.SetString("UserTypeDesc", User.list_company_details[0].Usertypedescr);
                                    HttpContext.Session.SetString("CompanyList", JsonConvert.SerializeObject(User.list_company_details));
                                    HttpContext.Session.SetString("UserID", convert.ToString(User.Userid).Trim());
                                    HttpContext.Session.SetString("CustomerID", convert.ToString(User.list_company_details[0].CustomerId));
                                    //HttpContext.Session.SetString("UserTypeID", convert.ToString(User.uset));
                                    HttpContext.Session.SetString("UserEmail", User.UserEmail);
                                    HttpContext.Session.SetString("UserName", User.UserName);
                                    HttpContext.Session.SetString("AddressType", User.list_company_details[0].AddressType.ToLower());
                                    LeSDM.AddLog("User Logged in : " + User.UserName + " (" + User.Userid + ") for the company : " + User.list_company_details[0].Company_Description);

                                    if (SaveModuleStatuses(1) && SaveModuleStatuses(2))//1==Outbound shipment 2==delivery orders
                                    {
                                       
                                        return RedirectPreserveMethod(GetDefaltPage());
                                    }
                                    else
                                    {

                                        ViewBag.Message = "Internal Error";
                                        HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Something went wrong, It's not you but us!");
                                    }

                                }
                            }
                            else
                            {
                                Response.Message = Response.Message.Replace("ERROR", "").Replace(":", "");
                                ViewBag.Message = Response.Message;
                                LeSDM.AddLog(Response.Message);
                                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", Response.Message);

                            }


                        }
                        else
                        {
                            ViewBag.Message = "Internal error";
                            HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Unable to login please contact support!");

                        }
                          
                    }
                    else
                    {
                        ViewBag.Message = "Please enter valid username and password";
                        HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Please enter valid username and password");
                    }
                    
                }
                else
                {
                    ViewBag.Message = "Please enter valid username and password";
                    HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Please enter valid username and password");
                }
            
                
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Login Failed! Please contact support";
                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Login Failed! Please contact support");
                LeSDataMain.LeSDM.AddLog("Error in Content - " + ex.Message);
                LeSDataMain.LeSDM.AddLog("Error in Content - " + ex.GetBaseException().ToString());
                CommonRoutine.SetAudit("Login", "Error", "", "Login Failed! Please contact support", "");
            }
            
            return RedirectToAction("Login", "Authenticate");
            
        }
        public bool SaveModuleStatuses(int moduleId)
        {
           
            List<V_MODULE_STATUSES> Model = new();
            bool res = false;

            IDictionary<string, string> QueryParam = new Dictionary<string, string>();
            try
            {
                QueryParam.Add("Moduleid", moduleId.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetModuleStatus", null, null, QueryParam);
                if (jsonData != null && jsonData != "")
                {
                    HttpContext.Session.SetString("LogisticModuleStatus" + moduleId, jsonData);
                    res = true;

                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SaveModuleStatuses - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            
            return res;
        }

        public static string SaveIconFromBase64(string base64Data, string filePath)
        {
            
            try
            {
                byte[] iconBytes = Convert.FromBase64String(base64Data);

                using (MemoryStream ms = new MemoryStream(iconBytes))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(fs);
                    }
                }
                
                return filePath;
            }
            catch (Exception ex)
            {
                
                LeSDM.AddLog("An error occurred SaveIconFromBase64: " + ex.Message);
                return null;
            }
        }
        public void SetModuleAccess(LoginModalViews Users)
        {
           
            Dictionary<int?, Dictionary<string, int>> CompaniesAccess = new Dictionary<int?, Dictionary<string, int>>();
            var CompaniesGrp = Users.list_Application_Module_Access.GroupBy(x => x.CompanyId).ToList();
            for (int i = 0; i < CompaniesGrp.Count; i++)
            {
                Dictionary<string, int> CompanyAccess = new Dictionary<string, int>();
                var EachGroup = CompaniesGrp[i];
                var GroupedGrp = EachGroup.Select(x=> new {x.ModuleId,x.Access_Level }).ToList();
                for(int x=0; x < GroupedGrp.Count; x++)
                {
                    var EachEnt = GroupedGrp[x];
                    
                    CompanyAccess.Add(EachEnt.ModuleId.ToString(),EachEnt.Access_Level);
                }
                CompaniesAccess.Add(EachGroup.Select(x=>x.CompanyId).FirstOrDefault(),CompanyAccess);
            }
            HttpContext.Session.SetString("UserAccessRights", JsonConvert.SerializeObject(CompaniesAccess));
           
        }
        
        [Authorize]
        public IActionResult LoginInByMicrosoft()
        {
    
            try
            {
                cErrorMessage = "";
                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "");
                HttpContext.Session.SetString("UserID", "0");
                HttpContext.Session.SetString("UserName", "");

                if (User.Identity.IsAuthenticated)
                {
                    
                    var encrypredKey = GetPrivateEncodedKey(User);
                    
                    _apiroutine.RefreshTokenAsync();
                    if (HttpContext.Session.Keys.Any(x => x == "ApplicationID")) { }
                    else
                    {
                        //string Data =  _apiroutine.GetAPI("UserAdmin", "GetApplicationList");
                        //List<LES_APPLICATIONS> Apps = JsonConvert.DeserializeObject<List<LES_APPLICATIONS>>(Data);
                        //int applicationId = Apps.Where(x => x.application_name.ToLower() == "logistic management").FirstOrDefault().applicationid;
                        //HttpContext.Session.SetString("AppicationID", applicationId.ToString());

                        HttpContext.Session.SetString("AppicationID", Configuration.GetValue("AppSettings:HostURL", ""));
                        int applicationId = convert.ToInt(Configuration.GetValue("AppSettings:HostURL", ""));
                    }
                    var AIO = User.Claims.Where(x => x.Type == "aio").FirstOrDefault().Value;
                    var RH = User.Claims.Where(x => x.Type == "rh").FirstOrDefault().Value;
                    var UTI = User.Claims.Where(x => x.Type == "uti").FirstOrDefault().Value;
                    var currentDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ff");
                    currentDate = LeSEncryptionHelper.LeSEncryptionHelper.Encrypt(currentDate);
                    MicrosoftUserModal modal = new MicrosoftUserModal();
                    modal.aio = AIO;
                    modal.rh = RH;
                    modal.uti = UTI;
                    modal.nucleus = currentDate;
                    modal.emailid = User.Identity.Name;
                    modal.sigmakey = encrypredKey;

                    string body  = JsonConvert.SerializeObject(modal);
                    LeSDM.AddLog("SSO input: "+body);
                    string UserData = _apiroutine.PostAPI("UserAdmin", "UserLogInByMicrosoft", body);
                    LeSDM.AddLog("SSO result : " + UserData);
                    var ErrorObj = JsonConvert.DeserializeObject<ApiResponse>(UserData);
                    if (ErrorObj != null && !ErrorObj.isSuccess)
                    {
                        LoadProjectVersion();
                        if (!string.IsNullOrEmpty(UserData) && !string.IsNullOrWhiteSpace(UserData))
                        {
                            if (CommonRoutine.IsValidJson(UserData))
                            {

                                LoginModalViews User = JsonConvert.DeserializeObject<LoginModalViews>(UserData);
                                ApiResponse Response = JsonConvert.DeserializeObject<ApiResponse>(UserData);
                                if (Response.Message == null)
                                {
                                    var DefaultCompany = convert.ToInt(User.DefaultCompanyID);
                                    if (DefaultCompany != 0) //if no default company id found then showing first company
                                    {
                                        SetModuleAccess(User);
                                        var CompanyData = User.list_company_details.Where(x => x.CompanyId == DefaultCompany).FirstOrDefault();

                                        HttpContext.Session.SetString("CompanyId", convert.ToString(DefaultCompany));
                                        HttpContext.Session.SetString("Company", CompanyData.Company_Description);
                                        HttpContext.Session.SetString("CompanyCode", CompanyData.Company_Code);
                                        HttpContext.Session.SetString("UserTypeDesc", CompanyData.Usertypedescr);
                                        HttpContext.Session.SetString("CompanyList", JsonConvert.SerializeObject(User.list_company_details));
                                        HttpContext.Session.SetString("UserID", convert.ToString(User.Userid).Trim());
                                        HttpContext.Session.SetString("CustomerID", convert.ToString(User.list_company_details[0].CustomerId));
                                        //HttpContext.Session.SetString("UserTypeID", convert.ToString(User.uset));
                                        HttpContext.Session.SetString("UserEmail", User.UserEmail);
                                        HttpContext.Session.SetString("UserName", User.UserName);
                                        HttpContext.Session.SetString("AddressType", CompanyData.AddressType.ToLower());
                                        LeSDM.AddLog("User Logged in : " + User.UserName + " (" + User.Userid + ") for the company : " + CompanyData.Company_Description);
                                        if (SaveModuleStatuses(1) && SaveModuleStatuses(2))//1==Outbound shipment 2==delivery orders
                                        {
                                           
                                            return RedirectPreserveMethod(GetDefaltPage());
                                        }
                                        else
                                        {

                                            ViewBag.Message = "Internal Error";
                                            HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Something went wrong, It's not you but us!");
                                        }


                                    }
                                    else
                                    {
                                        SetModuleAccess(User);

                                        HttpContext.Session.SetString("CompanyId", convert.ToString(User.list_company_details[0].CompanyId));
                                        HttpContext.Session.SetString("Company", User.list_company_details[0].Company_Description);
                                        HttpContext.Session.SetString("CompanyCode", User.list_company_details[0].Company_Code);
                                        HttpContext.Session.SetString("UserTypeDesc", User.list_company_details[0].Usertypedescr);
                                        HttpContext.Session.SetString("CompanyList", JsonConvert.SerializeObject(User.list_company_details));
                                        HttpContext.Session.SetString("UserID", convert.ToString(User.Userid).Trim());
                                        HttpContext.Session.SetString("CustomerID", convert.ToString(User.list_company_details[0].CustomerId));
                                        //HttpContext.Session.SetString("UserTypeID", convert.ToString(User.uset));
                                        HttpContext.Session.SetString("UserEmail", User.UserEmail);
                                        HttpContext.Session.SetString("UserName", User.UserName);
                                        HttpContext.Session.SetString("AddressType", User.list_company_details[0].AddressType.ToLower());
                                        LeSDM.AddLog("User Logged in : " + User.UserName + " (" + User.Userid + ") for the company : " + User.list_company_details[0].Company_Description);

                                        if (SaveModuleStatuses(1) && SaveModuleStatuses(2))//1==Outbound shipment 2==delivery orders
                                        {
                                          
                                            return RedirectPreserveMethod(GetDefaltPage());
                                        }
                                        else
                                        {

                                            ViewBag.Message = "Internal Error";
                                            HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Something went wrong, It's not you but us!");
                                        }

                                    }
                                }
                                else
                                {
                                    Response.Message = Response.Message.Replace("ERROR", "").Replace(":", "");
                                    ViewBag.Message = Response.Message;
                                    HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", Response.Message);

                                }


                            }
                            else
                            {
                                ViewBag.Message = "Internal error";
                                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Unable to login please contact support!");

                            }

                        }
                        else
                        {
                            ViewBag.Message = "Please enter valid username and password";
                            HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Please enter valid username and password");
                        }
                    }
                    else
                    {
                        ViewBag.Message = ErrorObj.Message;
                        HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", ErrorObj.Message);

                    }


                }
                //return RedirectPreserveMethod(GetDefaltPage());
            }
            catch (Exception ex)
            {
                
                ViewBag.Message = "Please check the error - " + ex.Message;
                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Please check the error - " + ex.Message);
                LeSDataMain.LeSDM.AddLog("Error in Content - " + ex.Message);
                CommonRoutine.SetAudit("Login", "Error", "", "Login Failed "+ex.GetBaseException(), "");

            }
            
            return RedirectToAction("Login", "Authenticate");
        }
        private string GetPrivateEncodedKey(ClaimsPrincipal Claim)
        {
            
            string id = "";

            if(Claim != null)
            {
                var AIO = Claim.Claims.Where(x => x.Type == "aio").FirstOrDefault().Value;
                var RH = Claim.Claims.Where(x => x.Type == "rh").FirstOrDefault().Value;
                var UTI = Claim.Claims.Where(x => x.Type == "uti").FirstOrDefault().Value;
                var enAIO = LeSEncryptionHelperr.Encrypt(AIO);
                var enRH  = LeSEncryptionHelperr.Encrypt(RH);
                var enUTI  = LeSEncryptionHelperr.Encrypt(UTI);
                var CombinedKey = enAIO+"&|&"+enRH+"&|&"+enUTI;
                var EncryptedCombinedKey = LeSEncryptionHelperr.Encrypt(CombinedKey);             
                id = EncryptedCombinedKey;
            }
           
            return id;
        }
        
        private string GetDefaltPage()
        {
            return "~/Home/Index";
           
        }

        public IActionResult Logout(string msg)
        {
            try
            {
                cErrorMessage = "";
                HttpContext.Session.Remove("LOGIN_FAIL_MESSAGE");
                HttpContext.Session.Remove("UserName");
                HttpContext.Session.Remove("UserID");
                HttpContext.Session.Clear();
                ViewBag.Logout = msg;
                ViewBag.Message = "";
             
                AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme);

            }
            catch (Exception ex)
            {               
                ViewBag.Message = "Please check the error - " + ex.Message;
                HttpContext.Session.SetString("LOGIN_FAIL_MESSAGE", "Please check the error - " + ex.Message);
            }

            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult NotFound(string msg= "No such link exist !")
        {
            ViewBag.Message = msg;
            return View();
        }
       
    }
    public class RequestResponse
    {
        public object Value { get; set; }
        public List<object> Formatters { get; set; }
        public List<object> ContentTypes { get; set; }
        public object DeclaredType { get; set; }
        public int StatusCode { get; set; }
    }
    public class JsonStringResult : ContentResult
    {
        public JsonStringResult(string json)
        {
            Content = json;
            ContentType = "application/json";
        }
    }
    public class DeliveryOrderLine
    {
        public string deliveredQty { get; set; }
        public string receivedQty { get; set; }
        public string returnQty { get; set; }
        public string remark { get; set; }
        public string returnCode { get; set; }
    }
    partial class ResponseMessage
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
    partial class UserPassword
    {
        public string current { get; set; }
        public string new_pwd { get; set; }
        public string confirm { get; set; }
    }


    internal class UrlData
    {
        public string? encryptData { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
    }
    public class VesselAuthenticationModalViews
    {
        public int shipmentid { get; set; }
        public int? userid { get; set; }
        public string? username { get; set; }
        public string? usercode { get; set; }
        public string? defaultcompanyid { get; set; }
        public string? useremailid { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string? password { get; set; } = string.Empty;
        public string? vesselimo { get; set; } = null;
        public string? crewname { get; set; } = null;
        public string? role { get; set; } = null;
        public LES_USER_ADMINISTRATION_LIB.Model.V_COMPANY_DETAILS_DATA les_company_details { get; set; }
        public V_Shipment_Info shipmentInfo { get; set; }
        public List<LES_USER_ADMINISTRATION_LIB.Model.V_APPLICATION_MODULE_ACCESS> list_Application_Module_Access { get; set; }

    }

    internal class VesselAuthenticationUrlModalViews
    {
        public VesselAuthenticationModalViews value { get; set; }
        public List<object> formatters { get; set; }
        public List<object> contentTypes { get; set; }
        public object declaredType { get; set; }
        public int statusCode { get; set; }
    }
}






