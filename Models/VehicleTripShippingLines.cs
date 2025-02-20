using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace WMS_TMS.Models
{
    public partial class VehicleTripShippingLines
    {
        [Key]
        public int VehTripShipLineId { get; set; }
        public int? VehTripId { get; set; }
        public int? ContractId { get; set; }
        public int? LineNo { get; set; }
        public string ItemName { get; set; }
        public string SourceRef1 { get; set; }
        public string SourceRef2 { get; set; }
        public string SourceRef3 { get; set; }
        public string SalesPartNo { get; set; }
        public int? Qty { get; set; }
        public string Uom { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
