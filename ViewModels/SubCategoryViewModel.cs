// ViewModels/SubCategoryViewModel.cs
namespace WorkFlowWeb.ViewModels
{
    public class SubCategoryViewModel
    {
        public int SubCategoryId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; }
        public string InactivatedBy { get; set; }
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; } // To hold the Category code
    }
}
