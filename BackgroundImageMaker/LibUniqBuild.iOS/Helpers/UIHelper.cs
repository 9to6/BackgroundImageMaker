using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace LibUniqBuild.iOS.Helpers
{
    public class UIHelper
    {
        public static Task<bool> ShowConfirm(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            UIApplication.SharedApplication.InvokeOnMainThread(
                (() => {
                    UIAlertView alert = new UIAlertView(
                        title,
                        message,
                        null,
                        "Cancel",
                        "OK");
                    alert.Clicked += (sender, buttonArgs) => tcs.SetResult(buttonArgs.ButtonIndex != alert.CancelButtonIndex);
                    alert.Show();
                }));

            return tcs.Task;
        }

        public static void SetShadow(UIView view)
        {
            view.Layer.CornerRadius = 5;
            view.Layer.ShadowColor = UIColor.Black.CGColor;
            view.Layer.ShadowOpacity = 1.0f;
            view.Layer.ShadowRadius = 5.0f;
            view.Layer.ShadowOffset = new System.Drawing.SizeF(5f, 5f);
        }

        public static CGRect HeightWithoutStatus
        {
            get
            {
                var rect = UIScreen.MainScreen.Bounds;
                var statusHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
                rect.Y = rect.Y + statusHeight;
                rect.Height = rect.Height - statusHeight;
                return rect;
            }
        }
    }
}