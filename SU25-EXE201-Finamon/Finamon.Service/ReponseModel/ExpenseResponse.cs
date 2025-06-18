namespace Finamon.Service.ReponseModel
{
    public class ExpenseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Color { get; set; }
        public bool CategoryIsDelete { get; set; }
    }

    public class ExpenseDetailResponse : ExpenseResponse
    {
        public UserResponse User { get; set; }
        public CategoryResponse Category { get; set; }
    }
}