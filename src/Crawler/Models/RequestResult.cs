using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Models
{
    public class RequestResult
    {
        public string Html { get; set; }

        public MemoryStream Stream { get; set; }
    }
}
