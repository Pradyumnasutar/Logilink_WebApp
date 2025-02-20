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
using LeS_LogiLink_WebApp.Controllers;

namespace LeS_LogiLink_WebApp.Controllers
{
    public class GuestEPODController : Controller
    {
        private IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;
        public string cErrorMessage = "";
        public GuestEPODController(IConfiguration _configuration, ApiCallRoutine routine)
        {
            Configuration = _configuration;
            _apiroutine = routine;
        }


        public void SetModuleAccess(List<V_APPLICATION_MODULE_ACCESS> AccessLevels)
        {

            Dictionary<int?, Dictionary<string, int>> CompaniesAccess = new Dictionary<int?, Dictionary<string, int>>();
            var CompaniesGrp = AccessLevels.GroupBy(x => x.CompanyId).ToList();
            for (int i = 0; i < CompaniesGrp.Count; i++)
            {
                Dictionary<string, int> CompanyAccess = new Dictionary<string, int>();
                var EachGroup = CompaniesGrp[i];
                var GroupedGrp = EachGroup.Select(x => new { x.ModuleId, x.Access_Level }).ToList();
                for (int x = 0; x < GroupedGrp.Count; x++)
                {
                    var EachEnt = GroupedGrp[x];

                    CompanyAccess.Add(EachEnt.ModuleId.ToString(), EachEnt.Access_Level);
                }
                CompaniesAccess.Add(EachGroup.Select(x => x.CompanyId).FirstOrDefault(), CompanyAccess);
            }
            HttpContext.Session.SetString("UserAccessRightsEpod", JsonConvert.SerializeObject(CompaniesAccess));

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

        public void LoadProjectVersion()
        {

            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                HttpContext.Session.SetString("Projectversion", fvi.FileVersion.ToString());
            }
            catch (Exception ex)
            {
                LeSDataMain.LeSDM.AddLog("Error on Get_Version : " + ex.Message);
            }

        }

        #region EPOD
        public IActionResult EpodLogin()
        {

            LoadProjectVersion();
            if (HttpContext.Session.GetString("EPOD_SHIPMENTID") != null)
            {
                int SHIPMENTID = convert.ToInt(HttpContext.Session.GetString("EPOD_SHIPMENTID"));

                string _errorMessage = convert.ToString(HttpContext.Session.GetString("EPOD_LOGIN_FAIL_MESSAGE"));
                if (_errorMessage.Length > 0)
                {
                    LeSDM.AddLog("_errorMessage - " + _errorMessage);
                }
                string _errorNotice = convert.ToString(HttpContext.Session.GetString("EPOD_LOGIN_FAIL_NOTICE"));
                if (_errorNotice.Length > 0)
                {
                    LeSDM.AddLog("_errorNotice - " + _errorNotice);
                }
                ViewBag.Notice = _errorNotice;
                ViewBag.Message = _errorMessage;
                HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "");
                HttpContext.Session.SetString("EPOD_LOGIN_FAIL_NOTICE", "");
            }

            return View();
        }
        public IActionResult SaveEpodRemark(int shipmentid, string remarks)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var result = new { result = false, msg = "" };
            try
            {
                if (remarks != null && shipmentid > 0 && remarks.Length > 0)
                {
                    var epoduser = convert.ToInt(HttpContext.Session.GetString("EPOD_UserID"));
                    if (epoduser < 1)
                    {
                        epoduser = convert.ToInt(HttpContext.Session.GetString("UserID"));
                    }
                    EPOD_ShipmentDetails Modal = new EPOD_ShipmentDetails();
                    Modal.shipmentid = shipmentid;
                    Modal.EPOD_Shipment_Remarks = remarks;
                    Modal.updateBy = epoduser;
                    string Body = JsonConvert.SerializeObject(Modal);
                    var response = _apiroutine.PostAPI("Logistic", "UpdateEPODShipmentDetails", Body);
                    if (response != null)
                    {
                        var obj = JsonConvert.DeserializeObject<ApiResponse>(response);
                        if (obj.isSuccess)
                        {
                            result = new { result = true, msg = obj.Message };
                        }
                        else
                        {
                            result = new { result = false, msg = obj.Message };
                        }
                    }
                    else
                    {
                        result = new { result = false, msg = "unable to save ePOD details! please contact support." };
                    }
                }
                else
                {
                    result = new { result = false, msg = "unable to save ePOD details! please contact support." };
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SaveEpodRemark - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                result = new { result = false, msg = "Something went wrong! please contact support." };
            }

            return Json(result);

        }
        public IActionResult EpodUrlLogin(string shipmentdata)
        {

            LoadProjectVersion();
            if (shipmentdata != null)
            {

                return View();
            }
            else
            {

                ViewBag.EpodLogout = "Unable to open URL Link";
                ViewBag.Message = "";
                return View("EpodLogout");
            }
        }

