using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Fsm97Trainer
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }
    [Flags]
    public enum ThreadAccessFlags : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200)
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccessFlags dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);
        public static byte[] ReadMemory(IntPtr hProcess, IntPtr address, int numOfBytes, out int bytesRead)
        {
            byte[] buffer = new byte[numOfBytes];
            ReadProcessMemory(hProcess, address, buffer, numOfBytes, out bytesRead);
            return buffer;
        }

        public static byte ReadByte(Process process, int address)
        {
            int bytesRead = 0;
            byte[] resultBits = NativeMethods.ReadMemory(process.Handle, new IntPtr(address), 1, out bytesRead);
            if (bytesRead > 0)
                return resultBits[0];
            return 0;
        }
        public static byte[] ReadBytes(Process process, int address, int count)
        {
            int bytesRead = 0;
            byte[] resultBits = NativeMethods.ReadMemory(process.Handle, new IntPtr(address), count, out bytesRead);
            if (bytesRead == count)
                return resultBits;
            throw new InvalidOperationException();
        }
        static Byte[] writeByteBffer = new Byte[1];
        public static void WriteByte(Process process, int address, byte value)
        {
            int bytesWritten = 0; writeByteBffer[0] = value;
            NativeMethods.WriteProcessMemory(process.Handle, new IntPtr(address), writeByteBffer, 1, out bytesWritten);
        }
        static Byte[] intBuffer = new Byte[4];
        public static int ReadInt(Process process, int address)
        {
            int bytesRead = 0;
            NativeMethods.ReadProcessMemory(process.Handle, new IntPtr(address), intBuffer, 4, out bytesRead);
            if (bytesRead == 4)
            {
                return BitConverter.ToInt32(intBuffer, 0);
            }
            return 0;
        }
        public static void WriteInt(Process process, int address, int value)
        {
            byte[] resultBits = BitConverter.GetBytes(value);
            WriteBytes(process, address, resultBits, 0, 4);
        }
        public static ushort ReadUShort(Process process, int address)
        {
            int bytesRead = 0;
            NativeMethods.ReadProcessMemory(process.Handle, new IntPtr(address), intBuffer, 2, out bytesRead);
            if (bytesRead == 2)
            {
                return BitConverter.ToUInt16(intBuffer, 0);
            }
            return 0;
        }
        public static void WriteUShort(Process process, int address, ushort value)
        {
            byte[] resultBits = BitConverter.GetBytes(value);
            WriteBytes(process, address, resultBits, 0, 2);
        }
        public static string ReadString(Process process, int address, Encoding encoding, int length)
        {
            int bytesRead = 0;
            byte[] resultBits = NativeMethods.ReadMemory(process.Handle, new IntPtr(address), length, out bytesRead);
            if (bytesRead == length)
            {
                var stringRead = encoding.GetString(resultBits);
                int index = stringRead.IndexOf('\0');
                if (index < 0)
                    return stringRead;
                return stringRead.Substring(0, index);
            }
            return string.Empty;
        }


        public static void WriteBytes(Process process, int address, byte[] data, int offset, uint length)
        {
            int bytesWritten = 0;
            if (offset == 0)
            {
                if (!NativeMethods.WriteProcessMemory(process.Handle, new IntPtr(address), data, length, out bytesWritten))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                }

            }
            else
            {
                byte[] second = new byte[length];
                Buffer.BlockCopy(data, offset, second, 0, (int)length);
                NativeMethods.WriteProcessMemory(process.Handle, new IntPtr(address), second, length, out bytesWritten);
            }
        }

        public static void ResumeProcess(Process process)
        {
            foreach (ProcessThread processThread in process.Threads)
            {
                IntPtr hThread = OpenThread(ThreadAccessFlags.SUSPEND_RESUME, false, (uint)processThread.Id);

                if (hThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(hThread);
                } while (suspendCount > 0);

                CloseHandle(hThread);
            }
        }

        public static void SuspendProcess(Process process)
        {
            foreach (ProcessThread processThread in process.Threads)
            {
                IntPtr hThread = OpenThread(ThreadAccessFlags.SUSPEND_RESUME, false, (uint)processThread.Id);

                if (hThread == IntPtr.Zero)
                {
                    continue;
                }
                SuspendThread(hThread);
                CloseHandle(hThread);
            }
        }

    }
}
