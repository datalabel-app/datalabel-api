using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int Points { get; set; } = 0;

        public UserRole Role { get; set; }

        public string Status { get; set; }

        public bool IsChangePassword { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<Project> Projects { get; set; } = new List<Project>();

        public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();

        public ICollection<Token> Token { get; set; } = new List<Token>();

        public ICollection<TaskErrorHistory> ReviewedErrors { get; set; } = new List<TaskErrorHistory>();

        public ICollection<Label> CreatedLabels { get; set; } = new List<Label>();
    }
}
