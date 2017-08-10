using System;
using System.Drawing;

using CoreGraphics;
using Foundation;
using UIKit;

namespace LibUniqBuild.iOS.Views
{
    public partial class PaddingLabel : UILabel
    {
        public PaddingLabel()
        {
            Initialize();
        }

        protected internal PaddingLabel(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public PaddingLabel(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
        }

        public override void DrawText(CGRect rect)
        {
            UIEdgeInsets insets = new UIEdgeInsets(0, 10, 0, 10);
            base.DrawText(insets.InsetRect(rect));
            //[super drawTextInRect:UIEdgeInsetsInsetRect(rect, insets)];
        }
    }
}