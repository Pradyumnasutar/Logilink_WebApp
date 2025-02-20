using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace WMS_TMS.Models
{
    public partial class LES_WMS_TMSContext : DbContext
    {
        public LES_WMS_TMSContext()
        {
        }

        public LES_WMS_TMSContext(DbContextOptions<LES_WMS_TMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CustomerOrderLine> CustomerOrderLine { get; set; }
        public virtual DbSet<CustomerOrders> CustomerOrders { get; set; }
        public virtual DbSet<CustomerOrdersAddress> CustomerOrdersAddress { get; set; }
        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<Driver_Allocation> DriverAllocation { get; set; }
        public virtual DbSet<Driver_Profile_Tab> DriverProfileTab { get; set; }
        public virtual DbSet<MastOrderTypes> MastOrderTypes { get; set; }
        public virtual DbSet<Mast_Site> MastSite { get; set; }
        public virtual DbSet<MastUom> MastUom { get; set; }
        public virtual DbSet<SalesParts> SalesParts { get; set; }
        public virtual DbSet<VehicleTripContractDetails> VehicleTripContractDetails { get; set; }
        public virtual DbSet<VehicleTripPlanning> VehicleTripPlanning { get; set; }
        public virtual DbSet<VehicleTripShippingLines> VehicleTripShippingLines { get; set; }
    }
}
