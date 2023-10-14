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

            return gpuNameList;
        }

        // 返回值说明：1=成功 0=已经转换，无需转换 -1=失败
        public static int ModifyGpuReg(RegistryKey gpuRegKey)
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

        static void doall()
        {
            Console.WriteLine("NVIDIA Tesla 计算卡TTC切换WDDM工具");


            RegistryKey hk_lm = Registry.LocalMachine;
            RegistryKey hk_gpu_list =
                hk_lm.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}",
                    true);

            //Console.WriteLine(hk_gpu_list.SubKeyCount);
            string[] subnameStrings = hk_gpu_list.GetSubKeyNames();

            int max_count = 0;
            List<(string, string)> gpuNameList = new List<(string, string)>();

            for (int i = 0; i < subnameStrings.Length; i++)
            {
                var subNameStr = subnameStrings[i];
                if (string.IsNullOrEmpty(subNameStr)) { continue; }
                if (subNameStr.Length != 4) { continue; }

                var hk_gpu = hk_gpu_list.OpenSubKey(subNameStr);
                if (hk_gpu != null)
                {
                    max_count = i;
                    var gpuName = (string)hk_gpu.GetValue("DriverDesc");
                    if (gpuName != null)
                    {
                        gpuNameList.Add((subNameStr, gpuName));
                    }

                    hk_gpu.Close();
                }
                //else
                //{
                //    break;
                //}
            }


            Console.WriteLine("\n\n显卡列表：");
            foreach (var gpuName in gpuNameList)
            {
                Console.WriteLine(gpuName.Item1 + "\t" + gpuName.Item2);
                if (gpuName.Item2.ToLower().IndexOf("tesla".ToLower(), StringComparison.Ordinal) != -1)
                {
                    Console.WriteLine("\t这是一个NVIDIA Tesla计算卡！");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\n请输入4位的代码：");
            string gpuKey = Console.ReadLine();
            //const string gpuKey = "0001";

            if (gpuKey.Length == 4)
            {
                var hk_gpu = hk_gpu_list.OpenSubKey(gpuKey, true);
                if (hk_gpu != null)
                {
                    var gpuName = (string)hk_gpu.GetValue("DriverDesc");
                    if (gpuName != null)
                    {
                        Console.WriteLine("\n即将开始处理：" + gpuName);
                        handleReg(hk_gpu);
                    }

                    hk_gpu.Close();
                }
            }
            else
            {
                Console.WriteLine("输入非法！");
            }


            //foreach (var item in subnameStrings)
            //{
            //    Console.WriteLine(item);
            //}


            hk_gpu_list.Close();
            hk_lm.Close();


            Console.WriteLine("");
            Console.WriteLine("ok!");
            System.Console.ReadKey();
        }

        private static void handleReg(RegistryKey hk_gpu)
        {
            //Console.WriteLine((uint)(int)gpuRegKey.GetValue("AdapterType"));
            if ((uint)(int)hk_gpu.GetValue("AdapterType") != 1
                && (uint)(int)hk_gpu.GetValue("FeatureScore") != 209)
            {
                Console.WriteLine("即将转换为WDDM模式！");
                hk_gpu.SetValue("AdapterType", 1, RegistryValueKind.DWord);
                hk_gpu.SetValue("FeatureScore", 209, RegistryValueKind.DWord);
                hk_gpu.SetValue("GridLicensedFeatures", 7, RegistryValueKind.DWord);
                hk_gpu.SetValue("EnableMsHybrid", 1, RegistryValueKind.DWord);
            }
            else
            {
                Console.WriteLine("貌似不可以转换！");
                //Console.WriteLine((int)gpuRegKey.GetValue("AdapterType"));
                //Console.WriteLine((int)gpuRegKey.GetValue("FeatureScore"));
                if ((int)hk_gpu.GetValue("FeatureScore") == 209)
                {
                    Console.WriteLine("貌似已经工作在WDDM模式下了！");
                }
            }
            //string str16 = string.Format("{0:x}", 209); //转十六进制数
            //gpuRegKey.SetValue("test111", 209, RegistryValueKind.DWord);

            //gpuRegKey.SetValue("test111121", str16, RegistryValueKind.String);
        }

    }
}
