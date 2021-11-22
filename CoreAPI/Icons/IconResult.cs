using System.Drawing;

namespace CoreAPI.Icons
{
    public class IconResult
    {
        public string Path { get; set; }
        public Bitmap Icon { get; set; }

        public IconResult(Bitmap icon, string path)
        {
            Icon = icon;
            Path = path;
        }
    }
}
