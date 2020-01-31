using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using BetterOverwatch.Networking;

namespace BetterOverwatch.Game
{
    class BattleTag
    {
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        internal static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        [In, Out] byte[] buffer, int sizeout, out IntPtr lpNumberOfBytesRead);
        internal static string ReadFromMemory()
        {
            if (AppData.isAdmin)
            {
                Process[] processes = Process.GetProcessesByName("Battle.net");
                foreach (Process process in processes)
                {
                    if (AppData.blizzardAppOffset == 0)
                    {
                        if (!Server.FetchBlizzardAppOffset(process.MainModule.FileVersionInfo.FileVersion)) break;
                    }

                    foreach (ProcessModule processModule in process.Modules)
                    {
                        if (processModule.ModuleName == "battle.net.dll")
                        {
                            IntPtr processBaseAddress = processModule.BaseAddress;
                            byte[] battleTagBytes = ReadBytes(process.Handle, processBaseAddress + AppData.blizzardAppOffset, new[] { 0x28, 0x10, 0x8, 0x84, 0x0, 0x0 });
                            
                            if (battleTagBytes.Length > 0)
                            {
                                string[] battleTagSplit = Encoding.UTF8.GetString(battleTagBytes).Split('#');

                                if (battleTagSplit.Length == 2)
                                {
                                    return $"{battleTagSplit[0]}-{battleTagSplit[1].Substring(0, battleTagSplit[1].Length > 4 ? 5 : 4)}";
                                }
                            }
                            battleTagBytes = ReadBytes(process.Handle, processBaseAddress + AppData.blizzardAppOffset, new[] { 0x28, 0x10, 0x8, 0x84, 0x0 });

                            if (battleTagBytes.Length > 0)
                            {
                                string[] battleTagSplit = Encoding.UTF8.GetString(battleTagBytes).Split('#');

                                if (battleTagSplit.Length == 2)
                                {
                                    return $"{battleTagSplit[0]}-{battleTagSplit[1].Substring(0, battleTagSplit[1].Length > 4 ? 5 : 4)}";
                                }
                            }
                        }
                    }
                }
            }
            Functions.DebugMessage("Failed to find BattleTag");
            return "PLAYER-0000";
        }
        private static byte[] ReadBytes(IntPtr handle, IntPtr address, int[] offsets, int bytesToRead = 256)
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