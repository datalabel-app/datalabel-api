using System;

namespace DataLabeling.Entities
{
    public class User
    {
        public int UserId { get; set; }      

        public string FullName { get; set; }     

        public string Email { get; set; }

        public string Password { get; set; }

        public UserRole Role { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }  

        public DateTime UpdatedAt { get; set; }

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}