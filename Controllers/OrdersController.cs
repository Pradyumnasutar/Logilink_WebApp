
using LeSDataMain;
using Logistic_Management_Lib.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using LeS_LogiLink_WebApp.Data;
using Logistic_Management_Lib;
using Aspose.Cells;
using System.Globalization;
using System.IO;
using GlobalTools = LeS_LogiLink_WebApp.Data.GlobalTools;
using System.Drawing;
using LeS_LogiLink_WebApp.Models;
using System.Runtime.Versioning;
using LeS_LogiLink_WebApp.Interface;
using System.ComponentModel.Design;

namespace LeS_LogiLink_WebApp.Controllers
{
    public class OrdersController : Controller
    {
        private IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;
        private IUserDefaultData UserDefaultData;
        private IOrders _iorders;
        public OrdersController(IConfiguration _configuration, ApiCallRoutine routine, IUserDefaultData _userdefaultdata,IOrders party)
        {
            //_logger = logger;
            _iorders = party;
            Configuration = _configuration;
            _apiroutine = routine;
            UserDefaultData = _userdefaultdata;
        }
        #region Delivery Order List
        public IActionResult DeliveryOrderList()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                ViewBag.Module = "Delivery Orders";
                ViewBag.SubTitle = "Delivery Orders";
                ViewBag.SubTitleUrl = "DeliveryOrderList";

                ViewBag.EnableFilter = true;
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DeliveryOrderList - " + ex.GetBaseException());
                LeSDM.AddLog("StackTrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }

            return View();

        }
        public IActionResult GetDeliveryOrderList(int ValueRD)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            JsonResult _result = Json(new object[] { new object() });
            List<V_Delivery_Orders_Info> DO_list = new List<V_Delivery_Orders_Info>();
            try
            {
                int _rdValue = (HttpContext.Session.GetInt32("RANDOM") != null) ? convert.ToInt(HttpContext.Session.GetInt32("RANDOM")) : 0;
                if (_rdValue <= 0)
                {
                    HttpContext.Session.SetString("TRIPLIST_DATA", "");
                    HttpContext.Session.SetInt32("RANDOM", ValueRD);
                    HttpContext.Session.SetString("FilterDOList_DATA", "");
                }
                else
                {
                    if (ValueRD != _rdValue)
                    {
                        HttpContext.Session.SetString("TRIPLIST_DATA", "");
                        HttpContext.Session.SetInt32("RANDOM", ValueRD);
                        HttpContext.Session.SetString("FilterDOList_DATA", "");
                    }
                }

                var strList = HttpContext.Session.GetString("TRIPLIST_DATA");
                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                var searchValue = convert.ToString(Request.Form["search[value]"]);
                int _recordTotal = DO_list.Count;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : DO_list.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 1;
                if (!string.IsNullOrEmpty(strList))
                {
                    DO_list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Delivery_Orders_Info>>(strList);
                }
                else
                {
                    string strcompanyId = HttpContext.Session.GetString("CompanyId");
                    int companyId = convert.ToInt(strcompanyId);
                    if (UserDefaultData.isbuyer) { companyId = 0; }
                    if (HttpContext.Request.Query.ContainsKey("companyid") && convert.ToInt(HttpContext.Request.Query["companyid"]) > 0)
                    {

                        companyId = HttpContext.Request.Query["companyid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["companyid"]) : 0;

                    }
                    int statusid = HttpContext.Request.Query["status"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["status"]) : 0;
                    var modal = new FilterDeliveryOrdersList();
                    (modal, string statusdesc) = SetFilterCriteria();
                    modal.companyid = companyId;
                    modal.skip = skip;
                    modal.pagesize = pageSize;
                    modal.quicksearchvalue = searchValue;
                    modal.sortcolumn = sortColumn;
                    modal.sortingorder = sortColumnDir;
                    
                    var strBody = JsonConvert.SerializeObject(modal);
                    string Data = _iorders.GetDeliveryOrderList(strBody);
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
                }
                //if (HttpContext.Request.Query.Count > 0)
                //{
                //    int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                //    (var Modaldata, string statusdesc) = SetFilterCriteria();
                //    Modaldata.skip = skip;
                //    Modaldata.pagesize = pageSize;
                //    Modaldata.quicksearchvalue = searchValue;
                //    Modaldata.companyid = companyId;
                //    Modaldata.sortcolumn = sortColumn;
                //    Modaldata.sortingorder = sortColumnDir;
                //    //string Data = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderList", JsonConvert.SerializeObject(Modaldata));
                //    string Data = _iorders.GetDeliveryOrderList(JsonConvert.SerializeObject(Modaldata));
                //    if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                //    {
                //        var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(Data);
                //        if (apires != null && apires.isSuccess)
                //        {
                //            DO_list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Delivery_Orders_Info>>(apires.data.ToString());
                //            _recordTotal = apires.totalRecords;
                //        }
                //        else
                //        {
                //            throw new Exception("Something went wrong on our side, Please contact support!");
                //        }
                       
                //    }
                    
                //}
               

                List<V_Delivery_Orders_Info> templist = DO_list;
      
         
                int _recordsFiltered = DO_list.Count;
                _result = Json(new { draw = draw, recordsFiltered = _recordTotal, recordsTotal = _recordTotal, data = DO_list, TempDoList = templist });

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetDeliveryOrderList - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);

            }

