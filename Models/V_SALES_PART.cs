using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WMS_TMS.Models
{
    public class V_SALES_PART
    {
        [Key]
        public int SalesPartId { get; set; }
        public string Catalog_no { get; set; }
        public string CATALOG_DESC { get; set; }
        public string PARTNAME { get; set; }
        public string UOM { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public DateTime? UPDATED_DATE { get; set; }
        public string SALES_PART_TYPE { get; set; }
        public int? DRIVER_ALLOCATION_REQUIRED { get; set; }
        public int? Used_For_Purchase { get; set; }
        public double? PART_PRICE { get; set; }
        public string PART_NUMBER { get; set; }
        public double? PART_PRICE_INCL_TAX { get; set; }
        public int? PART_STATUS { get; set; }
        public int? SITEID { get; set; }
        public string Site_Name { get; set; }
        public string SITE_CODE { get; set; }
    }
}
