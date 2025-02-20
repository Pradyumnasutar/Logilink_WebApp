using LeSDataMain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Logistic_Management_Lib;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Aspose.Cells;
using Aspose.Cells.Drawing;
using Logistic_Management_Lib.Model;
using LeS_LogiLink_WebApp.Data;
using GlobalTools = LeS_LogiLink_WebApp.Data.GlobalTools;
using System.Globalization;
using Aspose.Pdf;
using System.Drawing;
using LeS_LogiLink_WebApp.Models;
using Aspose.Cells.Rendering;
using System.Reflection;
using LeS_LogiLink_WebApp.Interface;
using System.ComponentModel.Design;

namespace LeS_LogiLink_WebApp.Controllers
{
    public class OutboundController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;
        private IUserDefaultData UserDefaultData;
        private IOutbound _ioutbound;
        public OutboundController(IConfiguration _configuration, ApiCallRoutine apiroutine, IUserDefaultData _userdefaultdata, IOutbound party)
        {
            _ioutbound = party;
            Configuration = _configuration;
            _apiroutine = apiroutine;
            UserDefaultData = _userdefaultdata;
        }
        #region Outbound Shipment List
        public IActionResult OutboundShipmentsList()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                ViewBag.Module = "Outbound shipment";
                ViewBag.SubTitle = "Outbound shipment";
                ViewBag.SubTitleUrl = "OutboundShipmentsList";

