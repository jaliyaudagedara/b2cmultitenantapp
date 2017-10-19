﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2CMultiTenant.Models
{
    public class UserRole
    {
        [Key,Column(Order=0)]
        public string TenantId { get; set; }
        [Key, Column(Order = 1)]
        public string UserObjectId { get; set; }
        [Key, Column(Order = 2)]
        public string Role { get; set; }
    }
}