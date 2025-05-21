using System.Collections.Generic;

namespace Finamon.Service.ReponseModel
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string? Color { get; set; }

        public DateTime CreatedDate { get; set; }
        public ICollection<ExpenseResponse> Expenses { get; set; }
    }
} 