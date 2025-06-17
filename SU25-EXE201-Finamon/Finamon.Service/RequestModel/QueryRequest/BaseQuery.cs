using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon.Service.RequestModel.QueryRequest
{
    public class BaseQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsDeleted { get; set; }
        public SortByEnum? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
