using System.Collections.Generic;

namespace EDelivery.WebPortal.Models.Templates
{
    public class BuilderContainer
    {
        public Dictionary<string, string[]> Values { get; set; } =
            new Dictionary<string, string[]>();

        public List<BuilderModelStateError> Errors { get; set; } =
              new List<BuilderModelStateError>();
    }
}