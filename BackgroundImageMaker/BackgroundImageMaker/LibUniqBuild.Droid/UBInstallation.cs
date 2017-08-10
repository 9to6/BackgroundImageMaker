using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using static Android.Provider.Settings;
using Android.Telephony;
using Java.Util;

namespace LibUniqBuild.Droid
{
    public class UBInstallation
    {
        public const string Key_Uid = "DeviceUID";
        public const string Key_InstalledUtcDate = "InstalledUtcDate";


        private static UBInstallation _instance;
        private static readonly object _lock = new object();

        Context context;

        public UBInstallation(Context context)
        {
            this.context = context;
        }

        public static UBInstallation Instance(Context context)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new UBInstallation(context);
                }
                return _instance;
            }
        }

        public DateTime installedDateTime;
        public DateTime InstalledDateTime
        {
            get
            {
                var installedDateStr = UserPreference.Instance.GetString(Key_InstalledUtcDate);
                if (string.IsNullOrEmpty(installedDateStr))
                {
                    var now = DateTime.Now.ToUniversalTime();
                    installedDateStr = now.ToString("yyyy-MM-dd HH:mm");
                }

                installedDateTime = DateTime.Parse(installedDateStr).ToLocalTime();
                UserPreference.Instance.SetString(Key_InstalledUtcDate, installedDateStr);

                return installedDateTime;
            }
        }

        private string uid;
        public string UID
        {
            get
            {
                uid = AndroidSecureId();
                if (string.IsNullOrEmpty(uid))
                {
                    uid = AndroidRandomId();
                }
                SaveUid(uid);

                return uid;
            }
        }

        public string AndroidSecureId()
        {
            return Secure.GetString(context.ContentResolver, Secure.AndroidId);
        }

        public string AndroidRandomId()
        {
            var tm = context.GetSystemService(Context.TelephonyService) as TelephonyManager;
            string tmDevice, tmSerial;
            tmDevice = "" + tm.DeviceId;
            tmSerial = "" + tm.SimSerialNumber;

            var secureId = AndroidSecureId();
            UUID deviceUuid = new UUID(secureId.GetHashCode(), ((long)tmDevice.GetHashCode() << 32) | (long)tmSerial.GetHashCode());

            return deviceUuid.ToString();
        }

        private void SaveUid(string uid)
        {
            UserPreference.Instance.SetString(Key_Uid, uid);
        }

        public void CheckInstalledDateTime()
        {
            var ret = InstalledDateTime;
            if (ret == null) throw new Exception("Not Exists Installed Date");
        }
    }
}