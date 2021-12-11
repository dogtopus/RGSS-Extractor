using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RGSS_Extractor
{
    internal class RGSS3A_Fux2Pack_Parser : RGSS3A_Parser
    {
        public RGSS3A_Fux2Pack_Parser(BinaryReader file) : base(file) { }

        public override void Parse_file()
        {
            magickey = inFile.ReadInt32();
            Parse_table();
        }
    }
}
