using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace LibUniqBuild.Droid.Libraries.QuickAction
{
    public class SimpleQuickAction
    {
        private const string PARAM_STATUS_BAR_HEIGHT = "status_bar_height";
        private const string PARAM_DIMEN = "dimen";
        private const String PARAM_ANDROID = "android";

        private const int X_INDEX = 0;
        private const int Y_INDEX = 1;

        private Context context;
        private int screenWidth;
        private int screenHeight;

        private PopupWindow popupWindow;
        private IWindowManager windowManager;
        private RelativeLayout topRootLayout;
        private RelativeLayout bottomRootLayout;

        public SimpleQuickAction(Context context, int animationStyle, RelativeLayout topRootLayout, RelativeLayout bottomRootLayout)
        {
            windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            this.context = context;
            this.topRootLayout = topRootLayout;
            this.bottomRootLayout = bottomRootLayout;

            InitScreen();
            InitPopupWindow(animationStyle);
        }
        
        private void InitScreen()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.HoneycombMr2)
            {
                Display display = windowManager.DefaultDisplay;
                Point size = new Point();
                display.GetSize(size);
                screenWidth = size.X;
                screenHeight = size.Y;
            }
            else
            {
                screenWidth = windowManager.DefaultDisplay.Width;
                screenHeight = windowManager.DefaultDisplay.Height;
            }
        }

        private class TouchListener : Java.Lang.Object, View.IOnTouchListener
        {
            PopupWindow window;
            public TouchListener(PopupWindow window)
            {
                this.window = window;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                if (e.Action == MotionEventActions.Outside)
                {
                    window.Dismiss();
                    return true;
                }
                return false;
            }
        }

        private void InitPopupWindow(int animationStyle)
        {
            popupWindow = new PopupWindow(context);
            popupWindow.Width = WindowManagerLayoutParams.WrapContent;
            popupWindow.Height = WindowManagerLayoutParams.WrapContent;
            popupWindow.Touchable = true;
            popupWindow.Focusable = true;
            popupWindow.OutsideTouchable = true;
            popupWindow.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            popupWindow.AnimationStyle = animationStyle;
            popupWindow.SetTouchInterceptor(new TouchListener(popupWindow));
        }

        public void SetMaxHeightResource(int heightResource)
        {
            int maxHeight = context.Resources.GetDimensionPixelSize(heightResource);
            popupWindow.Height = maxHeight;
        }
        
        public void Dismiss()
        {
            popupWindow.Dismiss();
        }

        public void Show(View anchor, float eventX, float eventY)
        {
            try
            {
                int[] location = new int[2];
                anchor.GetLocationOnScreen(location);

                Rect anchorRect = new Rect(location[X_INDEX], location[Y_INDEX],
                        location[X_INDEX] + anchor.Width, location[Y_INDEX] + anchor.Height);

                RelativeLayout rootLayout;
                bool onTop = false;
                if (location[Y_INDEX] > screenHeight / 2)
                    rootLayout = bottomRootLayout;
                else
                {
                    rootLayout = topRootLayout;
                    onTop = true;
                }

                if (rootLayout.LayoutParameters == null) rootLayout.LayoutParameters = 
                        new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                rootLayout.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);

                int rootHeight = rootLayout.MeasuredHeight;
                int rootWidth = rootLayout.MeasuredWidth;

                int x = CalculateHorizontalPosition(anchor, anchorRect, rootWidth, screenWidth);
                int y = CalculateVerticalPosition(anchorRect, rootHeight, onTop);
                if (eventX != 0) x = (int)eventX;
                if (onTop)
                {
                    y -= rootHeight;
                }

                popupWindow.ContentView = rootLayout;
                popupWindow.Dismiss();
                popupWindow.ShowAtLocation(anchor, GravityFlags.NoGravity, x, y);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e);
            }
        }

        public void Show(View anchor)
        {
            Show(anchor, 0, 0);
        }

        private int CalculateHorizontalPosition(View anchor, Rect anchorRect, int rootWidth, int screenWidth)
        {
            int x;

            if ((anchorRect.Left + rootWidth) > screenWidth)
            {
                x = anchorRect.Left - (rootWidth - anchor.Width);
                if (x < 0) x = 0;
            }
            else
            {
                if (anchor.Width > rootWidth) x = anchorRect.CenterX() - (rootWidth / 2);
                else x = anchorRect.Left;
            }

            return x;
        }

        private int CalculateVerticalPosition(Rect anchorRect, int rootHeight, bool onTop)
        {
            int y;

            if (onTop) y = anchorRect.Top;
            else y = anchorRect.Bottom - rootHeight;

            return y;
        }

        private int GetStatusBarHeight()
        {
            int result = 0;
            int resourceId = context.Resources.GetIdentifier(PARAM_STATUS_BAR_HEIGHT, PARAM_DIMEN, PARAM_ANDROID);
            if (resourceId > 0) result = context.Resources.GetDimensionPixelSize(resourceId);

            return result;
        }
    }
}