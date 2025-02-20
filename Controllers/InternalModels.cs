using System;
using System.Collections.Generic;
using LES_USER_ADMINISTRATION_LIB.Model;
using Logistic_Management_Lib;

namespace LeS_LogiLink_WebApp.Controllers
{
    public class OutboundPrintModel
    {
        public ShipmentInfo ShipmentInfo { get; set; }
        public Logistic_Management_Lib.Model.V_SHIPMENT_TRIP_PLAN ShipmentTripPlan { get; set; }
        public List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> ShipmentDeliveryOrders { get; set; }
        public List<Logistic_Management_Lib.Model.Shipment_Documents> ShipmentDocuments { get; set; }

        public string QrModel { get; set; }

        public Logistic_Management_Lib.Companyinfodata LogoModel { get; set; }
    }
    public class OutboundShipmentUpdateModal
    {
        public UpdateShipmentOrderModal UpdateShipmentOrderModal { get; set; }
        public List<Logistic_Management_Lib.Model.V_SHIPMENT_DELIVERY_ORDERS> ShipmentDeliveryorders { get; set; }
        public bool IsBuyer {  get; set; }=false;

    }

    public class DeliveryOrderDetailsPrint
    {
        public Logistic_Management_Lib.Model.V_Delivery_Orders_Info DeliveryOrdersInfo { get; set; }
        public Logistic_Management_Lib.Model.V_DELIVERY_ORDERS_ADDRESS_EPOD ShippingAddressDetail { get; set; }
        public List<Logistic_Management_Lib.Model.V_DELIVERY_ORDER_LINES> DeliveryOrderLines { get; set; }

        public Logistic_Management_Lib.Companyinfodata LogoModel { get; set; }
    }

    public class PrintGoodsReturn
    {
        public Logistic_Management_Lib.Companyinfodata companydata { get; set; }
        public Logistic_Management_Lib.PrintGoodsReturnHeaderInfo goodsreturnheaderinfo { get; set; }
        public List<Logistic_Management_Lib.PrintGoodsReturnItemInfo> goodsreturniteminfo { get; set; }
    }

    public class ShipmentInfo
    {
        public int shipmentid { get; set; }

        public string order_no { get; set; }

        public int? receiverid { get; set; }

        public DateTime? planned_ship_date { get; set; }

        public DateTime? planned_delivery_date { get; set; }

        public string shipment_notes { get; set; }

        public int? shipment_statusid { get; set; }

        public string jobno { get; set; }

        public int? companyid { get; set; }

        public int? vessel_id { get; set; }

        public DateTime? vessel_eta { get; set; }

        public DateTime? vessel_ata { get; set; }

        public DateTime? delivery_date { get; set; }

        public int? anchorage_id { get; set; }

        public string agent { get; set; }

        public string agent_contact_person { get; set; }

        public string agent_contact_no { get; set; }

        public string supply_boat { get; set; }

        public string supply_boat_contact_person { get; set; }

        public string supply_boat_contact_no { get; set; }

        public string loading_point { get; set; }

        public DateTime? loading_time { get; set; }

        public string co_party { get; set; }

        public string vessel_code { get; set; }

        public string vessel_name { get; set; }

        public int? imo_no { get; set; }

        public string anchorage_code { get; set; }

        public string anchorage_description { get; set; }

        public string transport_type_code { get; set; }

        public string transport_type_description { get; set; }

        public int? transport_type_id { get; set; }

        public string driver_name { get; set; }

        public string cust_code { get; set; }

        public string cust_name { get; set; }

        public string shipment_statusdesc { get; set; }

        public string Epod_Shipment_Notes { get; set; }

        public int? Is_Delete { get; set; }

        public string Vehicle_no { get; set; }

        public string Boarding_Officer_Name { get; set; }
        public string initial_Receipt_Emailid { get; set; }
        public string initial_Receipt_Crew { get; set; }
        public string initial_Receipt_Role { get; set; }
        public DateTime? initial_Receipt_Date { get; set; }
        public string initial_Receipt_LoCode { get; set; }
        public string final_Receipt_Emailid { get; set; }
        public string final_Receipt_Crew { get; set; }
        public string final_Receipt_Role { get; set; }
        public DateTime? final_Receipt_Date { get; set; }
        public string final_Receipt_LoCode { get; set; }
    }

    public class UserDetailsData
    {
        public UserDetailsModal UserDetailModel { get; set; }
        public List<LES_USERTYPE> Roles { get; set; }
        public List<CompaniesList> Companies { get; set; }
    }

    public class CompaniesList
    {

        public int companyid { get; set; }
        public string company_code { get; set; }
        public string company_description { get; set; }
        public int addressId { get; set; }
        public string addr_type { get; set; }

    }

    public class UserDataDetails
    {
        public SM_EXTERNAL_USERS UserDetails { get; set; }
        public string Password { get; set; }
        public string Confirmpassword { get; set; }
        public int IsNewUser { get; set; } = 0;
    }

    public class CompanyRolesList
    {
        public List<CompaniesList> AllCompanyList {  get; set; }
        public List<LES_USERTYPE> AllRoles { get; set; }
    }

    public class UserAccessLevelsModal
    {
       public List<LES_USERTYPE> UserTypes {  get; set; }
    }


}
