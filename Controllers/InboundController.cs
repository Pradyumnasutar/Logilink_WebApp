using LeS_LogiLink_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;
using LeSDataMain;
using Microsoft.AspNetCore.Http;
using Logistic_Management_Lib;
using GlobalTools = LeS_LogiLink_WebApp.Data.GlobalTools;
using Logistic_Management_Lib.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using LeS_LogiLink_WebApp.Interface;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.Design;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LeS_LogiLink_WebApp.Controllers
{

    public class InboundController : Controller
    {
        private IInbound _iinbound;
        private IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;
        private IUserDefaultData UserDefaultData;
        public InboundController(IConfiguration _configuration, ApiCallRoutine routine, IUserDefaultData _userdefaultdata, IInbound iinbound)
        {
            Configuration = _configuration;
            _apiroutine = routine;
            UserDefaultData = _userdefaultdata;
            _iinbound = iinbound;
        }
        public IActionResult InboundShipments()
        {

            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                ViewBag.Module = "Inbound Shipments";
                ViewBag.SubTitle = "Inbound Shipments";
                ViewBag.SubTitleUrl = "InboundShipments";

                ViewBag.EnableFilter = true;
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in InboundShipments - " + ex.GetBaseException());
                LeSDM.AddLog("StackTrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
            return View();
        }
        public IActionResult GetInboundShipmentGrid(int ValueRD)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            try
            {
                List<V_INBOUND_SHIPMENTS> inbound_list = new List<V_INBOUND_SHIPMENTS>();
                int _rdValue = (HttpContext.Session.GetInt32("RANDOM") != null) ? convert.ToInt(HttpContext.Session.GetInt32("RANDOM")) : 0;
                if (_rdValue <= 0)
                {
                    HttpContext.Session.SetString("INBOUNDLIST_DATA", "");
                    HttpContext.Session.SetInt32("RANDOM", ValueRD);
                    HttpContext.Session.SetString("FilterDOList_DATA", "");
                }
                else
                {
                    if (ValueRD != _rdValue)
                    {
                        HttpContext.Session.SetString("INBOUNDLIST_DATA", "");
                        HttpContext.Session.SetInt32("RANDOM", ValueRD);
                        HttpContext.Session.SetString("FilterDOList_DATA", "");
                    }
                }
                var strList = HttpContext.Session.GetString("INBOUNDLIST_DATA");
                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                var searchValue = convert.ToString(Request.Form["search[value]"]);
                int _recordTotal = inbound_list.Count;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : inbound_list.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 1;
                if (!string.IsNullOrEmpty(strList))
                {
                    inbound_list = JsonConvert.DeserializeObject<List<V_INBOUND_SHIPMENTS>>(strList);
                }
                else
                {
                    FilterInboundShipment modal = new FilterInboundShipment();
                    string strcompanyId = HttpContext.Session.GetString("CompanyId");
                    int companyId = convert.ToInt(strcompanyId);
                    modal.skip = skip;
                    modal.pagesize = pageSize;
                    modal.quicksearchvalue = searchValue;
                    modal.sortcolumn = sortColumn;
                    modal.sortingorder = sortColumnDir;
                    if (UserDefaultData.isbuyer) { companyId = 0; }
                    if (HttpContext.Request.Query.ContainsKey("companyid") && convert.ToInt(HttpContext.Request.Query["companyid"]) > 0)
                    {

                        companyId = HttpContext.Request.Query["companyid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["companyid"]) : 0;

                    }
                    modal.companyid = companyId;
                    string shipmentno = HttpContext.Request.Query["shipmentNo"].ToString();
                    int statusid = HttpContext.Request.Query["status"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["status"]) : 0;
                    int customerid = HttpContext.Request.Query["customerid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["customerid"]) : 0;
                    int pono = HttpContext.Request.Query["PONo"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["PONo"]) : 0; ;
                    string jobno = HttpContext.Request.Query["jobNo"].ToString();
                    string vesselName = HttpContext.Request.Query["vesselName"].ToString();
                    string dtFrom = HttpContext.Request.Query["fromDate"].ToString();
                    string dtTo = HttpContext.Request.Query["toDate"].ToString();
                    int sessionCustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                    if (sessionCustId > 0)
                    {
                        customerid = sessionCustId;
                    }
                    modal.customerid = customerid;
                    modal.shipmentno = shipmentno;
                    modal.statusid = statusid;
                    modal.vesselName = vesselName;
                    modal.jobno = jobno;
                    modal.pono = pono;
                    modal.dtFrom = dtFrom;
                    modal.dtTo = dtTo;
                    var strBody = JsonConvert.SerializeObject(modal);
                    string Data = _iinbound.GetInboundShipmentList(strBody);

                    if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                    {
                        var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(Data);
                        if (apires != null && apires.isSuccess)
                        {
                            inbound_list = JsonConvert.DeserializeObject<List<V_INBOUND_SHIPMENTS>>(apires.data.ToString());
                            _recordTotal = apires.totalRecords;

                        }
                        else
                        {
                            throw new Exception("Something went wrong on our side, Please contact support!");
                        }

                    }
                    else
                    {
                        LeSDM.AddLog("made bad request to API for GetDeliveryOrderList");
                    }


                }
                //if (HttpContext.Request.Query.Count > 0)
                //{
                //    inbound_list = SetFilterCriteria(inbound_list);
                //}
                List<V_INBOUND_SHIPMENTS> templist = inbound_list;


                int _recordsFiltered = inbound_list.Count;
                _result = Json(new { draw = draw, recordsFiltered = _recordTotal, recordsTotal = _recordTotal, data = inbound_list, TempDoList = templist, Isbuyer = UserDefaultData.isbuyer });

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetInboundShipmentGrid : " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        private List<Logistic_Management_Lib.Model.V_INBOUND_SHIPMENTS> SetFilterCriteria(List<Logistic_Management_Lib.Model.V_INBOUND_SHIPMENTS> _list)
        {
            if (HttpContext.Request.Query["shipmentNo"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["shipmentNo"]);
                _list = _list.Where(a => a.shipmentno != null && _queryvar.ToUpper().Trim().Contains(Convert.ToString(a.shipmentno).ToUpper().Trim())).ToList();
            }

            if (HttpContext.Request.Query["jobNo"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["jobNo"]);
                _list = _list.Where(a => a.jobOrderNo != null && _queryvar.ToUpper().Trim().Contains(Convert.ToString(a.jobOrderNo).ToUpper().Trim())).ToList();
            }

            if (HttpContext.Request.Query["PONo"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["PONo"]);
                _list = _list.Where(a => a.internal_order_no != null && _queryvar.ToUpper().Trim().Contains(a.internal_order_no.ToUpper().Trim())).ToList();
            }

            if (HttpContext.Request.Query["customerid"].ToString().Length > 0)
            {
                int _queryvar = Convert.ToInt32(HttpContext.Request.Query["customerid"]);
                _list = _list.Where(a => a.customerId != null && Convert.ToInt32(a.customerId) == _queryvar).ToList();
            }

            if (HttpContext.Request.Query["status"].ToString().Length > 0)
            {
                int _queryvar = Convert.ToInt32(HttpContext.Request.Query["status"]);
                _list = _list.Where(a => a.statusId != null && a.statusId == _queryvar).ToList();
            }

            if (HttpContext.Request.Query["vesselName"].ToString().Length > 0)
            {
                string _queryvar = Convert.ToString(HttpContext.Request.Query["vesselName"]);
                _list = _list.Where(a => a.vessel_name != null && a.vessel_name.ToUpper().Trim().Contains(_queryvar.ToUpper().Trim())).ToList();
            }


            return _list;
        }
        public IActionResult InboundShipmentDetails(string inboundshipmentid = "", string quotationid = "")
        {
            try
            {
                LinkedPOandDOResponse result2 = null;
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;
                int shipmentId = 0;
                int quotationId = 0;
                if (inboundshipmentid != null && inboundshipmentid.Length > 0)
                {
                    shipmentId = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(inboundshipmentid);
                    result2 = GetLinkedDOandPOlinesCountbyInboundShipId(shipmentId);
                }
                else if (quotationid != "" && quotationid.Length > 0)
                {
                    quotationId = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(quotationid);
                    var result = InsertPoFromEsupplierToLogiLinkbyQuotId(quotationid);

                    var data = _apiroutine.GetAPI("Logistic", $"GetInboundShipmentByquotationId?quotationid={quotationId}", null, null);

                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            shipmentId = convert.ToInt(res.Data);
                            quotationId = 0;
                            result2 = GetLinkedDOandPOlinesCountbyInboundShipId(shipmentId);
                            //var resviewbag = JsonConvert.DeserializeObject<JsonResult>(result2);
                            
                        }
                        else
                        {
                            quotationId = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(quotationid);
                        }

                    }
                    else
                    {
                        quotationId = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(quotationid);
                    }

                }
                else
                {
                    return Redirect("/Home/Error?statuscode=400");
                }

                if (shipmentId == 0 && quotationId == 0)
                {
                    LeSDM.AddLog("Oops, decrypted id is 0 for :" + shipmentId);
                    return Redirect("/Home/Error?statuscode=400");
                }

                ViewBag.Module = "Inbound Shipments";
                ViewBag.SubTitle = "Inbound Shipments";
                ViewBag.SubTitleUrl = "InboundShipments";
                ViewBag.inbound = shipmentId;
                ViewBag.quotation = quotationId;

                ViewBag.docount = result2.docount;
                ViewBag.polinecount = result2.polinecount;
                ViewBag.postatusid = result2.postatusid;
            }
            catch (Exception ex)
            {
                LeSDM.AddConsoleLog("Exception in GetInboundShipmentDetails: " + ex.Message);
            }
            return View();
        }
        public IActionResult GetInboundShipmentDetails(string inboundshipmentid)
        {
            V_INBOUND_SHIPMENTS modal = new V_INBOUND_SHIPMENTS();
            try
            {
                int inboundhshipmentid = GlobalTools.DecryptID(inboundshipmentid);
                if (inboundhshipmentid > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"GetInboundShipmentDetails?inboundshipmentid={inboundhshipmentid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<V_INBOUND_SHIPMENTS>(res.Data.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetInboundShipmentDetails: " + ex.Message);
            }
            return Json(modal);
        }
        public IActionResult GetInboundShipmentDetailsEsupplier(string quotationid)
        {
            V_INBOUND_SHIPMENTS modal = new V_INBOUND_SHIPMENTS();
            try
            {
                int inboundhshipmentid = GlobalTools.DecryptID(quotationid);
                if (inboundhshipmentid > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"GetInboundShipmentDetailsESupplier?quotationid={inboundhshipmentid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<V_INBOUND_SHIPMENTS>(res.Data.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetInboundShipmentDetailsEsupplier: " + ex.Message);
            }
            return Json(modal);
        }
        public IActionResult GetPurchaseOrderDetails(string inboundshipmentid)
        {
            INTERNAL_ORDERS modal = new INTERNAL_ORDERS();

            try
            {
                int inboundhshipmentid = GlobalTools.DecryptID(inboundshipmentid);
                if (inboundhshipmentid > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"GetLinkedPurchaseOrder?inboundshipmentid={inboundhshipmentid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<INTERNAL_ORDERS>(res.Data.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetPurchaseOrderDetails: " + ex.Message);
            }
            return Json(modal);
        }
        public IActionResult GetPurchaseOrderDetailsEsupplier(string quotationid)
        {
            INTERNAL_ORDERS modal = new INTERNAL_ORDERS();

            try
            {
                int inboundhshipmentid = GlobalTools.DecryptID(quotationid);
                if (inboundhshipmentid > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"GetLinkedPurchaseOrderEsupplier?quotationid={inboundhshipmentid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<INTERNAL_ORDERS>(res.Data.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetPurchaseOrderDetailsEsupplier: " + ex.Message);
            }
            return Json(modal);
        }
        public IActionResult GetVRemainingInternalOrderLines(string internalorderid)
        {
            List<VRemainingInternalOrderLines> modal = new List<VRemainingInternalOrderLines>();
            try
            {
                int internalorder_id = convert.ToInt(internalorderid);
                if (internalorder_id > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"LoadVRemainingInternalOrderLines?internalorderid={internalorder_id}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<List<VRemainingInternalOrderLines>>(res.Data.ToString());
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetVRemainingInternalOrderLines: " + ex.Message);
            }
            return Json(modal);
        }
        public IActionResult InsertPoFromEsupplierToLogiLink(string quotationid)
        {
            JsonResult _result = Json(new object[] { new object() });
            try
            {
                int quotation_Id = GlobalTools.DecryptID(quotationid);
                if (quotationid != null && quotationid.Length > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"InsertPOfromEsupplierTologiLink?quotationid={quotation_Id}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess)
                        {
                            _result = Json(new { result = true, msg = "Success", data = res.Data });
                        }
                        else
                        {
                            _result = Json(new { result = false, msg = "Something went wrong at backend side!" });
                        }

                    }
                    else
                    {
                        _result = Json(new { result = false, msg = "No response from api!" });
                    }

                }
                else
                {
                    _result = Json(new { result = false, msg = "" });

                }
            }
            catch (Exception ex)
            {
                _result = Json(new { result = false, msg = "Exception occured!" });
                LeSDM.AddLog("Exception in InsertPoFromEsupplierToLogiLink: " + ex.Message);
            }
            return _result;
        }

        private JsonResult InsertPoFromEsupplierToLogiLinkbyQuotId(string quotationid)
        {
            JsonResult _result = Json(new object[] { new object() });
            try
            {
                int quotation_Id = GlobalTools.DecryptID(quotationid);
                if (quotationid != null && quotationid.Length > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"InsertPOfromEsupplierTologiLink?quotationid={quotation_Id}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess)
                        {
                            _result = Json(new { result = true, msg = "Success", data = res.Data });
                        }
                        else
                        {
                            _result = Json(new { result = false, msg = "Something went wrong at backend side!" });
                        }

                    }
                    else
                    {
                        _result = Json(new { result = false, msg = "No response from api!" });
                    }

                }
                else
                {
                    _result = Json(new { result = false, msg = "" });

                }
            }
            catch (Exception ex)
            {
                _result = Json(new { result = false, msg = "Exception occured!" });
                LeSDM.AddLog("Exception in InsertPoFromEsupplierToLogiLink: " + ex.Message);
            }
            return _result;
        }

        public IActionResult GetInternalOrderLines(string InternalOrderid)
        {
            List<Internal_Order_Line> modal = new List<Internal_Order_Line>();
            try
            {
                int internalorderid = GlobalTools.DecryptID(InternalOrderid);
                if (internalorderid > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"GetInternalOrderLines?InternalOrderId={internalorderid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<List<Internal_Order_Line>>(res.Data.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetPurchaseOrderDetails: " + ex.Message);
            }
            return Json(modal);
        }
        public IActionResult GetInternalOrderLinesEsupplier(string InternalOrderid)
        {
            List<Internal_Order_Line> modal = new List<Internal_Order_Line>();
            try
            {
                int internalorderid = GlobalTools.DecryptID(InternalOrderid);
                if (internalorderid > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"GetInternalOrderLineseSupplier?quotationid={internalorderid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            modal = JsonConvert.DeserializeObject<List<Internal_Order_Line>>(res.Data.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetInternalOrderLinesEsupplier: " + ex.Message);
            }
            return Json(modal);
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
                    string jsonData = _iinbound.GetCustomersList(QueryParam);
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
        [HttpPost]
        public async Task<IActionResult> UploadDeliveryOrderDocuments([FromForm] IFormFileCollection files)
        {
            var _result = Json(new object[] { new object() });
            try
            {
                if (files != null && files.Count > 0)
                {
                    var data = _apiroutine.PostAPI("Logistic", $"UploadDeliveryOrderDocuments", "", null, null, files);
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess)
                        {
                            _result = Json(new { result = true, msg = "Success", data = JsonConvert.SerializeObject(res.Data) });
                        }
                        else
                        {
                            _result = Json(new { result = false, msg = res.Message });
                        }

                    }
                    else
                    {
                        _result = Json(new { result = false, msg = "No response from api!" });
                    }
                }
                else
                {
                    _result = Json(new { result = false, msg = "Please select files!" });
                }
            }
            catch (Exception ex)
            {
                _result = Json(new { result = false, msg = ex.Message });
            }
            return _result;
        }

        [HttpPost]
        public IActionResult UploadAttachments(IFormFile formFile, int shipmentId)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

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
        public IActionResult GetInboundShipmentDocuments(int internalorderid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Logistic_Management_Lib.Model.Shipment_Documents> Model = new List<Logistic_Management_Lib.Model.Shipment_Documents>();
            JsonResult _result = Json(new { Data = Model });
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("ShipmentId", internalorderid.ToString());
                string jsonData = _apiroutine.PostAPI("Logistic", "GetShipmentDOdocumentsByshipmentid", null, null, QueryParam);
                Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Shipment_Documents>>(jsonData);
                _result = Json(new { Data = Model });


            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetInboundShipmentDocuments - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult AssignPOlinesToDo(string strmodal, string docmodal, int InternalOrderid, int Inboundshipmentid, string JobNo, string OrderNo, string vesselname, int isfullorsplit, string OrderDt)
        {
            JsonResult _result = Json(new object[] { new object() });
            _result = Json(new { result = false, msg = "Oops something went wrong! Please contact support." });
            try
            {
                V_INBOUND_SHIPMENTS shipment = new V_INBOUND_SHIPMENTS();
                int Userid = convert.ToInt(HttpContext.Session.GetString("UserID"));
                if (strmodal.Length > 0 && InternalOrderid > 0 && Inboundshipmentid > 0)
                {
                    var lines = JsonConvert.DeserializeObject<List<POlinesMin>>(strmodal);
                    var Documennts = JsonConvert.DeserializeObject<List<string>>(docmodal);
                    //string strcompanyId = HttpContext.Session.GetString("CompanyId");
                    //int sessionCustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                    //int companyId = convert.ToInt(strcompanyId);
                    var data = _apiroutine.PostAPI("Logistic", $"GetInboundShipmentDetails?inboundshipmentid={Inboundshipmentid}", "");
                    if (data != null && data != "" && data.Length > 0)
                    {
                        var res = JsonConvert.DeserializeObject<ApiResponse>(data);
                        if (res != null && res.isSuccess && res.Data != null)
                        {
                            shipment = JsonConvert.DeserializeObject<V_INBOUND_SHIPMENTS>(res.Data.ToString());
                        }

                    }
                    //if (UserDefaultData.isbuyer)
                    //{
                    //    COMP = convert.ToInt(shipment.companyid);
                    //    CUST = sessionCustId;
                    //}
                    //else
                    //{
                    //    COMP = companyId;
                    //    CUST = convert.ToInt(shipment.customerId);
                    //}


                    if (lines.Count > 0)
                    {
                        AssignDoToPO_Inbound modal = new AssignDoToPO_Inbound();
                        modal.createdBy = Userid;
                        LeSDM.AddLog("Delivery order date is : " + OrderDt);
                        modal.DeliveryOrderDate = convert.ToDateTime(OrderDt, "dd/MM/yyyy");
                        modal.POlines = lines;
                        modal.JobNo = JobNo;
                        modal.DeliveryOrderNo = OrderNo;
                        modal.Isfullorsplit = isfullorsplit;
                        modal.VesselName = vesselname;
                        modal.internalorderid = InternalOrderid;
                        modal.inboundshipmentid = Inboundshipmentid;
                        modal.customerid = convert.ToInt(shipment.customerId);
                        modal.companyid = convert.ToInt(shipment.companyid);
                        modal.Documents = Documennts;
                        string body = JsonConvert.SerializeObject(modal);

                        var response = _apiroutine.PostAPI("Logistic", "AssignPOlinesToDO", body, null, null);
                        if (response != null && response != "" && response.Length > 0)
                        {
                            var res = JsonConvert.DeserializeObject<ApiResponse>(response);
                            if (res != null && res.isSuccess)
                            {
                                _result = Json(new { result = true, msg = res.Message });
                            }
                            else
                            {
                                _result = Json(new { result = false, msg = res.Message ?? "Something went wrong, Please contact support." });
                            }

                        }

                    }
                    else
                    {
                        _result = Json(new { result = false, msg = "Oops no lines found!" });
                    }

                }


            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in AssignPOlinesToDo - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public (int, string) AttachDocumentInServer(IFormFile upfile, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return (0, "");

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
                    var response = _apiroutine.PostAPI("Logistic", "UploadInternalOrderDeliveryOrder", jstring);
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
                        LeSDM.AddLog("No response from API Logistic\\UploadInternalOrderDeliveryOrder");

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
        public IActionResult DownloadshipmentAttachement(int documentid, int shipmentid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "13", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

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
                    string jsonData = _apiroutine.PostAPI("Logistic", "DownloadInboundDOAttachment", jstring);

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

        public IActionResult GetDeliveryOrderList(string InternalOrderid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            JsonResult _result = Json(new object[] { new object() });
            List<V_Delivery_Orders_Info> DO_list = new List<V_Delivery_Orders_Info>();
            try
            {
                int _recordTotal = 0;
                int _rdValue = (HttpContext.Session.GetInt32("RANDOM") != null) ? convert.ToInt(HttpContext.Session.GetInt32("RANDOM")) : 0;
                //if (_rdValue <= 0)
                //{
                //    HttpContext.Session.SetString("TRIPLIST_DATA", "");
                //    HttpContext.Session.SetInt32("RANDOM", ValueRD);
                //    HttpContext.Session.SetString("FilterDOList_DATA", "");
                //}
                //else
                //{
                //    if (ValueRD != _rdValue)
                //    {
                //        HttpContext.Session.SetString("TRIPLIST_DATA", "");
                //        HttpContext.Session.SetInt32("RANDOM", ValueRD);
                //        HttpContext.Session.SetString("FilterDOList_DATA", "");
                //    }
                //}


                int internalorder_id = convert.ToInt(InternalOrderid);

                //var strBody = JsonConvert.SerializeObject(modal);
                //string Data = _iinbound.GetDeliveryOrderList(strBody);
                var Data = _apiroutine.PostAPI("Logistic", $"GetDeliveryOrdersByInternalOrderId?internalorderid={internalorder_id}", "");
                //string Data = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderList", strBody);

                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(Data);
                    if (apires != null && apires.isSuccess)
                    {
                        DO_list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Delivery_Orders_Info>>(apires.data.ToString());
                        _recordTotal = apires.totalRecords;
                    }
                    else
                    {
                        throw new Exception("Something went wrong on our side, Please contact support!");
                    }
                }
                else
                {
                    LeSDM.AddLog("made bad request to API for GetDeliveryOrderList");

                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetDeliveryOrderList - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);

            }

            return Json(DO_list);
        }

        public IActionResult UnAssignDOs(string deliveryOrderid)
        {
            JsonResult _result = Json(new object[] { new object() });
            try
            {
                int DeliveryOrder_Id = Convert.ToInt32(deliveryOrderid);
                string jsonString = $"{{" +

                               $"\"DeliveryOrderId\": \"{deliveryOrderid}\"" +

                    $"}}";
                //var res = _iinbound.UnassignedDeliveryOrder(jsonString); //UnAssignPOToDeliveryOrder
                var response = _apiroutine.PostAPI("Logistic", $"UnAssignPOToDeliveryOrder?deliveryOrderId={DeliveryOrder_Id}", "");
                if (response != null && response != "" && response.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (res != null && res.isSuccess)
                    {
                        _result = Json(new { result = true, msg = res.Message });
                    }
                    else
                    {
                        _result = Json(new { result = false, msg = res.Message });
                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in UnAssignDOs - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        
        public IActionResult GetLinkedDOandPOlinesCount(int internalorderid)
        {
            JsonResult _result = Json(new object[] { new object() });
            _result = Json(new { result = false, docount = -1, polinecount = -1, postatusid = -1 });
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("internalorderid", internalorderid.ToString());
                var response = _apiroutine.GetAPI("Logistic", "GetRemainingPOlinesCount",null, QueryParam);
                var response1 = _apiroutine.GetAPI("Logistic", "GetDeliveryOrderCount", null, QueryParam);
                int linkedpolinescount = -1;
                int POstatusid = 0;
                int linkedDocount = -1;
                if (response != null && response != "" && response.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (res != null && res.isSuccess)
                    {
                        var ids = convert.ToString(res.Data).Split('|');

                        linkedpolinescount = convert.ToInt(ids[0]);
                        POstatusid = convert.ToInt(ids[1]);
                    }
                }
                if (response1 != null && response1 != "" && response1.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response1);
                    if (res != null && res.isSuccess)
                    {
                        linkedDocount = convert.ToInt(res.Data);
                        
                    }
                }
                _result = Json(new { result = true, docount= linkedDocount,polinecount = linkedpolinescount,postatusid=POstatusid });

                //ViewBag.docount = linkedDocount;
                //ViewBag.polinecount = linkedpolinescount;
                //ViewBag.postatusid = POstatusid;
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetLinkedDOandPOlinesCount - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }

        private LinkedPOandDOResponse GetLinkedDOandPOlinesCountbyInboundShipId(int inboundShipmentId)
        {
            JsonResult _result = Json(new object[] { new object() });
            LinkedPOandDOResponse _response = new LinkedPOandDOResponse();
            _result = Json(new { result = false, docount = -1, polinecount = -1, postatusid = -1 });
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("inboundshipmentid", inboundShipmentId.ToString());
                var response = _apiroutine.GetAPI("Logistic", "GetRemainingPOlinesCountbyInboundId", null, QueryParam);
                var response1 = _apiroutine.GetAPI("Logistic", "GetDeliveryOrderCountbyInboundId", null, QueryParam);
                int linkedpolinescount = -1;
                int POstatusid = 0;
                int linkedDocount = -1;
                if (response != null && response != "" && response.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (res != null && res.isSuccess)
                    {
                        var ids = convert.ToString(res.Data).Split('|');

                        linkedpolinescount = convert.ToInt(ids[0]);
                        POstatusid = convert.ToInt(ids[1]);
                    }
                }
                if (response1 != null && response1 != "" && response1.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response1);
                    if (res != null && res.isSuccess)
                    {
                        linkedDocount = convert.ToInt(res.Data);

                    }
                }
                // Set the final values in the response
                 _response = new LinkedPOandDOResponse
                {
                    result = true,
                    docount = linkedDocount,
                    polinecount = linkedpolinescount,
                    postatusid = POstatusid
                };
                _result = Json(new { result = true, docount = linkedDocount, polinecount = linkedpolinescount, postatusid = POstatusid });

               
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetLinkedDOandPOlinesCount - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _response;
        }
        public IActionResult ReleaseDOs(string deliveryOrderid,string InternalOrderId)
        {
            JsonResult _result = Json(new object[] { new object() });
            try
            {
                int DeliveryOrder_Id = Convert.ToInt32(deliveryOrderid);
                int internalOrderId = Convert.ToInt32(InternalOrderId);
                int Userid = convert.ToInt(HttpContext.Session.GetString("UserID"));

                ReleasedData releasedData = new ReleasedData();
                releasedData.deliveryOrderid = DeliveryOrder_Id;
                releasedData.internalOrderId = internalOrderId;
                releasedData.createdBy = Userid;

                string jsonString = JsonConvert.SerializeObject(releasedData);

                //var response = _apiroutine.PostAPI("Logistic", $"ReleaseDeliveryOrder?deliveryOrderId={DeliveryOrder_Id}", "");
                var response = _apiroutine.PostAPI("Logistic", "ReleaseDeliveryOrder", jsonString);
                if (response != null && response != "" && response.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (res != null && res.isSuccess)
                    {
                        _result = Json(new { result = true, msg = res.Message });
                    }
                    else
                    {
                        _result = Json(new { result = false, msg = res.Message });
                    }

                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in ReleaseDOs - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }

        public IActionResult ReleaseInboundShipment(string inboundShipmentId, string InternalOrderId)
        {
            JsonResult _result = Json(new object[] { new object() });
            try
            {
                int inboundShipment_Id = Convert.ToInt32(inboundShipmentId);
                int internalOrder_Id = Convert.ToInt32(InternalOrderId);
                int Userid = convert.ToInt(HttpContext.Session.GetString("UserID"));
                AssignDoToPO_Inbound modal = new AssignDoToPO_Inbound();

                modal.updatedBy = Userid;
                modal.internalorderid = internalOrder_Id;
                modal.inboundshipmentid = inboundShipment_Id;
                modal.POlines = new List<POlinesMin>();
                modal.Documents = new List<string>();
                string body = JsonConvert.SerializeObject(modal);

                var response = _apiroutine.PostAPI("Logistic", "ReleaseInboundShipment", body);
                if (response != null && response != "" && response.Length > 0)
                {
                    var res = JsonConvert.DeserializeObject<ApiResponse>(response);
                    if (res != null && res.isSuccess)
                    {
                        _result = Json(new { result = true, msg = res.Message });
                    }
                    else
                    {
                        _result = Json(new { result = false, msg = res.Message ?? "Something went wrong, Please contact support." });
                    }

                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in ReleasePOs - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
    }
    public class LinkedPOandDOResponse
    {
        public bool result { get; set; }
        public int docount { get; set; }
        public int polinecount { get; set; }
        public int postatusid { get; set; }
    }
}
