using AdSupport;
using Security;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace LibUniqBuild.iOS
{
    public class UBInstallation
    {
        public const string Key_Uid = "DeviceUID";
        public const string Key_InstalledUtcDate = "InstalledUtcDate";

        public static DateTime installedDateTime;
        public static DateTime InstalledDateTime
        {
            get
            {
                var installedDateStr = GetRecordsFromKeychain("installedDateTime", "deviceInfo");
                if (string.IsNullOrEmpty(installedDateStr)) installedDateStr = UserPreference.Instance.GetString(Key_InstalledUtcDate);
                if (string.IsNullOrEmpty(installedDateStr))
                {
                    var now = DateTime.Now.ToUniversalTime();
                    installedDateStr = now.ToString("yyyy-MM-dd HH:mm");
                }

                installedDateTime = DateTime.Parse(installedDateStr).ToLocalTime();
                SaveInstalledDateTime(installedDateTime);

                return installedDateTime;
            }
        }

        private static void SaveInstalledDateTime(DateTime installedDateTime)
        {
            var installedDateStr = installedDateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm");
            StoreKeysInKeychain(Key_InstalledUtcDate, installedDateStr, "deviceInfo");
            UserPreference.Instance.SetString(Key_InstalledUtcDate, installedDateStr);
        }

        public static void CheckInstalledDateTime()
        {
            var ret = InstalledDateTime;
            if (ret == null) throw new Exception("Not Exists Installed Date");
        }

        private static string AppleIFA()
        {
            string ret = null;
            if (ASIdentifierManager.SharedManager != null &&
                ASIdentifierManager.SharedManager.AdvertisingIdentifier != null)
            {
                ret = ASIdentifierManager.SharedManager.AdvertisingIdentifier.AsString();
            }
            return ret;
        }

        private static string AppleIFV()
        {
            if (UIDevice.CurrentDevice != null &&
                UIDevice.CurrentDevice.IdentifierForVendor != null)
            {
                return UIDevice.CurrentDevice.IdentifierForVendor.AsString();
            }
            return null;
        }

        private static string RandomUUID()
        {
            return new NSUuid().AsString();
        }

        private static void SaveUid(string uid)
        {
            StoreKeysInKeychain("UID", uid, "deviceInfo");
            UserPreference.Instance.SetString(Key_Uid, uid);
        }

        private static string uid;
        public static string UID
        {
            get
            {
                uid = GetRecordsFromKeychain("UID", "deviceInfo");
                if (uid == null) uid = UserPreference.Instance.GetString(Key_Uid);
                if (uid == null) uid = AppleIFA();
                if (uid == null) uid = AppleIFV();
                if (uid == null) uid = RandomUUID();
                SaveUid(uid);

                return uid;
            }
        }

        private static void StoreKeysInKeychain(string key, string value, string service)
        {
            var s = new SecRecord(SecKind.GenericPassword)
            {
                ValueData = NSData.FromString(value, NSStringEncoding.UTF8),
                Generic = NSData.FromString(key),
                Account = key,
                Accessible = SecAccessible.Always,
                Service = service,
            };
            var err = SecKeyChain.Add(s);
        }


        private static string GetRecordsFromKeychain(string key, string service)
        {
            string ret = null;
            SecStatusCode res;
            var rec = new SecRecord(SecKind.GenericPassword)
            {
                Generic = NSData.FromString(key),
                Account = key,
                Accessible = SecAccessible.Always,
                Service = service,
            };
            var match = SecKeyChain.QueryAsRecord(rec, out res);
            if (match != null)
            {
                // nsdata object :  match.ValueData;
                ret = match.ValueData.ToString(NSStringEncoding.UTF8);
            }
            return ret;
        }
    }
}