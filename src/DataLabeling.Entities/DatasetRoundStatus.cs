using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.Entities
{
    public enum DatasetRoundStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}
