using Microsoft.AspNetCore.Identity;

namespace GlobalTadka.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Order>? Orders { get; set; }
    }
}
