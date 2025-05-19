using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class BudgetAlertQueryRequest : BaseQuery
    {
        public string Message { get; set; }
        public int? BudgetId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 