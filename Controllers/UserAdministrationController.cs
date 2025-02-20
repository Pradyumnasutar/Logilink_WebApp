using Aspose.Pdf.Operators;
using LeSDataMain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using LeS_LogiLink_WebApp.Data;
using LeS_LogiLink_WebApp.Models;
using Logistic_Management_Lib;
using GlobalTools = LeS_LogiLink_WebApp.Data.GlobalTools;
using System.Linq;
using LES_USER_ADMINISTRATION_LIB.Model;
using Logistic_Management_Lib.Model;
using System.Text;


namespace LeS_LogiLink_WebApp.Controllers
{

    public class UserAdministrationController : Controller
    {

        private IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;
        private IUserDefaultData UserDefaultData;
        public UserAdministrationController(IConfiguration _configuration, ApiCallRoutine apiroutine, IUserDefaultData userDefaultData)
        {
            Configuration = _configuration;
            _apiroutine = apiroutine;
            UserDefaultData = userDefaultData;
        }

        #region user
        public IActionResult UsersList()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                ViewBag.Module = "User Administration";
                ViewBag.SubTitle = "User Administration";
                ViewBag.SubTitleUrl = "UsersList";

                ViewBag.EnableFilter = false;
                return View();
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in UserList - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }

        }
        public IActionResult GetUserList(int ValueRD)
        {

            JsonResult _result = Json(new object[] { new() });
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                int _rdValue = (HttpContext.Session.GetInt32("RANDOM") != null) ? convert.ToInt(HttpContext.Session.GetInt32("RANDOM")) : 0;
                if (_rdValue <= 0)
                {
                    HttpContext.Session.SetString("USER_DATA", "");
                    HttpContext.Session.SetInt32("RANDOM", ValueRD);
                }
                else
                {
                    if (ValueRD != _rdValue)
                    {
                        HttpContext.Session.SetString("USER_DATA", "");
                        HttpContext.Session.SetInt32("RANDOM", ValueRD);
                    }
                }

                var strList = HttpContext.Session.GetString("USER_DATA");
                List<V_USER_LIST> list = new List<V_USER_LIST>();


                if (!string.IsNullOrEmpty(strList))
                {
                    list = JsonConvert.DeserializeObject<List<V_USER_LIST>>(strList);
                }
                else
                {

                    IDictionary<string, string> queryParam = new Dictionary<string, string>();
                    int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    queryParam.Add("Userid", userId.ToString());

                    string jsonData = _apiroutine.PostAPI("UserAdmin", "GetAllUsers", null, null, queryParam);
                    var data = JsonConvert.DeserializeObject<ApiResponse>(jsonData);
                    list = JsonConvert.DeserializeObject<List<V_USER_LIST>>(data.Data.ToString());
                    HttpContext.Session.SetString("USER_DATA", JsonConvert.SerializeObject(list));

                }

                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                int _recordTotal = list?.Count ?? 0;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : list.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 0;

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumnDir == "asc")
                    {
                        switch (sortColumn)
                        {
                            case "user_id": list = list.OrderBy(x => x.ex_usercode).ToList(); break;
                            case "user_name": list = list.OrderBy(x => x.ex_username).ToList(); break;
                            case "role": list = list.OrderBy(x => x.usertypedescr).ToList(); break;
                            case "status": list = list.OrderBy(x => x.isactive).ToList(); break;
                            case "company": list = list.OrderBy(x => x.companyid).ToList(); break;
                            case "password_expiry_date": list = list.OrderBy(x => x.pwd_expired).ToList(); break;
                            default: list = list.OrderBy(x => x.ex_userid).ToList(); break;
                        }
                    }
                    else
                    {
                        switch (sortColumn)
                        {
                            case "user_id": list = list.OrderByDescending(x => x.ex_usercode).ToList(); break;
                            case "user_name": list = list.OrderByDescending(x => x.ex_username).ToList(); break;
                            case "role": list = list.OrderByDescending(x => x.usertypedescr).ToList(); break;
                            case "status": list = list.OrderByDescending(x => x.isactive).ToList(); break;
                            case "company": list = list.OrderByDescending(x => x.companyid).ToList(); break;
                            case "password_expiry_date": list = list.OrderByDescending(x => x.pwd_expired).ToList(); break;
                            default: list = list.OrderByDescending(x => x.ex_userid).ToList(); break;
                        }
                    }
                }
                int _recordsFiltered = list.Count;
                list = list.Skip(skip).Take(pageSize).ToList();

                _result = Json(new { draw = draw, recordsFiltered = _recordsFiltered, recordsTotal = _recordTotal, data = list });

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetUserList : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
            }
            return _result;
        }
        public IActionResult UserDetails(string userId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            //JsonResult jsonResult = Json(new object[] { new() });
            UserDetailsModal userData = new();
            UserDetailsData userDetailsData = new UserDetailsData();
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                int id = Convert.ToInt32(LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(userId));
                ViewBag.Module = "User Administration";
                ViewBag.SubTitle = "User Administration";
                ViewBag.SubTitleUrl = "User Details";

                ViewBag.EnableFilter = false;

                int accessId = convert.ToInt(HttpContext.Session.GetString("UserID"));
                var _data = JsonConvert.DeserializeObject<ApiResponse>
                    (_apiroutine.PostAPI("UserAdmin", $"GetUserDetails?accessUserid={accessId}&foruserid={id}", null, null));
                userDetailsData.UserDetailModel = JsonConvert.DeserializeObject<UserDetailsModal>(_data.Data.ToString());
               
                userDetailsData.Roles = JsonConvert.DeserializeObject<List<LES_USERTYPE>>(_apiroutine.GetAPI("UserAdmin", "GetAllRoles", null));
                var companies = JsonConvert.DeserializeObject<ApiResponse>(_apiroutine.PostAPI("UserAdmin", $"GetAllCompanyList?userId={accessId}", null));
                userDetailsData.Companies = JsonConvert.DeserializeObject<List<CompaniesList>>(companies.Data.ToString());
               
                //ViewBag.Roles = roles;
                //ViewBag.UserDetails = JsonConvert.SerializeObject(userData.userdetails);
                //ViewBag.LinkedCompanies = JsonConvert.SerializeObject(userData.linkedcompanies);
            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in UserDetails : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            }
            return View(userDetailsData);

        }
        public IActionResult SaveUser(string userData)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            try
            {
                UserDataDetails _details = new UserDataDetails();
                if (userData == null)
                {
                    return Json(new { result = false, message = "Invalid data." });
                }
                _details = JsonConvert.DeserializeObject<UserDataDetails>(userData);
                _details.UserDetails.created_date = convert.ToDateTime(_details.UserDetails.created_date).AddDays(1);
                //if (!string.IsNullOrEmpty(_details.Confirmpassword) && !string.IsNullOrEmpty(_details.Password))
                //{
                //    _details.Confirmpassword  = LeS_LogiLink_WebApp.Data.GlobalTools.EncodePWD(_details.Confirmpassword);
                //    _details.Password = LeS_LogiLink_WebApp.Data.GlobalTools.EncodePWD(_details.Password);
                //}

                
                string jsonData = JsonConvert.SerializeObject(_details);
                var response = JsonConvert.DeserializeObject<ApiResponse>(_apiroutine.PostAPI("UserAdmin", "CreateOrUpdateUserDetails", jsonData));
                if (response != null&& response.isSuccess)
                {
                    var deSerialized = JsonConvert.DeserializeObject<SM_EXTERNAL_USERS>(response.Data.ToString());
                    return Json(new { result = response.isSuccess, message = response.Message, data = deSerialized });

                }
                else
                {
                    throw new Exception(response.Message);
                }

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in SaveUser : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
                return Json(new { result = false, message = e.Message });
                //return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
        }
        public IActionResult RemoveLinkedCompany(string userId, string companyId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            
            JsonResult _result = Json(new object[] { new() });
            try
            {
                var apiResponse = _apiroutine.PostAPI("UserAdmin", $"RemoveUserCompanyLink?accessuserid={HttpContext.Session.GetString("UserID")}&companyid={companyId}&foruserid={userId}","");
                _result =  Json(new { response = apiResponse, isSuccess= true });
            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in RemoveLinkedCompany : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
                return Json(new { response = false});
            }
            return _result;
        }
        public IActionResult AddLinkCompany(string CompanyId, string RoleId, string UserId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            JsonResult _result = Json(new { });
            try 
            {
                int accessId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                int compId = Convert.ToInt32(CompanyId);
                int roleId = Convert.ToInt32(RoleId);
                int forUserId = Convert.ToInt32(UserId);
                var apiResponse =JsonConvert.DeserializeObject<ApiResponse>(_apiroutine.PostAPI("UserAdmin", $"AddCompanyLink?companyId={compId}&foruserid={forUserId}&UsertypeId={RoleId}&accessUserid={accessId}", ""));
                if (apiResponse.isSuccess)
                {
                    _result = Json(new { data = apiResponse, isSuccess = true });
                }
                else { 
                    _result = Json(new {data = apiResponse, isSuccess = false});
                }
            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in AddLinkCompany : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            }
            return _result;

        }
        public IActionResult CreateNewUser()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext,"1")) return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            CompanyRolesList companyRolesList = new CompanyRolesList();
            ViewBag.Module = "User Administration";
            ViewBag.SubTitle = "User Administration";
            ViewBag.SubTitleUrl = "UsersList";
            try
            {
                int accessId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                var apiRespCompany = JsonConvert.DeserializeObject<ApiResponse>(_apiroutine.PostAPI("UserAdmin", $"GetAllCompanyList?userId={accessId}", ""));
                var apiRespRoles = JsonConvert.DeserializeObject<List<LES_USERTYPE>>(_apiroutine.GetAPI("UserAdmin", "GetAllRoles", null));

                if (apiRespCompany.isSuccess)
                {
                    companyRolesList.AllCompanyList = JsonConvert.DeserializeObject<List<CompaniesList>>(apiRespCompany.Data.ToString());
                }
                if (apiRespRoles.Count > 0)
                {
                    companyRolesList.AllRoles = apiRespRoles;
                }

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in CreateNewUser : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            }
            return View(companyRolesList);
        }
        #endregion
        #region Company
        public IActionResult CompanyList()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                string Print_Key = "CompanyList_PRINT";
                ViewBag.Module = "User Administration";
                ViewBag.SubTitle = "User Administration";
                ViewBag.SubTitleUrl = "CompanyList";
                ViewBag.EnableFilter = false;



                ViewData["Print_Key"] = Print_Key;
                return View();

            }
            catch (Exception ex)
            {

                LeSDM.AddLog("Exception in CompanyList - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });

            }
        }
        public IActionResult GetCompanyList(int ValueRD)
        {
            JsonResult _result = Json(new object[] { new() });
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                int _rdValue = (HttpContext.Session.GetInt32("RANDOM") != null) ? convert.ToInt(HttpContext.Session.GetInt32("RANDOM")) : 0;
                if (_rdValue <= 0)
                {
                    HttpContext.Session.SetString("COMPANY_DATA", "");
                    HttpContext.Session.SetInt32("RANDOM", ValueRD);
                }
                else
                {
                    if (ValueRD != _rdValue)
                    {
                        HttpContext.Session.SetString("COMPANY_DATA", "");
                        HttpContext.Session.SetInt32("RANDOM", ValueRD);
                    }
                }

                var strList = HttpContext.Session.GetString("COMPANY_DATA");
                List<CompanyList> list = new List<CompanyList>();


                if (!string.IsNullOrEmpty(strList))
                {
                    list = JsonConvert.DeserializeObject<List<CompanyList>>(strList);
                }
                else
                {

                    IDictionary<string, string> queryParam = new Dictionary<string, string>();
                    int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    queryParam.Add("Userid", userId.ToString());

                    string jsonData = _apiroutine.PostAPI("UserAdmin", "GetAllCompanyList", null, null, queryParam);
                    var data = JsonConvert.DeserializeObject<ApiResponse>(jsonData);
                    list = JsonConvert.DeserializeObject<List<CompanyList>>(data.Data.ToString());
                    HttpContext.Session.SetString("COMPANY_DATA", JsonConvert.SerializeObject(list));

                }

                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                int _recordTotal = list?.Count ?? 0;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : list.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 0;

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumnDir == "asc")
                    {
                        switch (sortColumn)
                        {
                            case "companyid": list = list.OrderBy(x => x.companyid).ToList(); break;
                            case "company_code": list = list.OrderBy(x => x.company_code).ToList(); break;
                            case "company_description": list = list.OrderBy(x => x.company_description).ToList(); break;
                            case "addressId": list = list.OrderBy(x => x.addressId).ToList(); break;
                            case "addr_type": list = list.OrderBy(x => x.addr_type).ToList(); break;
                            case "country": list = list.OrderBy(x => x.Country).ToList(); break;
                            default: list = list.OrderBy(x => x.companyid).ToList(); break;
                        }
                    }
                    else
                    {
                        switch (sortColumn)
                        {
                            case "companyid": list = list.OrderByDescending(x => x.companyid).ToList(); break;
                            case "company_code": list = list.OrderByDescending(x => x.company_code).ToList(); break;
                            case "company_description": list = list.OrderByDescending(x => x.company_description).ToList(); break;
                            case "addressId": list = list.OrderByDescending(x => x.addressId).ToList(); break;
                            case "addr_type": list = list.OrderByDescending(x => x.addr_type).ToList(); break;
                            case "country": list = list.OrderByDescending(x => x.Country).ToList(); break;
                            default: list = list.OrderByDescending(x => x.companyid).ToList(); break;
                        }
                    }
                }
                int _recordsFiltered = list.Count;
                list = list.Skip(skip).Take(pageSize).ToList();

                _result = Json(new { draw = draw, recordsFiltered = _recordsFiltered, recordsTotal = _recordTotal, data = list });

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetCompanyList : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
            }
            return _result;
        }
        public IActionResult CompanyDetails(string companyId)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));

                int companyid = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(companyId);
                if (companyid == 0)
                {
                    return Redirect("/Home/Error?statuscode=400");
                }

                IDictionary<string, string> queryParam = new Dictionary<string, string> { { "companyid", companyid.ToString() } };

                string data = _apiroutine.PostAPI("Logistic", "GetCompanyDetails", null, null, queryParam);
                if (!string.IsNullOrEmpty(data))
                {
                    var companyInfo = JsonConvert.DeserializeObject<Companyinfodata>(data);
                    HttpContext.Session.SetString("COMPANYID", companyid.ToString());
                    ViewBag.Module = "User Administration";
                    ViewBag.SubTitle = "User Administration";
                    ViewBag.SubTitleUrl = "CompanyList";
                    ViewBag.CompanyInfo = JsonConvert.SerializeObject(companyInfo);
                    ViewBag.EnableFilter = true;
                    ViewBag.AddressId = companyInfo.AddressId;
                    ViewBag.userId = userId;

                    return View();
                }
                else
                {
                    LeSDM.AddLog("Exception in CompanyDetails - No response from API");
                    return RedirectToAction("Error", "Home", new { statuscode = 404 });
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in CompanyDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
        }
        [HttpGet]
        public IActionResult GetAllSMAddress()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            List<Logistic_Management_Lib.Model.SM_ADDRESS> model = new List<Logistic_Management_Lib.Model.SM_ADDRESS>();
            JsonResult result = Json(new { Data = model });

            try
            {
                IDictionary<string, string> queryParam = new Dictionary<string, string> { };
                int accessuserid = Convert.ToInt32( HttpContext.Session.GetString("UserID"));
                queryParam.Add("accessuserid", accessuserid.ToString());

                var response = _apiroutine.GetAPI("UserAdmin", "GetAllSMAddress", null, queryParam);
                if (!string.IsNullOrEmpty(response))
                {
                    var responseObject = JsonConvert.DeserializeObject<SMAddressResponse>(response);
                    if (responseObject != null && responseObject.Data != null)
                    {
                        model = responseObject.Data;
                        result = Json(new { Data = model });
                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetAllSMAddress - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return result;
        }

        [HttpGet]
        public IActionResult GetSMaddressDetails(string addressid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            List<Logistic_Management_Lib.Model.SM_ADDRESS> model = new List<Logistic_Management_Lib.Model.SM_ADDRESS>();
            JsonResult result = Json(new { Data = model });
            try
            {

                IDictionary<string, string> queryParam = new Dictionary<string, string> { };
                int accessuserid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                queryParam.Add("accessuserid", accessuserid.ToString());
                queryParam.Add("addressid", addressid);
               

                
                string jsonData = _apiroutine.GetAPI("UserAdmin", "GetSMaddressDetails", null, queryParam);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var data = JsonConvert.DeserializeObject<ApiResponse>(jsonData);
                    var list = JsonConvert.DeserializeObject<SM_ADDRESS>(data.Data.ToString());

                    return Json(new { data = list });
                }
                else
                {
                    return StatusCode(404, "No data found.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                LeSDM.AddLog("Exception in GetSMaddressDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public IActionResult UpdateCompanyDetails(string data)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            CompanyUpdateModal model = JsonConvert.DeserializeObject<CompanyUpdateModal>(data);
            try
            {
             

                    if (model == null)
                {
                    return Json(new { success = false, message = "Company details are missing." });
                }
                var request = new
                {
                    companydetails = new
                    {
                        companyid = model.companydetails.companyid,
                        companydescription = model.companydetails.companydescription,
                        companycode = model.companydetails.companycode,
                        addressid = model.companydetails.addressid,
                        mainlogo = model.companydetails.mainlogo,
                        mainlogofilename = model.companydetails.mainlogofilename,
                        printlogo = model.companydetails.printlogo,
                        printlogofilename = model.companydetails.printlogofilename,
                        faviconlogo = model.companydetails.faviconlogo,
                        faviconlogofilename = model.companydetails.faviconlogofilename
                    },
                    accessuserid = model.accessuserid
                };

                string jsonString = JsonConvert.SerializeObject(request);
                string jsonData = _apiroutine.PostAPI("UserAdmin", "UpdateCompanyDetails", jsonString);

                var response = JsonConvert.DeserializeObject<ApiResponse>(jsonData);

                if (response != null && response.isSuccess)
                {
                    return Json(new { success = true, message = "Company details updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "Failed to update company details." });
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in UpdateCompanyDetails - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "An error occurred while updating company details." });
            }
        }
        public IActionResult CreateNeWCompany(string data)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            LES_COMPANY_UPDATE model = JsonConvert.DeserializeObject<LES_COMPANY_UPDATE>(data);
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                if (model == null)
                {
                    return Json(new { success = false, message = "Company details are missing." });
                }
                var request = new
                {

                    companyid = model.companyid,
                    companydescription = model.companydescription,
                    companycode = model.companycode,
                    addressid = model.addressid,
                    mainlogo = model.mainlogo,
                    mainlogofilename = model.mainlogofilename,
                    printlogo = model.printlogo,
                    printlogofilename = model.printlogofilename,
                    faviconlogo = model.faviconlogo,
                    faviconlogofilename = model.faviconlogofilename


                };
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                queryParam.Add("accessuserid", userId.ToString());
                string jsonString = JsonConvert.SerializeObject(request);
                string jsonData = _apiroutine.PostAPI("UserAdmin", "CreateNewCompany", jsonString, null, queryParam);

                var response = JsonConvert.DeserializeObject<ApiResponse>(jsonData);

                if (response != null && response.isSuccess)
                {
                    return Json(new { success = true, message = "Company details added successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "Failed to add company details." });
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in UpdateCompanyDetails - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "An error occurred while updating company details." });
            }

        }
        public class SMAddressResponse
        {
            public List<Logistic_Management_Lib.Model.SM_ADDRESS> Data { get; set; }
            public string Status { get; set; }
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }

        public class CompanyUpdateRequest
        {
            public CompanyUpdateModal CompanyDetails { get; set; }
            public int AccessUserId { get; set; }
        }
        #endregion
        #region User Roles & Permissions
        public IActionResult UserRoles()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            ViewBag.Module = "User Administration";
            ViewBag.SubTitle = "User Administration";
            ViewBag.SubTitleUrl = "UserRoles";
            ViewBag.EnableFilter = false;
            UserAccessLevelsModal modal = new UserAccessLevelsModal();
            try
            {
                var AllRoles = JsonConvert.DeserializeObject<List<LES_USERTYPE>>(_apiroutine.GetAPI("UserAdmin", "GetAllRoles", null));
                modal.UserTypes = AllRoles;
                return View(modal);
            }
            catch(Exception ex)
            {
                LeSDM.AddLog("Exception in UserRoles - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
            
        }
        public IActionResult FilterUserTypeModuleAccessData(int UserTypeId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            if (UserTypeId > 0)
            {
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                queryParam.Add("accessuserid", userId.ToString());
                queryParam.Add("usertypeid", UserTypeId.ToString());
                var res = _apiroutine.GetAPI("UserAdmin", "GetUserTypeModuleAccess", null, queryParam);
                var response = JsonConvert.DeserializeObject<ApiResponse>(res);
                if (res!=""||response.isSuccess)
                {
                    var Alldata = JsonConvert.DeserializeObject<List<V_USERTYPE_MODULE_ACCESS>>(response.Data.ToString());
                    Alldata = Alldata.Where(x => x.ACCESS_LEVEL != null).ToList();
                    var _result = Json(new { draw = 1, recordsFiltered = Alldata.Count(), recordsTotal = Alldata.Count(), data = Alldata });
                    return _result;
                }
                else
                {
                    var _res = Json(new { draw = 1, recordsFiltered = 0, recordsTotal = 0, data = "" });
                    return _res;
                }
                
            }
            else
            {
                var _result = Json(new { draw = 1, recordsFiltered = 0, recordsTotal = 0, data = "" });
                return _result;
            }

            
            
        }
        public IActionResult FilterUserTypeModuleAccessDataEdit(int UserTypeId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;
            if (UserTypeId > 0)
            {
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                queryParam.Add("accessuserid", userId.ToString());
                queryParam.Add("usertypeid", UserTypeId.ToString());
                var res = _apiroutine.GetAPI("UserAdmin", "GetUserTypeModuleAccess", null, queryParam);
                var response = JsonConvert.DeserializeObject<ApiResponse>(res);
                if (res != "" || response.isSuccess)
                {
                    var Alldata = JsonConvert.DeserializeObject<List<V_USERTYPE_MODULE_ACCESS>>(response.Data.ToString());
                    //Alldata = Alldata.Where(x => x.ACCESS_LEVEL != null).ToList();
                    var _result = Json(new { draw = 1, recordsFiltered = Alldata.Count(), recordsTotal = Alldata.Count(), data = Alldata });
                    return _result;
                }
                else
                {
                    var _res = Json(new { draw = 1, recordsFiltered = 0, recordsTotal = 0, data = "" });
                    return _res;
                }

            }
            else
            {
                var _result = Json(new { draw = 1, recordsFiltered = 0, recordsTotal = 0, data = "" });
                return _result;
            }



        }
        public IActionResult SaveUserTypeDetails(string options, int usertypeid, string usertypename)
        {

            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "7", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                if (options != null && options.Length > 0 && usertypeid > 0)
                {
                    IDictionary<string, string> queryParam = new Dictionary<string, string>();
                    int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    queryParam.Add("accessuserid", userId.ToString());
                    queryParam.Add("usertypeid", usertypeid.ToString());
                    queryParam.Add("Options", options);
                    var res = _apiroutine.PostAPI("UserAdmin", "UpdateUserTypeAccessLevelDetails", null, null, queryParam);
                    var response = JsonConvert.DeserializeObject<ApiResponse>(res);
                    if (res != "" && response.isSuccess)
                    {
                        var result  =new {result = true,msg= "User access levels saved successfully!"};
                        return Json(result);

                    }
                    else
                    {
                        var result = new { result = false, msg = response.Message };
                        return Json(result);

                    }
                }
                else
                {
                    var result = new { result = false, msg ="Something went wrong at our side!" };
                    return Json(result);
                }
                
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Error on saving User type Details : " + ex.GetBaseException());
                var sData = new { result = false, msg = ex.GetBaseException() };
                CommonRoutine.SetAudit("UserAdministration", "Error", "", "Unable to save " + usertypename, "");

                return Json(sData);
            }
        }
        #endregion User Roles & Permissions
    }
    public class USERTYPE
    {
        public string moduleId { get; set; }
        public string SelectedValue { get; set; }
    }
}
