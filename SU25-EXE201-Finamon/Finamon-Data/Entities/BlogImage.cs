using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon_Data.Entities
{
    public class BlogImage
    {
        public int BlogId { get; set; }
        public int ImageId { get; set; }
        public virtual Blog Blog { get; set; }
        public virtual Image Image { get; set; }
    }
}
