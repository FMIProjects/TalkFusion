using System.ComponentModel.DataAnnotations;

namespace TalkFusion.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is mandatory")]
        public string? CategoryName { get; set; }

        public virtual ICollection<Group>? Groups { get; set; }
    }
}
