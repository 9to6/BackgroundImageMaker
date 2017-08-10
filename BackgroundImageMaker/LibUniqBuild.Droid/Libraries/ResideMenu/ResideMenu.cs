using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Views.Animations;
using Android.Graphics;

namespace LibUniqBuild.Droid.Libraries.ResideMenu
{
    public class ResideMenu : FrameLayout
    {
        public enum Direction
        {
            Left = 0,
            Right = 1,
        }

        public enum PressState
        {
            MoveHorizontal = 0,
            Down,
            Done,
            MoveVertical,
        }

        private ImageView imageViewShadow;
        private ImageView imageViewBackground;

        private LinearLayout layoutLeftMenu;
        private LinearLayout layoutRightMenu;

        private View scrollViewLeftMenu;
        public View ScrollViewLeftMenu
        {
            get
            {
                return scrollViewLeftMenu;
            }
        }
        private View scrollViewRightMenu;
        public View ScrollViewRightMenu
        {
            get
            {
                return scrollViewRightMenu;
            }
        }
        private View scrollViewMenu;

        private List<ResideMenuItem> leftMenuItems;
        public List<ResideMenuItem> LeftMenuItems
        {
            get
            {
                return leftMenuItems;
            }
            set
            {
                leftMenuItems = value;
                RebuildMenu();
            }
        }
        private List<ResideMenuItem> rightMenuItems;
        public List<ResideMenuItem> RightMenuItems
        {
            get
            {
                return rightMenuItems;
            }
            set
            {
                rightMenuItems = value;
                RebuildMenu();
            }
        }

        /**
         * Current attaching activity.
         */
        private Activity activity;
        /**
         * The DecorView of current activity.
         */
        private ViewGroup viewDecor;
        private TouchDisableView viewActivity;

        public bool IsOpened { get; set; }
        private float shadowAdjustScaleX;
        private float shadowAdjustScaleY;

        private List<View> ignoredViews;

        private DisplayMetrics displayMetrics = new DisplayMetrics();

        private float lastRawX;
        private bool isInIgnoredView = false;
        private Direction scaleDirection = Direction.Left;
        private PressState pressedState = PressState.Done;
        private List<Direction> disabledSwipeDirection = new List<Direction>();
        public float ScaleValueX { get; set; }
        public float ScaleValueY { get; set; }

        public bool Use3D { get; set; }
        private const int ROTATE_Y_ANGLE = 10;

        public event EventHandler Opened;
        public event EventHandler Closed;

        public ResideMenu(Context context) :
            base(context)
        {
            Initialize();
        }

