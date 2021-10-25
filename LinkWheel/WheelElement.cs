using System;
using System.Collections.Generic;
using System.Drawing;

namespace LinkWheel
{
    public class WheelElement
    {
        public string Name;
        public string Description;
        public IEnumerable<string> CommandAction;
        public Bitmap Icon => IconLazy.Value;
        private Lazy<Bitmap> IconLazy;
        public Func<Bitmap> IconFetcher { set { IconLazy = new Lazy<Bitmap>(value); } }
    }
}
