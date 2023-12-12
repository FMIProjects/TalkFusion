using Microsoft.AspNetCore.Identity;

namespace TalkFusion.Models
{
    public class ApplicationUser : IdentityUser
    {

        // a user can be a member of many groups
        public virtual ICollection<UserGroup>? UserGroups { get; set; }

        // a user can post many comments
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
