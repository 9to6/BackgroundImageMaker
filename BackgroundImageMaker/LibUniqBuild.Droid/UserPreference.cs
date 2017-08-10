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

namespace LibUniqBuild.Droid
{
    public class UserPreference
    {
        const string PreferenceName = "MySharedPrefs";
        private static UserPreference _instance;
        private static readonly object _lock = new object();

        public static UserPreference Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new UserPreference();
                    }
                    return _instance;
                }
            }
        }

        public void SetString(string key, string value)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            var prefsEditor = prefs.Edit();

            prefsEditor.PutString(key, value);
            prefsEditor.Commit();
        }

        public string GetString(string key)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            return prefs.GetString(key, "");
        }

        public void SetInt(string key, int value)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            var prefsEditor = prefs.Edit();

            prefsEditor.PutInt(key, value);
            prefsEditor.Commit();
        }

        public int GetInt(string key)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            return prefs.GetInt(key, 0);
        }

        public void SetLong(string key, long value)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            var prefsEditor = prefs.Edit();
            prefsEditor.PutLong(key, value);
            prefsEditor.Commit();
        }

        public long GetLong(string key)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            return prefs.GetLong(key, 0);
        }

        public void SetBool(string key, bool value)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            var prefsEditor = prefs.Edit();

            prefsEditor.PutBoolean(key, value);
            prefsEditor.Commit();
        }

        public bool GetBool(string key)
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PreferenceName, FileCreationMode.Private);
            return prefs.GetBoolean(key, false);
        }

    }
}