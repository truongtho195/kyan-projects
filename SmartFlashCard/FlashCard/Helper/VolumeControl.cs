using System;
using System.Runtime.InteropServices;

namespace FlashCard.Helper
{
    
    public static class VolumeControl
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public static ushort GetCurrentVolume()
        {
            // By the default set the volume to 0
            uint CurrVol = 0;
            // At this point, CurrVol gets assigned the volume
            waveOutGetVolume(IntPtr.Zero, out CurrVol);
            // Calculate the volume
            ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
            return CalcVol;
        }

        public static void SetCurrentVolume(uint NewVolumeAllChannels)
        {
            if (NewVolumeAllChannels >= 0)
            {
                // Set the volume
                waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
            }
        }
    }
}
