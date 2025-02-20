using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace WMS_TMS.Models
{
    public partial class VehicleTripContractDetails
    {
        [Key]
        public int ContractId { get; set; }
        public int? ShippingId { get; set; }
        public int? TripPlanNo { get; set; }
        public int? TripStatus { get; set; }
        public string Lot { get; set; }
        public string FromLoc { get; set; }
        public string ToLoc { get; set; }
        public DateTime? EtaEtd { get; set; }
        public string Permit { get; set; }
        public string VehicleType { get; set; }
        public DateTime? BLcutoff { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
