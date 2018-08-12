using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSplash.Datatypes
{
    public class ImageOption
    {
        public string icon { get; set; }
        public string text { get; set; }

        public ImageOption(string icon, string text)
        {
            this.icon = icon;
            this.text = text;
        }

		public override string ToString()
        {
            return text;
        }
    }
}
