using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WorkFlowWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(25)]
        public string Code { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        [MaxLength(15)]
        [AllowNull]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsActive { get; set; }

        [MaxLength(15)]
        [AllowNull]
        public string? InactivatedBy { get; set; }

        [AllowNull]
        public ICollection<SubCategory>? SubCategories { get; set; }
    }

}