                ViewBag.EnableFilter = true;
                return View();
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in OutboundShipmentsList - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }

        }
        //public IActionResult GetOutboundShipmentLists(string custCode="",string Custname="",int transtypeid=0,string jobno="",string Vesselname="")
        //{
        //    JsonResult _result = Json(new object[] { new object() });
        //    try
        //    {
        //        int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
        //        string jsonString = $"{{" +
        //                   $"\"companyid\": {companyid}, " +
        //                   $"\"custCode\": \"{custCode}\", " +
        //                   $"\"custName\": \"{Custname}\", " +
        //                   $"\"transTypeid\": {transtypeid}, " +
        //                   $"\"jobno\": \"{jobno}\", " +
        //                   $"\"vesselName\": \"{Vesselname}\"" +
        //        $"}}";
        //        string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentList", jsonString);
        //        var Data = JToken.Parse(jsonData);
        //        _result = Json(new {   data = jsonData });

        //    }
        //    catch(Exception ex)
        //    {

        //    }
        //    return _result;
        //}
        public IActionResult GetOutboundShipmentTable(int ValueRD)
        {
            JsonResult _result = Json(new object[] { new() });
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int _rdValue = (HttpContext.Session.GetInt32("RANDOM") != null) ? convert.ToInt(HttpContext.Session.GetInt32("RANDOM")) : 0;
                if (_rdValue <= 0)
                {
                    HttpContext.Session.SetString("TRIPLIST_DATA", "");
                    HttpContext.Session.SetInt32("RANDOM", ValueRD);
                }
                else
                {
                    if (ValueRD != _rdValue)
                    {
                        HttpContext.Session.SetString("TRIPLIST_DATA", "");
                        HttpContext.Session.SetInt32("RANDOM", ValueRD);
                    }
                }
                var strList = HttpContext.Session.GetString("TRIPLIST_DATA");
                List<Logistic_Management_Lib.Model.V_Shipment_Info> list = new List<V_Shipment_Info>();
                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                var searchValue = convert.ToString(Request.Form["search[value]"]);
                int _recordTotal = list?.Count ?? 0;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : list.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 1;

                int companyid = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (HttpContext.Request.Query.ContainsKey("companyid")&& convert.ToInt(HttpContext.Request.Query["companyid"])> 0)
                {
                    
                    companyid = HttpContext.Request.Query["companyid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["companyid"]) : 0;

                }
                string shipmentno = HttpContext.Request.Query["shipmentNo"].ToString();
                int statusid = HttpContext.Request.Query["status"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["status"]) : 0;
                int customerid = HttpContext.Request.Query["customerid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["customerid"]) : 0;
                int sessionCustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                if (sessionCustId > 0)
                {
                    customerid = sessionCustId;
                }
                int transTypeid = HttpContext.Request.Query["transportType"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["transportType"]) : 0;
                string jobno = HttpContext.Request.Query["jobNo"].ToString();
                string vesselName = HttpContext.Request.Query["vesselName"].ToString();

                //Update by Sadanand 21.06.2024
                string dtFrom = "";
                string dtTo = "";
                bool isDeliveryToday = false;
                bool isDeliveryin3days = false;
                bool isDeliveryThisWeek = false;
                dtFrom = HttpContext.Request.Query["fromDate"].ToString();
                dtTo = HttpContext.Request.Query["toDate"].ToString();

                if (HttpContext.Request.Query["deliveryIn"].ToString().Length > 0)
                {
                    string deliveryIn = HttpContext.Request.Query["deliveryIn"].ToString();
                    switch (deliveryIn)
                    {
                        case "day":
                            isDeliveryToday = true;
                            break;
                        case "3days":
                            isDeliveryin3days = true;
                            break;
                        case "week":
                            isDeliveryThisWeek = true;
                            break;
                    }
                }
                else
                {
                }
                string jsonString = $"{{" +
                            $"\"companyid\": {companyid}, " +
                            $"\"shipmentno\": \"{shipmentno}\", " +
                            $"\"statusid\": {statusid}, " +
                            $"\"custCode\": \"{""}\", " +
                            $"\"customerid\": {customerid}, " +
                            $"\"custName\": \"{""}\", " +
                            $"\"transTypeid\": {transTypeid}, " +
                            $"\"jobno\": \"{jobno}\", " +
                            $"\"vesselName\": \"{vesselName}\", " +
                            $"\"dtFrom\": \"{dtFrom}\", " +
                            $"\"dtTo\": \"{dtTo}\", " +
                            $"\"skip\": \"{skip}\", " +
                            $"\"pagesize\": \"{pageSize}\", " +
                            $"\"sortcolumn\": \"{sortColumn}\", " +
                            $"\"sortingorder\": \"{sortColumnDir}\", " +
                            $"\"quicksearchvalue\": \"{searchValue}\", " +
                            $"\"isDeliveryToday\": {isDeliveryToday.ToString().ToLower()}, " +
                            $"\"isDeliveryThisWeek\": {isDeliveryThisWeek.ToString().ToLower()}, " +
                            $"\"isDeliveryin3days\": {isDeliveryin3days.ToString().ToLower()} " +
                            $"}}";

                //string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentList", jsonString);
                string jsonData = _ioutbound.GetOutboundShipmentList(jsonString);
                var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(jsonData);
                if (apires != null && apires.isSuccess)
                {
                    list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Shipment_Info>>(apires.data.ToString());
                    _recordTotal = apires.totalRecords;
                }
                else
                {
                    throw new Exception("Something went wrong on our side, Please contact support!");
                }
                HttpContext.Session.SetString("TRIPLIST_DATA", JsonConvert.SerializeObject(list));

                //if (HttpContext.Request.Query.Count > 0)
                //{
                //    list = SetFilterCriteria(list);
                //}
                if (HttpContext.Request.Query["deliveryIn"].ToString() == "3days")
                {
                    DateTime today = DateTime.Today;
                    DateTime endDate = today.AddDays(3);
                    list = list.Where(m => m.delivery_date.HasValue && m.delivery_date.Value > today && m.delivery_date.Value <= endDate).ToList();
                }

                _result = Json(new { draw = draw, recordsFiltered = _recordTotal, recordsTotal = _recordTotal, data = list });

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetOutboundShipmentTable : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
            }
            return _result;
        }
        private List<Logistic_Management_Lib.Model.V_Shipment_Info> SetFilterCriteria(List<Logistic_Management_Lib.Model.V_Shipment_Info> _list)
        {
            if (HttpContext.Request.Query["shipmentNo"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["shipmentNo"]);
                _list = _list.Where(a => a.order_no != null && _queryvar.ToUpper().Trim().Contains(Convert.ToString(a.order_no).ToUpper().Trim())).ToList();
            }

            if (HttpContext.Request.Query["jobNo"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["jobNo"]);
                _list = _list.Where(a => a.jobno != null && _queryvar.ToUpper().Trim().Contains(Convert.ToString(a.jobno).ToUpper().Trim())).ToList();
            }

            if (HttpContext.Request.Query["transportType"].ToString().Length > 0)
            {
                int _queryvar = Convert.ToInt32(HttpContext.Request.Query["transportType"]);
                _list = _list.Where(a => a.transport_type_id != null && Convert.ToInt32(a.transport_type_id) == _queryvar).ToList();
            }

            if (HttpContext.Request.Query["customerid"].ToString().Length > 0)
            {
                int _queryvar = Convert.ToInt32(HttpContext.Request.Query["customerid"]);
                _list = _list.Where(a => a.receiverid != null && Convert.ToInt32(a.receiverid) == _queryvar).ToList();
            }

            if (HttpContext.Request.Query["status"].ToString().Length > 0)
            {
                int _queryvar = Convert.ToInt32(HttpContext.Request.Query["status"]);
                _list = _list.Where(a => a.shipment_statusid != null && a.shipment_statusid == _queryvar).ToList();
            }

            if (HttpContext.Request.Query["vesselName"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["vesselName"]);
                _list = _list.Where(a => a.vessel_name != null && a.vessel_name.ToUpper().Trim().Contains(_queryvar.ToUpper().Trim())).ToList();
            }


            return _list;
        }
        public IActionResult SetShipmentListDropDown()
        {
            List<string> moduleIds = new List<string> { "1", "2" };
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), moduleIds, HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.V_Shipment_Info> list = new List<Logistic_Management_Lib.Model.V_Shipment_Info>();
            JsonResult _result = Json(new { Data = list });
            //06-06-2024
            try
            {
                FilterShipmentList ModalFShipList = new FilterShipmentList();
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                int customerid = HttpContext.Request.Query["customerid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["customerid"]) : 0;
                int sessionCustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                if (sessionCustId > 0)
                {
                    customerid = sessionCustId;
                }
                //string jsonString = $"{{" +
                //           $"\"companyid\": {companyid}, " +
                //           $"\"shipmentno\": \"{""}\", " +
                //           $"\"statusid\": \"{0}\", " +
                //           $"\"custCode\": \"{""}\", " +
                //           $"\"custName\": \"{""}\", " +
                //           $"\"transTypeid\": {0}, " +
                //           $"\"jobno\": \"{""}\", " +
                //           $"\"vesselName\": \"{""}\"" +
                //$"}}";
                ModalFShipList.companyid = companyid;
                ModalFShipList.custCode = "";
                ModalFShipList.custName = "";
                ModalFShipList.customerid = customerid;
                ModalFShipList.jobno = "";
                ModalFShipList.quicksearchvalue = "";
                ModalFShipList.shipmentno = "";
                ModalFShipList.skip = 0;
                ModalFShipList.vesselName = "";
                ModalFShipList.pagesize = -1;
                ModalFShipList.sortcolumn = "";
                ModalFShipList.sortingorder = "";

                ModalFShipList.statusid = 0;
                ModalFShipList.transTypeid = 0;
                string jsonString = JsonConvert.SerializeObject(ModalFShipList);
                string jsonData = _ioutbound.GetOutboundShipmentList(jsonString);
                //string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentList", jsonString);
                //string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentList", JsonConvert.SerializeObject(ModalFShipList));

                if (!string.IsNullOrEmpty(jsonData) && !string.IsNullOrWhiteSpace(jsonData))
                {
                    var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(jsonData);
                    if (apires != null && apires.isSuccess)
                    {
                        list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Shipment_Info>>(apires.data.ToString());
                        list = list.Where(m => m.shipment_statusdesc.ToUpper().Contains("DRAFT")).ToList();
                    }
                }

                _result = Json(new { data = list });
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SetShipmentListDropDown - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);

            }
            return _result;
        }
        public IActionResult GetModuleStatuses(int moduleId)
        {
            List<string> moduleIds = new List<string> { "1", "2" };
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), moduleIds, HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;


            List<V_MODULE_STATUSES> Model = new List<V_MODULE_STATUSES>();
            JsonResult _result = Json(new { Data = Model });

            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();

                if (moduleId > 0)
                {
                    string jsonData = HttpContext.Session.GetString("LogisticModuleStatus" + moduleId);
                    Model = JsonConvert.DeserializeObject<List<V_MODULE_STATUSES>>(jsonData);
                    _result = Json(new { Data = Model });
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetModuleStatuses - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }

            return _result;
        }
        public IActionResult GetCustomers()
        {
            List<string> moduleIds = new List<string> { "1", "2", "3" };
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), moduleIds, HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Customer> Model = new List<Customer>();
            JsonResult _result = Json(new { Data = Model });
            int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();

                if (companyId > 0)
                {
                    QueryParam.Add("companyid", companyId.ToString());
                    //string jsonData = _apiroutine.PostAPI("Logistic", "GetCustomerInfoList", null, null, QueryParam);
                    string jsonData = _ioutbound.GetCustomersList(QueryParam);
                    Model = JsonConvert.DeserializeObject<List<Customer>>(jsonData);
                    Model = Model.Where(x => x.Cust_Type == "Customer").ToList();
                    _result = Json(new { Data = Model });
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetCustomers - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }

            return _result;
        }
        public IActionResult GetTransportTypeList()
        {
            List<string> moduleIds = new List<string> { "1", "3" };
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), moduleIds, HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE> Model = new List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE>();
            JsonResult _result = Json(new { Data = Model });
            try
            {

                string jsonData = _apiroutine.PostAPI("Logistic", "GetTransportTypeList", null);
                Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE>>(jsonData);
                _result = Json(new { Data = Model });


            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetTransportTypeList - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;

        }
        public IActionResult GetVesselMaster()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.VesselDetails> Model = new List<Logistic_Management_Lib.Model.VesselDetails>();
            JsonResult _result = Json(new { Data = Model });

            try
            {
                var response = _apiroutine.PostAPI("Logistic", "GetVesselList", null);
                if (response != "")
                {
                    Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.VesselDetails>>(response);
                    _result = Json(new { Data = Model });

                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetVesselMaster - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult GetAnchorageMast()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.Mast_Anchorage> Model = new List<Logistic_Management_Lib.Model.Mast_Anchorage>();
            JsonResult _result = Json(new { Data = Model });
            try
            {

                string jsonData = _apiroutine.PostAPI("Logistic", "GetAnchorageList", null);
                Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Anchorage>>(jsonData);
                _result = Json(new { Data = Model });


            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetAnchorageMast - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        #endregion Outbound Shipment List

        #region Create Outbound Shipment
        public IActionResult CreateOutboundShipmentDetails(string _model)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                Logistic_Management_Lib.CreateShipmentOrderModal model = new CreateShipmentOrderModal();
                ViewBag.Module = "Outbound shipment";
                ViewBag.SubTitle = "Outbound shipment";
                ViewBag.SubTitleUrl = "CreateOutboundShipmentDetails";
                string keyName = "SHIPMENTDETAILSWITHDOs" + UserDefaultData.username;
                if (!string.IsNullOrEmpty(_model))
                {
                    HttpContext.Session.SetString(keyName, _model);
                }
                else
                {
                    HttpContext.Session.SetString(keyName, "");
                }
                model.shipment_Info = new Logistic_Management_Lib.Model.CURD_Shipment_Info();
                model.shipment_trip_plan = new Logistic_Management_Lib.Model.CURD_SHIPMENT_TRIP_PLAN();
                model.shipment_document = new List<Logistic_Management_Lib.Model.Shipment_Documents>();
                model.deliveryOrderIds = new List<int>();


                return View(model);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in CreateOutboundShipmentDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }



        }
        public IActionResult InitializeCreateShipmentWithDOs()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            RootObject model = new RootObject();
            try
            {
                string keyName = "SHIPMENTDETAILSWITHDOs" + UserDefaultData.username;
                string data = HttpContext.Session.GetString(keyName);
                if (!string.IsNullOrEmpty(data))
                {
                    model = JsonConvert.DeserializeObject<RootObject>(data);
                }
                var daata = new { data = model };
                return Json(daata);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in InitializeCreateShipmentWithDOs - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }

        }
        public IActionResult SaveCreatedOutboundShipmentDetails(string OutboundShipmentData)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            var _result = new { result = false, msg = "Something went wrong!" };
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int ShipmentId = 0;
                var ModelData = JsonConvert.DeserializeObject<CreateShipmentOrderModal>(OutboundShipmentData);
                int UserId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));;
                int CustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                int CompanyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                ModelData.shipment_Info.shipmentid = ShipmentId;
                ModelData.createdBy = UserId;

                ModelData.shipment_Info.companyid = CompanyId;


                if (ModelData != null)
                {
                    if (ModelData.shipment_Info.shipmentid == 0)
                    {
                        //string Modified = JsonConvert.SerializeObject(ModelData);
                        //var response = _apiroutine.PostAPI("Logistic", "CreateShipmentDetails", Modified);
                        var response = _ioutbound.SaveCreatedOutboundShipmentDetails(ModelData, CustId,CompanyId);

                        if (response != null && response.Length > 0)
                        {
                            var data = JsonConvert.DeserializeObject<ApiResponse>(response);

                            if (data != null)
                            {
                                if (data.Data != null && data.Data.ToString().Contains("1"))
                                {
                                    var dataObject = JObject.Parse(data.Data.ToString());
                                    int shipmentId = dataObject["shipmentId"].Value<int>();
                                    var daata = new { result = true, msg = "Outbound shipment successfully updated!", shipmentid = shipmentId };
                                    return Json(daata);
                                }
                                else
                                {
                                    var daata = new { result = false, msg = data.Message };
                                    return Json(daata);
                                }
                            }
                            else
                            {
                                var daata = new { result = false, msg = "Something went wrong!" };
                                return Json(daata);
                            }
                        }
                        else
                        {
                            var daata = new { result = false, msg = "Unable to save outbound shipment details!" };
                            return Json(daata);
                        }
                    }
                    else
                    {
                        var daata = new { result = false, msg = "Unable to save outbound shipment details!" };
                        return Json(daata);
                    }
                }
                else
                {
                    var Res = new { result = false, msg = "Unable to save outbound shipment details!" };
                    return Json(Res);
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SaveCreatedOutboundShipmentDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                var sData = new { result = false, msg = ex.Message };
                return Json(sData);
            }
        }
        public IActionResult CreateOutboundShipmentwithDOs(string[] _DOids)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                Logistic_Management_Lib.CreateShipmentOrderModal model = new CreateShipmentOrderModal();
                model.shipment_Info = new Logistic_Management_Lib.Model.CURD_Shipment_Info();
                model.shipment_trip_plan = new Logistic_Management_Lib.Model.CURD_SHIPMENT_TRIP_PLAN();
                model.shipment_document = new List<Logistic_Management_Lib.Model.Shipment_Documents>();
                model.deliveryOrderIds = _DOids.Select(int.Parse).ToList();
                var sd = JsonConvert.SerializeObject(model);

                var daata = new { result = true, msg = "", data = model };
                return Json(daata);

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in CreateOutboundShipmentwithDOs - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
        }
        public IActionResult ValidateDOswithNewShipment(string[] _DOids)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            RootObject model = new RootObject();
            try
            {
                string strcompanyId = HttpContext.Session.GetString("CompanyId");
                int companyId = convert.ToInt(strcompanyId);
                List<int> doids = _DOids.Select(int.Parse).ToList();
                string doidsjson = JsonConvert.SerializeObject(doids);

                string body = $"{{" +
                               $"\"companyid\": {companyId}, " +
                               $"\"lstdo\": {doidsjson} " +
                    $"}}";
                //string Data = _apiroutine.PostAPI("Logistic", "GetNewShipmentWithDOList", body);
                string Data = _ioutbound.ValidateDOswithNewShipment(body);
                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    if (CommonRoutine.IsValidJson(Data))
                    {
                        model = JsonConvert.DeserializeObject<RootObject>(Data);

                    }
                }
                if (model.data != null)
                {
                    int _recordTotal = model.data.deliveryorders.Count;
                    _result = Json(new { recordsTotal = _recordTotal, data = model });
                }
                else
                {
                    int _recordTotal = 0;
                    _result = Json(new { recordsTotal = _recordTotal, data = model });
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in CreateOutboundShipmentwithDOs - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
            return _result;
        }
        #endregion Create Outbound Shipment

        #region Edit Outbound Shipment
        public IActionResult EditOutboundShipmentDetails(string ShipmentId)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int shipmentId = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(ShipmentId);
                if (shipmentId == 0)
                {
                    LeSDM.AddLog("Oops, decrypted id is 0 for :" + shipmentId);
                    return Redirect("/Home/Error?statuscode=400");
                }

                ViewBag.Module = "Outbound Shipments";
                ViewBag.SubTitle = "Outbound Shipments";
                ViewBag.SubTitleUrl = "OutboundShipmentsList";

                ViewBag.Key = shipmentId;
                OutboundShipmentUpdateModal Modal = new OutboundShipmentUpdateModal();
                Logistic_Management_Lib.UpdateShipmentOrderModal Model = new UpdateShipmentOrderModal();
                Model.shipment_Info = new Logistic_Management_Lib.Model.CURD_Shipment_Info();
                Model.shipment_trip_plan = new Logistic_Management_Lib.Model.CURD_SHIPMENT_TRIP_PLAN();
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", shipmentId.ToString());
                QueryParam.Add("CompanyId", companyid.ToString());
                //string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentInfo", null, null, QueryParam);
                string jsonData = _ioutbound.EditOutboundShipmentDetails(true, QueryParam);
                if (jsonData != null && jsonData != "")
                {
                    Model.shipment_Info = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.CURD_Shipment_Info>(jsonData);

                    List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> list;
                    // int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));

                    //string jsData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, QueryParam);
                    string jsData = _ioutbound.EditOutboundShipmentDetails(false, QueryParam);
                    list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS>>(jsData);
                    Modal.UpdateShipmentOrderModal = Model;
                    Modal.ShipmentDeliveryorders = list;
                    Modal.IsBuyer = UserDefaultData.isbuyer;
                 
                    return View(Modal);
                }
                else
                {
                    LeSDM.AddLog("Exception in EditOutboundShipmentDetails - No response from API");
                    return RedirectToAction("Error", "Home", new { statuscode = 404 });
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in EditOutboundShipmentDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
        }
        public IActionResult SaveOutboundShipmentDetails(string OutboundData)
        {
            var _result = new { result = false, msg = "Something went wrong !" };
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                var Modeldata = JsonConvert.DeserializeObject<UpdateShipmentOrderModal>(OutboundData);
                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
                int CompanyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                int CustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                Modeldata.updatedBy = UserId;
                Modeldata.shipment_Info.companyid = CompanyId;
                
                if (Modeldata != null)
                {

                    var response = _ioutbound.SaveOutboundShipmentDetails(Modeldata, CustId, CompanyId);
                    if (response != null && response.Length > 0)
                    {

                        var data = JsonConvert.DeserializeObject<ApiResponse>(response);
                        if (data != null)
                        {
                            if (data.Data != null && data.Data.ToString().Contains("1"))
                            {
                                var daata = new { result = true, msg = "Outbound shipment successfully updated!" };
                                return Json(daata);
                            }
                            else
                            {
                                var daata = new { result = false, msg = data.Message };
                                return Json(daata);
                            }
                        }
                        else
                        {
                            var daata = new { result = false, msg = "Something went wrong !" };
                            return Json(data);
                        }
                    }
                }
                else
                {
                    var Res = new { result = false, msg = "Unable to save outbound shipment details!" };
                    return Json(Res);
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SaveOutboundShipmentDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                var sData = new { result = false, msg = ex.Message };
                return Json(sData);
            }
            return Json(_result);


        }
        public IActionResult GetDeliveryOrderToAssign(int customerId, string jobno, string vesselName, string vesselETA)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Delivery_Orders_DropDown> Modal = new();
            var _result = new { result = false, msg = "Something went wrong !", data = Modal };
            try
            {
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));

                if (customerId > 0)
                {
                    //IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                    //QueryParam.Add("companyid", companyid.ToString());
                    //QueryParam.Add("customerid", customerId.ToString());

                    string body = $"{{" +
                              $"\"companyid\": {companyid}, " +
                              $"\"customerid\": {customerId}, " +
                              $"\"jobno\": \"{jobno}\", " +
                              $"\"vesselName\": \"{vesselName}\", " +
                              $"\"vesselETA\": \"{Convert.ToDateTime(vesselETA).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}\"}}";

                    string jsonData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersDropDownByVerification", body, null, null);
                    Modal = JsonConvert.DeserializeObject<List<Delivery_Orders_DropDown>>(jsonData);

                    if (Modal != null)
                    {
                        _result = new { result = true, msg = "", data = Modal };
                    }
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SaveOutboundShipmentDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return Json(_result);
        }
        public IActionResult GetShipmentInfo(int ShipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            Logistic_Management_Lib.Model.V_Shipment_Info Model = new Logistic_Management_Lib.Model.V_Shipment_Info();
            JsonResult _result = Json(new { Data = Model });
            int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", ShipmentId.ToString());
                QueryParam.Add("Companyid", companyid.ToString());
                //string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentInfo", null, null, QueryParam);
                string jsonData = _ioutbound.EditOutboundShipmentDetails(true, QueryParam);
                Model = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.V_Shipment_Info>(jsonData);
                _result = Json(new { Data = Model });
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetShipmentInfo - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult GetShipmentTripPlan(int ShipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            Logistic_Management_Lib.Model.V_SHIPMENT_TRIP_PLAN Model = new Logistic_Management_Lib.Model.V_SHIPMENT_TRIP_PLAN();
            JsonResult _result = Json(new { Data = Model });
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", ShipmentId.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetVShipmentTripPlan", null, null, QueryParam);
                Model = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.V_SHIPMENT_TRIP_PLAN>(jsonData);
                _result = Json(new { Data = Model });
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetShipmentTripPlan - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult GetDeliveryOrderList(int ShipmentId)
        {
            JsonResult _result = Json(new object[] { new object() });
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> list;
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", ShipmentId.ToString());
                //string jsonData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, QueryParam);
                string jsonData = _ioutbound.EditOutboundShipmentDetails(false, QueryParam);
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

                int _recordsFiltered = list.Count;
                list = list.Skip(skip).Take(pageSize).ToList();

                _result = Json(new { draw = draw, recordsFiltered = _recordsFiltered, recordsTotal = _recordTotal, data = list });

            }
            catch (Exception e)
            {
                LeSDM.AddLog("Exception in GetDeliveryOrderList : " + e.GetBaseException().ToString());
                LeSDM.AddLog("Stacktrace - " + e.StackTrace);
            }
            return _result;
        }
        public IActionResult GetDeliveryOrderCount(int ShipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            try
            {

                List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> list;
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Shipmentid", ShipmentId.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByShipmentId", null, null, QueryParam);
                list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS>>(jsonData);
                _result = Json(new { docount = list.Count });

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetDeliveryOrderCount - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult GetOutboundShipmentDocuments(int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

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
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

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
                LeSDM.AddLog("Exception in UploadAttachments - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "Unable to upload file: " + Path.GetFileName(formFile.FileName) });
            }
        }
        [HttpPost]
        public IActionResult DeleteAttachment(string filename, int shipmentId, int documentId)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                // Handle document deletion logic with documentId
                if (DeleteShipmentDocument(documentId, shipmentId))
                {
                    return Json(new { success = true, message = "File successfully deleted: " + filename });
                }
                else
                {
                    return Json(new { success = false, message = "Oops,something went wrong! " });
                }


            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DeleteAttachment - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(new { success = false, message = "Unable to delete file: " + filename });
            }
        }
        public (int, string) AttachDocumentInServer(IFormFile upfile, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return (0, "");

            int id = 0;
            string msg = "";
            int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
            try
            {

                if (upfile != null && upfile.Length > 0)
                {
                    var Model2 = new AttachDocumentsDataModal();
                    Model2.DocRefId = shipmentid;
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

                    Model2.Companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));

                    Model2.UpdatedBy = UserId;
                    var jstring = JsonConvert.SerializeObject(Model2);
                    var response = _apiroutine.PostAPI("Logistic", "UploadShipmentDocument", jstring);
                    if (response != "")
                    {
                        var result = JsonConvert.DeserializeObject<ApiResponse>(response);
                        if (result.isSuccess)
                        {
                            var Model = JsonConvert.DeserializeObject<Logistic_Management_Lib.Model.Shipment_Documents>(result.Data.ToString());
                            id = (int)Model.ShipmentDocumentId;
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
        public bool DeleteShipmentDocument(int shipmentDocumentid, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return false;

            bool res = false;
            int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
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
        public IActionResult DownloadshipmentAttachement(int documentid, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

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
        //public string GetAttachmentPath(int moduleid)
        //{
        //    string AttachPath = "";
        //    int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
        //    try
        //    {
        //        IDictionary<string, string> QueryParam = new Dictionary<string, string>();
        //        QueryParam.Add("Moduleid", moduleid.ToString());
        //        QueryParam.Add("companyid", companyId.ToString());
        //        var response = _apiroutine.PostAPI("Logistic", "GetSiteConfigByModuleidAndCompanyid", null, null, QueryParam);
        //        if (response != null)
        //        {
        //            var dec = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_SITE_CONFIG>>(response);
        //            if (dec != null)
        //            {

        //                var Attach = dec.Where(x => x.siteconfig_paramid == 1015).FirstOrDefault();
        //                return Attach.param_value;
        //            }
        //            else
        //            {
        //                LeSDM.AddLog("no data found in response : GetAttachmentPath");
        //                return "";
        //            }
        //        }
        //        else
        //        {
        //            LeSDM.AddLog("no response received from API Logistic/GetSiteConfigByModuleidAndCompanyid : GetAttachmentPath");
        //            return "";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LeSDM.AddLog("Exception in GetAttachmentPath - " + ex.GetBaseException());
        //        LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
        //    }
        //    return AttachPath;
        //}

        #endregion Edit Outbound Shipment 
        #region Unassigned Delivery Order by Shipment
        public IActionResult UnassignedDeliveryOrder(int _DoId, int _ShipmentId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
                List<int> DOrdersNos = new()
                {
                    _DoId
                };
                string json = JsonConvert.SerializeObject(DOrdersNos);
                string jsonString = $"{{" +
                               $"\"deliveryOrderIds\": {json}, " +
                               $"\"shipmentId\": \"{_ShipmentId}\", " +
                            $"\"updated_by\": {UserId}" +
                    $"}}";
                //var res = _apiroutine.PostAPI("Logistic", "UpdateUnAssignShipmentToDeliveryOrder", jsonString);
                var res = _ioutbound.UnassignedDeliveryOrder(jsonString);
                if (res != "" && res.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                    if (data != null)
                    {
                        if (data.Status != null && data.Status == "SUCCESS")
                        {
                            var daata = new { result = true, msg = data.Message };
                            return Json(daata);
                        }
                        else
                        {
                            var daata = new { result = false, msg = data.Message };
                            return Json(daata);
                        }
                    }
                    else
                    {
                        var daata = new { result = false, msg = "Something went wrong !" };
                        return Json(data);
                    }
                }
                else
                {
                    var data = new { result = false, msg = "Something went wrong !" };
                    return Json(data);
                }


            }
            catch (Exception ex)
            {
                //CommonRoutine.SetAudit("TransportType", "Error", "", "Exception in TransportType " + ex.GetBaseException(), "");
                LeSDM.AddLog("Exception in UnassignedDeliveryOrder - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }
        }
        #endregion Unassigned Delivery Order by Shipment

        #region Print Shipment orders
        // Added by Gaurav Tiwary 12-june-2024
        private List<string> DeliveryOrdersPdfPaths = new List<string>();
        public IActionResult PrintShipmentOrders(int _shipmentId, bool printWithAllOrders)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            byte[] pdfBytes = null;
            try
            {
                SetAsposeLicense();
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
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
                    LeSDM.AddLog($"Error while searching file '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);

                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Shipment Order - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }

        }
        OutboundPrintModel printModel;
        public OutboundPrintModel DeserializeOutboundShipment(string _shipmentInfo, string _shipmentTripPlan, string _deliveryOrderList, string _shipmentDocument, string _companyInfo)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return null;

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
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return "";

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
                if(!Path.Exists(Path.Combine(tempTemplate, Path.GetFileName(templatePath))))
                {
                    System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                }
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

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
                    worksheet.PageSetup.HeaderMargin = 0.5;

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
                    worksheet.Cells[$"G{rowIndex}"].SetStyle(globalStyle);
                    //worksheet.Cells[$"H{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrder.dept_code) ? " " : "\n" + deliveryOrder.dept_code);
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
                //for electronic proof of delivery
                //worksheet.Cells["C38"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Emailid) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Emailid}");
                //worksheet.AutoFitRow(37);
                //worksheet.Cells["C40"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Crew) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Crew}");
                //worksheet.AutoFitRow(39);
                //worksheet.Cells["C42"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Role) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Role}");
                //worksheet.AutoFitRow(41);
                //worksheet.Cells["C44"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                //worksheet.AutoFitRow(43);
                //worksheet.Cells["C46"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.initial_Receipt_LoCode) ? ": " : $": {printModel.ShipmentInfo.initial_Receipt_LoCode}");
                //worksheet.AutoFitRow(45);

                //worksheet.Cells["F38"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Emailid) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Emailid}");
                //worksheet.Cells["F40"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Crew) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Crew}");
                //worksheet.Cells["F42"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Role) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Role}");
                //worksheet.Cells["F44"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_Date?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                //worksheet.Cells["F46"].PutValue(string.IsNullOrEmpty(printModel.ShipmentInfo.final_Receipt_LoCode) ? ": " : $": {printModel.ShipmentInfo.final_Receipt_LoCode}");






                //int rw = worksheet.HorizontalPageBreaks.Horizon;


                #endregion
                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                string finalfilename = $"{outputDirectory}/Shipment_Order_{printModel.ShipmentInfo.order_no}_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                newWorkbook.Save(finalfilename);
                newWorkbook.Dispose();
                //newWorkbook.Save($"{outputDirectory}/Shipment_Order_{printModel.ShipmentInfo.order_no}_{DateTime.Now.ToString("ddMMyy")}.xlsx");
                Directory.Delete(tempTemplate, true);
                savedFilePath = finalfilename;
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
                LeSDM.AddLog("Exception in Append Data in Template - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return savedFilePath;
        }
        public List<string> DownloadDOAttachments(List<V_SHIPMENT_DELIVERY_ORDERS> DOs)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return null;

            List<string> attachmentFilePaths = new List<string>();
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
            if (GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext))
            {
                try
                {
                    foreach (Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS _deliveryOrder in _deliveryOrderList)
                    {
                        DeliveryOrderDetailsPrint _deliveryOrderDetails = new();
                        string _id = Convert.ToString(_deliveryOrder.delivery_order_id);
                        int _companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                        _deliveryOrderDetails.LogoModel = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(_companyId)}", ""));
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
                    LeSDM.AddLog("Exception in DeliveryOrderDetails- " + ex.Message);
                    LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                }
            }
        }
        public void PrintDeliveryOrderDetails(DeliveryOrderDetailsPrint deliveryOrderDetails)
        {
            if (GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext))
            {
                try
                {
                    var templatePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Template\\Delivery Order Details.xlsx";
                    var sessionId = HttpContext.Session.Id;

                    var tempTemplate = Directory.GetCurrentDirectory() + $"\\wwwroot\\Template\\temp\\{sessionId}";

                    Directory.CreateDirectory(tempTemplate);
                    System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)), true);

                    Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                    //Workbook newWorkbook = new Workbook();
                    //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
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
                    if (logoByte != null && logoByte.Count() > 0)
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
                    string finalFile = $"{outputFolder}/Delivery_Order_List_{deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no}_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                    newWorkbook.Save(finalFile);
                    newWorkbook.Dispose();
                    Directory.Delete(tempTemplate, true);
                    DeliveryOrdersPdfPaths.Add(finalFile);
                }
                catch (Exception ex)
                {
                    LeSDM.AddLog("Exception in PrintDeliveryOrderDetails - " + ex.Message);
                    LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                }
            }
        }
        public IActionResult PrintOutBoundList()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "1", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ; ;

            byte[] pdfBytes;
            string savedPdfPath = "";
            List<Logistic_Management_Lib.Model.V_Shipment_Info> FilteredList = new();
            try
            {
                SetAsposeLicense();

                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:OutboundListTemplate", "");
                var outputDirectory = Directory.GetCurrentDirectory() + "\\wwwroot\\ShipmentOrderPdfs";

                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both OutboundListTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }

                tempTemplate += $"\\{sessionId}";

                Directory.CreateDirectory(tempTemplate);
                if(!Path.Exists(Path.Combine(tempTemplate, Path.GetFileName(templatePath))))
                {
                    System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                }

                //Workbook templateWorkbook = new Workbook(templatePath);
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();

                int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                string body = string.Empty;
                string shipmentno = "", jobno = "", vesselName = "", dtFrom = "", dtTo = "", customerName = "", transportName = "", statusText = "", sortingColumn = "", sortingOrder = "";
                int statusid = 0, customerid = 0, transTypeid = 0, skip = 0, length = 0;
                bool isDeliveryToday = false;
                bool isDeliveryin3days = false;
                bool isDeliveryThisWeek = false;
                string quickSearch = "";
                if (HttpContext.Request.Query.Count > 0)
                {

                    shipmentno = HttpContext.Request.Query["shipmentNo"].ToString();
                    statusid = HttpContext.Request.Query["status"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["status"]) : 0;
                    skip = HttpContext.Request.Query["skip"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["skip"]) : 0;
                    length = HttpContext.Request.Query["length"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["length"]) : 0;
                    customerid = HttpContext.Request.Query["customerid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["customerid"]) : 0;
                    int sessionCustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                    if (sessionCustId > 0)
                    {
                        customerid = sessionCustId;
                    }
                    transTypeid = HttpContext.Request.Query["transportType"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["transportType"]) : 0;
                    jobno = HttpContext.Request.Query["jobNo"].ToString();
                    if (HttpContext.Request.Query.ContainsKey("companyid")&& convert.ToInt(HttpContext.Request.Query["companyid"]) > 0)
                    {

                        companyId = HttpContext.Request.Query["companyid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["companyid"]) : 0;

                    }
                    vesselName = HttpContext.Request.Query["vesselName"].ToString();
                    customerName = HttpContext.Request.Query["customername"].ToString();
                    transportName = HttpContext.Request.Query["transportName"].ToString();
                    statusText = HttpContext.Request.Query["statusText"].ToString();
                    quickSearch = HttpContext.Request.Query["quickStatus"].ToString();
                    sortingColumn = HttpContext.Request.Query["sortColumn"].ToString() ?? "";
                    sortingOrder = HttpContext.Request.Query["sortOrder"].ToString() ?? "";


                    dtFrom = HttpContext.Request.Query["fromDate"].ToString();
                    dtTo = HttpContext.Request.Query["toDate"].ToString();

                    if (HttpContext.Request.Query["deliveryIn"].ToString().Length > 0)
                    {
                        string deliveryIn = HttpContext.Request.Query["deliveryIn"].ToString();
                        switch (deliveryIn)
                        {
                            case "day":
                                isDeliveryToday = true;
                                quickSearch = "Today's Delivery";
                                break;
                            case "3days":
                                isDeliveryin3days = true;
                                quickSearch = "Next 3 Days Delivery";
                                break;
                            case "week":
                                isDeliveryThisWeek = true;
                                quickSearch = "This Week Delivery";
                                break;
                        }
                    }
                    else
                    {

                    }


                    body = $"{{" +
                               $"\"companyid\": {companyId}, " +
                               $"\"shipmentno\": \"{shipmentno}\", " +
                               $"\"statusid\": {statusid}, " +
                               $"\"custCode\": \"{""}\", " +
                               $"\"customerid\": {customerid}, " +
                               $"\"custName\": \"{""}\", " +
                               $"\"transTypeid\": {transTypeid}, " +
                               $"\"jobno\": \"{jobno}\", " +
                               $"\"vesselName\": \"{vesselName}\", " +
                               $"\"dtFrom\": \"{dtFrom}\", " +
                               $"\"dtTo\": \"{dtTo}\", " +
                               $"\"skip\": \"{0}\", " +
                               $"\"pagesize\": \"{-1}\", " +
                               $"\"quicksearchvalue\": \"{""}\", " +
                               $"\"sortcolumn\": \"{sortingColumn}\", " +
                               $"\"sortingorder\": \"{sortingOrder}\", " +
                               $"\"isDeliveryToday\": {isDeliveryToday.ToString().ToLower()}, " +
                               $"\"isDeliveryThisWeek\": {isDeliveryThisWeek.ToString().ToLower()}, " +
                               $"\"isDeliveryin3days\": {isDeliveryin3days.ToString().ToLower()} " +
                               $"}}";


                }
                else
                {
                    body = $"{{" +
                              $"\"companyid\": {companyId}, " +
                              $"\"shipmentno\": \"{""}\", " +
                              $"\"statusid\": {0}, " +
                              $"\"custCode\": \"{""}\", " +
                              $"\"customerid\": {0}, " +
                              $"\"custName\": \"{""}\", " +
                              $"\"transTypeid\": {0}, " +
                              $"\"jobno\": \"{""}\", " +
                              $"\"vesselName\": \"{""}\", " +
                              $"\"dtFrom\": \"{""}\", " +
                              $"\"dtTo\": \"{""}\", " +
                              $"\"skip\": \"{0}\", " +
                              $"\"pagesize\": \"{-1}\", " +
                              $"\"quicksearchvalue\": \"{quickSearch}\", " +
                              $"\"sortcolumn\": \"{sortingColumn}\", " +
                              $"\"sortingorder\": \"{sortingOrder}\", " +
                              $"\"isDeliveryToday\": {isDeliveryToday.ToString().ToLower()}, " +
                              $"\"isDeliveryThisWeek\": {isDeliveryThisWeek.ToString().ToLower()}, " +
                              $"\"isDeliveryin3days\": {isDeliveryin3days.ToString().ToLower()} " +
                              $"}}";
                }



                string jsonData = _ioutbound.GetOutboundShipmentList( body);
                //string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentList", body);
                var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(jsonData);
                if (apires != null && apires.isSuccess)
                {
                    FilteredList = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Shipment_Info>>(apires.data.ToString());

                }
                else
                {
                    throw new Exception("Something went wrong on side, Please contact support!");
                }
                #region Outbound headers

                worksheet.Cells["C3"].PutValue(string.IsNullOrEmpty(shipmentno) ? ": " : $": {shipmentno}");
                worksheet.Cells["C4"].PutValue(string.IsNullOrEmpty(customerName) ? ": " : $": {customerName}");
                worksheet.Cells["C5"].PutValue(string.IsNullOrEmpty(jobno) ? ": " : $": {jobno}");
                worksheet.Cells["C6"].PutValue(string.IsNullOrEmpty(vesselName) ? ": " : $": {vesselName}");
                worksheet.Cells["J3"].PutValue(string.IsNullOrEmpty(transportName) ? ": " : $": {transportName}");
                worksheet.Cells["J4"].PutValue(string.IsNullOrEmpty(statusText) ? ": " : $": {statusText}");
                worksheet.Cells["J5"].PutValue(string.IsNullOrEmpty(dtFrom) ? ": " : $": {dtFrom}");
                worksheet.Cells["J6"].PutValue(string.IsNullOrEmpty(dtTo) ? ": " : $": {dtTo}");
                //if (isDeliveryToday || isDeliveryin3days || isDeliveryThisWeek) worksheet.Cells["C7"].PutValue($": {quickSearch}");
                //else worksheet.Cells["C7"].PutValue($": {statusText}");
                worksheet.Cells["C7"].PutValue(string.IsNullOrEmpty(quickSearch) ? ": " : $": {quickSearch}");
                #endregion
                int i = 0;
                int rowIndex = 11;
                var globalStyle = worksheet.Cells[$"A{rowIndex}"].GetStyle();
                foreach (var data in FilteredList)
                {
                    i++;
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue($"{i.ToString()}.");
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(data.order_no) ? " " : data.order_no);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(data.shipment_statusdesc) ? " " : data.shipment_statusdesc);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"D{rowIndex}"].PutValue(string.IsNullOrEmpty(data.delivery_date.ToString()) ? " " : data.delivery_date.ToString());
                    worksheet.Cells[$"D{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"E{rowIndex}"].PutValue(string.IsNullOrEmpty(data.cust_code+"-"+ data.cust_name) ? " " : data.cust_code + "-" + data.cust_name);
                    worksheet.Cells[$"E{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"F{rowIndex}"].PutValue(string.IsNullOrEmpty(data.CompanyCode+"-"+data.CompanyName) ? " " : data.CompanyCode + "-" + data.CompanyName);
                    worksheet.Cells[$"F{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"G{rowIndex}"].PutValue(string.IsNullOrEmpty(data.jobno) ? " " : data.jobno);
                    worksheet.Cells[$"G{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"H{rowIndex}"].PutValue(string.IsNullOrEmpty(data.vessel_name) ? " " : data.vessel_name);
                    worksheet.Cells[$"H{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"I{rowIndex}"].PutValue(string.IsNullOrEmpty(data.vessel_eta.ToString()) ? " " : data.vessel_eta.ToString());
                    worksheet.Cells[$"I{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"J{rowIndex}"].PutValue(string.IsNullOrEmpty(data.anchorage_description) ? " " : data.anchorage_description);
                    worksheet.Cells[$"J{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"K{rowIndex}"].PutValue(string.IsNullOrEmpty(data.agent) ? " " : data.agent);
                    worksheet.Cells[$"K{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"L{rowIndex}"].PutValue(string.IsNullOrEmpty(data.supply_boat) ? " " : data.supply_boat);
                    worksheet.Cells[$"L{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"M{rowIndex}"].PutValue(string.IsNullOrEmpty(data.transport_type_description) ? " " : data.transport_type_description);
                    worksheet.Cells[$"M{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"N{rowIndex}"].PutValue(string.IsNullOrEmpty(data.driver_name) ? " " : data.driver_name);
                    worksheet.Cells[$"N{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"O{rowIndex}"].PutValue(string.IsNullOrEmpty(data.Vehicle_no) ? " " : data.Vehicle_no);
                    worksheet.Cells[$"O{rowIndex}"].SetStyle(globalStyle);
                    worksheet.Cells[$"P{rowIndex}"].PutValue(string.IsNullOrEmpty(data.Boarding_Officer_Name) ? " " : data.Boarding_Officer_Name);
                    worksheet.Cells[$"P{rowIndex}"].SetStyle(globalStyle);
                    Aspose.Cells.Row row = worksheet.Cells.Rows[rowIndex];
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    StyleFlag flag = new();
                    flag.Borders = true;
                    row.ApplyStyle(style, flag);
                    rowIndex++;
                }
                worksheet.Cells["C9"].PutValue(string.IsNullOrEmpty(FilteredList.Count.ToString()) ? "0" : FilteredList.Count.ToString());
                Companyinfodata companydetails = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(HttpContext.Session.GetString("CompanyId"))}", ""));
                if (companydetails.base64printLogo != null)
                {

                    byte[] logoByte = Convert.FromBase64String(companydetails.base64printLogo);

                    if (logoByte != null && logoByte.Count() > 0)
                    {
                        // Resize the logo
                        using (MemoryStream ms = new MemoryStream(logoByte))
                        {
                            using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                            {
                                int newWidth = 400; // Set the desired width
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
                        worksheet.PageSetup.SetHeader(0, companydetails.Company_Description);
                    }
                }
                worksheet.PageSetup.SetFooter(0, $" {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                worksheet.AutoFitRows();

                string outputFolder = Directory.GetCurrentDirectory() + "\\wwwroot\\OutBoundList";
                DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                string finalFile = $"{outputFolder}\\OutBound_List_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                newWorkbook.Save(finalFile);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
                savedPdfPath = finalFile;
                //return savedPdfPath;

                string filePath = savedPdfPath;
                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Directory.GetCurrentDirectory() + "\\wwwroot\\OutBoundList\\" + fileName;
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file for delivery order details '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Delivery Order List - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
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
        public IActionResult EncryptId(int id)
        {
            try
            {
                var encryptedId = LeS_LogiLink_WebApp.Data.GlobalTools.EncryptID(id);
                return Json(new { success = true, encryptedId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Cust_Code { get; set; }
        public string Cust_Name { get; set; }
        public string Cust_Type { get; set; }
        public int CompanyId { get; set; }
    }
    public class RootObject
    {
        public CreateShipmentwithDOModal data { get; set; }
        public string status { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }
    }
}