            return _result;
        }
        private (FilterDeliveryOrdersList, string) SetFilterCriteria()
        {
            string statusdesc = "";
            FilterDeliveryOrdersList orderInfoList = new();
            try
            {
                if (HttpContext.Request.Query["dono"].ToString().Length > 0)
                {
                    string _queryvar = convert.ToString(HttpContext.Request.Query["dono"]);
                    orderInfoList.doNo = _queryvar;
                }
                if (HttpContext.Request.Query["status"].ToString().Length > 0)
                {
                    string _queryvar = convert.ToString(HttpContext.Request.Query["status"]);
                    orderInfoList.statusid = convert.ToInt(_queryvar);

                    //List<V_MODULE_STATUSES> Model = new List<V_MODULE_STATUSES>();
                    //string jsonData = HttpContext.Session.GetString("LogisticModuleStatus" + 2);
                    //Model = JsonConvert.DeserializeObject<List<V_MODULE_STATUSES>>(jsonData);
                    //int statusid = Model.FirstOrDefault(a => a.status_desc == orderInfoList.status_desc)?.statusid ?? 0;
                    //orderInfoList.statusid = statusid;
                }
                if (HttpContext.Request.Query["statusdesc"].ToString().Length > 0)
                {
                    string _queryvar = convert.ToString(HttpContext.Request.Query["statusdesc"]);
                    statusdesc = _queryvar;
                }
                if (HttpContext.Request.Query["shipmentNo"].ToString().Length > 0)
                {
                    string _queryvar = convert.ToString(HttpContext.Request.Query["shipmentNo"]);
                    orderInfoList.shipmentno = _queryvar;
                }                                        
                string shipmentno = HttpContext.Request.Query["shipmentNo"].ToString();
                int statusid = HttpContext.Request.Query["status"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["status"]) : 0;
                int customerid = HttpContext.Request.Query["customerid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["customerid"]) : 0;
                int sessionCustId = convert.ToInt(HttpContext.Session.GetString("CustomerID"));
                if (sessionCustId > 0)
                {
                    orderInfoList.customerid = sessionCustId;
                }
                else
                {
                    orderInfoList.customerid = customerid;
                }

                if (HttpContext.Request.Query["customer"].ToString().Length > 0)
                {
                    string _queryvar = convert.ToString(HttpContext.Request.Query["customer"]);
                    int digitCount = _queryvar.Count(char.IsDigit);
                    if (digitCount > 2)
                    {
                        orderInfoList.custCode = _queryvar;
                    }
                    else
                    {
                        orderInfoList.custName = _queryvar;
                    }
                }
                if (HttpContext.Request.Query["shipmentDate"].ToString().Length > 0)
                {
                    string _queryFromvar = convert.ToString(HttpContext.Request.Query["shipmentDate"]);

                    orderInfoList.shipmentdate = _queryFromvar;
                }
                if (HttpContext.Request.Query["orderFromDate"].ToString().Length > 0)
                {
                    string _queryFromvar = convert.ToString(HttpContext.Request.Query["orderFromDate"]);
                    orderInfoList.dtFrom = _queryFromvar;
                }
                if (HttpContext.Request.Query["orderToDate"].ToString().Length > 0)
                {
                    string _queryTovar = convert.ToString(HttpContext.Request.Query["orderToDate"]);
                    orderInfoList.dtTo = _queryTovar;

                }
                bool isShipmentin14days = false;

                if (HttpContext.Request.Query["shipmentIn"].ToString().Length > 0)
                {
                    string shipmentIn = HttpContext.Request.Query["shipmentIn"].ToString();
                    switch (shipmentIn)
                    {

                        case "14days":
                            orderInfoList.shipment14days = true;
                            break;

                    }
                }
                else
                {
                    orderInfoList.shipment14days = isShipmentin14days;
                }

                if (HttpContext.Request.Query["btnUnassignedDO"].ToString().Length > 0)
                {
                    string UnassignedDO = HttpContext.Request.Query["btnUnassignedDO"].ToString();
                    switch (UnassignedDO)
                    {

                        case "1":
                            orderInfoList.unassignDO = true;
                            break;

                    }
                }
                else { orderInfoList.unassignDO = false; }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SetFilterCriteria - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);

            }
            return (orderInfoList, statusdesc);
        }
        public IActionResult AssignToShipment(int id, int[] deliveryOrderNos)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
                List<int> DOrdersNos = deliveryOrderNos.ToList();
                string json = JsonConvert.SerializeObject(DOrdersNos);
                string jsonString = $"{{" +
                               $"\"deliveryOrderIds\": {json}, " +
                               $"\"shipmentId\": \"{id}\", " +
                            $"\"updated_by\": {UserId}" +
                    $"}}";
                //var res = _apiroutine.PostAPI("Logistic", "UpdateAssignShipmentToDeliveryOrder", jsonString);
                var res = _iorders.AssignToShipment(jsonString);
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
                LeSDM.AddLog("Exception in AssignToShipment - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }

        }
        public IActionResult GetOrderAttachment(int DeliveryOrderId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

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
        public IActionResult DownloadOrderAttachment(int documentid, int deliveryId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

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
        public string GetAttachmentPath(int moduleid)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return "";

            string AttachPath = "";
            int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
            try
            {
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                QueryParam.Add("Moduleid", moduleid.ToString());
                QueryParam.Add("companyid", companyId.ToString());
                var response = _apiroutine.PostAPI("Logistic", "GetSiteConfigByModuleidAndCompanyid", null, null, QueryParam);
                if (response != null)
                {
                    var dec = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_SITE_CONFIG>>(response);
                    if (dec != null)
                    {

                        var Attach = dec.Where(x => x.siteconfig_paramid == 1015).FirstOrDefault();
                        return Attach.param_value;
                    }
                    else
                    {
                        LeSDM.AddLog("no data found in response : GetAttachmentPath");
                        return "";
                    }
                }
                else
                {
                    LeSDM.AddLog("no response received from API Logistic/GetSiteConfigByModuleidAndCompanyid : GetAttachmentPath");
                    return "";
                }
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GetAttachmentPath - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return AttachPath;
        }
        #endregion
        #region Delivery Order Details
        public IActionResult DeliveryOrderDetails(string id)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

                int deliveryorderid = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(id);
                int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                if (deliveryorderid == 0)
                {
                    return Redirect("/Home/Error?statuscode=400");
                }
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                //int _DOid = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(_id);
                QueryParam.Add("DeliveryOrderid", deliveryorderid.ToString());
                QueryParam.Add("Companyid", companyid.ToString());
                //string Data = _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersInfo", null, null, QueryParam);
                string Data = _iorders.DeliveryOrderDetails(QueryParam);
                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    HttpContext.Session.SetString("DELIVERYORDERID", "");
                    ViewBag.Module = "Delivery order";
                    ViewBag.SubTitle = "Delivery order";
                    ViewBag.SubTitleUrl = "DeliveryOrderList";
                    ViewBag.EnableFilter = true;
                    HttpContext.Session.SetString("DELIVERYORDERID", deliveryorderid.ToString());
                }
                else
                {
                    LeSDM.AddLog("Exception in DeliveryOrderDetails - No response from API");
                    return RedirectToAction("Error", "Home", new { statuscode = 404 });
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DeliveryOrderDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }

            return View();
        }
        public IActionResult DO_Information()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            JsonResult _result = Json(new object[] { new object() });
            V_Delivery_Orders_Info DO_info = new V_Delivery_Orders_Info();
            int companyid = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
            try
            {
                string _id = HttpContext.Session.GetString("DELIVERYORDERID");
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                //int _DOid = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(_id);
                QueryParam.Add("DeliveryOrderid", _id.ToString());
                QueryParam.Add("Companyid", companyid.ToString());
                string Data = _iorders.DeliveryOrderDetails(QueryParam);
                //string Data = _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersInfo", null, null, QueryParam);
                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    if (CommonRoutine.IsValidJson(Data))
                    {
                        DO_info = JsonConvert.DeserializeObject<V_Delivery_Orders_Info>(Data);
                    }
                }
                _result = Json(new { data = DO_info });
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DO_Information - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult DO_ShippingDetails()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            JsonResult _result = Json(new object[] { new object() });
            List<V_DELIVERY_ORDERS_ADDRESS_EPOD> DO_shippingdetails = new List<V_DELIVERY_ORDERS_ADDRESS_EPOD>();

