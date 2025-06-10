using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class UserQueryRequest : BaseQuery
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }

        public string? Country { get; set; }
        public bool? Status { get; set; }
        public int? RoleId { get; set; }
        public bool? EmailVerified { get; set; }
        public string? Sort { get; set; }
        //public string? Order { get; set; } // "asc" or "desc"
    }
}
