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

        public static List<(string, string)> ReadGpuList()
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
                // 如果系统直接没有这个键，那后续就不需要任何操作了！
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
                var gpuNameStr = (string)hkeyGpu.GetValue("DriverDesc");
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
                // 如果系统直接没有这个键，那后续就不需要任何操作了！
                return;
            }

            foreach (var gpuId in gpuIdList)
            {
                if (gpuId.Length == 4)
                {
                    var gpuSubKey = hkeyGpuList.OpenSubKey(gpuId, true);
                    if (gpuSubKey != null)
                    {
                        var gpuName = (string)gpuSubKey.GetValue("DriverDesc");
                        if (gpuName != null)
                        {
                            Debug.WriteLine("\n即将开始处理：" + gpuName);
                            ModifyGpuRegByRegKey(gpuSubKey);
                        }

                        gpuSubKey.Close();
                    }
                }
                else
                {
                    Debug.WriteLine(gpuId+" 输入非法！");
                }
            }

            hkeyGpuList.Close();
            hkeyLocalMachine.Close();
        }

        // 返回值说明：1=成功 0=已经转换，无需转换 -1=失败
        public static int ModifyGpuRegByRegKey(RegistryKey gpuRegKey)
        {
            //Console.WriteLine((uint)(int)gpuRegKey.GetValue("AdapterType"));
            if (
                (uint)(int)gpuRegKey.GetValue("AdapterType") != 1
                && (uint)(int)gpuRegKey.GetValue("FeatureScore") != 209
                )
            {
                Debug.WriteLine("即将转换为WDDM模式！");
                gpuRegKey.SetValue("AdapterType", 1, RegistryValueKind.DWord);
                gpuRegKey.SetValue("FeatureScore", 209, RegistryValueKind.DWord);
                gpuRegKey.SetValue("GridLicensedFeatures", 7, RegistryValueKind.DWord);
                gpuRegKey.SetValue("EnableMsHybrid", 1, RegistryValueKind.DWord);
                return 1;
            }
            else
            {
                Debug.WriteLine("貌似不可以转换！");
                Debug.WriteLine((int)gpuRegKey.GetValue("AdapterType"));
                Debug.WriteLine((int)gpuRegKey.GetValue("FeatureScore"));
                if ((int)gpuRegKey.GetValue("FeatureScore") == 209)
                {
                    Debug.WriteLine("貌似已经工作在WDDM模式下了！");
                    return 0;
                }
                return -1;
            }
        }


    }
}
