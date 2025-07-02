using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class SiteAnalytic : BaseEntity
    {
        public string? SiteName { get; set; }

        public int? Count { get; set; } = 0;

        public string? Note { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
