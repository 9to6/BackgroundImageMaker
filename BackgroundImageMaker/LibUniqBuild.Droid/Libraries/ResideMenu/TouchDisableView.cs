using Android.Content;

using Android.Util;
using Android.Views;


namespace LibUniqBuild.Droid.Libraries.ResideMenu
{
    public class TouchDisableView : ViewGroup
    {
        private View content;
        public View Content {
            get
            {
                return content;
            }
            set
            {
                if (content != null)
                {
                    RemoveView(content);
                }
                content = value;
                AddView(content);
            }
        }
        
        public bool TouchDisabled { get; set; }

        public TouchDisableView(Context context) : base(context, null)
        {
            Initialize();
        }

        public TouchDisableView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public TouchDisableView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }
        
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {

            int width = GetDefaultSize(0, widthMeasureSpec);
            int height = GetDefaultSize(0, heightMeasureSpec);
            SetMeasuredDimension(width, height);

            int contentWidth = GetChildMeasureSpec(widthMeasureSpec, 0, width);
            int contentHeight = GetChildMeasureSpec(heightMeasureSpec, 0, height);
            content.Measure(contentWidth, contentHeight);
        }
        
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int width = r - l;
            int height = b - t;
            content.Layout(0, 0, width, height);
        }
        
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return TouchDisabled;
        }

        private void Initialize()
        {
            TouchDisabled = false;
        }
    }
}