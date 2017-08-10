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
using Android.Graphics.Drawables;
using Android;
using Android.Graphics;

namespace LibUniqBuild.Droid.Libraries.QuickAction
{
    public class CustomPopupWindow
    {
        protected View anchor;
	    protected PopupWindow window;
	    private View root;
        private Drawable background = null;
        protected IWindowManager windowManager;
        protected int screenWidth;
        protected int screenHeight;

        public event EventHandler Dismissed;

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

        public CustomPopupWindow(View anchor)
        {
            this.anchor = anchor;
            this.window = new PopupWindow(anchor.Context);

            // when a touch even happens outside of the window
            // make the window go away
            window.SetTouchInterceptor(new TouchListener(window));
            windowManager = anchor.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            InitScreen();
            OnCreate();
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

        protected virtual void OnCreate() { }

        /**
         * In case there is stuff to do right before displaying.
         */
        protected virtual void OnShow() { }

        protected void PreShow()
        {
            if (root == null)
            {
                throw new Exception("setContentView was not called with a view to display.");
            }

            OnShow();

            if (background == null)
            {
                window.SetBackgroundDrawable(new BitmapDrawable());
            }
            else
            {
                window.SetBackgroundDrawable(background);
            }

            // if using PopupWindow#setBackgroundDrawable this is the only values of the width and hight that make it work
            // otherwise you need to set the background of the root viewgroup
            // and set the popupwindow background to an empty BitmapDrawable
            
            window.Width = WindowManagerLayoutParams.WrapContent;
            window.Height = WindowManagerLayoutParams.WrapContent;
            window.Touchable = true;
            window.Focusable = true;
            window.OutsideTouchable = true;

            window.ContentView = root;
        }

        public void SetBackgroundDrawable(Drawable background)
        {
            this.background = background;
        }

        /**
         * Sets the content view. Probably should be called from {@link onCreate}
         * 
         * @param root
         *            the view the popup will display
         */
         
        public View ContentView
        {
            set
            {
                this.root = value;
                window.ContentView = root;
            }
        }

        /**
         * Will inflate and set the view from a resource id
         * 
         * @param layoutResID
         */
        public void SetContentView(int layoutResID)
        {
            LayoutInflater inflator =
                    (LayoutInflater)anchor.Context.GetSystemService(Context.LayoutInflaterService);

            ContentView = inflator.Inflate(layoutResID, null);
        }

        /**
         * Displays like a popdown menu from the anchor view
         */
        public void ShowDropDown()
        {
            ShowDropDown(0, 0);
        }

        /**
         * Displays like a popdown menu from the anchor view.
         * 
         * @param xOffset
         *            offset in X direction
         * @param yOffset
         *            offset in Y direction
         */
        public void ShowDropDown(int xOffset, int yOffset)
        {
            PreShow();

            window.AnimationStyle = Resource.Style.Animations_PopDownMenu;

            window.ShowAsDropDown(anchor, xOffset, yOffset);
        }

        /**
         * Displays like a QuickAction from the anchor view.
         */
        public void ShowLikeQuickAction()
        {
            ShowLikeQuickAction(0, 0);
        }

        /**
         * Displays like a QuickAction from the anchor view.
         * 
         * @param xOffset
         *            offset in the X direction
         * @param yOffset
         *            offset in the Y direction
         */
        public void ShowLikeQuickAction(int xOffset, int yOffset)
        {
            PreShow();

            window.AnimationStyle = Resource.Style.Animations_PopUpMenu_Center;

            int[] location = new int[2];
            anchor.GetLocationOnScreen(location);

            Rect anchorRect =
                    new Rect(location[0], location[1], location[0] + anchor.Width, location[1]
                        + anchor.Height);

            root.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            root.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            int rootWidth = root.MeasuredWidth;
            int rootHeight = root.MeasuredHeight;

            int screenWidth = windowManager.DefaultDisplay.Width;
            //int screenHeight 	= windowManager.getDefaultDisplay().getHeight();

            int xPos = ((screenWidth - rootWidth) / 2) + xOffset;
            int yPos = anchorRect.Top - rootHeight + yOffset;

            // display on bottom
            if (rootHeight > anchorRect.Top)
            {
                yPos = anchorRect.Bottom + yOffset;

                window.AnimationStyle = Resource.Style.Animations_PopDownMenu_Center;
            }

            window.ShowAtLocation(anchor, GravityFlags.NoGravity, xPos, yPos);
        }

        public void Dismiss()
        {
            window.Dismiss();
        }
    }
}