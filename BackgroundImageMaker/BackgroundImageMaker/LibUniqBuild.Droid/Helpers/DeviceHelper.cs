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

namespace LibUniqBuild.Droid.Helpers
{
    using Uri = Android.Net.Uri;
    public class DeviceHelper
    {
        public static string DeviceName
        {
            get
            {
                string manufacturer = Build.Manufacturer;
                string model = Build.Model;
                if (model.StartsWith(manufacturer))
                {
                    return Capitalize(model);
                }
                else
                {
                    return Capitalize(manufacturer) + " " + model;
                }
            }
        }


        private static string Capitalize(string s)
        {
            if (s == null || s.Length == 0)
            {
                return "";
            }
            char first = s[0];
            
            if (char.IsUpper(first))
            {
                return s;
            }
            else
            {
                return char.ToUpper(first) + s.Substring(1);
            }
        }

        public static void UpdateIconBadgeCount(Context context, int count)
        {

            Intent intent = new Intent("android.intent.action.BADGE_COUNT_UPDATE");

            // Component를 정의
            intent.PutExtra("badge_count_package_name", context.PackageName);
            intent.PutExtra("badge_count_class_name", GetLauncherClassName(context));

            // 카운트를 넣어준다.
            intent.PutExtra("badge_count", count);

            // Version이 3.1이상일 경우에는 Flags를 설정하여 준다.
            if (Build.VERSION.SdkInt > BuildVersionCodes.GingerbreadMr1)
            {
                intent.SetFlags(ActivityFlags.IncludeStoppedPackages);
            }

            // send
            context.SendBroadcast(intent);
        }

        public static string GetLauncherClassName(Context context)
        {

            Intent intent = new Intent(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryLauncher);
            intent.SetPackage(context.PackageName);

            var resolveInfos = context.PackageManager.QueryIntentActivities(intent, 0);
            foreach (var resolveInfo in resolveInfos)
            {
                var pkgName = resolveInfo.ActivityInfo.ApplicationInfo.PackageName;
                if (pkgName.Equals(context.PackageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var className = resolveInfo.ActivityInfo.Name;
                    return className;
                }
            }

            if (resolveInfos != null && resolveInfos.Count > 0)
            {
                return resolveInfos[0].ActivityInfo.Name;
            }
            return null;
        }
    }
}