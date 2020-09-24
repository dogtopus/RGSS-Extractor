using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RGSS_Extractor
{
    public class WinFileIO : IDisposable
    {
        private const uint GENERIC_READ = 2147483648u;

        private const uint GENERIC_WRITE = 1073741824u;

        private const uint OPEN_EXISTING = 3u;

        private const uint CREATE_ALWAYS = 2u;

        private const int BlockSize = 65536;

        private GCHandle gchBuf;

        private IntPtr pHandle;

        private unsafe void* pBuffer;

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr CreateFile(string FileName, uint DesiredAccess, uint ShareMode, uint SecurityAttributes, uint CreationDisposition, uint FlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe bool ReadFile(IntPtr hFile, void* pBuffer, int NumberOfBytesToRead, int* pNumberOfBytesRead, int Overlapped);

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe bool WriteFile(IntPtr handle, void* pBuffer, int NumberOfBytesToWrite, int* pNumberOfBytesWritten, int Overlapped);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        public WinFileIO()
        {
            pHandle = IntPtr.Zero;
        }

        public WinFileIO(Array Buffer)
        {
            PinBuffer(Buffer);
            pHandle = IntPtr.Zero;
        }

        protected void Dispose(bool disposing)
        {
            Close();
            UnpinBuffer();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WinFileIO()
        {
            Dispose(false);
        }

        unsafe public void PinBuffer(Array Buffer)
        {
            UnpinBuffer();
            gchBuf = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            pBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(Buffer, 0).ToPointer();
        }

        public void UnpinBuffer()
        {
            if (gchBuf.IsAllocated)
            {
                gchBuf.Free();
            }
        }

        public void OpenForReading(string FileName)
        {
            Close();
            pHandle = CreateFile(FileName, 2147483648u, 0u, 0u, 3u, 0u, 0);
            if (pHandle == IntPtr.Zero)
            {
                Win32Exception ex = new Win32Exception();
                ApplicationException ex2 = new ApplicationException("WinFileIO:OpenForReading - Could not open file " + FileName + " - " + ex.Message);
                throw ex2;
            }
        }

        public void OpenForWriting(string FileName)
        {
            Close();
            pHandle = CreateFile(FileName, 1073741824u, 0u, 0u, 2u, 0u, 0);
            if (pHandle == IntPtr.Zero)
            {
                Win32Exception ex = new Win32Exception();
                ApplicationException ex2 = new ApplicationException("WinFileIO:OpenForWriting - Could not open file " + FileName + " - " + ex.Message);
                throw ex2;
            }
        }

        public unsafe int Read(int BytesToRead)
        {
            int result = 0;
            if (!ReadFile(pHandle, pBuffer, BytesToRead, &result, 0))
            {
                Win32Exception ex = new Win32Exception();
                ApplicationException ex2 = new ApplicationException("WinFileIO:Read - Error occurred reading a file. - " + ex.Message);
                throw ex2;
            }
            return result;
        }

        public unsafe int ReadUntilEOF()
        {
            int num = 0;
            int num2 = 0;
            byte* ptr = (byte*)pBuffer;
            while (ReadFile(pHandle, (void*)ptr, 65536, &num, 0))
            {
                if (num == 0)
                {
                    return num2;
                }
                num2 += num;
                ptr += num;
            }
            Win32Exception ex = new Win32Exception();
            ApplicationException ex2 = new ApplicationException("WinFileIO:ReadUntilEOF - Error occurred reading a file. - " + ex.Message);
            throw ex2;
        }

        public unsafe int ReadBlocks(int BytesToRead)
        {
            int num = 0;
            int num2 = 0;
            byte* ptr = (byte*)pBuffer;
            while (true)
            {
                int numberOfBytesToRead = Math.Min(65536, BytesToRead - num2);
                if (!ReadFile(pHandle, (void*)ptr, numberOfBytesToRead, &num, 0))
                {
                    break;
                }
                if (num == 0)
                {
                    return num2;
                }
                num2 += num;
                ptr += num;
                if (num2 >= BytesToRead)
                {
                    return num2;
                }
            }
            Win32Exception ex = new Win32Exception();
            ApplicationException ex2 = new ApplicationException("WinFileIO:ReadBytes - Error occurred reading a file. - " + ex.Message);
            throw ex2;
        }

        public unsafe int Write(int BytesToWrite)
        {
            int result;
            if (!WriteFile(pHandle, pBuffer, BytesToWrite, &result, 0))
            {
                Win32Exception ex = new Win32Exception();
                ApplicationException ex2 = new ApplicationException("WinFileIO:Write - Error occurred writing a file. - " + ex.Message);
                throw ex2;
            }
            return result;
        }

        public unsafe int WriteBlocks(int NumBytesToWrite)
        {
            int num = 0;
            int num2 = 0;
            byte* ptr = (byte*)pBuffer;
            int num3 = NumBytesToWrite;
            while (true)
            {
                int num4 = Math.Min(num3, 65536);
                if (!WriteFile(pHandle, (void*)ptr, num4, &num, 0))
                {
                    break;
                }
                ptr += num4;
                num2 += num4;
                num3 -= num4;
                if (num3 <= 0)
                {
                    return num2;
                }
            }
            Win32Exception ex = new Win32Exception();
            ApplicationException ex2 = new ApplicationException("WinFileIO:WriteBlocks - Error occurred writing a file. - " + ex.Message);
            throw ex2;
        }

        public bool Close()
        {
            bool result = true;
            if (pHandle != IntPtr.Zero)
            {
                result = CloseHandle(pHandle);
                pHandle = IntPtr.Zero;
            }
            return result;
        }
    }
}
