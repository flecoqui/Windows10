//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.Devices.Enumeration.Pnp;

namespace TranslatorText
{
    public static class SystemInformation
    {

        public static string SystemFamily { get; }
        public static string SystemVersion { get; }
        public static string SystemArchitecture { get; }
        public static string ApplicationName { get; }
        public static string ApplicationVersion { get; }
        public static string DeviceManufacturer { get; }
        public static string DeviceModel { get; }
        public static string AppSpecificHardwareID { get;  }

        public static string GetWindowsVersion()
        {
            string str;
            string[] strArray = { "{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3" };
            PnpObject halDevice = GetHalDevice(strArray);
            if (halDevice == null || !halDevice.Properties.ContainsKey("{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3"))
                str = "UNKNOWN";
            else
                str = halDevice.Properties["{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3"].ToString();
            return str;
        }

        private static PnpObject GetHalDevice(params String[] properties)
        {
            String[] strArray = properties;
            String[] strArray1 = { "{A45C254E-DF1C-4EFD-8020-67D146A850E0},10" };
            IEnumerable<String> enumerable = strArray.Concat(strArray1);
            try
            {
                PnpObjectCollection pnpObjectCollection = PnpObject.FindAllAsync(PnpObjectType.Device, enumerable, "System.Devices.ContainerId:=\"{00000000-0000-0000-FFFF-FFFFFFFFFFFF}\"").GetResults();

                foreach (PnpObject pnpObject in pnpObjectCollection)
                {
                    if (pnpObject.Properties == null || !pnpObject.Properties.Any())
                        continue;

                    KeyValuePair<String, Object> keyValuePair = pnpObject.Properties.Last();
                    if (keyValuePair.Value == null || !keyValuePair.Value.ToString().Equals("4d36e966-e325-11ce-bfc1-08002be10318"))
                        continue;
                    return pnpObject;
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                return null;
            }
            return null;
        }
        private static string GetAppSpecificHardwareID()
        {
            Windows.System.Profile.HardwareToken packageSpecificToken;
            try
            {
                packageSpecificToken = Windows.System.Profile.HardwareIdentification.GetPackageSpecificToken(null);
                if (packageSpecificToken != null)
                {

                    Windows.Storage.Streams.DataReader dataReader = Windows.Storage.Streams.DataReader.FromBuffer(packageSpecificToken.Id);

                    byte[] bytes = new byte[packageSpecificToken.Id.Length];
                    dataReader.ReadBytes(bytes);
                    return BitConverter.ToString(bytes);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception in GetAppSpecificHardwareID: " + e.Message);
            }
            return "UNKNOWN";
        }
        static SystemInformation()
        {
            // get the system family name
            AnalyticsVersionInfo ai = AnalyticsInfo.VersionInfo;
            SystemFamily = ai.DeviceFamily;

            // get the system version number
#if WINDOWS_UWP
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            SystemVersion = $"{v1}.{v2}.{v3}.{v4}";
#else
            // get os version
            SystemVersion = GetWindowsVersion();

#endif
            // get the package architecure
            Package package = Package.Current;
            SystemArchitecture = package.Id.Architecture.ToString();

            // get the user friendly app name
            ApplicationName = package.DisplayName;

            // get the app version
            PackageVersion pv = package.Id.Version;
            ApplicationVersion = $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}";

            // get the device manufacturer and model name
            EasClientDeviceInformation eas = new EasClientDeviceInformation();
            DeviceManufacturer = eas.SystemManufacturer;
            DeviceModel = eas.SystemProductName;


            // get App Specific Hardware ID
            AppSpecificHardwareID = GetAppSpecificHardwareID();
        }
        public static string GetString()
        {
            string result = string.Empty;
            try {
                result = string.Format("System Information:\r\nFamily: {0}\r\nVersion: {1}\r\nArchitecture: {2}\r\nApplication Name: {3}\r\nApplication Version: {4}\r\nDevice Manufacturer: {5}\r\nDevice Model: {6}\r\nHardware ID: {7}\r\n",
                            SystemFamily,
                            SystemVersion,
                            SystemArchitecture,
                            ApplicationName,
                            ApplicationVersion,
                            DeviceManufacturer,
                            DeviceModel,
                            AppSpecificHardwareID
                    );
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                result = "OS version: UNKNOWN\r\nFamily: UNKNOWN\r\nVersion: UNKNOWN\r\nArchitecture: UNKNOWN\r\nApplication Name: UNKNOWN\r\nApplication Version: UNKNOWN\r\nDevice Manufacturer: UNKNOWN\r\nDevice Model: UNKNOWN\r\nHardware ID: UNKNOWN\r\n";
            }

            return result;
        }
    }
}