        public ResideMenu(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public ResideMenu(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            Use3D = false;
            ScaleValueX = 0.7f;
            ScaleValueY = 0.95f;
            viewActivityOnClickListener = new OnClickListener(this);
            animationListener = new AnimatorListener(this);
            InitViews(Context, -1, -1);
        }

        //private void InitScreen()
        //{
        //    if (Build.VERSION.SdkInt >= Build.VERSION_CODES.HoneycombMr2)
        //    {
        //        Display display = windowManager.DefaultDisplay;
        //        Point size = new Point();
        //        display.GetSize(size);
        //        screenWidth = size.X;
        //        screenHeight = size.Y;
        //    }
        //    else
        //    {
        //        screenWidth = windowManager.DefaultDisplay.Width;
        //        screenHeight = windowManager.DefaultDisplay.Height;
        //    }
        //}

        private void InitViews(Context context, int customLeftMenuId,
                               int customRightMenuId)
        {
            LayoutInflater Inflater = (LayoutInflater)context
                    .GetSystemService(Context.LayoutInflaterService);
            Inflater.Inflate(Resource.Layout.residemenu_custom, this);

            if (customLeftMenuId >= 0)
            {
                scrollViewLeftMenu = Inflater.Inflate(customLeftMenuId, this, false);
            }
            else
            {
                scrollViewLeftMenu = Inflater.Inflate(
                        Resource.Layout.residemenu_custom_left_scrollview, this, false);
                layoutLeftMenu = (LinearLayout)scrollViewLeftMenu.FindViewById(Resource.Id.layout_left_menu);
            }

            if (customRightMenuId >= 0)
            {
                scrollViewRightMenu = Inflater.Inflate(customRightMenuId, this, false);
            }
            else
            {
                scrollViewRightMenu = Inflater.Inflate(
                        Resource.Layout.residemenu_custom_right_scrollview, this, false);
                layoutRightMenu = (LinearLayout)scrollViewRightMenu.FindViewById(Resource.Id.layout_right_menu);
            }

            imageViewShadow = (ImageView)FindViewById(Resource.Id.iv_shadow);
            imageViewBackground = (ImageView)FindViewById(Resource.Id.iv_background);

            RelativeLayout menuHolder = (RelativeLayout)FindViewById(Resource.Id.sv_menu_holder);
            menuHolder.AddView(scrollViewLeftMenu);
            menuHolder.AddView(scrollViewRightMenu);
        }

        private void InitValue(Activity activity)
        {
            this.activity = activity;
            leftMenuItems = new List<ResideMenuItem>();
            rightMenuItems = new List<ResideMenuItem>();
            ignoredViews = new List<View>();
            viewDecor = (ViewGroup)activity.Window.DecorView;
            viewActivity = new TouchDisableView(this.activity);

            View mContent = viewDecor.GetChildAt(0);
            viewDecor.RemoveViewAt(0);
            viewActivity.Content = mContent;
            AddView(viewActivity);

            ViewGroup parent = (ViewGroup)scrollViewLeftMenu.Parent;
            parent.RemoveView(scrollViewLeftMenu);
            parent.RemoveView(scrollViewRightMenu);
        }

        //protected override bool FitSystemWindows(Rect insets)
        //{
        //    int bottomPadding = viewActivity.PaddingBottom + insets.Bottom;
        //    bool hasBackKey = KeyCharacterMap.DeviceHasKey(Keycode.Back);
        //    bool hasHomeKey = KeyCharacterMap.DeviceHasKey(Keycode.Home);
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop || !hasBackKey || !hasHomeKey)
        //    {//there's a navigation bar
        //        bottomPadding += GetNavigationBarHeight();
        //    }

        //    this.SetPadding(viewActivity.PaddingLeft + insets.Left,
        //            viewActivity.PaddingTop + insets.Top,
        //            viewActivity.PaddingRight + insets.Right,
        //            bottomPadding);
        //    insets.Left = insets.Top = insets.Right = insets.Bottom = 0;
        //    return true;
        //}

        private int GetNavigationBarHeight()
        {
            int resourceId = Resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                return Resources.GetDimensionPixelSize(resourceId);
            }
            return 0;
        }

        private void SetShadowAdjustScaleXByOrientation()
        {
            var orientation = Resources.Configuration.Orientation;
            if (orientation == Android.Content.Res.Orientation.Landscape)
            {
                shadowAdjustScaleX = 0.034f;
                shadowAdjustScaleY = 0.12f;
            }
            else if (orientation == Android.Content.Res.Orientation.Portrait)
            {
                shadowAdjustScaleX = 0.06f;
                shadowAdjustScaleY = 0.07f;
            }
        }

        public void AttachToActivity(Activity activity)
        {
            InitValue(activity);
            SetShadowAdjustScaleXByOrientation();
            viewDecor.AddView(this, 0);
        }

        public void SetBackground(int imageResource)
        {
            imageViewBackground.SetImageResource(imageResource);
        }

        public void AddLeftMenuItem(ResideMenuItem menuItem)
        {
            leftMenuItems.Add(menuItem);
            layoutLeftMenu.AddView(menuItem);
        }

        public void AddRightMenuItem(ResideMenuItem menuItem)
        {
            rightMenuItems.Add(menuItem);
            layoutRightMenu.AddView(menuItem);
        }

