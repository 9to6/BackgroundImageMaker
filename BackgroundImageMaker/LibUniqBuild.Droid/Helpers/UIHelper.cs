
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System;
using Android.Runtime;
using Android.Util;

namespace LibUniqBuild.Droid.Helpers
{
    public class UIHelper
    {
        public static ViewGroup RootView(Activity activity)
        {
            return activity.FindViewById(Android.Resource.Id.Content) as ViewGroup;
        }

        public static Task<bool> ShowConfirm(Context context, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            if (!string.IsNullOrEmpty(title)) builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (senderAlert, args) =>
            {
                tcs.SetResult(true);
            });
            builder.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                tcs.SetResult(false);
                //Toast.MakeText(Activity, "Cancelled!", ToastLength.Short).Show();
            });
            builder.Create().Show();
            return tcs.Task;
        }

        class DialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            TaskCompletionSource<string> task;
            public DialogInterfaceOnCancelListener(TaskCompletionSource<string> task)
            {
                this.task = task;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                task.SetResult("");
            }
        }

        public static Task<string> ShowDialog(Context context, string title, params string[] items)
        {
            var tcs = new TaskCompletionSource<string>();
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);
            //public Builder SetSingleChoiceItems(string[] items, int checkedItem, EventHandler<DialogClickEventArgs> handler);
            builder.SetItems(items, (s, e) =>
            {
                var item = items[e.Which];
                tcs.SetResult(item);
                var dialog = s as AlertDialog;
                dialog.Dismiss();
            });
            var listener = new DialogInterfaceOnCancelListener(tcs);
            builder.SetOnCancelListener(listener);
            builder.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                tcs.SetResult("");
            });
            builder.Create().Show();
            return tcs.Task;
        }

        class PickerDialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            TaskCompletionSource<int> task;
            public PickerDialogInterfaceOnCancelListener(TaskCompletionSource<int> task)
            {
                this.task = task;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                task.SetResult(-1);
            }
        }

        public static Task<int> ShowPickerDialog(Context context, int defaultIndex, string[] displayedData)
        {
            var tcs = new TaskCompletionSource<int>();
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            var picker = new NumberPicker(context);
            picker.MinValue = 0;
            picker.MaxValue = displayedData.Length-1;
            picker.Value = defaultIndex;
            picker.SetDisplayedValues(displayedData);
            var parent = new FrameLayout(context);
            parent.AddView(picker, new FrameLayout.LayoutParams(
                    FrameLayout.LayoutParams.WrapContent,
                    FrameLayout.LayoutParams.WrapContent,
                    GravityFlags.Center));
            builder.SetView(parent);
            builder.SetPositiveButton("OK", (senderAlert, args) =>
            {
                tcs.SetResult(picker.Value);
            });
            builder.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                tcs.SetResult(-1);
            });

            var listener = new PickerDialogInterfaceOnCancelListener(tcs);
            builder.SetOnCancelListener(listener);
            builder.Create().Show();
            return tcs.Task;
        }

        public static bool InsertColorFilterOnTouch(View view, MotionEventActions action)
        {
            switch (action)
            {
                case MotionEventActions.Down:
                    {
                        var colorDrawable = view.Background as ColorDrawable;
                        if (colorDrawable != null)
                        {
                            var drawable = new ColorDrawable(colorDrawable.Color);
                            drawable.SetColorFilter(new Color(0x77000000), PorterDuff.Mode.SrcAtop);
                            view.Background = drawable;
                        }
                        else
                        {
                            view.Background.SetColorFilter(new Color(0x77000000), PorterDuff.Mode.SrcAtop);
                        }
                        //view.Background = ((Activity)view.Context).Resources.GetDrawable(Resource.Drawable.ic_delete);
                        view.Invalidate();
                        break;
                    }
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    {
                        var colorDrawable = view.Background as ColorDrawable;
                        if (colorDrawable != null)
                        {
                            var drawable = new ColorDrawable(colorDrawable.Color);
                            view.Background = drawable;
                        }
                        else
                        {
                            view.Background.ClearColorFilter();
                        }
                        view.Invalidate();
                        break;
                    }
            }
            return false;
        }

        public static bool InsertColorFilterOnTouch(ImageView imageView, MotionEventActions action)
        {
            switch (action)
            {
                case MotionEventActions.Down:
                    {
                        imageView.Drawable.SetColorFilter(new Color(0x77000000), PorterDuff.Mode.SrcAtop);
                        imageView.Invalidate();
                        break;
                    }
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    {
                        imageView.Drawable.ClearColorFilter();
                        imageView.Invalidate();
                        break;
                    }
            }
            return false;
        }

        public static bool IsVisible(View view)
        {
            return view.Visibility == ViewStates.Visible;
        }

        public static void SetVisible(View view, bool visible)
        {
            if (visible)
            {
                view.Visibility = ViewStates.Visible;
            }
            else
            {
                view.Visibility = ViewStates.Gone;
            }
        }

        public static IWindowManager WindowManager
        {
            get
            {
                return Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            }
        }

        public static int ScreenWidth
        {
            get
            {
                var windowManager = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                DisplayMetrics metrics = new DisplayMetrics();
                windowManager.DefaultDisplay.GetMetrics(metrics);
                return metrics.WidthPixels;
            }
        }

        public static int ScreenHeight
        {
            get
            {
                var windowManager = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                DisplayMetrics metrics = new DisplayMetrics();
                windowManager.DefaultDisplay.GetMetrics(metrics);
                return metrics.HeightPixels;
            }
        }
    }
}