using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.CryptoCrawler
{
    
    
    public class Addresses
    {
       
        public Addresses() { }

        public Addresses(string address, string url, string metaText, string timestamp)
        {
            this.Address = address;
            this.URL = url;
            this.MetaText = metaText;
            this.Timestamp = timestamp;
            
        }

        
        public string Address { get; set; }
        public string URL { get; set; }
        public string MetaText { get; set; }
        public string Timestamp { get; set; }
    }
}
