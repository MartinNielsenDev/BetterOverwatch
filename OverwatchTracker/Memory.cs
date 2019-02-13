using System;
using System.Runtime.InteropServices;

namespace BetterOverwatch
{
    class Memory
    {
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        [In, Out] byte[] buffer, Int32 sizeout, out IntPtr lpNumberOfBytesRead);

        public static byte[] ReadBytes(IntPtr handle, IntPtr address, int[] offsets, int bytesToRead = 256)
        {
            byte[] buffer = new byte[bytesToRead];

            ReadProcessMemory(handle, address, buffer, bytesToRead, out IntPtr ptrBytesRead);

            for (int i = 0; i < offsets.Length; i++)
            {
                ReadProcessMemory(handle, new IntPtr(BitConverter.ToInt32(buffer, 0) + offsets[i]), buffer, bytesToRead, out ptrBytesRead);
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                if(buffer[i] == 0)
                {
                    byte[] result = new byte[i];
                    Array.Copy(buffer, result, i);
                    return result;
                }
            }
            return new byte[0];
        }
    }
}