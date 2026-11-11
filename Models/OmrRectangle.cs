using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmrDesktop.Models
{
    /// <summary>
    /// مستطیل قابل تنظیم برای عناصر مختلف پاسخنامه.
    /// مختصات نسبت به محدوده والد ذخیره می‌شوند.
    /// </summary>
    public sealed class OmrRectangle
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "Rectangle";

        // مختصات نسبی نسبت به محدوده والد
        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
    }
}