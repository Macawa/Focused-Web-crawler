using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.CryptoCrawler
{
    public class UrlInfo
    {
        public UrlInfo() { }

        public UrlInfo(string Url, string httpStatusCode, string timestamp)
        {
            this.URL = Url;
            this.HttpStatusCode = httpStatusCode;
            this.Timestamp = timestamp;
            
        }

        public string URL { get; set; }
        public string HttpStatusCode { get; set; }
        public string Timestamp { get; set; }
        
    }
}
