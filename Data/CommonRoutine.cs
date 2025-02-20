using Aspose.Cells;
using LeSDataMain;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LeS_LogiLink_WebApp.Data
{
    public class CommonRoutine
    {
        //public static int GetCompanyId(HttpContext Context)
        //{
        //    return convert.ToInt(Context.Session.GetString("CompanyId"));
        //}
        public static void SetAudit(string ModuleName, string Action, string FileName, string Log_msg, string refNo)
        {
            try
            {
                LeSDataMain.LeSDM.SetAuditLogFile(ModuleName, Action, FileName, Log_msg, "", refNo, "", "");
            }
            catch (Exception ex)
            {
                LeSDataMain.LeSDM.AddLog("Error on SetAudit : " + ex.Message + " Stack Trace : " + ex.StackTrace);
            }
        }
        public static bool IsValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim();

            if ((input.StartsWith("{") && input.EndsWith("}")) ||
                (input.StartsWith("[") && input.EndsWith("]")))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject(input);
                    return true;
                }
                catch (JsonException)
                {
                    return false;
                }
                catch (Exception) 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //public static Dictionary<string, (object oldValue, object newValue)> GetDifferences(PartInventoryStockDetails_Exist OldObj, PartInventoryStockDetails NewObj)
        //{
        //    Dictionary<string, (object, object)> differences = new Dictionary<string, (object, object)>();

        //    Type type = typeof(PartInventoryStockDetails);
        //    PropertyInfo[] properties = type.GetProperties();

        //    Type typeExt = typeof(PartInventoryStockDetails_Exist);
        //    PropertyInfo[] propertiesExt = typeExt.GetProperties();

        //    foreach (var property in properties)
        //    {
        //        var propertyExt =  propertiesExt.FirstOrDefault(x => x.Name == property.Name);
        //        object value1 = propertyExt.GetValue(OldObj);
        //        object value2 = property.GetValue(NewObj);

        //        if (!object.Equals(value1, value2))
        //        {
        //            differences[property.Name] = (value1, value2);
        //        }
        //    }

        //    return differences;
        //}

        //public static PartInventoryStockDetails_Exist FillPartInventoryStockDetails(PartInventoryStockDetails eObj)
        //{
        //    PartInventoryStockDetails_Exist fObj = new PartInventoryStockDetails_Exist();
        //    fObj.PART_STOCK_DETAIL_ID = eObj.PART_STOCK_DETAIL_ID;
        //    fObj.SalesPartId = eObj.SalesPartId;
        //    fObj.LOGTYPE = eObj.LOGTYPE;
        //    fObj.LOGDATE = eObj.LOGDATE;
        //    fObj.USERID = eObj.USERID;
        //    fObj.QUANTITY = eObj.QUANTITY;
        //    fObj.UOM = eObj.UOM;
        //    fObj.PARTLOCATIONID = eObj.PARTLOCATIONID;
        //    fObj.LOGPRICE = eObj.LOGPRICE;
        //    fObj.CURRENCYID = eObj.CURRENCYID;
        //    fObj.LOGEXCHRATE = eObj.LOGEXCHRATE;
        //    fObj.LOGNOTE = eObj.LOGNOTE;
        //    fObj.UPDATED_DATE = eObj.UPDATED_DATE;
        //    fObj.CREATED_DATE = eObj.CREATED_DATE;
        //    fObj.SITEID = eObj.SITEID;
        //    fObj.OrderId = eObj.OrderId;
        //    fObj.TRANS_TYPE = eObj.TRANS_TYPE;
        //    fObj.DATE_IN = eObj.DATE_IN;
        //    fObj.DATE_OUT = eObj.DATE_OUT;
        //    fObj.M3 = eObj.M3;
        //    fObj.SERIAL = eObj.SERIAL;
        //    fObj.BATCH = eObj.BATCH;
        //    fObj.EXPIRY_DATE = eObj.EXPIRY_DATE;
        //    fObj.internallineid = eObj.internallineid;
        //    fObj.shipmentlineid = eObj.shipmentlineid;
        //    fObj.shipmentid = eObj.shipmentid;
        //    fObj.PICKED_QTY = eObj.PICKED_QTY;
        //    fObj.customerid = eObj.customerid;
        //    fObj.marking = eObj.marking;
        //    fObj.stock_typeid = eObj.stock_typeid;
        //    fObj.qty_intransit_to_location = eObj.qty_intransit_to_location;
        //    fObj.weight = eObj.weight;
        //    fObj.dimension = eObj.dimension;
        //    fObj.grn_exported = eObj.grn_exported;
        //    fObj.pick_exported = eObj.pick_exported;
        //    fObj.move_exported = eObj.move_exported;
        //    fObj.receive_with_bulk = eObj.receive_with_bulk;
        //    fObj.stock_statusid = eObj.stock_statusid;
        //    fObj.customerorderlineid = eObj.customerorderlineid;
        //    fObj.customerorderid = eObj.customerorderid;
        //    return fObj;
        //}

        public static void SetCompanyName(ref Worksheet _sheet, string CompanyName, int HeaderSection) { 
            if (_sheet != null) {
                PageSetup pageSetup = _sheet.PageSetup;
                if (pageSetup != null)
                {
                    pageSetup.SetHeader(HeaderSection, CompanyName);
                }         
            }
        }
    }

    public partial class GlobalTools
    {
        public static string cPWDecrypted = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@#$%^&*()[]-_~!+:,'{}";
        public static string cPWEncrypted = @".-,+*)(~&%$#}!@?>=<;:98765^]\[ZYXWV_ABCDEFGHIJKLMNOPQRSTU";

        public static bool IsSafeDataSet(System.Data.DataSet ds)
        {
            return GlobalTools.IsSafeDataSet(ds, 1);
        }

        public static bool IsSafeDataSet(System.Data.DataSet ds, int nbTables)
        {
            if ((((ds != null)
                        && (ds.Tables != null))
                        && (ds.Tables.Count == nbTables)))
            {
                for (int i = 0; (i < nbTables); i = (i + 1))
                {
                    if ((ds.Tables[i].Rows == null) || ((ds.Tables[i].Rows != null) && ds.Tables[i].Rows.Count == 0))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static string EncodePassword(string cPWD)
        {
            string cOut = ""; int i;
            try
            {
                for (i = 0; i < cPWD.Length; i++)
                {
                    cOut = cOut + cPWEncrypted[cPWDecrypted.IndexOf(char.ToUpper(cPWD[i]))];
                }
                return cOut;
            }
            catch { return ""; }
        }

        public static string DecodePassword(string cPWD)
        {
            string cOut = ""; int i;
            try
            {
                for (i = 0; i < cPWD.Length; i++)
                {
                    cOut = cOut + cPWDecrypted[cPWEncrypted.IndexOf(char.ToUpper(cPWD[i]))];
                }
                return cOut;
            }
            catch { return ""; }
        }

        public static string EncryptID(int Quotationid)
        {
            try
            {
                string _encryptVal = EncodePWD(Quotationid.ToString());
                if (_encryptVal.Trim().Length > 0)
                {
                    DateTime dtDateTime = DateTime.Now;
                    _encryptVal = EncodePWD(dtDateTime.ToString("ssmmHHyyMMMdd")) + "|" + _encryptVal + "|" + EncodePWD(dtDateTime.ToString("ddMMMyyHHmmss"));
                    byte[] byteArr = System.Text.Encoding.ASCII.GetBytes(_encryptVal.Trim());
                    return Convert.ToBase64String(byteArr);
                }
                else return "";
            }
            catch (Exception ex)
            {
                LeSDataMain.LeSDM.AddLog("Error in EncryptID() : " + ex.Message);
                return "";
            }
        }

        public static int DecryptID(string KeyID)
        {
            try
            {
                string decryptKeyID = "";
                if (KeyID.Trim().Length > 0)
                {
                    KeyID = KeyID.Replace("%2f", "/").Replace("%2F", "/");
                    KeyID = KeyID.Replace("%3d", "=").Replace("%3d", "=");
                    byte[] byteArr = Convert.FromBase64String(KeyID);
                    decryptKeyID = System.Text.Encoding.UTF8.GetString(byteArr);
                }
                if (decryptKeyID.Trim().Length > 0 && decryptKeyID.Split('|').Length > 2)
                {
                    string[] _keys = decryptKeyID.Split('|');
                    DateTime dt1 = DateTime.MinValue, dt2 = DateTime.MinValue;
                    DateTime.TryParseExact(DecodePassword(_keys[0]), "ssmmHHyyMMMdd", null, System.Globalization.DateTimeStyles.None, out dt1);
                    DateTime.TryParseExact(DecodePassword(_keys[2]), "ddMMMyyHHmmss", null, System.Globalization.DateTimeStyles.None, out dt2);

                    //if (dt1 != DateTime.MinValue && dt2 != DateTime.MinValue && dt1 == dt2)
                    if (dt1 != DateTime.MinValue && dt2 != DateTime.MinValue)
                    {
                        decryptKeyID = DecodePassword(_keys[1]);
                    }
                }
                //decryptKeyID = decryptKeyID.Replace("Key=", "");
                return convert.ToInt(decryptKeyID.Trim());
            }
            catch (Exception ex)
            {
                LeSDataMain.LeSDM.AddLog("Error in DecryptID() : " + ex.Message);
                return 0;
            }
        }

        public static string EncodePWD(string cPWD)
        {
            string cOut = ""; int i;
            try
            {
                for (i = 0; i < cPWD.Length; i++)
                {
                    cOut = cOut + cPWEncrypted[cPWDecrypted.IndexOf(char.ToUpper(cPWD[i]))];
                }
                return cOut;
            }
            catch { return ""; }
        }

        public static bool HasAccess(string UserAccessDetails, string ModuleID,HttpContext context, string AccessLevel = "")
        {
            if (UserAccessDetails != null)
            {
                Dictionary<int,Dictionary<string, string>> UserAccesses = JsonConvert.DeserializeObject<Dictionary<int,Dictionary<string, string>>>(UserAccessDetails);
                int company = convert.ToInt(context.Session.GetString("CompanyId"));
                if (company == 0)
                {
                    company =convert.ToInt(context.Session.GetString("EPOD_CompanyId"));
                }
                var UserAccess = UserAccesses[company];
                if (!UserAccess.ContainsKey(ModuleID) || (UserAccess.ContainsKey(ModuleID) && UserAccess[ModuleID] == "0") || (AccessLevel != "" && UserAccess.ContainsKey(ModuleID) && convert.ToInt(UserAccess[ModuleID]) <= convert.ToInt(AccessLevel))) return false;
                else return true;

            }
            else return false;
        }
        // New overloaded method for multiple ModuleIDs
        public static bool HasAccess(string UserAccessDetails, List<string> ModuleIDs, HttpContext context, string AccessLevel = "")
        {
            if (UserAccessDetails != null)
            {
                // Deserialize the user's access details
                Dictionary<int, Dictionary<string, string>> UserAccesses = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, string>>>(UserAccessDetails);
                int company = Convert.ToInt32(context.Session.GetString("CompanyId"));

                // Check if the user's company access exists
                if (!UserAccesses.ContainsKey(company))
                    return false;

                var UserAccess = UserAccesses[company];

                // Iterate through all provided ModuleIDs
                foreach (var ModuleID in ModuleIDs)
                {
                    // If access is granted for any module, return true
                    if (UserAccess.ContainsKey(ModuleID) && UserAccess[ModuleID] != "0" &&
                        (AccessLevel == "" || Convert.ToInt32(UserAccess[ModuleID]) > Convert.ToInt32(AccessLevel)))
                    {
                        return true;
                    }
                }

                // If no module has access, return false
                return false;
            }
            else
            {
                return false; // Return false if UserAccessDetails is null
            }
        }




    }

}
