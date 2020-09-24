using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RGSS_Extractor
{
    internal abstract class Parser
    {
        protected BinaryReader inFile;

        protected BinaryWriter outFile;

        protected int magickey;

        public List<Entry> entries = new List<Entry>();

        protected byte[] data;

        public Parser(BinaryReader file)
        {
            inFile = file;
        }

        public string Get_string(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public void Create_file(string path, string saveDir)
        {
            string directoryName =
                string.IsNullOrWhiteSpace(saveDir) || !Directory.Exists(saveDir)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : saveDir;
            string path2 = Path.Combine(directoryName, Path.GetDirectoryName(path));
            string path3 = Path.Combine(directoryName, path);
            Directory.CreateDirectory(path2);
            outFile = new BinaryWriter(File.OpenWrite(path3));
        }

        public byte[] Read_data(long offset, long size, int datakey)
        {
            inFile.BaseStream.Seek(offset, SeekOrigin.Begin);
            data = inFile.ReadBytes((int)size);
            int num = (int)size / 4;
            int i;
            for (i = 0; i < num; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    byte[] expr_43_cp_0 = data;
                    int expr_43_cp_1 = i * 4 + j;
                    expr_43_cp_0[expr_43_cp_1] ^= (byte)(datakey >> 8 * j);
                }
                datakey = datakey * 7 + 3;
            }
            int num2 = i * 4;
            while (num2 < size)
            {
                byte[] expr_82_cp_0 = data;
                int expr_82_cp_1 = num2;
                expr_82_cp_0[expr_82_cp_1] ^= (byte)(datakey >> 8 * num2);
                num2++;
            }
            return data;
        }

        public void Write_file(Entry e, string saveDir)
        {
            Create_file(e.name, saveDir);
            data = Read_data(e.offset, e.size, e.datakey);
            outFile.Write(data);
            outFile.Close();
            Console.WriteLine("{0} wrote out successfully", e.name);
        }

        public void Write_entries(string saveDir)
        {
            Form1.GetForm1.progressBar1.Visible = true;
            Form1.GetForm1.progressBar1.Maximum = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                Write_file(entries[i], saveDir);
                Form1.GetForm1.progressBar1.Value = i + 1;
            }
            Form1.GetForm1.progressBar1.Visible = false;
        }

        public void Close_file()
        {
            inFile.Dispose();
            inFile.Close();
        }

        public abstract void Parse_file();
    }
}
