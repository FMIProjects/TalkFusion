using System.ComponentModel.DataAnnotations;

namespace TalkFusion.Models
{
    public class Channel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Channel title is mandatory")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Channel description is mandatory")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Channel group is mandatory")]
        public int GroupId { get; set; }

        public virtual Group group { get; set; }

        virtual public ICollection<Comment> Comments { get; set; }


    }
}