            try
            {
                string _id = HttpContext.Session.GetString("DELIVERYORDERID");
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                //int _DOid = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(_id);
                QueryParam.Add("DeliveryOrderid", _id.ToString());
                string Data = _apiroutine.PostAPI("Logistic", "GetDeliveryOrdersAddressByDeliveryOrderId", null, null, QueryParam);
                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    if (CommonRoutine.IsValidJson(Data))
                    {
                        DO_shippingdetails = JsonConvert.DeserializeObject<List<V_DELIVERY_ORDERS_ADDRESS_EPOD>>(Data);
                    }
                }
                _result = Json(new { data = DO_shippingdetails });
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DO_ShippingDetails - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        public IActionResult DO_Lines()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            JsonResult _result = Json(new object[] { new object() });
            List<V_DELIVERY_ORDER_LINES> DO_Lines = new List<V_DELIVERY_ORDER_LINES>();

            try
            {
                string _id = HttpContext.Session.GetString("DELIVERYORDERID");
                IDictionary<string, string> QueryParam = new Dictionary<string, string>();
                //int _DOid = LeS_LogiLink_WebApp.Data.GlobalTools.DecryptID(_id);
                QueryParam.Add("DeliveryOrderid", _id.ToString());
                string Data = _apiroutine.PostAPI("Logistic", "GetVDeliveryOrderLines", null, null, QueryParam);
                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    if (CommonRoutine.IsValidJson(Data))
                    {
                        DO_Lines = JsonConvert.DeserializeObject<List<V_DELIVERY_ORDER_LINES>>(Data);
                    }
                }
                var draw = Request.Form["draw"];
                var start = convert.ToString(Request.Form["start"]);
                var length = convert.ToString(Request.Form["length"]);
                var sortColumn = Request.Form["columns[" + convert.ToString(Request.Form["order[0][column]"]) + "][data]"];//.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"];
                var searchValue = convert.ToString(Request.Form["search[value]"]);
                int _recordTotal = DO_Lines.Count;
                int pageSize = (length != null && convert.ToInt(length) > 0) ? Convert.ToInt32(length) : DO_Lines.Count;
                int skip = (!string.IsNullOrWhiteSpace(start)) ? Convert.ToInt32(start) : 1;
                if (!string.IsNullOrEmpty(searchValue))
                {
                    //    DO_list = DO_list.Where(m => (convert.ToString(m.delivery_order_no.ToUpper() + " " + convert.ToString(m.order_no).ToUpper() + " " + m.customer_no.ToUpper() + " " +
                    //        convert.ToString(m.customer_name).ToUpper() + " " + convert.ToString(m.vessel_eta).ToUpper() + " " + convert.ToString(m.sales_person_code).ToUpper() + " " +
                    //        convert.ToString(m.internal_dept).ToUpper() + " " + convert.ToDateTime(m.promised_delivery_date).ToString("dd'/'MM'/'yyyy") + " "
                    //        + convert.ToDateTime(m.planned_ship_date).ToString("dd'/'MM'/'yyyy") + " " + convert.ToString(m.jobno).ToUpper()).IndexOf(searchValue.ToUpper()) > -1)).ToList();
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumnDir == "asc")
                    {
                        switch (sortColumn)
                        {
                            case "Row_No": DO_Lines = DO_Lines.OrderBy(x => x.delivery_order_id).ToList(); break;
                            case "delivery_order_no": DO_Lines = DO_Lines.OrderBy(x => x.delivery_order_no).ToList(); break;
                            case "item_no": DO_Lines = DO_Lines.OrderBy(x => x.item_no).ToList(); break;
                            case "item_ref_no": DO_Lines = DO_Lines.OrderBy(x => x.item_ref_no).ToList(); break;
                            case "item_description": DO_Lines = DO_Lines.OrderBy(x => x.item_description).ToList(); break;
                            case "location_code": DO_Lines = DO_Lines.OrderBy(x => x.location_code).ToList(); break;
                            case "quantity": DO_Lines = DO_Lines.OrderBy(x => x.quantity).ToList(); break;
                            case "uom": DO_Lines = DO_Lines.OrderBy(x => x.uom).ToList(); break;
                            case "internal_dept": DO_Lines = DO_Lines.OrderBy(x => x.internal_dept).ToList(); break;
                            case "awb_bl": DO_Lines = DO_Lines.OrderBy(x => x.awb_bl).ToList(); break;
                            case "jobno": DO_Lines = DO_Lines.OrderBy(x => x.jobno).ToList(); break;
                            case "vessel_code": DO_Lines = DO_Lines.OrderBy(x => x.vessel_code).ToList(); break;
                            case "sales_person_code": DO_Lines = DO_Lines.OrderBy(x => x.sales_person_code).ToList(); break;
                            case "broker_code": DO_Lines = DO_Lines.OrderBy(x => x.broker_code).ToList(); break;
                            case "dept_code": DO_Lines = DO_Lines.OrderBy(x => x.dept_code).ToList(); break;
                            case "quantity_invoiced": DO_Lines = DO_Lines.OrderBy(x => x.quantity_invoiced).ToList(); break;
                            default: DO_Lines = DO_Lines.OrderBy(x => x.delivery_order_id).ToList(); break;
                        }
                    }
                    else
                    {
                        switch (sortColumn)
                        {
                            case "delivery_order_no": DO_Lines = DO_Lines.OrderByDescending(x => x.delivery_order_no).ToList(); break;
                            case "item_no": DO_Lines = DO_Lines.OrderByDescending(x => x.item_no).ToList(); break;
                            case "item_ref_no": DO_Lines = DO_Lines.OrderByDescending(x => x.item_ref_no).ToList(); break;
                            case "item_description": DO_Lines = DO_Lines.OrderByDescending(x => x.item_description).ToList(); break;
                            case "location_code": DO_Lines = DO_Lines.OrderByDescending(x => x.location_code).ToList(); break;
                            case "quantity": DO_Lines = DO_Lines.OrderByDescending(x => x.quantity).ToList(); break;
                            case "uom": DO_Lines = DO_Lines.OrderByDescending(x => x.uom).ToList(); break;
                            case "internal_dept": DO_Lines = DO_Lines.OrderByDescending(x => x.internal_dept).ToList(); break;
                            case "awb_bl": DO_Lines = DO_Lines.OrderByDescending(x => x.awb_bl).ToList(); break;
                            case "jobno": DO_Lines = DO_Lines.OrderByDescending(x => x.jobno).ToList(); break;
                            case "vessel_code": DO_Lines = DO_Lines.OrderByDescending(x => x.vessel_code).ToList(); break;
                            case "sales_person_code": DO_Lines = DO_Lines.OrderByDescending(x => x.sales_person_code).ToList(); break;
                            case "broker_code": DO_Lines = DO_Lines.OrderByDescending(x => x.broker_code).ToList(); break;
                            case "dept_code": DO_Lines = DO_Lines.OrderByDescending(x => x.dept_code).ToList(); break;
                            case "quantity_invoiced": DO_Lines = DO_Lines.OrderByDescending(x => x.quantity_invoiced).ToList(); break;
                            default: DO_Lines = DO_Lines.OrderByDescending(x => x.delivery_order_id).ToList(); break;
                        }
                    }
                }
                int _recordsFiltered = DO_Lines.Count;
                DO_Lines = DO_Lines.Skip(skip).Take(pageSize).ToList();
                _result = Json(new { draw = draw, recordsFiltered = _recordsFiltered, recordsTotal = _recordTotal, data = DO_Lines });
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in DO_Lines - " + ex.GetBaseException());
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return _result;
        }
        #endregion
        #region Print Delivery Order
        public IActionResult PrintDeliveryOrderDetails(int _deliveryOrderId)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            byte[] pdfBytes;
            try
            {
                SetAsposeLicense();
                DeliveryOrderDetailsPrint _deliveryOrderDetails = new();
                string _id = Convert.ToString(_deliveryOrderId);
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
                string filePath = PrintOrderDetails(_deliveryOrderDetails);
                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Directory.GetCurrentDirectory() + "\\wwwroot\\DeliveryOrderDetails\\" + fileName;
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
                LeSDM.AddLog("Exception in Delivery Order Details - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }

        }
        public string PrintOrderDetails(DeliveryOrderDetailsPrint deliveryOrderDetails)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return "";

            string savedPdfPath = "";
            try
            {
                var templatePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Template\\Delivery Order Details.xlsx";
                var sessionId = HttpContext.Session.Id;

                var tempTemplate = Directory.GetCurrentDirectory() + $"\\wwwroot\\Template\\temp\\{sessionId}";

                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
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
                byte[] logoByte = Convert.FromBase64String(deliveryOrderDetails.LogoModel.base64printLogo);
                if (logoByte != null&&logoByte.Count()>0)
                {
                    worksheet.PageSetup.SetHeaderPicture(0, logoByte);
                    worksheet.PageSetup.SetHeader(0, "&G");
                    worksheet.PageSetup.HeaderMargin = 0.09;
                }
                else
                {
                    string CompanyLogo = $"{deliveryOrderDetails.LogoModel.Company_Code}\n {deliveryOrderDetails.LogoModel}";
                    worksheet.PageSetup.SetHeader(0, deliveryOrderDetails.LogoModel.Company_Description);
                }
                worksheet.PageSetup.SetFooter(0, "&P of &N");
                worksheet.PageSetup.SetFooter(2, $"Print Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                #endregion

                string outputFolder = Directory.GetCurrentDirectory() + "\\wwwroot\\DeliveryOrderDetails";
                DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                style.Font.Size = 11;
                StyleFlag styleFlag = new StyleFlag();
                styleFlag.FontSize = true;
                worksheet.Cells.ApplyStyle(style, styleFlag);
                //worksheet.AutoFitRows();
                string finalFile = $"{outputFolder}/Delivery_Order_List_{deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no}_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                newWorkbook.Save(finalFile);
                //newWorkbook.Save($"{outputFolder}/Delivery_Order_List_{deliveryOrderDetails.DeliveryOrdersInfo.delivery_order_no}_{DateTime.Now.ToString("ddMMyy")}.xlsx");
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
                savedPdfPath = finalFile;
                return savedPdfPath;
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Delivery Order Details - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);

            }
            return savedPdfPath;
        }
        [Obsolete]
        public IActionResult PrintDeliveryOrderList()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "2", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 });;

            byte[] pdfBytes;
            string savedPdfPath = "";
            List<V_Delivery_Orders_Info> DO_list = new List<V_Delivery_Orders_Info>();
            try
            {
                SetAsposeLicense();
                int skip, length = 0;
                string sortingColumn = "", sortingOrder = "";
                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:DeliveryOrderListTemplate", "");
                //var templatePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Template\\Delivery Order List.xlsx";

                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) && string.IsNullOrEmpty(tempTemplate))
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

                //Workbook templateWorkbook = new Workbook(templatePath);
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();
                string statusdesc = "";
                #region Delivery Order List API 
                int companyId = convert.ToInt(HttpContext.Session.GetString("CompanyId"));
                if (UserDefaultData.isbuyer) { companyId = 0; }
                if (HttpContext.Request.Query.ContainsKey("companyid")&&convert.ToInt(HttpContext.Request.Query["companyid"])>0)
                {

                    companyId = HttpContext.Request.Query["companyid"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["companyid"]) : 0;
                   
                }
                
                string body = string.Empty;
                var data = new FilterDeliveryOrdersList();
                //var _filterDoList = HttpContext.Session.GetString("FilterDOList_DATA");
                if (HttpContext.Request.Query.Count > 0)
                {
                    (data, statusdesc) = SetFilterCriteria();
                    skip = HttpContext.Request.Query["skip"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["skip"]) : 0;
                    length = HttpContext.Request.Query["length"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["length"]) : 0;
                    sortingColumn = HttpContext.Request.Query["sortColumn"].ToString() ?? "";
                    sortingOrder = HttpContext.Request.Query["sortOrder"].ToString() ?? "";

                    data.pagesize=-1;
                    data.skip=0;
                    data.sortcolumn = sortingColumn;
                    data.sortingorder = sortingOrder;
                    data.companyid = companyId;
                    body = JsonConvert.SerializeObject(data);
                }
                else
                {
                    skip = HttpContext.Request.Query["skip"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["skip"]) : 0;
                    length = HttpContext.Request.Query["length"].ToString().Length > 0 ? Convert.ToInt32(HttpContext.Request.Query["length"]) : 0;
                    sortingColumn = HttpContext.Request.Query["sortColumn"].ToString() ?? "";
                    sortingOrder = HttpContext.Request.Query["sortOrder"].ToString() ?? "";
                    data.sortcolumn = sortingColumn;
                    data.sortingorder = sortingOrder;
                    data.pagesize = -1;
                    data.skip = 0;


                    data.companyid = companyId;
                    body = JsonConvert.SerializeObject(data);
                }

                //DO_list = JsonConvert.DeserializeObject<List<V_Delivery_Orders_Info>>(_filterDoList);
                string Data = string.Empty;
                Data = _iorders.GetDeliveryOrderList(body);
                //Data = _apiroutine.PostAPI("Logistic", "GetDeliveryOrderListByCompanyId", body);

                if (!string.IsNullOrEmpty(Data) && !string.IsNullOrWhiteSpace(Data))
                {
                    var apires = JsonConvert.DeserializeObject<Logistic_Management_Lib.StandardAPIresponse>(Data);
                    if (apires != null && apires.isSuccess)
                    {
                        DO_list = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.V_Delivery_Orders_Info>>(apires.data.ToString());
                       
                    }
                    else
                    {
                        throw new Exception("Something went wrong on our side, Please contact support!");
                    }
                    
                }

                #endregion

                #region Filter Section
                if (data != null)
                {
                    worksheet.Cells["C4"].PutValue(string.IsNullOrEmpty(data.doNo) ? " " : data.doNo);
                    worksheet.Cells["C5"].PutValue(string.IsNullOrEmpty(data.custCode + "" + data.custName) ? " " : data.custCode + "" + data.custName);
                    //worksheet.Cells["C6"].PutValue(string.IsNullOrEmpty(data.customer_name) ? " " : data.customer_name);
                    worksheet.Cells["C6"].PutValue(string.IsNullOrEmpty(statusdesc) ? " " : statusdesc);
                    worksheet.Cells["C7"].PutValue(string.IsNullOrEmpty(data.shipmentno) ? " " : data.shipmentno);
                    if (data.unassignDO) { worksheet.Cells["C8"].PutValue("Unassigned DO's"); }
                    else if (data.shipment14days) { worksheet.Cells["C8"].PutValue("Shipment 14 days"); }
                    else { worksheet.Cells["C8"].PutValue(""); }
                    worksheet.Cells["H4"].PutValue(string.IsNullOrEmpty(data.shipmentdate) ? " " : data.shipmentdate);
                    worksheet.Cells["H5"].PutValue(string.IsNullOrEmpty(data.dtFrom) ? " " : data.dtFrom);
                    worksheet.Cells["H6"].PutValue(string.IsNullOrEmpty(data.dtTo) ? " " : data.dtTo);
                }
                #endregion

                worksheet.Cells["C10"].PutValue(string.IsNullOrEmpty(DO_list.Count.ToString()) ? "0" : DO_list.Count.ToString());

                #region delivery order lines
                int i = 1;
                int rowIndex = 12;
                var globlaStyle = worksheet.Cells["A12"].GetStyle();
                foreach (Logistic_Management_Lib.Model.V_Delivery_Orders_Info deliveryOrderLines in DO_list)
                {
                    //worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.delivery_order_no) ? " " : deliveryOrderLines.delivery_order_no);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.customer_no+"-"+ deliveryOrderLines.customer_name) ? " " : deliveryOrderLines.customer_no.Trim()+"-"+ deliveryOrderLines.customer_name);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"D{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.CompanyCode+"-"+deliveryOrderLines.CompanyName) ? " " : deliveryOrderLines.CompanyCode+"-"+ deliveryOrderLines.CompanyName);
                    worksheet.Cells[$"D{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"E{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.sales_order_no) ? " " : deliveryOrderLines.sales_order_no);
                    worksheet.Cells[$"E{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"F{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.shipmentdate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)) ? " " : deliveryOrderLines.shipmentdate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    worksheet.Cells[$"F{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"G{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.vessel_name) ? " " : deliveryOrderLines.vessel_name);
                    worksheet.Cells[$"G{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"H{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.vessel_eta?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)) ? " " : deliveryOrderLines.vessel_eta?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    worksheet.Cells[$"H{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"I{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.do_job_no) ? " " : deliveryOrderLines.do_job_no);
                    worksheet.Cells[$"I{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"J{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.internal_dept) ? " " : deliveryOrderLines.internal_dept);
                    worksheet.Cells[$"J{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"K{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.status_desc) ? " " : deliveryOrderLines.status_desc);
                    worksheet.Cells[$"K{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"L{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.order_no) ? " " : deliveryOrderLines.order_no);
                    worksheet.Cells[$"L{rowIndex}"].SetStyle(globlaStyle);
                    worksheet.Cells[$"M{rowIndex}"].PutValue(string.IsNullOrEmpty(deliveryOrderLines.sales_person_code) ? " " : deliveryOrderLines.sales_person_code);
                    worksheet.Cells[$"M{rowIndex}"].SetStyle(globlaStyle);
                    Aspose.Cells.Row row = worksheet.Cells.Rows[rowIndex];
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    StyleFlag flag = new();
                    flag.Borders = true;
                    row.ApplyStyle(style, flag);
                    i++;
                    rowIndex++;
                }
                #endregion

                #region company logo
                Companyinfodata companydetails = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(HttpContext.Session.GetString("CompanyId"))}", ""));

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
                #endregion
                worksheet.PageSetup.SetFooter(0, $" {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
                worksheet.AutoFitRows();


                string outputFolder = Directory.GetCurrentDirectory() + "\\wwwroot\\DeliveryOrderList";
                DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                string Finalfile = $"{outputFolder}/Delivery_Order_List_{DateTime.Now.ToString("ddMMyyhhmmssfff")}.pdf";
                newWorkbook.Save(Finalfile);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
                savedPdfPath = Finalfile;
                //return savedPdfPath;
                //savedPdfPath = newWorkbook.AbsolutePath;
                string filePath = savedPdfPath;
                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Directory.GetCurrentDirectory() + "\\wwwroot\\DeliveryOrderList\\" + fileName;
                //if (!System.IO.File.Exists(serverFilePath))
                //{
                //    LeSDM.AddLog($"Error while searching file for delivery order list '{serverFilePath}'");
                //    return StatusCode(404, "File Not Found");
                //}

                pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Delivery Order List - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
            //return savedPdfPath;
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
}
