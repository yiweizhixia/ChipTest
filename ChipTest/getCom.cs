using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace ChipTest
{
    class getCom
    {
        public static string GetComNumber(string devices_name)
        {
            char[] separatorMark = { '(', ')' };
            string[] strList1 = devices_name.Split(separatorMark);
            if (strList1[1].Substring(0, 3) == "COM")
                return strList1[1];
            else
                return null;
        }
        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\" and Name LIKE '%USB Serial Port%'");

            foreach (var device in searcher.Get())
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description"),
                (string)device.GetPropertyValue("Name")
                ));
            }

            return devices;
        }
    }
    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string name)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
            this.Name = name;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }

    }
}
