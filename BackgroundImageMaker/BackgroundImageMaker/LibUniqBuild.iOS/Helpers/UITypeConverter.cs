using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibUniqBuild.iOS.Helpers
{
    public class UITypeConverter
    {
        public static byte[] ToByte(NSData data)
        {
            MemoryStream ms = new MemoryStream();
            data.AsStream().CopyTo(ms);
            return ms.ToArray();
        }
    }
}