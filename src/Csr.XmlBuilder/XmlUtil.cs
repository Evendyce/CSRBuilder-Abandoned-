using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Csr.XmlBuilder
{
    public static class XmlUtil
    {
        public static void AddIfNotNull(XElement parent, XElement? child) { if (child != null) parent.Add(child); }
    }
}
