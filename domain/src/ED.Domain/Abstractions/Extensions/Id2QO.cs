using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    [Keyless]
    public class Id2QO
    {
        public int Id1 { get; set; }

        public int Id2 { get; set; }
    }
}
