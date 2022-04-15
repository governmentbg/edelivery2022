using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    [Keyless]
    public class StringIdQO
    {
        public string? Id { get; set; }
    }
}