        private void RebuildMenu()
        {
            if (layoutLeftMenu != null)
            {
                layoutLeftMenu.RemoveAllViews();
                foreach (var leftMenuItem in leftMenuItems)
                    layoutLeftMenu.AddView(leftMenuItem);
            }

            if (layoutRightMenu != null)
            {
                layoutRightMenu.RemoveAllViews();
                foreach (var rightMenuItem in rightMenuItems)
                    layoutRightMenu.AddView(rightMenuItem);
            }
        }

        /**
         * Show the menu;
         */
        public void OpenMenu(Direction direction)
        {
            SetScaleDirection(direction);

            IsOpened = true;
            AnimatorSet scaleDown_activity = BuildScaleDownAnimation(viewActivity, ScaleValueX, ScaleValueY);
            AnimatorSet scaleDown_shadow = BuildScaleDownAnimation(imageViewShadow,
                    ScaleValueX + shadowAdjustScaleX, ScaleValueY + shadowAdjustScaleY);
            AnimatorSet alpha_menu = BuildMenuAnimation(scrollViewMenu, 1.0f);
            scaleDown_shadow.AddListener(animationListener);
            scaleDown_activity.PlayTogether(scaleDown_shadow);
            scaleDown_activity.PlayTogether(alpha_menu);
            scaleDown_activity.Start();
        }

        /**
         * Close the menu;
         */
        public void CloseMenu()
        {
            IsOpened = false;
            AnimatorSet scaleUp_activity = BuildScaleUpAnimation(viewActivity, 1.0f, 1.0f);
            AnimatorSet scaleUp_shadow = BuildScaleUpAnimation(imageViewShadow, 1.0f, 1.0f);
            AnimatorSet alpha_menu = BuildMenuAnimation(scrollViewMenu, 0.0f);
            scaleUp_activity.AddListener(animationListener);
            scaleUp_activity.PlayTogether(scaleUp_shadow);
            scaleUp_activity.PlayTogether(alpha_menu);
            scaleUp_activity.Start();
        }

        public void SetSwipeDirectionDisable(Direction direction)
        {
            disabledSwipeDirection.Add(direction);
        }

        public int GetScreenHeight()
        {
            var windowManager = activity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.DefaultDisplay.GetMetrics(displayMetrics);
            return displayMetrics.HeightPixels;
        }

        public int GetScreenWidth()
        {
            var windowManager = activity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.DefaultDisplay.GetMetrics(displayMetrics);
            return displayMetrics.WidthPixels;
        }

        private void SetScaleDirection(Direction direction)
        {

            int screenWidth = GetScreenWidth();
            float pivotX;
            float pivotY = GetScreenHeight() * 0.5f;

            if (direction == Direction.Left)
            {
                scrollViewMenu = scrollViewLeftMenu;
                pivotX = screenWidth * 1.5f;
            }
            else
            {
                scrollViewMenu = scrollViewRightMenu;
                pivotX = screenWidth * -0.5f;
            }

            viewActivity.PivotX = pivotX;
            viewActivity.PivotY = pivotY;
            imageViewShadow.PivotX = pivotX;
            imageViewShadow.PivotY = pivotY;
            scaleDirection = direction;
        }

        /**
         * A helper method to build scale down animation;
         *
         * @param target
         * @param targetScaleX
         * @param targetScaleY
         * @return
         */
        private AnimatorSet BuildScaleDownAnimation(View target, float targetScaleX, float targetScaleY)
        {

            AnimatorSet scaleDown = new AnimatorSet();
            scaleDown.PlayTogether(
                    ObjectAnimator.OfFloat(target, "scaleX", targetScaleX),
                    ObjectAnimator.OfFloat(target, "scaleY", targetScaleY)
            );

            if (Use3D)
            {
                int angle = scaleDirection == Direction.Left ? -ROTATE_Y_ANGLE : ROTATE_Y_ANGLE;
                scaleDown.PlayTogether(ObjectAnimator.OfFloat(target, "rotationY", angle));
            }

            scaleDown.SetInterpolator(AnimationUtils.LoadInterpolator(activity,
                    Android.Resource.Animation.DecelerateInterpolator));
            scaleDown.SetDuration(250);
            return scaleDown;
        }

