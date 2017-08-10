using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UIKit;

namespace LibUniqBuild.iOS
{
    class Extentions
    {
    }

    public static class UIColorExtensions
    {
        public static UIColor FromHex(this UIColor color, int hexValue)
        {
            return UIColor.FromRGB(
                (((float)((hexValue & 0xFF0000) >> 16)) / 255.0f),
                (((float)((hexValue & 0xFF00) >> 8)) / 255.0f),
                (((float)(hexValue & 0xFF)) / 255.0f)
            );
        }

        public static UIColor FromHexA(this UIColor color, long hexValue)
        {
            return UIColor.FromRGBA(
                (((float)((hexValue & 0xFF0000) >> 16)) / 255.0f),
                (((float)((hexValue & 0xFF00) >> 8)) / 255.0f),
                (((float)(hexValue & 0xFF)) / 255.0f),
                (((float)((hexValue & 0xFF000000) >> 24)) / 255.0f)
            );
        }
    }

    public static class UIViewExtensions
    {
        public static float X(this UIView view)
        {
            return (float)view.Frame.X;
        }

        public static float Y(this UIView view)
        {
            return (float)view.Frame.Y;
        }

        public static float Width(this UIView view)
        {
            return (float)view.Frame.Width;
        }

        public static float Height(this UIView view)
        {
            return (float)view.Frame.Height;
        }
    }

    public static class NSDataExtensions
    {
        public static byte[] ToByte(this NSData data)
        {
            MemoryStream ms = new MemoryStream();
            data.AsStream().CopyTo(ms);
            return ms.ToArray();
        }
    }
}