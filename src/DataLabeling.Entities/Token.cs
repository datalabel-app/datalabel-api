using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public class Token
    {
        public int TokenId { get; set; }

        public int UserId { get; set; }

        public string TokenType { get; set; } = null!;


        [Required]
        public string TokenValue { get; set; } = null!;

        public bool IsUsed { get; set; } = false;

        public DateTime Expired { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;


    }
}
