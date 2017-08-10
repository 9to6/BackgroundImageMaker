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
using static Android.Views.View;

namespace LibUniqBuild.Droid.Libraries.QuickAction
{
    public class ActionItem
    {
        /**
         * Constructor
         */
        public ActionItem() { }

        /**
         * Constructor
         * 
         * @param icon {@link Drawable} action icon
         */
        public ActionItem(Drawable icon)
        {
            Icon = icon;
        }

        public string Title { get; set; }

        public Drawable Icon { get; set; }
    }
}