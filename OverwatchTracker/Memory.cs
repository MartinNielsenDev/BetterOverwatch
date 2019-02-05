using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace BetterOverwatch
{
    class Memory
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, Int32 size, out IntPtr lpNumberOfBytesRead);
        public static byte[] ReadBytes(IntPtr Handle, IntPtr Address, int BytesToRead)
        {
            IntPtr ptrBytesRead;
            byte[] buffer = new byte[BytesToRead];
            ReadProcessMemory(Handle, Address, buffer, BytesToRead, out ptrBytesRead);
            return buffer;
        }
        public static Int64 ReadInt64(IntPtr Address, int length = 4, IntPtr? Handle = null)
        {
            return BitConverter.ToInt32(ReadBytes((IntPtr)Handle, Address, length), 0);
        }
        public static IntPtr ReadMultiLevelPointer(Process process, IntPtr[] offsets, string moduleName = null)
        {
            IntPtr pointer = (IntPtr)0;
            IntPtr processBaseAddress = process.MainModule.BaseAddress;

            if (moduleName != null)
            {
                foreach (ProcessModule processModule in process.Modules)
                {
                    if (processModule.ModuleName == moduleName)
                    {
                        processBaseAddress = processModule.BaseAddress;
                        break;
                    }
                }
            }

            for (int i = 0; i <= offsets.Length - 1; i++)
            {
                if (i == 0)
                {
                    IntPtr ptr = IntPtr.Add(processBaseAddress, (int)offsets[i]);
                    pointer = (IntPtr)ReadInt64(ptr, 8, process.Handle);
                }
                else if (i != offsets.Length - 1)
                {
                    IntPtr ptr2 = IntPtr.Add(pointer, (int)offsets[i]);
                    pointer = (IntPtr)ReadInt64(ptr2, 8, process.Handle);
                }
                if (pointer == IntPtr.Zero)
                {
                    break;
                }
            }

            return pointer;
        }
        public static string FetchString(string processName, string moduleName, IntPtr[] offsets, int size = 256)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0) return String.Empty;
                foreach (Process process in processes)
                {
                    IntPtr pointer = ReadMultiLevelPointer(process, offsets, moduleName);

                    byte[] memoryBytes = ReadBytes(process.Handle, pointer, size);
                    process.Dispose();

                    if (memoryBytes.Length > 0 && memoryBytes[0] != 0 && memoryBytes[1] != 0)
                        return Encoding.UTF8.GetString(memoryBytes);
                }
            }catch{ }

            return String.Empty;
        }
    }
}