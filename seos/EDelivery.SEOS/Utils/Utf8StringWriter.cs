using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.Utils
{
    public class Utf8StringWriter: StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }
}
