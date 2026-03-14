using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public enum TaskStatus
    {
        Pending = 0,
        Annotating = 1,
        Approved = 2,
        Rejected = 3
    }
}
