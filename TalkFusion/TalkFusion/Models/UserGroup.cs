using System.ComponentModel.DataAnnotations.Schema;

namespace TalkFusion.Models
{
    public class UserGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? GroupId { get; set; }
        public bool? IsModerator { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Group? Group { get; set; }
    }
}
