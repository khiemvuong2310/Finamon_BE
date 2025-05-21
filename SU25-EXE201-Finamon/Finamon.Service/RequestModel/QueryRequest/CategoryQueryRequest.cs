using System;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class CategoryQueryRequest : BaseQuery
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 