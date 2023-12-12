using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalkFusion.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Group title is mandatory")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Group description is mandatory")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Group category is mandatory")]
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<Channel>? Channels { get; set; }

        //used in New() or Edit()
        [NotMapped]
        public IEnumerable<SelectListItem>? AllCategories { get; set; }

    }
}
