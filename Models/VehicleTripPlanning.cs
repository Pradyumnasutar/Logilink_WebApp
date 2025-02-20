using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace WMS_TMS.Models
{
    public partial class VehicleTripPlanning
    {
        [Key]
        public int VehTripId { get; set; }
        public int? ContractId { get; set; }
        public int? ShippingId { get; set; }
        public int? PlanStatus { get; set; }
        public string PointType { get; set; }
        public string ProjectId { get; set; }
        public string ActivityType { get; set; }
        public int? Column8 { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