        public IActionResult EpodLogout()
        {

            try
            {
                cErrorMessage = "";
                HttpContext.Session.Remove("EPOD_LOGIN_FAIL_MESSAGE");
                HttpContext.Session.Remove("EPOD_UserName");
                HttpContext.Session.Remove("EPOD_UserID");
                HttpContext.Session.Clear();
                AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme);

            }
            catch (Exception ex)
            {
                ViewBag.Message = "Please check the error - " + ex.Message;
                HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Please check the error - " + ex.Message);
            }

            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult EPODLoginByCrew(int shipmentid, string emailId, int imono, string crewName, string designation, string latitude, string longitude)
        {
            try
            {
                HttpContext.Session.SetString("EPOD_SHIPMENTID", "0");

                if (latitude != "" && longitude != "")
                {
                    if (emailId != "")
                    {
                        if (imono > 0)
                        {
                            if (crewName != "")
                            {
                                if (designation != "")
                                {
                                    _apiroutine.RefreshTokenAsync();
                                    VesselAuthenticationParameter _vesselData = new VesselAuthenticationParameter();
                                    _vesselData.shipmentid = convert.ToInt(shipmentid);
                                    _vesselData.useremailid = convert.ToString(emailId);
                                    _vesselData.vesselimo = convert.ToString(imono);
                                    _vesselData.crewname = convert.ToString(crewName);
                                    _vesselData.role = convert.ToString(designation);
                                    _vesselData.latitude = convert.ToString(latitude);
                                    _vesselData.longitude = convert.ToString(longitude);
                                    string body = JsonConvert.SerializeObject(_vesselData);
                                    string Data = _apiroutine.PostAPI("Logistic", "GetVesselAuthenticationCrew", body);
                                    VesselAuthenticationModalViews vesselEpodData = JsonConvert.DeserializeObject<VesselAuthenticationModalViews>(Data);
                                    if (vesselEpodData != null)
                                    {
                                        var DefaultCompany = convert.ToInt(vesselEpodData.les_company_details.CompanyId);
                                        if (DefaultCompany != 0) //if no default company id found then showing first company
                                        {
                                            SetModuleAccess(vesselEpodData.list_Application_Module_Access);
                                            //var CompanyData = User.list_company_details.Where(x => x.CompanyId == DefaultCompany).FirstOrDefault();

                                            HttpContext.Session.SetString("EPOD_CompanyId", convert.ToString(vesselEpodData.les_company_details.CompanyId));
                                            HttpContext.Session.SetString("EPOD_logoflag", convert.ToString(vesselEpodData.les_company_details.poweredby_flag));
                                            HttpContext.Session.SetString("EPOD_Company", vesselEpodData.les_company_details.Company_Description);
                                            HttpContext.Session.SetString("EPOD_CompanyCode", vesselEpodData.les_company_details.Company_Code);
                                            HttpContext.Session.SetString("EPOD_UserTypeDesc", vesselEpodData.les_company_details.Usertypedescr);
                                            HttpContext.Session.SetString("EPOD_SHIPMENTID", convert.ToString(vesselEpodData.shipmentid));
                                            HttpContext.Session.SetString("EPOD_UserID", convert.ToString(vesselEpodData.userid).Trim());
                                            //HttpContext.Session.SetString("UserTypeID", convert.ToString(User.uset));
                                            HttpContext.Session.SetString("EPOD_UserEmail", vesselEpodData.useremailid);
                                            HttpContext.Session.SetString("EPOD_UserName", vesselEpodData.username);
                                            HttpContext.Session.SetString("EPOD_VesselAuthenticationModalViews", body);
                                            HttpContext.Session.SetString("EPOD_USER_INFO", Data);
                                            LeSDM.AddLog("User Logged in : " + vesselEpodData.username + " for Shipment Order no  (" + vesselEpodData.shipmentInfo.order_no + ") for the company : " + vesselEpodData.les_company_details.Company_Description);
                                            if (SaveModuleStatuses(3))//1==Outbound shipment 2==delivery orders 3==EPOD
                                            {
                                                var data1 = new { result = true };

                                                return Json(data1);
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
                                        ViewBag.Message = "Unable to login please contact support!";
                                        HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Unable to login please contact support!");
                                    }
                                }
                                else
                                {
                                    ViewBag.Message = "Designation/Role not found";
                                    HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Designation/Role not found");
                                }
                            }
                            else
                            {
                                ViewBag.Message = "IMO no. not found";
                                HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "IMO no. not found");
                            }
                        }
                        else
                        {
                            ViewBag.Message = "crewName not found";
                            HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "crewName not found");
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Email id not found";
                        HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Email id not found");
                    }
                }
                else
                {
                    ViewBag.Message = "Unable to get current location";
                    HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Unable to get current location");
                }
            }
            catch (Exception ex)
            {

                ViewBag.Message = "Vessel Crew Login Failed! Please contact support";
                HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Login Failed! Please contact support");
                LeSDataMain.LeSDM.AddLog("Error in Content - " + ex.Message);
                CommonRoutine.SetAudit("Login", "Error", "", "Vessel Crew Login Failed! Please contact support", "");
            }
            var data = new { result = false, msg = ViewBag.Message };
            return Json(data);
        }

        public IActionResult EPODLoginByURl(string shipmentdata, string latitude, string longitude)
        {
            try
            {
                if (latitude != "" && longitude != "")
                {
                    if (shipmentdata != "")
                    {
                        _apiroutine.RefreshTokenAsync();
                        UrlData _urlData = new UrlData();
                        _urlData.encryptData = convert.ToString(HttpUtility.UrlEncode(shipmentdata));
                        _urlData.latitude = convert.ToString(latitude);
                        _urlData.longitude = convert.ToString(longitude);
                        string body = JsonConvert.SerializeObject(_urlData);
                        string str = HttpUtility.UrlEncode(shipmentdata);
                        string Data = _apiroutine.PostAPI("Logistic", "EpodLoginByVesselAuthenticationCrewEncryptData", body);
                        VesselAuthenticationUrlModalViews vesselEpodData = JsonConvert.DeserializeObject<VesselAuthenticationUrlModalViews>(Data);
                        if (vesselEpodData != null && vesselEpodData.value != null)
                        {
                            var DefaultCompany = convert.ToInt(vesselEpodData.value.les_company_details.CompanyId);
                            if (DefaultCompany != 0) //if no default company id found then showing first company
                            {
                                Data = JsonConvert.SerializeObject(vesselEpodData.value);
                                SetModuleAccess(vesselEpodData.value.list_Application_Module_Access);
                                //var CompanyData = User.list_company_details.Where(x => x.CompanyId == DefaultCompany).FirstOrDefault();

                                HttpContext.Session.SetString("EPOD_CompanyId", convert.ToString(vesselEpodData.value.les_company_details.CompanyId));
                                HttpContext.Session.SetString("EPOD_logoflag", convert.ToString(vesselEpodData.value.les_company_details.poweredby_flag));
                                HttpContext.Session.SetString("EPOD_Company", vesselEpodData.value.les_company_details.Company_Description);
                                HttpContext.Session.SetString("EPOD_CompanyCode", vesselEpodData.value.les_company_details.Company_Code);
                                HttpContext.Session.SetString("EPOD_UserTypeDesc", vesselEpodData.value.les_company_details.Usertypedescr);
                                HttpContext.Session.SetString("EPOD_SHIPMENTID", convert.ToString(vesselEpodData.value.shipmentid));
                                HttpContext.Session.SetString("EPOD_UserID", convert.ToString(vesselEpodData.value.userid).Trim());
                                //HttpContext.Session.SetString("UserTypeID", convert.ToString(User.uset));
                                HttpContext.Session.SetString("EPOD_UserEmail", vesselEpodData.value.useremailid);
                                HttpContext.Session.SetString("EPOD_UserName", vesselEpodData.value.username);
                                HttpContext.Session.SetString("EPOD_VesselAuthenticationModalViews", Data);
                                LeSDM.AddLog("User Logged in : " + vesselEpodData.value.username + " for Shipment Order no  (" + vesselEpodData.value.shipmentInfo.order_no + ") for the company : " + vesselEpodData.value.les_company_details.Company_Description);
                                if (SaveModuleStatuses(3))//1==Outbound shipment 2==delivery orders 3==EPOD
                                {
                                    var data1 = new { result = true, data = vesselEpodData };
                                    return Json(data1);
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
                            ViewBag.Message = "Unable to login please contact support!";
                            HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Unable to login please contact support!");
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Email id not found";
                        HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Email id not found");
                    }
                }
                else
                {
                    ViewBag.Message = "Unable to get current location";
                    HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Unable to get current location");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Vessel Crew Login Failed! Please contact support";
                HttpContext.Session.SetString("EPOD_LOGIN_FAIL_MESSAGE", "Login Failed! Please contact support");
                LeSDataMain.LeSDM.AddLog("Error in Content - " + ex.Message);
                CommonRoutine.SetAudit("Login", "Error", "", "Vessel Crew Login Failed! Please contact support", "");
            }
            var data = new { result = false, msg = ViewBag.Message };
            return Json(data);
        }

        public IActionResult EpodShipmentDetails()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            VesselAuthenticationModalViews vesselEpodData = new();
            try
            {
                ViewBag.SubTitle = "ePOD Shipment Details";
                if (HttpContext.Session.GetString("EPOD_SHIPMENTID") != null)
                {
                    var AllUserData = HttpContext.Session.GetString("EPOD_VesselAuthenticationModalViews");
                    if (AllUserData != null)
                    {
                        string Data = _apiroutine.PostAPI("Logistic", "GetVesselAuthenticationCrew", AllUserData);
                        vesselEpodData = JsonConvert.DeserializeObject<VesselAuthenticationModalViews>(Data);
                        return View(vesselEpodData);
                    }
                    else
                    {
                        var Data = HttpContext.Session.GetString("EPOD_USER_INFO");
                        var latitude = HttpContext.Session.GetString("EPOD_USER_LATITUDE");
                        var longitude = HttpContext.Session.GetString("EPOD_USER_LONGITUDE");
                        vesselEpodData = JsonConvert.DeserializeObject<VesselAuthenticationModalViews>(Data);
                        VesselAuthenticationParameter _vesselData = new VesselAuthenticationParameter();
                        _vesselData.shipmentid = convert.ToInt(vesselEpodData.shipmentid);
                        _vesselData.useremailid = convert.ToString(vesselEpodData.useremailid);
                        _vesselData.vesselimo = convert.ToString(vesselEpodData.vesselimo);
                        _vesselData.crewname = convert.ToString(vesselEpodData.crewname);
                        _vesselData.role = convert.ToString(vesselEpodData.role);
                        _vesselData.latitude = latitude;
                        _vesselData.longitude = longitude;
                        AllUserData = JsonConvert.SerializeObject(_vesselData);
                        string Data1 = _apiroutine.PostAPI("Logistic", "GetVesselAuthenticationCrew", AllUserData);
                        vesselEpodData = JsonConvert.DeserializeObject<VesselAuthenticationModalViews>(Data1);
                        return View(vesselEpodData);
                    }


                }
                else
                {
                    LeSDM.AddLog("EpodShipmentDetails process stopped ");
                    //return View(vesselEpodData);
                    //LeSDM.AddLog("Exception in Anchorage - " + ex.Message);
                    //LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                    return RedirectToAction("Error", "Home", new { statuscode = 404 });
                }

            }
            catch (Exception ex)
            {

                LeSDM.AddLog("Exception in EpodShipmentDetails " + ex.Message);
                ViewBag.Message = ex.Message;
                LeSDM.AddLog("EpodShipmentDetails process stopped ");
                return View();
            }
        }
        public IActionResult GetShipmentLinkedDeliveryOrders(int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<V_SHIPMENT_DELIVERY_ORDERS> Modal = new List<V_SHIPMENT_DELIVERY_ORDERS>();
            var result = new { result = false, data = Modal, msg = "Unable to get delivery orders!" };
            try
            {
                if (shipmentid > 0)
                {
                    IDictionary<string, string> queryParam = new Dictionary<string, string>();

                    queryParam.Add("ShipmentId", shipmentid.ToString());
                    var jsData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, queryParam);
                    if (jsData != null)
                    {
                        Modal = JsonConvert.DeserializeObject<List<V_SHIPMENT_DELIVERY_ORDERS>>(jsData);
                        result = new { result = true, data = Modal, msg = "" };
                    }
                    else
                    {
                        result = new { result = false, data = Modal, msg = "Unable to get delivery orders!" };
                    }
                }
                else
                {
                    result = new { result = false, data = Modal, msg = "Unable to get shipment details!" };
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetShipmentLinkedDeliveryOrders - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);

            }
            return Json(result);

        }
        public IActionResult GetDeliveryOrderLinesByDeliveryOrder(int deliveryorderid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            EPODDeliveryOrderInfoLinesModal modal = new();
            var _result = new { result = false, data = modal, count = 0 };
            try
            {
                if (deliveryorderid > 0)
                {
                    IDictionary<string, string> queryParam = new Dictionary<string, string>();

                    queryParam.Add("deliveryOrderId", deliveryorderid.ToString());
                    var Data = _apiroutine.PostAPI("Logistic", "GetEPODDDeliveryOrderInfoLinesByDeliveryOrderId", null, null, queryParam);
                    if (Data != "")
                    {
                        var Rendered = JsonConvert.DeserializeObject<EPODDeliveryOrderInfoLinesModal>(Data);
                        _result = new { result = true, data = Rendered, count = Rendered._DOLines.Count };

                    }
                }
            }
            catch (Exception ex)
            {
                _result = new { result = false, data = modal, count = modal._DOLines.Count };
                LeSDM.AddLog("Exception in GetShipmentLinkedDeliveryOrders - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(_result);
        }
        public IActionResult GetDeliveryOrderListforEpodTabs(int ShipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            try
            {

                List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> list;
                int companyid = convert.ToInt(HttpContext.Session.GetString("EPOD_Company"));
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", ShipmentId.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, QueryParam);
                list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS>>(jsonData);

                _result = Json(new { data = list });


            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetDeliveryOrderListforEpodTabs : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
            }
            return Json(_result);
        }
        public IActionResult GetDeliveryOrderListforEpod(int ShipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            try
            {

                List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> list;
                int companyid = convert.ToInt(HttpContext.Session.GetString("EPOD_Company"));
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", ShipmentId.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, QueryParam);
                list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS>>(jsonData);

                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                var searchValue = convert.ToString(Request.Form["search[value]"]);
                int _recordTotal = list.Count;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : list.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 1;
                if (!string.IsNullOrEmpty(searchValue))
                {
                    list = list.Where(m => (convert.ToString(convert.ToString(m.order_no).ToUpper() + " " + m.packaging_unit_no + " " + convert.ToString(m.delivery_order_no).ToUpper() + " " + convert.ToString(m.dept_code).ToUpper() + " " + convert.ToString(m.pono).ToUpper()
                      + " " + convert.ToString(m.jobno).ToUpper() + " " + convert.ToString(m.do_status_desc).ToUpper() + " " + convert.ToString(m.sales_person_code)
                         ).IndexOf(searchValue.ToUpper()) > -1)).ToList();

                }

                int _recordsFiltered = list.Count;
                list = list.Skip(skip).Take(pageSize).ToList();

                _result = Json(new { draw = draw, recordsFiltered = _recordsFiltered, recordsTotal = _recordTotal, data = list });

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetDeliveryOrderListforEpod : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
            }
            return _result;
        }
        public IActionResult GetgoodreturnReasonlist()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var Model = new List<Mast_Goods_Return_Reasons>();
            var result = new { result = false, data = Model, msg = "" };
            try
            {
                var Data = _apiroutine.PostAPI("Logistic", "GetGoodsReturnReasonsList", null, null, null);
                if (Data != null && Data != "")
                {
                    Model = JsonConvert.DeserializeObject<List<Mast_Goods_Return_Reasons>>(Data);
                    result = new { result = true, data = Model, msg = "" };
                }
                else
                {
                    result = new { result = false, data = Model, msg = "" };
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetgoodreturnReasonlist : " + ex.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(result);
        }
        public IActionResult SaveDeliveryOrderLines(int shipmentid, int deliveyrorderid, string shipremark, string dolines)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var result = new { result = false, msg = "" };
            try
            {


                var epoduser = convert.ToInt(HttpContext.Session.GetString("EPOD_UserID"));
                if (epoduser < 1)
                {
                    epoduser = convert.ToInt(HttpContext.Session.GetString("UserID"));
                }
                if (shipmentid > 0) { }
                else
                {
                    result = new { result = false, msg = "unable to get shipment details !" };
                }
                if (deliveyrorderid > 0) { }
                else
                {
                    result = new { result = false, msg = "unable to get delivery order details !" };
                }
                if (string.IsNullOrEmpty(dolines) | string.IsNullOrWhiteSpace(dolines))
                {
                    result = new { result = false, msg = "unable to get delivery order lines !" };
                }
                var rendered = JsonConvert.DeserializeObject<Dictionary<string, DeliveryOrderLine>>(dolines);
                var Modal = new Update_EPOD_DeliveryOrderItems();
                var ListModal = new List<EPODItemListField>();
                for (int i = 0; i < rendered.Count; i++)
                {
                    var item = rendered.ElementAt(i);


                    EPODItemListField eachLine = new EPODItemListField();
                    eachLine.DeliveryOrderLinesId = convert.ToInt(item.Key);
                    eachLine.QuantityInvoiced = convert.ToFloat(item.Value.receivedQty);
                    eachLine.Epod_line_remarks = item.Value.remark;
                    eachLine.GrnReasonId = convert.ToInt(item.Value.returnCode);
                    eachLine.Quantity = convert.ToFloat(item.Value.deliveredQty);
                    ListModal.Add(eachLine);
                }
                Modal.GoodsReturnRemarks = shipremark;
                Modal.ShipmentId = convert.ToInt(shipmentid);
                Modal.DeliveryOrderId = deliveyrorderid;
                Modal.UpdatedDate = DateTime.Now;
                Modal.UpdatedBy = epoduser;
                Modal.EPOD_ItemDetails = ListModal;
                string body = JsonConvert.SerializeObject(Modal);
                var Response = _apiroutine.PostAPI("Logistic", "UpdateEpodDeliveryOrderLines", body);
                if (Response != null)
                {
                    var jsRes = JsonConvert.DeserializeObject<ApiResponse>(Response);
                    if (jsRes.isSuccess)
                    {
                        result = new { result = true, msg = jsRes.Message };
                    }
                    else
                    {
                        result = new { result = false, msg = jsRes.Message };
                    }
                }
                else
                {
                    result = new { result = true, msg = "Something went wrong at our side, Please contact support for assistance!" };
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetgoodreturnReasonlist : " + ex.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(result);
        }
        public IActionResult ConfirmIntialReceiptshipment()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var result = new { result = false, msg = "" };
            try
            {
                var data = HttpContext.Session.GetString("EPOD_VesselAuthenticationModalViews");
                if (data == null)
                {
                    data = HttpContext.Session.GetString("EPOD_USER_INFO");
                }

                var UserDetails = JsonConvert.DeserializeObject<VesselAuthenticationParameter>(data);

                var Modal = new UpdateEpodStatusModal();
                Modal.userid = convert.ToInt(UserDetails.userid);
                Modal.emailid = UserDetails.useremailid;
                Modal.latitude = UserDetails.latitude;
                Modal.longitude = UserDetails.longitude;
                Modal.shipmentid = UserDetails.shipmentid;
                Modal.role = UserDetails.role;
                Modal.vesselimo = UserDetails.vesselimo;
                Modal.username = UserDetails.crewname;
                string body = JsonConvert.SerializeObject(Modal);
                var Response = _apiroutine.PostAPI("Logistic", "EpodUpdateShippedStatus", body);
                if (Response != null)
                {
                    var jsRes = JsonConvert.DeserializeObject<ApiResponse>(Response);
                    if (jsRes.isSuccess)
                    {
                        result = new { result = true, msg = jsRes.Message };
                    }
                    else
                    {
                        result = new { result = false, msg = jsRes.Message };
                    }
                }
                else
                {
                    result = new { result = true, msg = "Something went wrong at our side, Please contact support for assistance!" };
                }
            }

            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetgoodreturnReasonlist : " + ex.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(result);

        }
        public IActionResult ConfirmVerificationshipment()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var result = new { result = false, msg = "" };
            try
            {
                var data = HttpContext.Session.GetString("EPOD_VesselAuthenticationModalViews");
                if (data == null)
                {
                    data = HttpContext.Session.GetString("EPOD_USER_INFO");
                }
                var UserDetails = JsonConvert.DeserializeObject<VesselAuthenticationParameter>(data);

                var Modal = new UpdateEpodStatusModal();
                Modal.userid = convert.ToInt(UserDetails.userid);
                Modal.emailid = UserDetails.useremailid;
                Modal.latitude = UserDetails.latitude;
                Modal.longitude = UserDetails.longitude;
                Modal.shipmentid = UserDetails.shipmentid;
                Modal.role = UserDetails.role;
                Modal.vesselimo = UserDetails.vesselimo;
                Modal.username = UserDetails.crewname;
                string body = JsonConvert.SerializeObject(Modal);
                var Response = _apiroutine.PostAPI("Logistic", "EpodUpdateVerificationStatus", body);
                if (Response != null)
                {
                    var jsRes = JsonConvert.DeserializeObject<ApiResponse>(Response);
                    if (jsRes.isSuccess)
                    {
                        result = new { result = true, msg = jsRes.Message };
                    }
                    else
                    {
                        result = new { result = false, msg = jsRes.Message };
                    }
                }
                else
                {
                    result = new { result = true, msg = "Something went wrong at our side, Please contact support for assistance!" };
                }
            }

            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetgoodreturnReasonlist : " + ex.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(result);

        }
        public IActionResult ConfirmFinalReceipt()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var result = new { result = false, msg = "" };
            try
            {
                var data = HttpContext.Session.GetString("EPOD_VesselAuthenticationModalViews");
                if (data == null)
                {
                    data = HttpContext.Session.GetString("EPOD_USER_INFO");
                }
                var UserDetails = JsonConvert.DeserializeObject<VesselAuthenticationParameter>(data);

                var Modal = new UpdateEpodStatusModal();
                Modal.userid = convert.ToInt(UserDetails.userid);
                Modal.emailid = UserDetails.useremailid;
                Modal.latitude = UserDetails.latitude;
                Modal.longitude = UserDetails.longitude;
                Modal.shipmentid = UserDetails.shipmentid;
                Modal.role = UserDetails.role;
                Modal.vesselimo = UserDetails.vesselimo;
                Modal.username = UserDetails.crewname;
                string body = JsonConvert.SerializeObject(Modal);
                var Response = _apiroutine.PostAPI("Logistic", "EpodUpdateConfirmedStatus", body);
                if (Response != null)
                {
                    var jsRes = JsonConvert.DeserializeObject<ApiResponse>(Response);
                    if (jsRes.isSuccess)
                    {
                        result = new { result = true, msg = jsRes.Message };
                    }
                    else
                    {
                        result = new { result = false, msg = jsRes.Message };
                    }
                }
                else
                {
                    result = new { result = true, msg = "Something went wrong at our side, Please contact support for assistance!" };
                }
            }

            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetgoodreturnReasonlist : " + ex.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(result);

        }
        [HttpPost]
        public IActionResult UploaddoAttachments(IFormFile formFile, int Doid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {

                if (formFile != null && formFile.Length > 0)
                {

                    (int id, string msg) = AttachDODocumentInServer(formFile, Doid);
                    if (id > 0)
                    {

                        LeSDM.AddLog("Document successfully pushed to api : " + formFile.FileName);
                    }
                    else
                    {
                        return Json(new { success = false, message = msg });
                    }

                    return Json(new { success = true, message = "File successfully uploaded: " + formFile.FileName, filename = formFile.FileName, documentId = id });
                }
                else
                {
                    return Json(new { success = false, message = "No file selected." });
                }
            }
            catch (Exception ex)
            {
                // Assuming LeSDM is a logging utility in your project
                LeSDM.AddLog("Exception in UploadAttachments - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "Unable to upload file: " + Path.GetFileName(formFile.FileName) });
            }
        }
        public (int, string) AttachDocumentInServer(IFormFile upfile, int doid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return (0, "");

            int id = 0;
            string msg = "";
            int UserId = convert.ToInt(HttpContext.Session.GetString("EPOD_UserID"));
            try
            {

                if (upfile != null && upfile.Length > 0)
                {
                    var Model2 = new AttachDocumentsDataModal();
                    Model2.DocRefId = doid;
                    Model2.Docid = 0;
                    Model2.Document_Name = Path.GetFileName(upfile.FileName);
                    Model2.FileType = upfile.ContentType;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        upfile.CopyTo(memoryStream);
                        byte[] bytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(bytes);
                        Model2.Base64Data = base64String;
                    }

                    Model2.Companyid = convert.ToInt(HttpContext.Session.GetString("EPOD_CompanyId"));

                    Model2.UpdatedBy = UserId;
                    var jstring = JsonConvert.SerializeObject(Model2);
                    var response = _apiroutine.PostAPI("Logistic", "UploadShipmentDocument", jstring);
                    if (response != "")
                    {
                        var result = JsonConvert.DeserializeObject<ApiResponse>(response);
                        if (result.isSuccess)
                        {
                            var Model = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.Shipment_Documents>(result.Data.ToString());
                            id = (int)Model.DocumentNo;
                            msg = result.Message;
                        }
                        else
                        {
                            msg = result.Message;
                            LeSDM.AddLog("something went wrong - " + result.Message);
                        }

                    }
                    else
                    {
                        LeSDM.AddLog("No response from API Logistic\\CreateShipmentDocument");

                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in AttachDocumentInServer - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return (id, msg);
        }
        public (int, string) AttachDODocumentInServer(IFormFile upfile, int doid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return (0, "");

            int id = 0;
            string msg = "";
            int UserId = convert.ToInt(HttpContext.Session.GetString("EPOD_UserID"));
            try
            {

                if (upfile != null && upfile.Length > 0)
                {
                    var Model2 = new AttachDocumentsDataModal();
                    Model2.DocRefId = doid;
                    Model2.Docid = 0;
                    Model2.Document_Name = Path.GetFileName(upfile.FileName);
                    Model2.FileType = upfile.ContentType;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        upfile.CopyTo(memoryStream);
                        byte[] bytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(bytes);
                        Model2.Base64Data = base64String;
                    }

                    Model2.Companyid = convert.ToInt(HttpContext.Session.GetString("EPOD_CompanyId"));

                    Model2.UpdatedBy = UserId;
                    var jstring = JsonConvert.SerializeObject(Model2);
                    var response = _apiroutine.PostAPI("Logistic", "UploadDeliveryOrderDocument", jstring);
                    if (response != "")
                    {
                        var result = JsonConvert.DeserializeObject<ApiResponse>(response);
                        if (result.isSuccess)
                        {
                            var Model = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.Shipment_Documents>(result.Data.ToString());
                            id = (int)Model.DocumentNo;
                            msg = result.Message;
                        }
                        else
                        {
                            msg = result.Message;
                            LeSDM.AddLog("something went wrong - " + result.Message);
                        }

                    }
                    else
                    {
                        LeSDM.AddLog("No response from API Logistic\\UploadDeliveryOrderDocument");

                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in AttachDODocumentInServer - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return (id, msg);
        }
        public IActionResult RemoveDeliveryOrderDocumnet(int deliverydocumentid, int deliveryorderid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var result = new { result = false, msg = "Something went wrong!" };
            Remove_Delivery_Order_Documents Modal = new();
            var epoduser = convert.ToInt(HttpContext.Session.GetString("EPOD_UserID"));
            if (epoduser < 1)
            {
                epoduser = convert.ToInt(HttpContext.Session.GetString("UserID"));
            }
            try
            {
                Modal.DeliveryDocumentId = deliverydocumentid;
                Modal.UpdatedDate = DateTime.Now;
                Modal.DeliveryOrderId = deliveryorderid;
                Modal.updated_by = epoduser;
                var body = JsonConvert.SerializeObject(Modal);
                var response = _apiroutine.PostAPI("Logistic", "RemoveDeliveryOrderDocument", body);
                if (response != null)
                {
                    var jsRes = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (jsRes.isSuccess)
                    {
                        result = new { result = true, msg = jsRes.Message };
                    }
                    else
                    {
                        result = new { result = false, msg = jsRes.Message };
                    }
                }
                else
                {
                    result = new { result = true, msg = "Something went wrong at our side, Please contact support for assistance!" };
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in RemoveDeliveryOrderDocumnet - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(result);
        }
        public IActionResult GetOutboundShipmentDocuments(int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.Shipment_Documents> Model = new List<Logistic_Management_Lib.Model.Shipment_Documents>();
            JsonResult _result = Json(new { Data = Model });
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("ShipmentId", shipmentid.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentDocumentByShipmentId", null, null, QueryParam);
                Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Shipment_Documents>>(jsonData);
                _result = Json(new { Data = Model });


            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetOutboundShipmentDocuments - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        [HttpPost]
        public IActionResult UploadAttachments(IFormFile formFile, int shipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {

                if (formFile != null && formFile.Length > 0)
                {

                    (int id, string msg) = AttachDocumentInServer(formFile, shipmentId);
                    if (id > 0)
                    {

                        LeSDM.AddLog("Document successfully pushed to api : " + formFile.FileName);
                    }
                    else
                    {
                        return Json(new { success = false, message = msg });
                    }

                    return Json(new { success = true, message = "File successfully uploaded: " + formFile.FileName, filename = formFile.FileName, documentId = id });
                }
                else
                {
                    return Json(new { success = false, message = "No file selected." });
                }
            }
            catch (Exception ex)
            {
                // Assuming LeSDM is a logging utility in your project
                LeSDM.AddLog("Exception in UploadAttachments - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "Unable to upload file: " + Path.GetFileName(formFile.FileName) });
            }
        }
        [HttpPost]
        public IActionResult DeleteAttachment(string filename, int shipmentId, int documentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {

                // Handle document deletion logic with documentId
                if (DeleteShipmentDocument(documentId, shipmentId))
                {
                    LeSDM.AddLog("DeleteAttachment from authenticate process stopped ");
                    return Json(new { success = true, message = "File successfully deleted: " + filename });
                }
                else
                {
                    LeSDM.AddLog("DeleteAttachment from authenticate process stopped ");
                    return Json(new { success = false, message = "Oops,something went wrong! " });
                }


            }
            catch (Exception ex)
            {

                // Assuming LeSDM is a logging utility in your project
                LeSDM.AddLog("Exception in DeleteAttachment - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "Unable to delete file: " + filename });
            }
        }
        public IActionResult GetOrderAttachment(int DeliveryOrderId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            HttpContext.Session.SetString("ATTACHMENTDATA", "");
            List<Logistic_Management_Lib.Model.Delivery_Order_Documents> Model = new List<Logistic_Management_Lib.Model.Delivery_Order_Documents>();
            JsonResult _result = Json(new { Data = Model });
            IDictionary<string, string> QueryParam = new Dictionary<string, string>();
            try
            {
                QueryParam.Add("DeliveryOrderid", DeliveryOrderId.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderDocumentsByDeliveryOrderId", null, null, QueryParam);
                Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Delivery_Order_Documents>>(jsonData);
                HttpContext.Session.SetString("ATTACHMENTDATA", JsonConvert.SerializeObject(Model));
                //Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Delivery_Orders_Info>>(jsonData);
                _result = Json(new { Data = Model });

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetOrderAttachment - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }
            return _result;
        }
        public bool DeleteShipmentDocument(int shipmentDocumentid, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return false;

            bool res = false;
            int UserId = convert.ToInt(HttpContext.Session.GetString("EPOD_UserID"));
            try
            {
                string jsonString = $"{{" +
                               $"\"shipmentDocumentId\": {shipmentDocumentid}, " +
                               $"\"shipmentId\": \"{shipmentid}\", " +
                               $"\"isDelete\": \"{1}\", " +
                            $"\"updatedBy\": \"{UserId}\" " +

                    $"}}";
                var response = _apiroutine.PostAPI("Logistic", "RemoveShipmentDocument", jsonString);
                if (response != "")
                {
                    var Model = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (Model.isSuccess)
                    {
                        res = Model.isSuccess;
                    }
                    else
                    {
                        LeSDM.AddLog(Model.Message);
                    }
                }
                else
                {
                    LeSDM.AddLog("No response from API Logistic/RemoveShipmentDocument ");
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DeleteShipmentDocument - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return res;
        }
        public IActionResult DownloadOrderAttachment(int documentid, int deliveryId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.Shipment_Documents> Model = new List<Logistic_Management_Lib.Model.Shipment_Documents>();
            Shipment_Documents shipment_Documents = new Shipment_Documents();
            JsonResult _result = Json(new object[] { new object() });

            string _base64Data = string.Empty;
            try
            {
                string _attachData = HttpContext.Session.GetString("ATTACHMENTDATA");

                if (!string.IsNullOrEmpty(_attachData))
                {
                    if (shipment_Documents != null)
                    {
                        var model2 = new AttachDocumentsDataModal();
                        model2.DocRefId = deliveryId;
                        model2.Docid = documentid;
                        model2.Document_Name = "";
                        model2.Base64Data = "";
                        model2.UpdatedBy = 0;
                        var jstring = JsonConvert.SerializeObject(model2);
                        string jsonData = _apiroutine.PostAPI("Logistic", "DownloadDeliveryOrderDocument", jstring);

                        model2 = JsonConvert.DeserializeObject<AttachDocumentsDataModal>(jsonData);
                        if (model2 != null)
                        {
                            if (!string.IsNullOrEmpty(model2.Base64Data))
                            {
                                _base64Data = model2.Base64Data;

                                _result = Json(new { result = "SUCCESS", msg = "File Found Successfull.", Data = model2, base64Data = _base64Data });
                                return _result;
                            }
                            else
                            {
                                _result = Json(new { result = "ERROR", msg = "File not Found.", Data = shipment_Documents, base64Data = _base64Data });
                                return _result;
                            }
                        }
                    }
                    else
                    {
                        _result = Json(new { result = "ERROR", msg = "Document not Found.", Data = shipment_Documents, base64Data = _base64Data });
                        return _result;
                    }
                }
                else
                {
                    _result = Json(new { result = "ERROR", msg = "Attachment Data not Found.", Data = shipment_Documents, base64Data = _base64Data });
                    return _result;
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DownloadOrderAttachment - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                //return Json(false);
            }
            _result = Json(new { result = "ERROR", msg = "Something went wrong in downloading attachment", Data = shipment_Documents, base64Data = _base64Data });
            return _result;
        }
        public IActionResult DownloadshipmentAttachement(int documentid, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            string _base64Data = string.Empty;
            try
            {
                if (documentid > 0 && shipmentid > 0)
                {
                    List<Logistic_Management_Lib.Model.Shipment_Documents> Model = new List<Logistic_Management_Lib.Model.Shipment_Documents>();
                    IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                    QueryParam.Add("ShipmentId", shipmentid.ToString());

                    var model2 = new AttachDocumentsDataModal();
                    model2.DocRefId = shipmentid;
                    model2.Docid = documentid;
                    model2.Document_Name = "";
                    model2.Base64Data = "";
                    model2.UpdatedBy = 0;
                    var jstring = JsonConvert.SerializeObject(model2);
                    string jsonData = _apiroutine.PostAPI("Logistic", "DownloadShipmentDocument", jstring);

                    var Model2 = JsonConvert.DeserializeObject<Logistic_Management_Lib.AttachDocumentsDataModal>(jsonData);
                    if (Model2 != null)
                    {
                        if (!string.IsNullOrEmpty(Model2.Base64Data))
                        {

                            _base64Data = Model2.Base64Data;
                            _result = Json(new { result = true, msg = "File Found Successfull.", Data = Model2, base64Data = _base64Data });
                            return _result;
                        }
                        else
                        {
                            _result = Json(new { result = false, msg = "File not Found.", Data = "", base64Data = "" });

                        }
                    }
                    else
                    {
                        _result = Json(new { result = false, msg = "Something went wrong!", Data = "", base64Data = "" });

                    }

                }
                else
                {
                    _result = Json(new { result = false, msg = "Something went wrong!", Data = "", base64Data = "" });

                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DownloadshipmentAttachement - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        #endregion EPOD
        #region EPOD details by url
        public IActionResult EpodTripDetails(string shipmentdata)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                ViewBag.SubTitle = "ePOD shipment Details";
                shipmentdata = HttpUtility.UrlEncode(shipmentdata);

                if (!string.IsNullOrEmpty(shipmentdata) && !string.IsNullOrWhiteSpace(shipmentdata) && shipmentdata.Length > 0)
                {
                    HttpContext.Session.SetString("ShipmentByUrl", shipmentdata);
                    return View(null);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in EpodTripDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return NotFound();
            }
        }
        [HttpPost]
        public IActionResult ProcessShipmetTrip(string longitude, string latitude)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {

                string shipmentdata = HttpContext.Session.GetString("ShipmentByUrl");
                if (shipmentdata.Length > 0 && longitude.Length > 0 && latitude.Length > 0)
                {
                    VesselAuthenticationUrlParameter modal = new();
                    modal.latitude = latitude;
                    modal.longitude = longitude;
                    modal.EncryptData = shipmentdata;
                    string body = JsonConvert.SerializeObject(modal);
                    var Response = _apiroutine.PostAPI("Logistic", "EpodLoginByVesselAuthenticationCrewEncryptData", body);
                    if (Response != "")
                    {
                        var jsRes = JsonConvert.DeserializeObject<RequestResponse>(Response);
                        if (jsRes.StatusCode == 200)
                        {
                            VesselAuthenticationModalViews vesselEpodData = JsonConvert.DeserializeObject<VesselAuthenticationModalViews>(jsRes.Value.ToString());
                            vesselEpodData.longitude = longitude;
                            vesselEpodData.latitude = latitude;
                            jsRes.Value = JsonConvert.SerializeObject(vesselEpodData);
                            HttpContext.Session.SetString("EPOD_SHIPMENTID", vesselEpodData.shipmentid.ToString());
                            HttpContext.Session.SetString("EPOD_USER_INFO", jsRes.Value.ToString());
                            HttpContext.Session.SetString("EPOD_USER_LATITUDE", latitude.ToString());
                            HttpContext.Session.SetString("EPOD_USER_LONGITUDE", longitude.ToString());
                            HttpContext.Session.SetString("EPOD_CompanyId", vesselEpodData.les_company_details.CompanyId.ToString());
                            HttpContext.Session.SetString("EPOD_UserID", vesselEpodData.userid.ToString());
                            return RedirectToAction("EpodShipmentDetails", "Authenticate");

                        }
                        else
                        {
                            return BadRequest("link is expired!");
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in ProcessShipmetTrip - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return NotFound();
            }

        }

        #endregion EPOD details by url



        #region Printing Functionalities
        //Gaurav
        private List<string> DeliveryOrdersPdfPaths = new List<string>();
        public IActionResult PrintShipmentOrders(int _shipmentId, bool printWithAllOrders)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            byte[] pdfBytes = null;
            try
            {

                SetAsposeLicense();
                int companyid = convert.ToInt(HttpContext.Session.GetString("EPOD_CompanyId"));
                var shipmentInfo = _apiroutine.PostAPI("Logistic", $"GetPrintShipmentInfo?Shipmentid={Convert.ToString(_shipmentId)}&Companyid={Convert.ToString(companyid)}", "");
                var shipmentTripPlan = _apiroutine.PostAPI("Logistic", $"GetVShipmentTripPlan?Shipmentid={Convert.ToString(_shipmentId)}", "");
                var deliveryOrderList = _apiroutine.PostAPI("Logistic", $"GetDeliveryOrderListByShipmentId?Shipmentid={Convert.ToString(_shipmentId)}", "");
                var shipmentDocuments = _apiroutine.PostAPI("Logistic", $"GetShipmentDocumentByShipmentId?Shipmentid={Convert.ToString(_shipmentId)}", "");
                var companyInfo = _apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyid)}", "");
                OutboundPrintModel printModel = DeserializeOutboundShipment(shipmentInfo, shipmentTripPlan, deliveryOrderList, shipmentDocuments, companyInfo);
                string filePath = AppendDataInTemplate(printModel, printWithAllOrders);


                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Directory.GetCurrentDirectory() + "\\wwwroot\\ShipmentOrderPdfs\\" + fileName;
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file in PrintShipmentOrders Authenticate controller:  '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);

                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Shipment Order in Authenticate Controller - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }

        }



        OutboundPrintModel printModel;
        public OutboundPrintModel DeserializeOutboundShipment(string _shipmentInfo, string _shipmentTripPlan, string _deliveryOrderList, string _shipmentDocument, string _companyInfo)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return null;

            try
            {

                printModel = new OutboundPrintModel();
                printModel.ShipmentInfo = JsonConvert.DeserializeObject<ShipmentInfo>(_shipmentInfo);
                printModel.ShipmentTripPlan = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.V_SHIPMENT_TRIP_PLAN>(_shipmentTripPlan);
                printModel.ShipmentDeliveryOrders = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS>>(_deliveryOrderList);
                printModel.ShipmentDocuments = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Shipment_Documents>>(_shipmentDocument);
                printModel.LogoModel = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_companyInfo);
                // for qr Code
                QRModal qrModel = new();
                qrModel.ShipmentId = printModel.ShipmentInfo.shipmentid;
                qrModel.ShipmentNo = printModel.ShipmentInfo.order_no;
                qrModel.VesselName = printModel.ShipmentInfo.vessel_name;
                qrModel.CustomerName = printModel.ShipmentInfo.cust_name;
                qrModel.JobNo = printModel.ShipmentInfo.jobno;
                string jsondata = JsonConvert.SerializeObject(qrModel);
                printModel.QrModel = _apiroutine.PostAPI("Logistic", "GenerateQRCode", jsondata, null, null);
                return printModel;

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Deserialing Shipment Orders - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return null;
            }

        }


        public string AppendDataInTemplate(OutboundPrintModel printModel, bool printWithAllOrders)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return "";

            string savedFilePath = "";
            try
            {
                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:OutboundTemplate", "");
                var outputDirectory = Directory.GetCurrentDirectory() + "\\wwwroot\\ShipmentOrderPdfs";

                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both OutboundTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }

                tempTemplate += $"\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                //Workbook templateWorkbook = new Workbook(templatePath);

                //Workbook newWorkbook = new Workbook();
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                #region shipment order infos
                worksheet.Cells["C6"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.order_no) ? ": " : $": {printModel.ShipmentInfo.order_no}");
                worksheet.AutoFitRow(5);
                worksheet.Cells["C8"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.vessel_name) ? ": " : $": {printModel.ShipmentInfo.vessel_name}");
                worksheet.AutoFitRow(7);
                worksheet.Cells["C10"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.cust_name) ? ": " : $": {printModel.ShipmentInfo.cust_name}");
                worksheet.AutoFitRow(9);
                worksheet.Cells["C12"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.jobno) ? ": " : $": {printModel.ShipmentInfo.jobno}");
                worksheet.AutoFitRow(11);
                worksheet.Cells["C14"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.delivery_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? ": " : $": {printModel.ShipmentInfo.delivery_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                worksheet.AutoFitRow(13);

                worksheet.Cells["G4"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.loading_point) ? ": " : $": {printModel.ShipmentInfo.loading_point}");
                worksheet.Cells["G6"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.loading_time?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? ": " : $": {printModel.ShipmentInfo.loading_time?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                string anchorage = $"{printModel.ShipmentInfo.anchorage_code} {printModel.ShipmentInfo.anchorage_description}";
                worksheet.Cells["G8"].PutValue(string.IsNullOrEmpty(anchorage) ? ": " : $": {anchorage}");
                worksheet.Cells["G10"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.transport_type_description) ? ": " : $": {printModel.ShipmentInfo.transport_type_description}");
                string driverName = $"{printModel.ShipmentInfo.driver_name} {printModel.ShipmentInfo.Vehicle_no}";
                worksheet.Cells["G12"].PutValue(string.IsNullOrEmpty(driverName) ? ": " : $": {driverName}");
                worksheet.Cells["G14"].PutValue(string.IsNullOrEmpty(printModel.ShipmentTripPlan.estimate_packaging_unit.ToString()) ? ": " : $": {printModel.ShipmentTripPlan.estimate_packaging_unit.ToString()}");
                worksheet.AutoFitRow(13);
                worksheet.Cells["G16"].PutValue(string.IsNullOrEmpty(printModel.ShipmentTripPlan.boarding_officer_name) ? ": " : $": {printModel.ShipmentTripPlan.boarding_officer_name}");
                worksheet.AutoFitRow(15);



                //for logo code
                byte[] logoByte = Convert.FromBase64String(printModel.LogoModel.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {
                    //logoByte = ResizeImage(logoByte, 500, 400);
                    //Resize the logo
                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 450; // Set the desired width
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width; // Maintain aspect ratio

                            using (System.Drawing.Bitmap resizedImage = new System.Drawing.Bitmap(originalImage, new Size(newWidth, newHeight)))
                            {
                                using (MemoryStream resizedMs = new MemoryStream())
                                {
                                    resizedImage.Save(resizedMs, originalImage.RawFormat);
                                    logoByte = resizedMs.ToArray();
                                }
                            }
                        }
                    }
                    worksheet.PageSetup.SetHeaderPicture(0, logoByte);
                    worksheet.PageSetup.SetHeader(0, "&G");
                    worksheet.PageSetup.HeaderMargin = 0.9;

                }
                else
                {
                    worksheet.PageSetup.SetHeader(0, printModel.LogoModel.Company_Description);
                }

                //for qr code 
                byte[] imageBytes = Convert.FromBase64String(printModel.QrModel);
                if (imageBytes != null)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        int pictureIndex = worksheet.Pictures.Add(3, 4, ms);
                        Picture picture = worksheet.Pictures[pictureIndex];
                        picture.HeightCM = picture.WidthCM = 4.2;
                    }
                }
                else
                {
                    worksheet.Cells["E4"].PutValue("");
                }
                //worksheet.PageSetup.SetFooter(0, "&P of &N");
                worksheet.PageSetup.SetFooter(2, $"Print Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                #endregion

                #region shipment agents info
                worksheet.Cells["C20"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.agent) ? ": " : $": {printModel.ShipmentInfo.agent}");
                worksheet.AutoFitRow(19);
                worksheet.Cells["C22"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.agent_contact_person) ? ": " : $": {printModel.ShipmentInfo.agent_contact_person}");
                worksheet.AutoFitRow(21);
                worksheet.Cells["C24"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.agent_contact_no) ? ": " : $": {printModel.ShipmentInfo.agent_contact_no}");
                worksheet.AutoFitRow(23);
                worksheet.Cells["C26"].PutValue(string.IsNullOrEmpty(printModel.ShipmentTripPlan.ctm.ToString()) ? ": " : $": {printModel.ShipmentTripPlan.ctm}");
                worksheet.AutoFitRow(25);
                worksheet.Cells["G20"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.supply_boat) ? ": " : $": {printModel.ShipmentInfo.supply_boat}");
                worksheet.Cells["G22"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.supply_boat_contact_person) ? ": " : $": {printModel.ShipmentInfo.supply_boat_contact_person}");
                worksheet.Cells["G24"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.supply_boat_contact_no) ? ": " : $": {printModel.ShipmentInfo.supply_boat_contact_no}");


                #endregion

                #region shipment line items
                int i = 1;
                int rowIndex = 32;
                double totalPackingUnit = 0;
                Style boldStyle = newWorkbook.CreateStyle();
                var globalStyle = worksheet.Cells[$"B{rowIndex}"].GetStyle();
                globalStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                globalStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                globalStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                globalStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                globalStyle.VerticalAlignment = TextAlignmentType.Center;
                globalStyle.IsTextWrapped = true;
                boldStyle.Copy(globalStyle);
                boldStyle.Font.IsBold = true;
                //string strForTest = "apple\nbanana\ncherry\ndate\nelderberry\nfig\ngrape\nhoneydew\nkiwi\nlemon";

                foreach (Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS deliveryOrder in printModel.ShipmentDeliveryOrders)
                {
                    worksheet.Cells.InsertRow(rowIndex);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue("\n" + i.ToString() + "\n");
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.delivery_order_no) ? " " : "\n" + deliveryOrder.delivery_order_no + "\n");
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(boldStyle);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.sales_order_no) ? " " : "\n" + deliveryOrder.sales_order_no + "\n");
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"D{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.internal_dept) ? " " : "\n" + deliveryOrder.internal_dept + "\n");
                    worksheet.Cells[$"D{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"E{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.pono) ? " " : "\n" + deliveryOrder.pono + "\n");
                    worksheet.Cells[$"E{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"F{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.packaging_unit_no.ToString()) ? $" " : "\n" + deliveryOrder.packaging_unit_no.ToString() + "\n");
                    worksheet.Cells[$"F{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"G{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.dept_code) ? " " : "\n" + deliveryOrder.dept_code + "\n");
                    worksheet.Cells[$"G{rowIndex}"].SetStyle(boldStyle);
                    worksheet.Cells[$"H{rowIndex}"].SetStyle(globalStyle);
                    totalPackingUnit += Convert.ToDouble(deliveryOrder.packaging_unit_no);
                    worksheet.AutoFitRow(rowIndex - 1);
                    i++;
                    rowIndex++;
                }


                worksheet.Cells["C29"].PutValue(totalPackingUnit.ToString());

                int lastRowWorkSheet = worksheet.Cells.MaxDataRow;
                ImageOrPrintOptions im = new ImageOrPrintOptions();
                //SheetRender sr = new SheetRender(worksheet, im);
                CellArea[] pageBreak = worksheet.GetPrintingPageBreaks(im);
                int pageCountBefore = pageBreak.Length;

                //newWorkbook.Worksheets.Add();
                //newWorkbook.Worksheets[1].Copy(templateWorkbook.Worksheets[1]);
                Worksheet worksheet1 = newWorkbook.Worksheets[1];
                IDictionary<string, string> placeholders = new Dictionary<string, string>()
                {
                    {"#initLoginId" , string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Emailid) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Emailid}" },
                    {"#initNameofCrew", string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Crew) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Crew}"},
                    {"#initPositionCrew",  string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Role) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Role}"},
                    {"#initDateTime",  string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}"},
                    {"#initLocation",  string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_LoCode) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_LoCode}"},
                    {"#finalLoginId",  string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Emailid) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Emailid}"},
                    {"#finalNameofCrew",  string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Crew) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Crew}"},
                    {"#finalPositionCrew",  string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Role) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Role}"},
                    {"#finalDateTime",  string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}"},
                    {"#finalLocation", string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_LoCode) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_LoCode}"}
                };

                foreach (Aspose.Cells.Cell cell in worksheet1.Cells)
                {
                    if (placeholders.ContainsKey(cell.StringValue))
                    {
                        cell.PutValue(placeholders[cell.StringValue]);
                    }
                }

                worksheet.Cells.CopyRows(worksheet1.Cells, 0, lastRowWorkSheet + 2, worksheet1.Cells.MaxDataRow + 2);
                newWorkbook.Worksheets.RemoveAt(1);


                ImageOrPrintOptions im1 = new ImageOrPrintOptions();
                CellArea[] pageBreak1 = worksheet.GetPrintingPageBreaks(im1);
                int pageCountAfter = pageBreak1.Length;
                if (pageCountBefore != pageCountAfter)
                {
                    worksheet.HorizontalPageBreaks.Add(rowIndex);
                }



                #endregion
                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                string finalFile = $"{outputDirectory}/Shipment_Order_{printModel.ShipmentInfo.order_no}_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                newWorkbook.Save(finalFile);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
                savedFilePath = finalFile;
                Document shipmentDetails = new Document(savedFilePath);
                if (printWithAllOrders)
                {
                    DeliveryOrderDetails(printModel.ShipmentDeliveryOrders);


                    foreach (string doPaths in DeliveryOrdersPdfPaths)
                    {
                        Document DOShipmentFile = new Document(doPaths);
                        shipmentDetails.Pages.Add(DOShipmentFile.Pages);
                        shipmentDetails.Save(savedFilePath);
                        if (System.IO.File.Exists(doPaths))
                        {
                            System.IO.File.Delete(doPaths);
                        }
                    }

                    if (printModel.ShipmentDeliveryOrders.Count > 0)
                    {
                        List<string> attchmentsPath = DownloadDOAttachments(printModel.ShipmentDeliveryOrders);

                        if (attchmentsPath.Count > 0)
                        {
                            foreach (string _path in attchmentsPath)
                            {
                                if (Path.GetExtension(_path) != ".pdf")
                                {
                                    //FileSpecification fileSpecification = new FileSpecification(_path, "Append file");
                                    //shipmentDetails.EmbeddedFiles.Add(fileSpecification);
                                    //shipmentDetails.Save(savedFilePath);
                                    if (System.IO.File.Exists(_path))
                                    {
                                        System.IO.File.Delete(_path);
                                    }
                                }
                                else if (Path.GetExtension(_path) == ".pdf")
                                {
                                    Document DoAttachments = new Document(_path);
                                    shipmentDetails.Pages.Add(DoAttachments.Pages);
                                    shipmentDetails.Save(savedFilePath);
                                    if (System.IO.File.Exists(_path))
                                    {
                                        System.IO.File.Delete(_path);
                                    }
                                }
                                else
                                {
                                    LeSDM.AddLog("Invaild extension of the file.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Append Data in Template in authenticate controller - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return savedFilePath;
        }

        //private void ConvertImageToPdf(string inputFilePath, string outputPdfPath)
        //{

        //    var image = Aspose.Imaging.Image.Load(inputFilePath);
        //    using (var stream = new FileStream(outputPdfPath, FileMode.Create))
        //    {
        //        var pdfOptions = new PdfOptions();
        //        image.Save(stream, pdfOptions);
        //    }
        //}

        public List<string> DownloadDOAttachments(List<V_SHIPMENT_DELIVERY_ORDERS> DOs)
        {
            List<string> attachmentFilePaths = new List<string>();
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return attachmentFilePaths;

            try
            {
                Dictionary<string, string> MimeTypes = new Dictionary<string, string>
                    {
                        { "application/pdf", ".pdf" },
                        { "image/png", ".png" },
                        { "image/jpeg", ".jpg" },
                        { "image/gif", ".gif" },
                        { "text/plain", ".txt" },
                        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx" }
                    };
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                var outputDirectory = Directory.GetCurrentDirectory() + "\\wwwroot\\DOAttachments";
                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                byte[] attachmentFile;
                foreach (V_SHIPMENT_DELIVERY_ORDERS DO in DOs)
                {

                    List<Delivery_Order_Documents> attachments = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Delivery_Order_Documents>>
                                                (_apiroutine.PostAPI("Logistic", $"GetDeliveryOrderDocumentsByDeliveryOrderId?DeliveryOrderid={DO.delivery_order_id}", ""));
                    foreach (Delivery_Order_Documents attachment in attachments)
                    {
                        string body = $"{{" +
                                   $"\"docRefId\": {attachment.DocumentNo}, " +
                                   $"\"updatedBy\": \"{0}\", " +
                                   $"\"document_Name\": \"{""}\", " +
                                   $"\"fileType\": \"{""}\"," +
                                   $"\"{"base64Data"}\": \"{""}\", " +
                                   $"\"{"docid"}\":{attachment.DeliveryDocumentId}, " +
                                   $"\"{"Companyid"}\":{companyid}" +
                                   $"}}";

                        var attachmentData = JsonConvert.DeserializeObject<Logistic_Management_Lib.AttachDocumentsDataModal>(_apiroutine.PostAPI("Logistic", "DownloadDeliveryOrderDocument", body));
                        if (attachmentData != null)
                        {
                            attachmentFile = Convert.FromBase64String(attachmentData.Base64Data);
                            string fileType = attachmentData.FileType;

                            string filePath = Path.Combine(outputDirectory, $"DOAttachments_{attachment.DeliveryDocumentId}{MimeTypes[fileType]}");
                            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(attachmentFile);
                            }
                            attachmentFilePaths.Add(filePath);
                        }
                    }

                    //var attachmentData = _apiroutine.PostAPI("Logistic", "DownloadDeliveryOrderDocument", body, null, null);
                }
                return attachmentFilePaths;
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DownloadDOAttachments - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return attachmentFilePaths;
            }
        }


        public void DeliveryOrderDetails(List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> _deliveryOrderList)
        {
            if (GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext))
            {

                try
                {
                    foreach (Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS _deliveryOrder in _deliveryOrderList)
                    {
                        DeliveryOrderDetailsPrint _deliveryOrderDetails = new();
                        string _id = Convert.ToString(_deliveryOrder.delivery_order_id);
                        int _companyId = convert.ToInt(HttpContext.Session.GetString("EPOD_CompanyId"));
                        _deliveryOrderDetails.DeliveryOrdersInfo = JsonConvert.DeserializeObject<V_Delivery_Orders_Info>(_apiroutine.PostAPI("Logistic", $"GetDeliveryOrdersInfoByCompanyId?DeliveryOrderid={_id}&Companyid={Convert.ToString(_companyId)}", ""));
                        string addressResponse = _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersAddressByDeliveryOrderId?DeliveryOrderid=" + _id, _id);
                        if (!string.IsNullOrEmpty(addressResponse))
                        {
                            _deliveryOrderDetails.ShippingAddressDetail = JsonConvert.DeserializeObject<List<V_DELIVERY_ORDERS_ADDRESS_EPOD>>(addressResponse)[0];
                        }

                        _deliveryOrderDetails.DeliveryOrderLines = JsonConvert.DeserializeObject<List<V_DELIVERY_ORDER_LINES>>(_apiroutine.PostAPI("Logistic", "GetVDeliveryOrderLines?DeliveryOrderid=" + _id, _id));
                        // give _deliveryOrderDetails to another function and append in the excel format save that excel in pdf
                        PrintDeliveryOrderDetails(_deliveryOrderDetails);
                    }
                }
                catch (Exception ex)
                {
                    LeSDM.AddLog("Exception in Delivery Order Details - " + ex.Message);
                    LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                }
            }
        }

        public void PrintDeliveryOrderDetails(DeliveryOrderDetailsPrint deliveryOrderDetails)
        {
            if (GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext))
            {

                try
                {
                    var templatePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Template\\Delivery Order Details.xlsx";
                    Workbook templateWorkbook = new Workbook(templatePath);
                    Workbook newWorkbook = new Workbook();
                    newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                    Worksheet worksheet = newWorkbook.Worksheets[0];
                    Style style = newWorkbook.CreateStyle();
                    #region delivery order infos
                    worksheet.Cells["D3"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no);
                    worksheet.AutoFitRow(2);
                    worksheet.Cells["D5"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.order_no) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.order_no);
                    worksheet.AutoFitRow(4);
                    worksheet.Cells["D7"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.vessel_code) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.vessel_code);
                    worksheet.AutoFitRow(6);
                    worksheet.Cells["D9"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.vessel_name) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.vessel_name);
                    worksheet.AutoFitRow(8);
                    worksheet.Cells["D11"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.promised_delivery_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.promised_delivery_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
                    worksheet.AutoFitRow(10);
                    //worksheet.Cells["E7"].PutValue(deliveryOrderDetails?.DeliveryOrdersInfo?.promised_delivery_date.HasValue ?? false ? deliveryOrderDetails.DeliveryOrdersInfo.promised_delivery_date.Value.ToString("dd/MM/yy"): "-");
                    worksheet.Cells["D13"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.pono) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.pono);
                    worksheet.AutoFitRow(12);
                    worksheet.Cells["D15"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.sales_person_code) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.sales_person_code);
                    worksheet.AutoFitRow(14);
                    worksheet.Cells["G3"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.quote_no) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.quote_no);

                    worksheet.Cells["G5"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.do_job_no) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.do_job_no);
                    worksheet.Cells["G7"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.customer_no) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.customer_no);
                    worksheet.Cells["G9"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.customer_name) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.customer_name);
                    worksheet.Cells["G11"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.dept_code) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.dept_code);
                    worksheet.Cells["G13"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.parent_customer) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.parent_customer);
                    worksheet.Cells["G15"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.location_code) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.location_code);
                    //worksheet.Cells["O3"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.ShippingAddressDetail.planned_ship_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? "-" : deliveryOrderDetails.ShippingAddressDetail.planned_ship_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
                    worksheet.Cells["K3"].PutValue(deliveryOrderDetails?.ShippingAddressDetail?.planned_ship_date.HasValue ?? false ? deliveryOrderDetails.ShippingAddressDetail.planned_ship_date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : " ");

                    worksheet.Cells["K5"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails?.ShippingAddressDetail?.contact_person) ? " " : deliveryOrderDetails?.ShippingAddressDetail?.contact_person);
                    worksheet.Cells["K7"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails?.ShippingAddressDetail?.contact_number) ? " " : deliveryOrderDetails.ShippingAddressDetail.contact_number);
                    worksheet.Cells["K9"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails?.ShippingAddressDetail?.emailid) ? " " : deliveryOrderDetails.ShippingAddressDetail.emailid);
                    worksheet.Cells["K11"].PutValue(string.IsNullOrEmpty(deliveryOrderDetails.DeliveryOrdersInfo.internal_dept) ? " " : deliveryOrderDetails.DeliveryOrdersInfo.internal_dept); string address = $"{deliveryOrderDetails?.ShippingAddressDetail?.address1} {deliveryOrderDetails?.ShippingAddressDetail?.address2} {deliveryOrderDetails?.ShippingAddressDetail?.address3}" +
                        $"{deliveryOrderDetails?.ShippingAddressDetail?.address4} {deliveryOrderDetails?.ShippingAddressDetail?.address5} {deliveryOrderDetails?.ShippingAddressDetail?.city} {deliveryOrderDetails?.ShippingAddressDetail?.country_region} {deliveryOrderDetails?.ShippingAddressDetail?.zipcode}";
                    worksheet.Cells["K13"].PutValue(string.IsNullOrEmpty(address) ? " " : address);

                    #endregion

                    #region delivery order lines
                    int i = 1;
                    int rowIndex = 19;


                    foreach (Logistic_Management_Lib.Model.V_DELIVERY_ORDER_LINES deliveryOrderLines in deliveryOrderDetails.DeliveryOrderLines)
                    {
                        worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                        worksheet.Cells[$"A{rowIndex}"].PutValue("\n" + i.ToString());
                        worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.item_ref_no) ? " " : "\n" + deliveryOrderLines.item_ref_no);
                        worksheet.Cells[$"D{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.item_description) ? " " : $"\n{deliveryOrderLines.item_description}");
                        worksheet.Cells[$"E{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.quantity.ToString()) ? " " : "\n" + deliveryOrderLines.quantity?.ToString("F2", CultureInfo.InvariantCulture));
                        worksheet.Cells[$"G{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.broker_code) ? " " : "\n" + deliveryOrderLines.broker_code);
                        worksheet.Cells[$"H{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.uom) ? " " : "\n" + deliveryOrderLines.uom);
                        worksheet.Cells[$"I{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.quantity_invoiced.ToString()) ? "0" : "\n" + deliveryOrderLines.quantity_invoiced?.ToString("F2", CultureInfo.InvariantCulture));
                        worksheet.Cells[$"K{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.awb_bl) ? " " : "\n" + deliveryOrderLines.awb_bl);
                        worksheet.AutoFitRow(rowIndex - 1);
                        i++;
                        rowIndex++;
                    }
                    // add header here
                    byte[] logoByte = Convert.FromBase64String(printModel.LogoModel.base64printLogo);
                    if (logoByte != null)
                    {
                        //logoByte = ResizeImage(logoByte, 540, 420);
                        // Resize the logo
                        using (MemoryStream ms = new MemoryStream(logoByte))
                        {
                            using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                            {
                                int newWidth = 450; // Set the desired width
                                int newHeight = (originalImage.Height * newWidth) / originalImage.Width; // Maintain aspect ratio

                                using (System.Drawing.Bitmap resizedImage = new System.Drawing.Bitmap(originalImage, new Size(newWidth, newHeight)))
                                {
                                    using (MemoryStream resizedMs = new MemoryStream())
                                    {
                                        resizedImage.Save(resizedMs, originalImage.RawFormat);
                                        logoByte = resizedMs.ToArray();
                                    }
                                }
                            }
                        }
                        worksheet.PageSetup.SetHeaderPicture(0, logoByte);
                        worksheet.PageSetup.SetHeader(0, "&G");
                        worksheet.PageSetup.HeaderMargin = 0.5;

                    }
                    else
                    {
                        worksheet.PageSetup.SetHeader(0, printModel.LogoModel.Company_Description);
                    }

                    //worksheet.PageSetup.SetFooter(0, "&P of &N");
                    worksheet.PageSetup.SetFooter(2, $"Print Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                    //worksheet.AutoFitRows(true);

                    #endregion

                    string outputFolder = Directory.GetCurrentDirectory() + "\\wwwroot\\DeliveryOrderDetails";
                    DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    //style.Font.Size = 11;
                    //StyleFlag styleFlag = new StyleFlag();
                    //styleFlag.FontSize = true;
                    //worksheet.Cells.ApplyStyle(style, styleFlag);
                    //worksheet.AutoFitRows();
                    newWorkbook.Save($"{outputFolder}/Delivery_Order_List_{deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no}_{DateTime.Now.ToString("ddMMyy")}.pdf");
                    DeliveryOrdersPdfPaths.Add($"{outputFolder}/Delivery_Order_List_{deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no}_{DateTime.Now.ToString("ddMMyy")}.pdf");
                }
                catch (Exception ex)
                {
                    LeSDM.AddLog("Exception in Print Delivery Order Details - " + ex.Message);
                    LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                }
            }
        }

        //for goods return report

        public IActionResult PrintGoodsReturnReport(string _shipmentid)
        {

            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            byte[] pdfBytes = null;
            try
            {

                string _companyid = HttpContext.Session.GetString("EPOD_CompanyId");
                Dictionary<string, string> bodyData = new Dictionary<string, string>();
                bodyData.Add("shipmentid", _shipmentid);
                bodyData.Add("companyid", _companyid);
                string jsonBody = JsonConvert.SerializeObject(bodyData);
                string responseBody = _apiroutine.PostAPI("Logistic", "GetPrintGRNReport", jsonBody);
                PrintGoodsReturn goodsReturn = JsonConvert.DeserializeObject<PrintGoodsReturn>(responseBody);
                string filePath = PrintGoodsReturn(goodsReturn);

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Directory.GetCurrentDirectory() + "\\wwwroot\\GoodsReturnFiles\\" + fileName;
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in PrintGoodsReturnReport - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }

        private string PrintGoodsReturn(PrintGoodsReturn _returnReport)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRightsEpod"), "3", HttpContext)) return "";

            string filePath = "";
            try
            {
                Aspose.Cells.License license = new Aspose.Cells.License();
                license.SetLicense("Aspose.Total.NET.lic");
                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:GoodsReturnTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both GoodsReturnTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate = tempTemplate + $"\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                //Workbook templateWorkBook = new Workbook(templatePath);
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                //newWorkbook.Worksheets[0].Copy(templateWorkBook.Worksheets[0]);

                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();




                #region Goods Return header info
                IDictionary<string, string> placeholder = new Dictionary<string, string>()
                {
                    {"#shipmentNumber",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.shipmentno) ? ": " : $": {_returnReport.goodsreturnheaderinfo.shipmentno}"},
                    {"#jobNumber",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.jobno) ? ": " : $": {_returnReport.goodsreturnheaderinfo.jobno }"},
                    {"#deliveryNumber",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.deliverydate) ? ": " : $": {_returnReport.goodsreturnheaderinfo.deliverydate}"},
                    {"#driverNumber",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.drivername) ? ": " : $": {_returnReport.goodsreturnheaderinfo.drivername}"},
                    {"#vesselName", string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.vesselname) ? ": " : $": {_returnReport.goodsreturnheaderinfo.vesselname}"},
                    {"#boardingOfficer",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.bordingofficer) ? ": " : $": {_returnReport.goodsreturnheaderinfo.bordingofficer}"},
                    {"#loadingLocation",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.loadinglocation) ? ": " : $": {_returnReport.goodsreturnheaderinfo.loadinglocation}"},
                    {"#loadingTime",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.loadingtime) ? ": " : $": {_returnReport.goodsreturnheaderinfo.loadingtime}"},
                    {"#purchaserTech",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.purchaser_tech) ? ": " : $": {_returnReport.goodsreturnheaderinfo.purchaser_tech}"},
                    {"#purchaserProvis", string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.purchaser_prov) ? ": " : $": {_returnReport.goodsreturnheaderinfo.purchaser_prov}" },
                    {"#packedBy",  string.IsNullOrEmpty(_returnReport.goodsreturnheaderinfo.packedby) ? ": " : $": {_returnReport.goodsreturnheaderinfo.packedby}"}
                };

                foreach (Aspose.Cells.Cell cell in worksheet.Cells)
                {
                    if (placeholder.ContainsKey(cell.StringValue))
                    {
                        cell.PutValue(placeholder[cell.StringValue]);
                    }
                }
                #endregion

                #region Goods Report Header
                byte[] logoByte = Convert.FromBase64String(_returnReport.companydata.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {
                    worksheet.PageSetup.SetHeaderPicture(0, logoByte);
                    worksheet.PageSetup.SetHeader(0, "&G");
                    worksheet.PageSetup.HeaderMargin = 0.5;
                }
                else
                {
                    worksheet.PageSetup.SetHeader(0, _returnReport.companydata.Company_Description);
                }
                #endregion

                #region Goods Report Line Items
                int rowIndex = 12;
                string outputFolder = Directory.GetCurrentDirectory() + "\\wwwroot\\GoodsReturnFiles";
                var globalStyle = worksheet.Cells["A12"].GetStyle();
                Style alignmentStyle = newWorkbook.CreateStyle();
                globalStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                globalStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                globalStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                globalStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                globalStyle.VerticalAlignment = TextAlignmentType.Center;
                globalStyle.IsTextWrapped = true;
                alignmentStyle.Copy(globalStyle);
                alignmentStyle.HorizontalAlignment = TextAlignmentType.Right;

                if (_returnReport.goodsreturniteminfo.Count > 0)
                {

                    foreach (PrintGoodsReturnItemInfo returnItem in _returnReport.goodsreturniteminfo)
                    {
                        worksheet.Cells.InsertRow(rowIndex + 1);
                        worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                        worksheet.Cells[$"A{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.deliveryorderno) ? " " : $"\n{returnItem.deliveryorderno}\n");
                        worksheet.Cells[$"A{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.cust_dept) ? " " : $"\n{returnItem.cust_dept}\n");
                        worksheet.Cells[$"B{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.cust_itemno) ? " " : $"\n{returnItem.cust_itemno}\n");
                        worksheet.Cells[$"C{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"D{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.itemdesc) ? " " : $"\n{returnItem.itemdesc}\n");
                        worksheet.Cells[$"D{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"E{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.itemno) ? " " : $"\n{returnItem.itemno}\n");
                        worksheet.Cells[$"E{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"F{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.deliveredQty?.ToString()) ? "0" : $"\n{returnItem.deliveredQty?.ToString("F2", CultureInfo.InvariantCulture)}\n");
                        worksheet.Cells[$"F{rowIndex}"].SetStyle(alignmentStyle);
                        worksheet.Cells[$"G{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.recvQty.ToString()) ? "0" : $"\n{returnItem.recvQty?.ToString("F2", CultureInfo.InvariantCulture)}\n");
                        worksheet.Cells[$"G{rowIndex}"].SetStyle(alignmentStyle);
                        worksheet.Cells[$"H{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.returnQty.ToString()) ? "0" : $"\n{returnItem.returnQty?.ToString("F2", CultureInfo.InvariantCulture)}\n");
                        worksheet.Cells[$"H{rowIndex}"].SetStyle(alignmentStyle);
                        worksheet.Cells[$"I{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.uom) ? " " : $"\n{returnItem.uom}\n");
                        worksheet.Cells[$"I{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"J{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.remarks) ? " " : $"\n{returnItem.remarks}\n");
                        worksheet.Cells[$"J{rowIndex}"].SetStyle(globalStyle);
                        worksheet.Cells[$"K{rowIndex}"].PutValue(string.IsNullOrEmpty(returnItem.reasoncode) ? " " : $"\n{returnItem.reasoncode}\n");
                        worksheet.Cells[$"K{rowIndex}"].SetStyle(globalStyle);

                        //Aspose.Cells.Row row = worksheet.Cells.Rows[rowIndex];


                        //style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                        //StyleFlag flag = new StyleFlag();
                        //flag.Borders = true;
                        //row.ApplyStyle(style, flag);
                        rowIndex++;
                    }
                    worksheet.AutoFitRows(true);
                    worksheet.PageSetup.SetFooter(2, $"Print Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");


                    //string outputFolder = Directory.GetCurrentDirectory() + "\\wwwroot\\GoodsReturnFiles";
                    DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    string finalFile = $"{outputFolder}/GRN_{_returnReport.goodsreturnheaderinfo.shipmentno}_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                    newWorkbook.Save(finalFile);
                    newWorkbook.Dispose();
                    Directory.Delete(tempTemplate, true);
                    filePath = finalFile;
                }
                else
                {
                    worksheet.Cells[$"A{rowIndex}"].PutValue("No Record Available");
                    string finalFile = $"{outputFolder}/GRN_{_returnReport.goodsreturnheaderinfo.shipmentno}_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                    newWorkbook.Save(finalFile);
                    newWorkbook.Dispose();
                    Directory.Delete(tempTemplate, true);
                    filePath = finalFile;
                }
                #endregion
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in PrintGoodsReturn (while appending data in template) - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return filePath;
        }


        private void SetAsposeLicense()
        {
            try
            {
                Aspose.Cells.License licenseExcel = new Aspose.Cells.License();
                licenseExcel.SetLicense("Aspose.Total.NET.lic");
                Aspose.Pdf.License pdfLicence = new Aspose.Pdf.License();
                pdfLicence.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in setting Aspose License - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
        }
        #endregion

    }
    
}
