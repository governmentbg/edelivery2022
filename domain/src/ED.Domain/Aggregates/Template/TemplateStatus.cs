using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public enum TemplateStatus
    {
        None = 0,
        Draft = 1,
        Published = 2,
        Archived = 3
    }
}