        /**
         * A helper method to build scale up animation;
         *
         * @param target
         * @param targetScaleX
         * @param targetScaleY
         * @return
         */
        private AnimatorSet BuildScaleUpAnimation(View target, float targetScaleX, float targetScaleY)
        {

            AnimatorSet scaleUp = new AnimatorSet();
            scaleUp.PlayTogether(
                    ObjectAnimator.OfFloat(target, "scaleX", targetScaleX),
                    ObjectAnimator.OfFloat(target, "scaleY", targetScaleY)
            );

            if (Use3D)
            {
                scaleUp.PlayTogether(ObjectAnimator.OfFloat(target, "rotationY", 0));
            }

            scaleUp.SetDuration(250);
            return scaleUp;
        }

        private AnimatorSet BuildMenuAnimation(View target, float alpha)
        {
            var alphaAnimation = new AnimatorSet();
            if (target != null)
            {
                alphaAnimation.PlayTogether(
                    ObjectAnimator.OfFloat(target, "alpha", alpha)
                );
                alphaAnimation.SetDuration(250);
            }
            return alphaAnimation;
        }


        private void ShowScrollViewMenu(View scrollViewMenu)
        {
            if (scrollViewMenu != null && scrollViewMenu.Parent == null)
            {
                AddView(scrollViewMenu);
            }
        }

        private void HideScrollViewMenu(View scrollViewMenu)
        {
            if (scrollViewMenu != null && scrollViewMenu.Parent != null)
            {
                RemoveView(scrollViewMenu);
            }
        }

        class OnClickListener : Java.Lang.Object, IOnClickListener
        {
            ResideMenu view;
            public OnClickListener(ResideMenu view)
            {
                this.view = view;
            }

            public void OnClick(View v)
            {
                if (view.IsOpened) view.CloseMenu();
            }
        }

        class AnimatorListener : Java.Lang.Object, Animator.IAnimatorListener
        {
            ResideMenu view;
            public AnimatorListener(ResideMenu view)
            {
                this.view = view;
            }

            public void OnAnimationCancel(Animator animation)
            {
                
            }

            public void OnAnimationEnd(Animator animation)
            {
                // reset the view;
                view.viewActivity.TouchDisabled = view.IsOpened;
                if (view.IsOpened)
                {
                    view.viewActivity.SetOnClickListener(view.viewActivityOnClickListener);
                }
                else
                {
                    view.viewActivity.SetOnClickListener(null);
                    view.HideScrollViewMenu(view.scrollViewLeftMenu);
                    view.HideScrollViewMenu(view.scrollViewRightMenu);
                    view.OnClosed();
                }
            }

            public void OnAnimationRepeat(Animator animation)
            {
                
            }

            public void OnAnimationStart(Animator animation)
            {
                if (view.IsOpened)
                {
                    view.ShowScrollViewMenu(view.scrollViewMenu);
                    view.OnOpened();
                }
            }
        }


        private OnClickListener viewActivityOnClickListener;

        private AnimatorListener animationListener;

