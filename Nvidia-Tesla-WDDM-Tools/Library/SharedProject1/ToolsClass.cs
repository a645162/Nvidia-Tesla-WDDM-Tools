using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SharedProject1
{
    internal class ToolsClass
    {
        private const string GpuListRegPath =
            @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

        public static List<(string, string)>? ReadGpuList()
        {
            var hkeyLocalMachine = Registry.LocalMachine;
            var hkeyGpuList =
                hkeyLocalMachine.OpenSubKey
                (
                    GpuListRegPath,
                    false
                );

            if (hkeyGpuList == null)
            {
                // If the system does not have this key directly, then no subsequent operation is required!
                return null;
            }

            Debug.WriteLine(hkeyGpuList.SubKeyCount);

            var subNameStrings = hkeyGpuList.GetSubKeyNames();

            var gpuNameList = new List<(string, string)>();

            foreach (var subNameStr in subNameStrings)
            {
                // 代号不能是空
                if (string.IsNullOrEmpty(subNameStr)) { continue; }
                // 代号都是4位数
                if (subNameStr.Length != 4) { continue; }

                var hkeyGpu = hkeyGpuList.OpenSubKey(subNameStr);
                // 打不开这个键就直接忽略
                if (hkeyGpu == null) continue;
                var gpuNameStr = (string?)hkeyGpu.GetValue("DriverDesc");
                if (gpuNameStr != null)
                {
                    gpuNameList.Add((subNameStr, gpuNameStr));
                }

                hkeyGpu.Close();
            }

            hkeyGpuList.Close();
            hkeyLocalMachine.Close();

            return gpuNameList;
        }

        public static void ModifyGpuReg(List<string> gpuIdList)
        {
            var hkeyLocalMachine = Registry.LocalMachine;
            var hkeyGpuList =
                hkeyLocalMachine.OpenSubKey
                (
                    GpuListRegPath,
                    true
                );

            if (hkeyGpuList == null)
            {
                // If the system does not have this key directly, then no subsequent operation is required!
                return;
            }

            foreach (var gpuId in gpuIdList)
            {
                if (gpuId.Length == 4)
                {
                    var gpuSubKey = hkeyGpuList.OpenSubKey(gpuId, true);
                    if (gpuSubKey == null) continue;
                    var gpuName = (string?)gpuSubKey.GetValue("DriverDesc");
                    if (gpuName != null)
                    {
                        Debug.WriteLine("\nComing soon:" + gpuName);
                        ModifyGpuRegByRegKey(gpuSubKey);
                    }

                    gpuSubKey.Close();
                }
                else
                {
                    Debug.WriteLine(gpuId + " Illegal input!");
                }
            }

            hkeyGpuList.Close();
            hkeyLocalMachine.Close();
        }

        // 返回值说明：1=成功 0=已经转换，无需转换 -1=失败
        private static int ModifyGpuRegByRegKey(RegistryKey gpuRegKey)
        {
            //Console.WriteLine((uint)(int)gpuRegKey.GetValue("AdapterType"));
            try
            {
                var intAdapterType = (int?)gpuRegKey.GetValue("AdapterType");
                var intFeatureScore = (int?)gpuRegKey.GetValue("FeatureScore");

                if (
                    (intAdapterType ?? -1) == -1
                    || (intFeatureScore ?? -1) == -1
                )
                {
                    Debug.WriteLine("It seems that it cannot be converted!");
                    return -1;
                }
                else
                {
                    if (
                        (intAdapterType ?? 0) != 1
                        || (intFeatureScore ?? 0) != 209
                    )
                    {
                        Debug.WriteLine("Converting to WDDM mode is about to be converted!");
                        gpuRegKey.SetValue("AdapterType", 1, RegistryValueKind.DWord);
                        gpuRegKey.SetValue("FeatureScore", 209, RegistryValueKind.DWord);
                        gpuRegKey.SetValue("GridLicensedFeatures", 7, RegistryValueKind.DWord);
                        gpuRegKey.SetValue("EnableMsHybrid", 1, RegistryValueKind.DWord);
                        Debug.WriteLine("Ok!");
                        return 1;
                    }

                    Debug.WriteLine((intAdapterType ?? -1));
                    Debug.WriteLine((intFeatureScore ?? -1));
                    if ((intFeatureScore ?? 0) != 209) return -1;
                    Debug.WriteLine("Looks like it's already working in WDDM mode!");
                    return 0;
                }
            }
            catch
            {
                return -1;
            }

        }


    }
}
