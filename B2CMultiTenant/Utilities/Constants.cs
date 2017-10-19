using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2CMultiTenant.Utilities
{
    public static class Constants
    {
        public static readonly string TenantIdClaim = "TenantId";
        public static readonly string TenantNameClaim = "TenantName";
        public static readonly string ObjectIdClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public static readonly string PolicyIdClaim = "tfp";
        public static readonly string RoleClaim = "role";

        public static readonly string Admin = "admin";
        public static readonly string User = "user";

    }
}