using System.ComponentModel.DataAnnotations;

namespace TalkFusion.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Message Text is mandatory")]
        public string? Text { get; set; }

        [Required(ErrorMessage = "Message Date is mandatory")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "ChannelId is mandatory")]
        public int? ChannelId { get; set; }

        public virtual Channel? Channel { get; set; }
    }
}
