using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class Image :BaseEntity
    {
        public string Base64Image { get; set; }
        public string ContentType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<BlogImage> BlogImages { get; set; }
    }
}
