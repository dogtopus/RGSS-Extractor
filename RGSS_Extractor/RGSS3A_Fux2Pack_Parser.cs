using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RGSS_Extractor
{
    internal class RGSS3A_Fux2Pack_Parser : RGSS3A_Parser
    {
        public RGSS3A_Fux2Pack_Parser(BinaryReader file) : base(file) { }

        public void Convert_data()
        {
            byte[] metadata_key = { };
            metadata_key = inFile.ReadBytes(4);
            metadata_key = BitConverter.GetBytes(((BitConverter.ToInt32(metadata_key, 0) - 3) * 0x38E38E39) & 0xffffffff);
            metadata_key = Encoding.UTF8.GetBytes("RGSSAD\x00\x03").Concat(metadata_key).ToArray();
            inFile.BaseStream.Seek(0, SeekOrigin.Begin);
            inFile.BaseStream.Write(metadata_key, 0, 12);
            inFile.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        public override void Parse_file()
        {
            Convert_data();
            base.Parse_file();
        }
    }
}
