using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS.Wpf.Controls.PhotoLoader.ImageLoaders
{
    internal static class LoaderFactory
    {
        public static ILoader CreateLoader(SourceType sourceType)
        {
            switch (sourceType)
            {
                case SourceType.LocalDisk:
                    return new LocalDiskLoader();
                case SourceType.ExternalResource:
                    return new ExternalLoader();
                default:
                    throw new ApplicationException("Unexpected exception");
            }
        }
    }
}
