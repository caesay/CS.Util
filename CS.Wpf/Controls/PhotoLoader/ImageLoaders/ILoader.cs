using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CS.Wpf.Controls.PhotoLoader.ImageLoaders
{
    internal interface ILoader
    {
        Stream Load(string source);
    }
}
