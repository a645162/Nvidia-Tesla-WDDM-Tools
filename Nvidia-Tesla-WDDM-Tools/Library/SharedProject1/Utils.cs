using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SharedProject1
{
    internal class Utils
    {
        public static bool OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}