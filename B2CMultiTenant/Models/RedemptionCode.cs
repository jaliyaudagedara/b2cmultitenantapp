using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace B2CMultiTenant.Models
{
    public class RedemptionCode
    {
        [Key]
        public string Code { get; set; }
        public string TenantId { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresBy { get; set; }
    }
}