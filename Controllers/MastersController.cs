using Aspose.Cells;
using LeSDataMain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using LeS_LogiLink_WebApp.Data;
using LeS_LogiLink_WebApp.Models;
using System.IO;
using System.Globalization;
using GlobalTools = LeS_LogiLink_WebApp.Data.GlobalTools;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Linq;
using LeS_LogiLink_WebApp.Interface;

namespace LeS_LogiLink_WebApp.Controllers
{
    public class MastersController : Controller
    {
        private IConfiguration Configuration;
        private readonly ApiCallRoutine _apiroutine;
        private IUserDefaultData UserDefaultData;
        private IMasters _imasters;
        public MastersController(IConfiguration _configuration, ApiCallRoutine apiroutine, IUserDefaultData userDefaultData, IMasters msobj)
        {
            Configuration = _configuration;
            _apiroutine = apiroutine;
            UserDefaultData = userDefaultData;
            _imasters = msobj;
        }

        #region Customer and supplier 
        public IActionResult CustomerCard()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "4", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Customer> model = new List<Customer>();
            try
            {
                string printKey = "Customer_PRINT";
                ViewBag.Module = "Masters";
                ViewBag.SubTitle = "Masters";
                ViewBag.SubTitleUrl = "CustomerCard";

                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    //var jsData = _apiroutine.PostAPI("Logistic", "GetCustomerInfoList", null, null, queryParam);
                    var jsData = _imasters.GetCustomersList(queryParam);
                    model = JsonConvert.DeserializeObject<List<Customer>>(jsData);
                    model = model.Where(x => x.CustomerId > 0).ToList();
                }
                ViewData["Print_Key"] = printKey;
                ViewBag.TotalRecords = model.Count;
                return View(model);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Customer - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
        }
        public IActionResult ExportCustomerCard(int _customerId)
        {


            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "4", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                SetAsposeLicense();
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                List<Customer> model = new List<Customer>();

                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    var jsData = _imasters.GetCustomersList(queryParam);
                    model = JsonConvert.DeserializeObject<List<Customer>>(jsData);
                    model = model.Where(x => x.CustomerId > 0).ToList();
                }

