using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public enum DataItemStatus
    {
        Pending = 0,
        InProgress = 1,
        Labeled = 2,
        Rejected = 3
    }
}
