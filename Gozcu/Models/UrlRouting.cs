using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gozcu.Models
{
    public class UrlRouting
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string RawUrl { get; set; }
    }
}