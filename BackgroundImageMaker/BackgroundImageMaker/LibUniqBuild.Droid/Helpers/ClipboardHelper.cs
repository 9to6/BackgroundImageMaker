using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;

namespace LibUniqBuild.Droid.Helpers
{
    public class ClipboardHelper
    {
        public static string GetText()
        {
            var clipboardmanager = (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);
            var item = clipboardmanager.PrimaryClip.GetItemAt(0);
            var text = item.Text;
            return text;
        }
        public static void SetText(string text)
        {
            var clipboardmanager = (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);
            ClipData clip = ClipData.NewPlainText("text", text);
            clipboardmanager.PrimaryClip = clip;
        }
    }
}