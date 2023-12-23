using System.ComponentModel.DataAnnotations.Schema;
namespace TalkFusion.Models
{
    public class JoinRequest
    {
        public string UserId { get; set; }
        public int GroupId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Group Group { get; set; }
    }
}

