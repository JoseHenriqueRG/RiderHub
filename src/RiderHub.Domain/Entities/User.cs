using RiderHub.Domain.Enums;

namespace RiderHub.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public UserRoleEnum Role { get; set; }
    }
}
