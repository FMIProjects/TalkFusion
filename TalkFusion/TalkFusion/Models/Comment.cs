using System.ComponentModel.DataAnnotations;

namespace TalkFusion.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Message Text is mandatory")]
        public string? Text { get; set; }

        public DateTime? Date { get; set; }

        public int? ChannelId { get; set; }

        public virtual Channel? Channel { get; set; }

        //user id
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
