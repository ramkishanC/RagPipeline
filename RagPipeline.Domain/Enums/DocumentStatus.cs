using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Domain.Enums
{
    public enum DocumentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
