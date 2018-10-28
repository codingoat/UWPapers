using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSplash.Datatypes
{
    public class PixaImage
    {
        public string url { get; set; }
        public int id { get; set; }

        public string smallUrl { get; set; }
        public string bigUrl { get; set; }
        public string pageUrl { get; set; }

        public PixaImage(int id, string url)
        {
            this.id = id;
            this.url = url;
        }

        public PixaImage(int id, string url, string smallUrl, string bigUrl, string pageUrl)
        {
            this.id = id;
            this.url = url;
            this.smallUrl = smallUrl;
            this.bigUrl = bigUrl;
            this.pageUrl = pageUrl;
        }
    }
}