                string filePath = AppendDataInTemplate(model);
                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Internal server error");
                }

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CustomerCardPdfs", fileName);
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file in ExportCustomerCard process: '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                byte[] pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Shipment Order - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }

        public string AppendDataInTemplate(List<Customer> printModel)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "4", HttpContext))
                return "User Access Denied!";
            string savedFilePath = "";
            try
            {
                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:CustomerCardTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both CustomerCardTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate += $"\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                if (!Path.Exists(Path.Combine(tempTemplate, Path.GetFileName(templatePath))))
                {
                    System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                }
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CustomerCardPdfs");
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));

                //Workbook templateWorkbook = new Workbook(templatePath);
                //Workbook newWorkbook = new Workbook();
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();





                // Configure the style for borders
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;

                #region Customer Card Info
                int i = 1;
                int rowIndex = 5;
                var globlaStyle = worksheet.Cells["A5"].GetStyle();
                worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                worksheet.Cells["B3"].PutValue(string.IsNullOrEmpty(printModel.Count.ToString()) ? "0" : printModel.Count.ToString());

                foreach (var customerlines in printModel)
                {
                    worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(customerlines.Cust_Code) ? " " : customerlines.Cust_Code);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(customerlines.Cust_Name) ? " " : customerlines.Cust_Name.Trim());

                    // Apply the style to each cell
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(style);

                    worksheet.Cells.SetRowHeight(rowIndex, 25);

                    i++;
                    rowIndex++;
                }
                #endregion

                #region Header Info
                var companyinfo = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyId)}", ""));

                byte[] logoByte = Convert.FromBase64String(companyinfo.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {

                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 400;
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width;

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
                    worksheet.PageSetup.SetHeader(0, companyinfo.Company_Description);
                }
                #endregion
                worksheet.PageSetup.SetFooter(0, $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");

                worksheet.AutoFitRows(true);
                worksheet.AutoFitColumn(3);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                savedFilePath = Path.Combine(outputDirectory, $"Customer_Card_{DateTime.Now:ddMMyyyyhhmmssfff}.pdf");
                newWorkbook.Save(savedFilePath);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in AppendDataInTemplate in MasterController - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }

            return savedFilePath;
        }

        #endregion Customer and supplier

        #region Supplier 
        public IActionResult SuppliersCard()
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "12", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            List<Customer> model = new List<Customer>();
            try
            {
                string printKey = "Customer_PRINT";
                ViewBag.Module = "Masters";
                ViewBag.SubTitle = "Masters";
                ViewBag.SubTitleUrl = "SupplierCard";

                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    //var jsData = _apiroutine.PostAPI("Logistic", "GetCustomerInfoList", null, null, queryParam);
                    var jsData = _imasters.GetCustomersList(queryParam);
                    model = JsonConvert.DeserializeObject<List<Customer>>(jsData);
                    model = model.Where(x => x.CompanyId > 0).ToList();
                }
                ViewData["Print_Key"] = printKey;
                ViewBag.TotalRecords = model.Count;
                return View(model);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in SupplierCard - " + ex.Message);
                LeSDM.AddLog("StackTrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });
            }
        }
        public IActionResult ExportSupplierCard(int _customerId)
        {


            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "4", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                SetAsposeLicense();
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                List<Customer> model = new List<Customer>();

                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    var jsData = _imasters.GetCustomersList(queryParam);
                    model = JsonConvert.DeserializeObject<List<Customer>>(jsData);
                    model = model.Where(x => x.CompanyId > 0).ToList();
                }

                string filePath = AppendDataInTemplateSupplier(model);
                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Internal server error");
                }

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CustomerCardPdfs", fileName);
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file in ExportCustomerCard process: '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                byte[] pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Shipment Order - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }
        public string AppendDataInTemplateSupplier(List<Customer> printModel)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "12", HttpContext))
                return "User Access Denied!";
            string savedFilePath = "";
            try
            {
                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:CustomerCardTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both CustomerCardTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate += $"\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                if (!Path.Exists(Path.Combine(tempTemplate, Path.GetFileName(templatePath))))
                {
                    System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                }
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CustomerCardPdfs");
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));

                //Workbook templateWorkbook = new Workbook(templatePath);
                //Workbook newWorkbook = new Workbook();
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();





                // Configure the style for borders
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;

                #region Customer Card Info
                int i = 1;
                int rowIndex = 5;
                var globlaStyle = worksheet.Cells["A5"].GetStyle();
                worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                worksheet.Cells["B3"].PutValue(string.IsNullOrEmpty(printModel.Count.ToString()) ? "0" : printModel.Count.ToString());

                foreach (var customerlines in printModel)
                {
                    worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(customerlines.Cust_Code) ? " " : customerlines.Cust_Code);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(customerlines.Cust_Name) ? " " : customerlines.Cust_Name.Trim());

                    // Apply the style to each cell
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(style);

                    worksheet.Cells.SetRowHeight(rowIndex, 25);

                    i++;
                    rowIndex++;
                }
                #endregion

                #region Header Info
                var companyinfo = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyId)}", ""));

                byte[] logoByte = Convert.FromBase64String(companyinfo.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {

                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 400;
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width;

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
                    worksheet.PageSetup.SetHeader(0, companyinfo.Company_Description);
                }
                #endregion
                worksheet.PageSetup.SetFooter(0, $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");

                worksheet.AutoFitRows(true);
                worksheet.AutoFitColumn(3);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                savedFilePath = Path.Combine(outputDirectory, $"Customer_Card_{DateTime.Now:ddMMyyyyhhmmssfff}.pdf");
                newWorkbook.Save(savedFilePath);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in AppendDataInTemplate in MasterController - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }

            return savedFilePath;
        }
        #endregion Supplier

        #region Anchorage  
        public IActionResult Anchorage()
        {
            try
            {

                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "5", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                string Print_Key = "Anchorage_PRINT";
                ViewBag.Module = "Masters";
                ViewBag.SubTitle = "Masters";
                ViewBag.SubTitleUrl = "Anchorage";

                var JsData = _apiroutine.PostAPI("Logistic", "GetAnchorageList", "");
                var Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Anchorage>>(JsData);

                ViewData["Print_Key"] = Print_Key;
                ViewBag.TotalRecords = Model.Count;
                return View(Model);

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Anchorage - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });

            }
        }

        public IActionResult SaveAnchorage(int id, string desc, string code)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "5", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));

                if (id == 0)//New record
                {

                    string jsonString = $"{{" +
                               $"\"anchorageId\": {id}, " +
                               $"\"anchorageCode\": \"{code}\", " +
                               $"\"anchorageDescription\": \"{desc}\", " +
                               $"\"createdDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                               $"\"updatedDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                               $"\"createdBy\": {UserId.ToString()}," +
                               $"\"updatedBy\": {0}" +
                    $"}}";
                    var res = _apiroutine.PostAPI("Logistic", "AddAnchorage", jsonString);

                    if (res != "" && res.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                        if (data != null)
                        {
                            if (data.isSuccess)
                            {
                                var daata = new { result = true, msg = "Anchor " + code + " successfully added !" };
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
                            var daata = new { result = false, msg = "Unable to save anchorage code!" };
                            return Json(data);
                        }
                    }
                    else
                    {
                        var data = new { result = false, msg = "Unable to save anchorage code!" };
                        return Json(data);
                    }

                }
                else if (id > 0)//update 
                {
                    string jsonString = $"{{" +
                               $"\"anchorageId\": {id}, " +
                               $"\"anchorageCode\": \"{code}\", " +
                               $"\"anchorageDescription\": \"{desc}\", " +
                               $"\"createdDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                               $"\"updatedDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                               $"\"createdBy\": {0}," +
                               $"\"updatedBy\": {UserId.ToString()}" +
                    $"}}";

                    var res = _apiroutine.PostAPI("Logistic", "UpdateAnchorage", jsonString);

                    if (res != "" && res.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                        if (data != null)
                        {
                            if (data.isSuccess)
                            {
                                var daata = new { result = true, msg = "Anchor code successfully updated !" };
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
                            var daata = new { result = false, msg = "Unable to save anchorage code!" };
                            return Json(data);
                        }
                    }
                    else
                    {
                        var data = new { result = false, msg = "Unable to save Anchorage code!" };
                        return Json(data);
                    }

                }
                else
                {
                    var data = new { result = false, msg = "Oops, It's not you but us!" };
                    return Json(data);
                }


            }
            catch (Exception ex)
            {

                CommonRoutine.SetAudit("Anchorage", "Error", "", "Exception in Anchorage " + ex.GetBaseException(), "");
                LeSDM.AddLog("Exception in Anchorage - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }

        }

        public IActionResult ExportAnchorage(int _anchorID)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "5", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {

                SetAsposeLicense();
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                List<Logistic_Management_Lib.Model.Mast_Anchorage> model = new List<Logistic_Management_Lib.Model.Mast_Anchorage>();

                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    var jsData = _apiroutine.PostAPI("Logistic", "GetAnchorageList", null, null, queryParam);
                    model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Anchorage>>(jsData);
                }

                string filePath = AppendDataInTemplate(model);
                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Internal server error");
                }

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AnchoragePdfs", fileName);
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                byte[] pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Anchorage - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }

        public string AppendDataInTemplate(List<Logistic_Management_Lib.Model.Mast_Anchorage> printModel)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "5", HttpContext)) return "";

            string savedFilePath = "";
            try
            {

                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:AnchorageTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both AnchorageTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate += $"\\{sessionId}";

                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AnchoragePdfs");
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                //Workbook templateWorkbook = new Workbook(templatePath);
                //Workbook newWorkbook = new Workbook();
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();





                // Configure the style for borders
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;

                #region Customer Card Info
                int i = 1;
                int rowIndex = 5;
                var globlaStyle = worksheet.Cells["A5"].GetStyle();
                worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                worksheet.Cells["B3"].PutValue(string.IsNullOrEmpty(printModel.Count.ToString()) ? "0" : printModel.Count.ToString());

                foreach (var anchorage in printModel)
                {
                    worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(anchorage.AnchorageCode) ? " " : anchorage.AnchorageCode);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(anchorage.AnchorageDescription) ? " " : anchorage.AnchorageDescription.Trim());

                    // Apply the style to each cell
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(style);

                    worksheet.Cells.SetRowHeight(rowIndex, 25);

                    i++;
                    rowIndex++;
                }
                #endregion

                #region Header Info
                var companyinfo = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyId)}", ""));

                byte[] logoByte = Convert.FromBase64String(companyinfo.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {

                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 400;
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width;

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
                    worksheet.PageSetup.SetHeader(0, companyinfo.Company_Description);
                }
                #endregion
                worksheet.PageSetup.SetFooter(0, $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");

                worksheet.AutoFitRows(true);
                worksheet.AutoFitColumn(3);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                savedFilePath = Path.Combine(outputDirectory, $"Anchorage_{DateTime.Now:ddMMyyyyhhmmssfff}.pdf");
                newWorkbook.Save(savedFilePath);
                Directory.Delete(tempTemplate, true);
                newWorkbook.Dispose();
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Append Data in Template - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return savedFilePath;
        }

        #endregion Anchorage

        #region GoodsReturnReasonCodes
        public IActionResult GoodsReturnReasonCode()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "6", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;
                string Print_Key = "GRRC_PRINT";
                ViewBag.Module = "Masters";
                ViewBag.SubTitle = "Masters";
                ViewBag.SubTitleUrl = "GoodsReturnReasonCode";


                var JsData = _apiroutine.PostAPI("Logistic", "GetGoodsReturnReasonsList", "");
                var Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Goods_Return_Reasons>>(JsData);

                ViewData["Print_Key"] = Print_Key;
                ViewBag.TotalRecords = Model.Count;
                return View(Model);

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in GoodsReturnReasonCode - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });

            }
        }
        public IActionResult SaveGoodsReturnReasonCode(int id, string GRRC_desc, string GRR_code)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "6", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));

                if (id == 0)//New record
                {

                    string jsonString = $"{{" +
                               $"\"grnReasonId\": {id}, " +
                               $"\"grnReasonCode\": \"{GRR_code}\", " +
                               $"\"grnReasonDescription\": \"{GRRC_desc}\", " +
                            $"\"created_date\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                            $"\"updated_date\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                            $"\"created_by\": {UserId}," +
                            $"\"updated_by\": {UserId}" +
                    $"}}";
                    var res = _apiroutine.PostAPI("Logistic", "AddGoodsReturnReasons", jsonString);
                    if (res != "" && res.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                        if (data != null)
                        {
                            if (data.isSuccess)
                            {
                                var daata = new { result = true, msg = "Goods Return Reasons Code " + GRR_code + " Successfully added !" };
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
                else if (id > 0)//update 
                {
                    string jsonString = $"{{" +
                               $"\"grnReasonId\": {id}, " +
                               $"\"grnReasonCode\": \"{GRR_code}\", " +
                               $"\"grnReasonDescription\": \"{GRRC_desc}\", " +

                                $"\"updated_date\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +

                                $"\"updated_by\": {UserId}" +
                    $"}}";

                    var res = _apiroutine.PostAPI("Logistic", "UpdateGoodsReturnReasons", jsonString);
                    if (res != "" && res.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                        if (data != null)
                        {
                            if (data.isSuccess)
                            {
                                var daata = new { result = true, msg = "Good Return Reasons Code " + GRR_code + " Successfully updated !" };
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
                else
                {
                    var data = new { result = false, msg = "Oops, It's not you but us!" };
                    return Json(data);
                }


            }
            catch (Exception ex)
            {
                CommonRoutine.SetAudit("GoodsReturnReasons", "Error", "", "Exception in GoodsReturnReasons " + ex.GetBaseException(), "");
                LeSDM.AddLog("Exception in GoodsReturnReasons - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }
        }

        public IActionResult ExportGRR(int _GRRID)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "6", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {

                SetAsposeLicense();
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                List<Logistic_Management_Lib.Model.Mast_Goods_Return_Reasons> model = new List<Logistic_Management_Lib.Model.Mast_Goods_Return_Reasons>();

                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    var jsData = _apiroutine.PostAPI("Logistic", "GetGoodsReturnReasonsList", null, null, queryParam);
                    model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Goods_Return_Reasons>>(jsData);
                }

                string filePath = AppendDataInTemplate(model);
                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Internal server error");
                }

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "GRRPdfs", fileName);
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                byte[] pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print GRR - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }

        public string AppendDataInTemplate(List<Logistic_Management_Lib.Model.Mast_Goods_Return_Reasons> printModel)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "6", HttpContext)) return "";

            string savedFilePath = "";
            try
            {

                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:GRRTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both GRRTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate += $"\\{sessionId}";

                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Template", "GRR Template.xlsx");
                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "GRRPdfs");
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                //var sessionId = HttpContext.Session.Id;
                // var tempTemplate = Directory.GetCurrentDirectory() + $"\\wwwroot\\Template\\temp\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                //Workbook templateWorkbook = new Workbook(templatePath);
                //Workbook newWorkbook = new Workbook();
                //newWorkbook.Worksheets[0].Copy(templateWorkbook.Worksheets[0]);
                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();

                // Configure the style for borders
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;

                #region Customer Card Info
                int i = 1;
                int rowIndex = 5;
                var globlaStyle = worksheet.Cells["A5"].GetStyle();
                worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                worksheet.Cells["B3"].PutValue(string.IsNullOrEmpty(printModel.Count.ToString()) ? "0" : printModel.Count.ToString());

                foreach (var GRR in printModel)
                {
                    worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(GRR.GrnReasonCode) ? " " : GRR.GrnReasonCode);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(GRR.GrnReasonDescription) ? " " : GRR.GrnReasonDescription.Trim());

                    // Apply the style to each cell
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(style);

                    worksheet.Cells.SetRowHeight(rowIndex, 25);

                    i++;
                    rowIndex++;
                }
                #endregion

                #region Header Info
                var companyinfo = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyId)}", ""));

                byte[] logoByte = Convert.FromBase64String(companyinfo.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {

                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 400;
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width;

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
                    worksheet.PageSetup.SetHeader(0, companyinfo.Company_Description);
                }
                #endregion
                worksheet.PageSetup.SetFooter(0, $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");

                worksheet.AutoFitRows(true);
                worksheet.AutoFitColumn(3);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                savedFilePath = Path.Combine(outputDirectory, $"GRR_{DateTime.Now:ddMMyyyyhhmmssfff}.pdf");
                newWorkbook.Save(savedFilePath);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Append Data in Template - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }

            return savedFilePath;
        }

        #endregion GoodsReturnReasonCodes

        #region TransportTypes
        public IActionResult TransportType()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE> Model = new List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE>();
                string Print_Key = "TransportType_PRINT";
                ViewBag.Module = "Masters";
                ViewBag.SubTitle = "Masters";
                ViewBag.SubTitleUrl = "TransportType";


                var JsData = _apiroutine.PostAPI("Logistic", "GetTransportTypeList", "");
                if (!string.IsNullOrEmpty(JsData) && !string.IsNullOrWhiteSpace(JsData))
                {
                    if (CommonRoutine.IsValidJson(JsData))
                    {
                        Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE>>(JsData);
                    }
                }
                ViewData["Print_Key"] = Print_Key;
                ViewBag.TotalRecords = Model.Count;
                return View(Model);

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in TransportType - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });

            }
        }
        public IActionResult SaveTransportType(int id, string TransportType_desc, string TransportType_code)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
                if (id == 0)//New record
                {

                    string jsonString = $"{{" +
                               $"\"transport_type_id\": {id}, " +
                               $"\"transport_type_code\": \"{TransportType_code}\", " +
                               $"\"transport_type_description\": \"{TransportType_desc}\", " +
                            $"\"created_date\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                            $"\"updated_date\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                            $"\"created_by\": {UserId}," +
                            $"\"updated_by\": {UserId}" +
                    $"}}";
                    var res = _apiroutine.PostAPI("Logistic", "AddTransportType", jsonString);
                    if (res != "" && res.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                        if (data != null)
                        {
                            if (data.isSuccess)
                            {
                                var daata = new { result = true, msg = "Transport Type " + TransportType_code + " Successfully added !" };
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
                else if (id > 0)//update 
                {
                    string jsonString = $"{{" +
                               $"\"transport_type_id\": {id}, " +
                               $"\"transport_type_code\": \"{TransportType_code}\", " +
                               $"\"transport_type_description\": \"{TransportType_desc}\", " +

                                $"\"updated_date\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +

                                $"\"updated_by\": {UserId}" +
                    $"}}";

                    var res = _apiroutine.PostAPI("Logistic", "UpdateTransportType", jsonString);
                    if (res != "" && res.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                        if (data != null)
                        {
                            if (data.isSuccess)
                            {
                                var daata = new { result = true, msg = "Transport Type " + TransportType_code + " Successfully updated !" };
                                return Json(daata);
                            }
                            else
                            {
                                var daata = new { result = true, msg = data.Message };
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
                else
                {
                    var data = new { result = false, msg = "Oops, It's not you but us!" };
                    return Json(data);
                }

            }
            catch (Exception ex)
            {
                CommonRoutine.SetAudit("TransportType", "Error", "", "Exception in TransportType " + ex.GetBaseException(), "");
                LeSDM.AddLog("Exception in Transport Type - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }

        }

        public IActionResult ExportTransportType(int _uomID)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                SetAsposeLicense();
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE> model = new List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE>();

                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    var jsData = _apiroutine.PostAPI("Logistic", "GetTransportTypeList", null, null, queryParam);
                    model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE>>(jsData);
                }

                string filePath = AppendDataInTemplate(model);
                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Internal server error");
                }

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TransportTypePdfs", fileName);
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                byte[] pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Transport Type - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }

        public string AppendDataInTemplate(List<Logistic_Management_Lib.Model.MAST_TRANSPORT_TYPE> printModel)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext)) return "";

            string savedFilePath = "";
            try
            {

                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:TransportTypeTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both TransportTypeTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate += $"\\{sessionId}";

                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Template", "Transport Type Template.xlsx");
                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TransportTypePdfs");
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));

                //var sessionId = HttpContext.Session.Id;
                //var tempTemplate = Directory.GetCurrentDirectory() + $"\\wwwroot\\Template\\temp\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();





                // Configure the style for borders
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;

                #region Customer Card Info
                int i = 1;
                int rowIndex = 5;
                var globlaStyle = worksheet.Cells["A5"].GetStyle();
                worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                worksheet.Cells["B3"].PutValue(string.IsNullOrEmpty(printModel.Count.ToString()) ? "0" : printModel.Count.ToString());

                foreach (var TransportType in printModel)
                {
                    worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(TransportType.transport_type_code) ? " " : TransportType.transport_type_code);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(TransportType.transport_type_description) ? " " : TransportType.transport_type_description.Trim());

                    // Apply the style to each cell
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(style);

                    worksheet.Cells.SetRowHeight(rowIndex, 25);

                    i++;
                    rowIndex++;
                }
                #endregion

                #region Header Info
                var companyinfo = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyId)}", ""));

                byte[] logoByte = Convert.FromBase64String(companyinfo.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {

                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 400;
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width;

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
                    worksheet.PageSetup.SetHeader(0, companyinfo.Company_Description);
                }
                #endregion
                worksheet.PageSetup.SetFooter(0, $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");

                worksheet.AutoFitRows(true);
                worksheet.AutoFitColumn(3);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                savedFilePath = Path.Combine(outputDirectory, $"Transport_Type_{DateTime.Now:ddMMyyyyhhmmssfff}.pdf");
                newWorkbook.Save(savedFilePath);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Append Data in Template - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return savedFilePath;
        }

        #endregion TransportTypes

        #region Vessels
        public IActionResult Vessels()
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                List<Logistic_Management_Lib.Model.Mast_Vessel> Model = new List<Logistic_Management_Lib.Model.Mast_Vessel>();
                string Print_Key = "Vessels_PRINT";
                ViewBag.Module = "Masters";
                ViewBag.SubTitle = "Masters";
                ViewBag.SubTitleUrl = "Vessels";


                var JsData = _apiroutine.PostAPI("Logistic", "GetVesselList", "");
                if (!string.IsNullOrEmpty(JsData) && !string.IsNullOrWhiteSpace(JsData))
                {
                    if (CommonRoutine.IsValidJson(JsData))
                    {
                        Model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Vessel>>(JsData);

                        if (Model != null)
                        {
                            Model = Model.Where(x => !string.IsNullOrEmpty(x.VesselCode)).ToList();
                        }
                    }
                }
                ViewData["Print_Key"] = Print_Key;
                ViewBag.TotalRecords = Model.Count;
                return View(Model);

            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Vessels - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return RedirectToAction("Error", "Home", new { statuscode = 500 });

            }
        }
        public IActionResult SaveVessels(int id, string Vessel_Code, string Vessel_Name, string imo_no)
        {
            try
            {
                if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext)) return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

                int UserId = convert.ToInt(HttpContext.Session.GetString("UserID"));
                if (id == 0)//New record
                {
                    if (!string.IsNullOrEmpty(Vessel_Code))
                    {
                        string jsonString = $"{{" +
                                   $"\"vesselId\": {id}, " +
                                   $"\"vesselCode\": \"{Vessel_Code}\", " +
                                   $"\"vesselName\": \"{Vessel_Name}\", " +
                                   $"\"imoNo\": \"{imo_no}\", " +
                                $"\"createdDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                                $"\"updatedDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +
                                $"\"createdBy\": {UserId}," +
                                $"\"updatedBy\": {UserId}" +

                        $"}}";
                        var res = _apiroutine.PostAPI("Logistic", "AddVessels", jsonString);
                        if (res != "" && res.Length > 0)
                        {
                            var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                            if (data != null)
                            {
                                if (data.isSuccess)
                                {
                                    var daata = new { result = true, msg = "Vessel " + Vessel_Code + " Successfully added !" };
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
                    else
                    {
                        var data = new { result = false, msg = "Vessel Code is mandatory !" };
                        return Json(data);
                    }

                }
                else if (id > 0)//update 
                {
                    if (!string.IsNullOrEmpty(Vessel_Code))
                    {
                        string jsonString = $"{{" +
                               $"\"vesselId\": {id}, " +
                               $"\"vesselCode\": \"{Vessel_Code}\", " +
                               $"\"vesselName\": \"{Vessel_Name}\", " +
                               $"\"imoNo\": \"{imo_no}\", " +

                                $"\"updatedDate\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\", " +

                                $"\"updatedBy\": {UserId}" +
                                 $"}}";

                        var res = _apiroutine.PostAPI("Logistic", "UpdateVessels", jsonString);
                        if (res != "" && res.Length > 0)
                        {
                            var data = JsonConvert.DeserializeObject<ApiResponse>(res);
                            if (data != null)
                            {
                                if (data.isSuccess)
                                {
                                    var daata = new { result = true, msg = "Vessel Code " + Vessel_Code + " Successfully updated !" };
                                    return Json(daata);
                                }
                                else
                                {
                                    var daata = new { result = true, msg = data.Message };
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
                    else
                    {
                        var data = new { result = false, msg = "Vessel Code is mandatory !" };
                        return Json(data);
                    }
                }
                else
                {
                    var data = new { result = false, msg = "Oops, It's not you but us!" };
                    return Json(data);
                }


            }
            catch (Exception ex)
            {
                CommonRoutine.SetAudit("TransportType", "Error", "", "Exception in TransportType " + ex.GetBaseException(), "");
                LeSDM.AddLog("Exception in Transport Type - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return Json(false);
            }

        }

        public IActionResult ExportVessels(int _uomID)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext))
                return RedirectToAction("Error", "Home", new { statuscode = 403 }); ;

            try
            {
                SetAsposeLicense();
                IDictionary<string, string> queryParam = new Dictionary<string, string>();
                List<Logistic_Management_Lib.Model.Mast_Vessel> model = new List<Logistic_Management_Lib.Model.Mast_Vessel>();

                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));
                if (companyId > 0)
                {
                    queryParam.Add("companyid", companyId.ToString());
                    var jsData = _apiroutine.PostAPI("Logistic", "GetVesselList", null, null, queryParam);
                    model = JsonConvert.DeserializeObject<List<Logistic_Management_Lib.Model.Mast_Vessel>>(jsData);
                    if (model != null)
                    {
                        model = model.Where(x => !string.IsNullOrEmpty(x.VesselCode) && !string.IsNullOrEmpty(x.ImoNo)).ToList();
                    }
                }

                string filePath = AppendDataInTemplate(model);
                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Internal server error");
                }

                string fileName = Path.GetFileName(filePath);
                string serverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "VesselsPdfs", fileName);
                if (!System.IO.File.Exists(serverFilePath))
                {
                    LeSDM.AddLog($"Error while searching file '{serverFilePath}'");
                    return StatusCode(404, "File Not Found");
                }

                byte[] pdfBytes = System.IO.File.ReadAllBytes(serverFilePath);
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Print Vessels - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }
        public string AppendDataInTemplate(List<Logistic_Management_Lib.Model.Mast_Vessel> printModel)
        {
            if (!GlobalTools.HasAccess(HttpContext.Session.GetString("UserAccessRights"), "8", HttpContext)) return "";

            string savedFilePath = "";
            try
            {

                string templatePath = Configuration.GetValue("AppSettings:TemplatePaths:VesselsTemplate", "");
                var sessionId = HttpContext.Session.Id;
                string tempTemplate = Configuration.GetValue("AppSettings:TemplatePaths:TemporaryTemplate", "");

                if (string.IsNullOrEmpty(templatePath) || string.IsNullOrEmpty(tempTemplate))
                {
                    LeSDM.AddLog("Both TransportTypeTemplate and TemporaryTemplate cannot be null or empty. Please ensure that the paths are filled.");
                    return null;
                }
                tempTemplate += $"\\{sessionId}";

                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Template", "Transport Type Template.xlsx");
                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "VesselsPdfs");
                int companyId = Convert.ToInt32(HttpContext.Session.GetString("CompanyId"));

                //var sessionId = HttpContext.Session.Id;
                //var tempTemplate = Directory.GetCurrentDirectory() + $"\\wwwroot\\Template\\temp\\{sessionId}";
                Directory.CreateDirectory(tempTemplate);
                System.IO.File.Copy(templatePath, Path.Combine(tempTemplate, Path.GetFileName(templatePath)));
                Workbook newWorkbook = new Workbook(Path.Combine(tempTemplate, Path.GetFileName(templatePath)));

                Worksheet worksheet = newWorkbook.Worksheets[0];
                Style style = newWorkbook.CreateStyle();





                // Configure the style for borders
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;

                #region Customer Card Info
                int i = 1;
                int rowIndex = 5;
                var globlaStyle = worksheet.Cells["A5"].GetStyle();
                worksheet.Cells[$"A{rowIndex}"].SetStyle(globlaStyle);
                worksheet.Cells["B3"].PutValue(string.IsNullOrEmpty(printModel.Count.ToString()) ? "0" : printModel.Count.ToString());

                foreach (var TransportType in printModel)
                {
                    worksheet.Cells.InsertRow(rowIndex + 1);
                    worksheet.Cells.CopyRow(worksheet.Cells, rowIndex, rowIndex + 1);
                    worksheet.Cells[$"A{rowIndex}"].PutValue(i.ToString());
                    worksheet.Cells[$"B{rowIndex}"].PutValue(string.IsNullOrEmpty(TransportType.VesselCode) ? " " : TransportType.VesselCode);
                    worksheet.Cells[$"C{rowIndex}"].PutValue(string.IsNullOrEmpty(TransportType.VesselName) ? " " : TransportType.VesselName.Trim());
                    worksheet.Cells[$"D{rowIndex}"].PutValue(string.IsNullOrEmpty(TransportType.ImoNo.ToString()) ? " " : TransportType.ImoNo.ToString().Trim());

                    // Apply the style to each cell
                    worksheet.Cells[$"A{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"B{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"C{rowIndex}"].SetStyle(style);
                    worksheet.Cells[$"D{rowIndex}"].SetStyle(style);

                    worksheet.Cells.SetRowHeight(rowIndex, 25);

                    i++;
                    rowIndex++;
                }
                #endregion

                #region Header Info
                var companyinfo = JsonConvert.DeserializeObject<Logistic_Management_Lib.Companyinfodata>(_apiroutine.PostAPI("Logistic", $"GetCompanyDetails?companyid={Convert.ToString(companyId)}", ""));

                byte[] logoByte = Convert.FromBase64String(companyinfo.base64printLogo);
                if (logoByte != null && logoByte.Count() > 0)
                {

                    using (MemoryStream ms = new MemoryStream(logoByte))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(ms))
                        {
                            int newWidth = 400;
                            int newHeight = (originalImage.Height * newWidth) / originalImage.Width;

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
                    worksheet.PageSetup.SetHeader(0, companyinfo.Company_Description);
                }
                #endregion
                worksheet.PageSetup.SetFooter(0, $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");

                worksheet.AutoFitRows(true);
                worksheet.AutoFitColumn(3);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputDirectory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                savedFilePath = Path.Combine(outputDirectory, $"Vessels_{DateTime.Now:ddMMyyyyhhmmssfff}.pdf");
                newWorkbook.Save(savedFilePath);
                newWorkbook.Dispose();
                Directory.Delete(tempTemplate, true);
            }
            catch (Exception ex)
            {
                LeSDM.AddLog("Exception in Append Data in Vessels Template - " + ex.Message);
                LeSDM.AddLog("Stacktrace - " + ex.StackTrace);
            }
            return savedFilePath;
        }
        #endregion Vessels

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
    }



    public class ApiResponse
    {
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
        public bool isSuccess { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

    }
    public class CustomerCard
    {
        public int CustomerId { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string CustType { get; set; }

    }
}