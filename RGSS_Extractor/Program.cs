using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RGSS_Extractor
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [STAThread]
        private static void Main(string[] args)
        {
            //if (args.Length > 0)
            //{
            //    IntPtr foregroundWindow = GetForegroundWindow();
            //    GetWindowThreadProcessId(foregroundWindow, out int processId);
            //    Process processById = Process.GetProcessById(processId);
            //    if (processById.ProcessName == "cmd")
            //    {
            //        AttachConsole(processById.Id);
            //    }
            //    else
            //    {
            //        AllocConsole();
            //    }
            //    Main_Parser main_Parser = new Main_Parser();
            //    main_Parser.Parse_file(args[0]);
            //    main_Parser.Export_archive(Path.GetDirectoryName(args[0]));
            //    FreeConsole();
            //    return;
            //}
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args.Length > 0 ? args[0] : null));
        }
    }
}
