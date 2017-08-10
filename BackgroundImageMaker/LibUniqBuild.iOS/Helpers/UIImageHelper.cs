using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace LibUniqBuild.iOS.Helpers
{
    public class UIImageHelper
    {
        public static UIImage FromFileAuto(string filename, string extension = "png")
        {
            UIImage img = null;
            if (Foundation.NSThread.Current.IsMainThread)
                img = LoadImageFromFile(filename, extension);
            else
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    img = LoadImageFromFile(filename, extension);
                });
            }

            return img;
        }

        private static UIImage LoadImageFromFile(string filename, string extension = "png")
        {
            if (UIScreen.MainScreen.Scale > 1.0)
            {
                var file = filename + "@2x." + extension;
                return System.IO.File.Exists(file) ? UIImage.FromFile(file) : UIImage.FromFile(filename + "." + extension);
            }
            else
            {
                var file = filename + "." + extension;
                return System.IO.File.Exists(file) ? UIImage.FromFile(file) : UIImage.FromFile(filename + "@2x." + extension);
            }
        }

        public static UIImage ConvertToGrayScale(UIImage image)
        {
            var imageRect = new RectangleF(PointF.Empty, (SizeF)image.Size);
            using (var colorSpace = CGColorSpace.CreateDeviceGray())
            using (var context = new CGBitmapContext(IntPtr.Zero, (int)imageRect.Width, (int)imageRect.Height, 8, 0, colorSpace, CGImageAlphaInfo.None))
            {
                context.DrawImage(imageRect, image.CGImage);
                using (var imageRef = context.ToImage())
                    return new UIImage(imageRef);
            }
        }

        public static UIImage ChangeColor(UIImage image, UIColor color)
        {
            UIGraphics.BeginImageContextWithOptions(image.Size, false, image.CurrentScale);
            var context = UIGraphics.GetCurrentContext();
            color.SetFill();
            context.TranslateCTM(0, image.Size.Height);
            context.ScaleCTM(1, -1);
            context.ClipToMask(new CGRect(0, 0, image.Size.Width, image.Size.Height), image.CGImage);
            context.FillRect(new CGRect(0, 0, image.Size.Width, image.Size.Height));
            var coloredImage = UIGraphics.GetImageFromCurrentImageContext();
            return coloredImage;
        }

        public static UIImage MaxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            if (maxResizeFactor > 1) return sourceImage;
            var width = maxResizeFactor * sourceSize.Width;
            var height = maxResizeFactor * sourceSize.Height;
            UIGraphics.BeginImageContext(new CGSize(width, height));
            sourceImage.Draw(new CGRect(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }

        // resize the image (without trying to maintain aspect ratio)
        public static UIImage ResizeImage(UIImage sourceImage, nfloat width, nfloat height)
        {
            return ResizeImage(sourceImage, (float)width, (float)height);
        }

        public static UIImage ResizeImage(UIImage sourceImage, float width, float height)
        {
            UIGraphics.BeginImageContext(new SizeF(width, height));
            sourceImage.Draw(new RectangleF(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }

        // crop the image, without resizing
        public static UIImage CropImage(UIImage sourceImage, int crop_x, int crop_y, int width, int height)
        {
            var imgSize = sourceImage.Size;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, width, height);
            context.ClipToRect(clippedRect);
            var drawRect = new CGRect(-crop_x, -crop_y, imgSize.Width, imgSize.Height);
            sourceImage.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return modifiedImage;
        }
    }
}
