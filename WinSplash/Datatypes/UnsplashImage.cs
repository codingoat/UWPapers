using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSplash.Datatypes
{
    public class UnsplashImage
    {
        public string url { get; set; }
        public int id { get; set; }

        public UnsplashImage(int id, string url)
        {
            this.id = id;
            this.url = url;
        }
    }
}
