using DataLabeling.Entities;

namespace DataLabeling.API.DTOs
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; }

        public UserRole Role { get; set; }

        public string Status { get; set; }
    }
}
