using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibUniqBuild.iOS
{
    public class UserPreference
    {
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
            NSUserDefaults.StandardUserDefaults.SetString(value, key);
        }

        public string GetString(string key)
        {
            return NSUserDefaults.StandardUserDefaults.StringForKey(key);
        }

        public void SetInt(string key, int value)
        {
            NSUserDefaults.StandardUserDefaults.SetInt(value, key);
        }

        public int GetInt(string key)
        {
            return (int)NSUserDefaults.StandardUserDefaults.IntForKey(key);
        }

        public void SetLong(string key, long value)
        {
            NSUserDefaults.StandardUserDefaults.SetValueForKey(NSNumber.FromInt64(value), new NSString(key));
        }

        public long GetLong(string key)
        {
            var numberObj = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString(key)) as NSNumber;
            if (numberObj == null) return 0;
            return numberObj.Int64Value;
        }

        public void SetBool(string key, bool value)
        {
            NSUserDefaults.StandardUserDefaults.SetBool(value, key);
        }

        public bool GetBool(string key)
        {
            return NSUserDefaults.StandardUserDefaults.BoolForKey(key);
        }
    }
}