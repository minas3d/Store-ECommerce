using ECommerce521.Validations;
using System.ComponentModel.DataAnnotations;

namespace ECommerce521.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, CustomLenght(5, 100)]
        public string Name { get; set; } = string.Empty;
        [MinLength(5)]
        public string? Description { get; set; }
        public bool Status { get; set; } = true;
    }
}
