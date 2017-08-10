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
using System.Threading;

namespace LibUniqBuild.Droid.Helpers
{
    using System.Globalization;
    using Debug = System.Diagnostics.Debug;
    public class LocalizationHelper
    {
        public static CultureInfo GetCurrentCultureInfo()
        {
            var androidLocale = Java.Util.Locale.Default;

            //var netLanguage = androidLocale.Language.Replace ("_", "-");
            var netLanguage = androidLocale.ToString().Replace("_", "-");

            //var netLanguage = androidLanguage.Replace ("_", "-");
            Debug.WriteLine("android:" + androidLocale.ToString());
            Debug.WriteLine("net:" + netLanguage);

            return new CultureInfo(netLanguage);
        }

        public static void SetLocale(CultureInfo ci)
        {
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        public static void InitLocale()
        {
            SetLocale(GetCurrentCultureInfo());
        }
    }
}