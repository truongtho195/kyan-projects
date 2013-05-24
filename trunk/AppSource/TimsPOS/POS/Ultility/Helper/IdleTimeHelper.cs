﻿using System;
using System.Runtime.InteropServices;

namespace CPC.Helper
{
    /// <summary>
    /// LASTINPUTINFO struct
    /// </summary>
    internal struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }

    /// <summary>
    /// Get system and application idle time
    /// </summary>
    class IdleTimeHelper
    {
        public static DateTime? LostFocusTime;

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static TimeSpan GetIdleTime()
        {
            // Initial last input info
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();

            // Set size for last input info
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            // Get last input info
            if (!GetLastInputInfo(ref lastInputInfo))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            // Return idle time
            return TimeSpan.FromMilliseconds(Environment.TickCount - lastInputInfo.dwTime);
        }
    }
}
