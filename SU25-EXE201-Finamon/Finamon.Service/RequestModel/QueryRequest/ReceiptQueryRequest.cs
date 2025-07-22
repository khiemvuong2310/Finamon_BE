using Finamon_Data.Enum;
using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class ReceiptQueryRequest : BaseQuery
    {
        public int? UserId { get; set; }
        public int? MembershipId { get; set; }
        public ReceiptStatus? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 