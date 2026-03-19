using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class RegisterRequest
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }


        public UserRole? Role { get; set; }
    }
}