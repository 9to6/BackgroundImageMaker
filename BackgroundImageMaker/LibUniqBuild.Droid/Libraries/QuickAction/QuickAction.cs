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
    public class QuickAction : CustomPopupWindow
    {
        private View root;
	    private ImageView mArrowUp;
	    private ImageView mArrowDown;
	    private LayoutInflater inflater;
	    private Context context;
	
	    protected const int ANIM_GROW_FROM_LEFT = 1;
        protected const int ANIM_GROW_FROM_RIGHT = 2;
        protected const int ANIM_GROW_FROM_CENTER = 3;
        protected const int ANIM_REFLECT = 4;
        protected const int ANIM_AUTO = 5;

        private ViewGroup mTrack;
        private ScrollView scroller;
        private List<ActionItem> actionList = new List<ActionItem>();

        private int animStyle;
        public int AnimStyle
        {
            /**
             * Set animation style
             * 
             * @param animStyle animation style, default is set to ANIM_AUTO
             */
            set
            {
                animStyle = value;
            }
        }

        public class ItemClickEventArgs : EventArgs
        {
            public int Position { get; set; }
        }
        public event EventHandler<ItemClickEventArgs> ItemClick;

        /**
         * Constructor
         * 
         * @param anchor {@link View} on where the popup window should be displayed
         */
        public QuickAction(View anchor) : base(anchor)
        {
            context = anchor.Context;
            inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

            root = (ViewGroup)inflater.Inflate(Resource.Layout.popup, null);

            mArrowDown = root.FindViewById<ImageView>(Resource.Id.arrow_down);
            mArrowUp = root.FindViewById<ImageView>(Resource.Id.arrow_up);

            ContentView = root;

            mTrack = root.FindViewById<ViewGroup>(Resource.Id.tracks);
            scroller = root.FindViewById<ScrollView>(Resource.Id.scroller);
            animStyle = ANIM_AUTO;
        }

        /**
         * Add action item
         * 
         * @param action  {@link ActionItem} object
         */
        public void AddActionItem(ActionItem action)
        {
            actionList.Add(action);
        }

        /**
         * Show popup window. Popup is automatically positioned, on top or bottom of anchor view.
         * 
         */
        public void Show()
        {
            PreShow();

            int xPos, yPos;

            int[] location = new int[2];

            anchor.GetLocationOnScreen(location);

            Rect anchorRect = new Rect(location[0], location[1], location[0] + anchor.Width, location[1]
                                + anchor.Height);

            CreateActionList();

            root.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            root.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            int rootHeight = root.MeasuredHeight;
            int rootWidth = root.MeasuredWidth;

            //automatically get X coord of popup (top left)
            if ((anchorRect.Left + rootWidth) > screenWidth)
            {
                xPos = anchorRect.Left - (rootWidth - anchor.Width);
            }
            else
            {
                if (anchor.Width > rootWidth)
                {
                    xPos = anchorRect.CenterX() - (rootWidth / 2);
                }
                else
                {
                    xPos = anchorRect.Left;
                }
            }

            int dyTop = anchorRect.Top;
            int dyBottom = screenHeight - anchorRect.Bottom;

            bool onTop = (dyTop > dyBottom) ? true : false;

            if (onTop)
            {
                if (rootHeight > dyTop)
                {
                    yPos = 15;
                    var l = scroller.LayoutParameters;
                    l.Height = dyTop - anchor.Height;
                }
                else
                {
                    yPos = anchorRect.Top - rootHeight;
                }
            }
            else
            {
                yPos = anchorRect.Bottom;

                if (rootHeight > dyBottom)
                {
                    var l = scroller.LayoutParameters;
                    l.Height = dyBottom;
                }
            }

            ShowArrow(((onTop) ? Resource.Id.arrow_down : Resource.Id.arrow_up), xPos);

            SetAnimationStyle(screenWidth, anchorRect.CenterX(), onTop);

            window.ShowAtLocation(anchor, GravityFlags.NoGravity, xPos, yPos);
        }

        public void Show(float eventX, float eventY)
        {
            PreShow();

            int xPos, yPos;

            int[] location = new int[2];

            anchor.GetLocationOnScreen(location);

            Rect anchorRect = new Rect(location[0], location[1], location[0] + anchor.Width, location[1]
                                + anchor.Height);

            CreateActionList();

            root.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            root.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            int rootHeight = root.MeasuredHeight;
            int rootWidth = root.MeasuredWidth;

            //automatically get X coord of popup (top left)
            if ((anchorRect.Left + rootWidth) > screenWidth)
            {
                xPos = anchorRect.Left - (rootWidth - anchor.Width);
            }
            else
            {
                if (anchor.Width > rootWidth)
                {
                    xPos = anchorRect.CenterX() - (rootWidth / 2);
                }
                else
                {
                    xPos = anchorRect.Left;
                }
            }

            int dyTop = anchorRect.Top;
            int dyBottom = screenHeight - anchorRect.Bottom;

            bool onTop = (dyTop > dyBottom) ? true : false;

            if (onTop)
            {
                if (rootHeight > dyTop)
                {
                    yPos = 15;
                    var l = scroller.LayoutParameters;
                    l.Height = dyTop - anchor.Height;
                }
                else
                {
                    yPos = anchorRect.Top - rootHeight;
                }
            }
            else
            {
                yPos = anchorRect.Bottom;

                if (rootHeight > dyBottom)
                {
                    var l = scroller.LayoutParameters;
                    l.Height = dyBottom;
                }
            }

            System.Diagnostics.Debug.WriteLine(string.Format("xpos:{0}, ypos:{1}, x:{2}, y:{3}, centerx:{4}", xPos, yPos, eventX, eventY, anchorRect.CenterX()));
            if (eventX != 0) xPos = (int)eventX;
            if (eventY != 0) yPos = (int)eventY;
            if (onTop)
            {
                yPos -= rootHeight;
            }
            
            ShowArrow(((onTop) ? Resource.Id.arrow_down : Resource.Id.arrow_up), anchorRect.CenterX() - xPos);

            SetAnimationStyle(screenWidth, anchorRect.CenterX(), onTop);

            window.ShowAtLocation(anchor, GravityFlags.NoGravity, xPos, yPos);
        }

        /**
         * Set animation style
         * 
         * @param screenWidth screen width
         * @param requestedX distance from left edge
         * @param onTop flag to indicate where the popup should be displayed. Set TRUE if displayed on top of anchor view
         * 		  and vice versa
         */
        private void SetAnimationStyle(int screenWidth, int requestedX, bool onTop)
        {
            int arrowPos = requestedX - mArrowUp.MeasuredWidth / 2;

            switch (animStyle)
            {
                case ANIM_GROW_FROM_LEFT:
                    window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Left : Resource.Style.Animations_PopDownMenu_Left;
                    break;

                case ANIM_GROW_FROM_RIGHT:
                    window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Right : Resource.Style.Animations_PopDownMenu_Right;
                    break;

                case ANIM_GROW_FROM_CENTER:
                    window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Center : Resource.Style.Animations_PopDownMenu_Center;
                    break;

                case ANIM_REFLECT:
                    window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Reflect : Resource.Style.Animations_PopDownMenu_Reflect;
                    break;

                case ANIM_AUTO:
                    if (arrowPos <= screenWidth / 4)
                    {
                        window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Left : Resource.Style.Animations_PopDownMenu_Left;
                    }
                    else if (arrowPos > screenWidth / 4 && arrowPos < 3 * (screenWidth / 4))
                    {
                        window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Center : Resource.Style.Animations_PopDownMenu_Center;
                    }
                    else
                    {
                        window.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Right : Resource.Style.Animations_PopDownMenu_Right;
                    }

                    break;
            }
        }

        /**
         * Create action list
         */
        private void CreateActionList()
        {
            mTrack.RemoveAllViews();
            View view;
            string title;
            Drawable icon;

            for (int i = 0; i < actionList.Count; i++)
            {
                var item = actionList[i];
                title = item.Title;
                icon = item.Icon;

                view = GetActionItem(title, icon);
                view.Tag = i;

                view.Focusable = true;
                view.Clickable = true;

                mTrack.AddView(view);
            }
        }

        /**
         * Get action item {@link View}
         * 
         * @param title action item title
         * @param icon {@link Drawable} action item icon
         * @param listener {@link View.OnClickListener} action item listener
         * @return action item {@link View}
         */
        private View GetActionItem(string title, Drawable icon)
        {
            LinearLayout container = (LinearLayout)inflater.Inflate(Resource.Layout.action_item, null);

            ImageView img = container.FindViewById<ImageView>(Resource.Id.icon);
            TextView text = container.FindViewById<TextView>(Resource.Id.title);

            if (icon != null)
            {
                img.SetImageDrawable(icon);
            }

            if (title != null)
            {
                text.Text = title;
            }

            container.Click += OnItemClick;

            return container;
        }

        /**
         * Show arrow
         * 
         * @param whichArrow arrow type resource id
         * @param requestedX distance from left screen
         */
        private void ShowArrow(int whichArrow, int requestedX)
        {
            View showArrow = (whichArrow == Resource.Id.arrow_up) ? mArrowUp : mArrowDown;
            View hideArrow = (whichArrow == Resource.Id.arrow_up) ? mArrowDown : mArrowUp;

            int arrowWidth = mArrowUp.MeasuredWidth;

            showArrow.Visibility = ViewStates.Visible;

            ViewGroup.MarginLayoutParams param = (ViewGroup.MarginLayoutParams)showArrow.LayoutParameters;

            param.LeftMargin = Math.Abs(requestedX) - arrowWidth / 2;

            hideArrow.Visibility = ViewStates.Invisible;
        }

        private void OnItemClick(object sender, EventArgs eventArgs)
        {
            var view = sender as View;
            if (view != null)
            {
                var index = (int)view.Tag;
                ItemClick?.Invoke(this, new ItemClickEventArgs { Position = index });
            }
        }
    }
}