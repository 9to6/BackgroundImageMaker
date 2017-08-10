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

namespace LibUniqBuild.Droid.Libraries.ResideMenu
{
    public class ResideMenuItem : LinearLayout
    {
        /** menu item  icon  */
        private ImageView iv_icon;
        /** menu item  title */
        private TextView tv_title;

        public ResideMenuItem(Context context) :
            base(context)
        {
            Initialize();
        }

        public ResideMenuItem(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public ResideMenuItem(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public ResideMenuItem(Context context, int icon, int title) :
            this(context)
        {
            Initialize();
            iv_icon.SetImageResource(icon);
            tv_title.SetText(title);
        }

        public ResideMenuItem(Context context, int icon, string title) :
            this(context)
        {
            Initialize();
            iv_icon.SetImageResource(icon);
            tv_title.Text = title;
        }

        private void Initialize()
        {
            initViews();
        }

        private void initViews()
        {
            LayoutInflater inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            inflater.Inflate(Resource.Layout.residemenu_item, this);
            iv_icon = (ImageView)FindViewById(Resource.Id.iv_icon);
            tv_title = (TextView)FindViewById(Resource.Id.tv_title);
        }

        /**
         * set the icon color;
         *
         * @param icon
         */
        
        public int IconRes
        {
            set
            {
                iv_icon.SetImageResource(value);
            }
        }

        /**
         * set the title with resource
         * ;
         * @param title
         */
        
        public int TitleRes
        {
            set
            {
                tv_title.SetText(value);
            }
        }

        /**
         * set the title with string;
         *
         * @param title
         */

        public string Title
        {
            set
            {
                tv_title.Text = value;
            }
        }
    }
}