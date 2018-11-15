using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OverwatchTracker
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
        public static IntPtr readMultiLevelPointer(Process process, IntPtr[] offsets, string moduleName = null)
        {
            IntPtr pointer = (IntPtr)0;
            IntPtr processBaseAddress = process.MainModule.BaseAddress;

            //Debug.WriteLine("Original base: " + processBaseAddress);
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
            //Debug.WriteLine("Module base?: " + processBaseAddress);

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
                //Debug.WriteLine(pointer);
                if (pointer == IntPtr.Zero)
                {
                    break;
                }
            }

            return pointer;
        }
        public static string getString(string processName, string moduleName, IntPtr[] offsets, int size = 256)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0) return String.Empty;
                foreach (Process process in processes)
                {
                    IntPtr pointer = readMultiLevelPointer(process, offsets, moduleName);

                    byte[] memoryBytes = ReadBytes(process.Handle, pointer, size);
                    process.Dispose();

                    if (memoryBytes.Length > 0 && memoryBytes[0] != 0 && memoryBytes[1] != 0)
                        return Encoding.Default.GetString(memoryBytes);
                }
            }
            catch (Exception e){ Functions.DebugMessage("readString() error:" + e.ToString()); }

            return String.Empty;
        }
    }
}