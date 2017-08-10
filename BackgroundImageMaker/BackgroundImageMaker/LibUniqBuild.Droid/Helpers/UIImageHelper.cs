using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Graphics;
using Android.Views;
using Android.Content;
using Android.Renderscripts;
using Android.Graphics.Drawables;

namespace LibUniqBuild.Droid.Helpers
{
    public class UIImageHelper
    {
        public static Bitmap AddShadow(Bitmap bm, int dstHeight, int dstWidth)
        {
            var color = Color.Black;
            int size = 7;
            int dx = 2;
            int dy = 5;
            
            var mask = Bitmap.CreateBitmap(dstWidth, dstHeight, Bitmap.Config.Alpha8);

            Matrix scaleToFit = new Matrix();
            RectF src = new RectF(0, 0, bm.Width, bm.Height);
            RectF dst = new RectF(0, 0, dstWidth - dx, dstHeight - dy);
            scaleToFit.SetRectToRect(src, dst, Matrix.ScaleToFit.Center);

            Matrix dropShadow = new Matrix(scaleToFit);
            dropShadow.PostTranslate(dx, dy);

            Canvas maskCanvas = new Canvas(mask);
            Paint paint = new Paint(PaintFlags.AntiAlias);
            maskCanvas.DrawBitmap(bm, scaleToFit, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcOut));
            maskCanvas.DrawBitmap(bm, dropShadow, paint);

            var filter = new BlurMaskFilter(size, BlurMaskFilter.Blur.Normal);
            paint.Reset();
            paint.AntiAlias = true;
            paint.Color = color;
            paint.SetMaskFilter(filter);
            paint.FilterBitmap = true;

            var ret = Bitmap.CreateBitmap(dstWidth, dstHeight, Bitmap.Config.Argb8888);
            var retCanvas = new Canvas(ret);
            retCanvas.DrawBitmap(mask, 0, 0, paint);
            retCanvas.DrawBitmap(bm, scaleToFit, null);
            mask.Recycle();
            return ret;
        }

        public static Bitmap AddShadowInBitmap(Bitmap bitmap)
        {
            return AddShadow(bitmap, bitmap.Width, bitmap.Height);
        }

        public static int GetDrawableResourceId(string fileName)
        {
            return (int)typeof(Resource.Drawable).GetField(fileName).GetValue(null);
        }

        public static Bitmap Blur(View v)
        {
            if (((int)Android.OS.Build.VERSION.SdkInt) >= 17)
            {
                return BlurBuilder.Blur(v);
            }
            else
            {
                var image = BlurBuilder.GetScreenshot(v);
                int width = (int)Math.Round(image.Width * BlurBuilder.BITMAP_SCALE);
                int height = (int)Math.Round(image.Height * BlurBuilder.BITMAP_SCALE);
                return Bitmap.CreateScaledBitmap(image, width, height, false);
            }
        }
    }

    public class BlurBuilder
    {
        public const float BITMAP_SCALE = 0.4f;
        public const float BLUR_RADIUS = 7.5f;

        public static Bitmap Blur(View v)
        {
            return Blur(v.Context, GetScreenshot(v));
        }

        public static Bitmap Blur(Context ctx, Bitmap image)
        {
            int width = (int)Math.Round(image.Width * BITMAP_SCALE);
            int height = (int)Math.Round(image.Height * BITMAP_SCALE);

            Bitmap inputBitmap = Bitmap.CreateScaledBitmap(image, width, height, false);
            Bitmap outputBitmap = Bitmap.CreateBitmap(inputBitmap);

            var rs = RenderScript.Create(ctx);
            ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            Allocation tmpIn = Allocation.CreateFromBitmap(rs, inputBitmap);
            Allocation tmpOut = Allocation.CreateFromBitmap(rs, outputBitmap);
            theIntrinsic.SetRadius(BLUR_RADIUS);
            theIntrinsic.SetInput(tmpIn);
            theIntrinsic.ForEach(tmpOut);
            tmpOut.CopyTo(outputBitmap);

            return outputBitmap;
        }

        public static Bitmap GetScreenshot(View v)
        {
            Bitmap b = Bitmap.CreateBitmap(v.Width, v.Height, Bitmap.Config.Argb8888);
            Canvas c = new Canvas(b);
            v.Draw(c);
            return b;
        }
    }
}