        private void OnOpened()
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }


        /**
         * If the motion event was relative to the view
         * which in ignored view list,return true;
         *
         * @param ev
         * @return
         */
        private bool IsInIgnoredView(MotionEvent ev)
        {
            Rect rect = new Rect();
            foreach (var v in ignoredViews)
            {
                v.GetGlobalVisibleRect(rect);
                if (rect.Contains((int)ev.GetX(), (int)ev.GetY()))
                    return true;
            }
            return false;
        }

        private bool IsInDisableDirection(Direction direction)
        {
            return disabledSwipeDirection.Contains(direction);
        }

        private void SetScaleDirectionByRawX(float currentRawX)
        {
            if (currentRawX < lastRawX)
                SetScaleDirection(Direction.Right);
            else
                SetScaleDirection(Direction.Left);
        }

        private float GetTargetScale(float currentRawX)
        {
            float scaleFloatX = ((currentRawX - lastRawX) / GetScreenWidth()) * 0.75f;
            scaleFloatX = scaleDirection == Direction.Right ? -scaleFloatX : scaleFloatX;

            float targetScale = viewActivity.ScaleX - scaleFloatX;
            targetScale = targetScale > 1.0f ? 1.0f : targetScale;
            targetScale = targetScale < 0.5f ? 0.5f : targetScale;
            return targetScale;
        }

        private float lastActionDownX, lastActionDownY;

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            float currentActivityScaleX = viewActivity.ScaleX;
            if (currentActivityScaleX == 1.0f)
                SetScaleDirectionByRawX(ev.RawX);

            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    lastActionDownX = ev.GetX();
                    lastActionDownY = ev.GetY();
                    isInIgnoredView = IsInIgnoredView(ev) && !IsOpened;
                    pressedState = PressState.Down;
                    break;

                case MotionEventActions.Move:
                    if (isInIgnoredView || IsInDisableDirection(scaleDirection))
                        break;

                    if (pressedState != PressState.Down &&
                            pressedState != PressState.MoveHorizontal)
                        break;

                    int xOffset = (int)(ev.GetX() - lastActionDownX);
                    int yOffset = (int)(ev.GetY() - lastActionDownY);

                    if (pressedState == PressState.Down)
                    {
                        if (yOffset > 25 || yOffset < -25)
                        {
                            pressedState = PressState.MoveVertical;
                            break;
                        }
                        if (xOffset < -50 || xOffset > 50)
                        {
                            pressedState = PressState.MoveHorizontal;
                            ev.Action = MotionEventActions.Cancel;
                        }
                    }
                    else if (pressedState == PressState.MoveHorizontal)
                    {
                        if (currentActivityScaleX < 0.95)
                            ShowScrollViewMenu(scrollViewMenu);

                        float targetScale = GetTargetScale(ev.RawX);
                        if (Use3D)
                        {
                            int angle = scaleDirection == Direction.Left ? -ROTATE_Y_ANGLE : ROTATE_Y_ANGLE;
                            angle *= (int)((1 - targetScale) * 2);
                            viewActivity.RotationY = angle;

                            imageViewShadow.ScaleX = targetScale - shadowAdjustScaleX;
                            imageViewShadow.ScaleY = targetScale - shadowAdjustScaleY;
                        }
                        else
                        {
                            imageViewShadow.ScaleX = targetScale + shadowAdjustScaleX;
                            imageViewShadow.ScaleY = targetScale + shadowAdjustScaleY;
                        }
                        viewActivity.ScaleX = targetScale;
                        viewActivity.ScaleY = targetScale;
                        scrollViewMenu.Alpha = (1 - targetScale) * 2.0f;

                        lastRawX = ev.RawX;
                        return true;
                    }

                    break;

                case MotionEventActions.Up:

                    if (isInIgnoredView) break;
                    if (pressedState != PressState.MoveHorizontal) break;

                    pressedState = PressState.Done;
                    if (IsOpened)
                    {
                        if (currentActivityScaleX > 0.56f)
                            CloseMenu();
                        else
                            OpenMenu(scaleDirection);
                    }
                    else
                    {
                        if (currentActivityScaleX < 0.94f)
                        {
                            OpenMenu(scaleDirection);
                        }
                        else
                        {
                            CloseMenu();
                        }
                    }

                    break;

            }
            lastRawX = ev.RawX;
            return base.DispatchTouchEvent(ev);
        }
    }
}