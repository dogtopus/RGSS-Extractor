using System.IO;

namespace RGSS_Extractor
{
    internal class RGSSAD_Parser : Parser
    {
        public RGSSAD_Parser(BinaryReader file) : base(file) { }

        public string Read_filename(int len)
        {
            byte[] array = inFile.ReadBytes(len);
            for (int i = 0; i < len; i++)
            {
                byte[] expr_18_cp_0 = array;
                int expr_18_cp_1 = i;
                expr_18_cp_0[expr_18_cp_1] ^= (byte)magickey;
                magickey = (magickey * 7) + 3;
            }
            return Get_string(array);
        }

        public void Parse_table()
        {
            while (inFile.BaseStream.Position != inFile.BaseStream.Length)
            {
                int num = inFile.ReadInt32();
                num ^= magickey;
                magickey = (magickey * 7) + 3;
                string name = Read_filename(num);
                long num2 = inFile.ReadInt32();
                num2 ^= magickey;
                magickey = (magickey * 7) + 3;
                long position = inFile.BaseStream.Position;
                inFile.BaseStream.Seek(num2, SeekOrigin.Current);
                Entry entry = new Entry
                {
                    name = name,
                    offset = position,
                    size = num2,
                    datakey = magickey
                };
                entries.Add(entry);
            }
        }

        public override void Parse_file()
        {
            uint magickey = 3735931646u;
            this.magickey = (int)magickey;
            Parse_table();
        }
    }
}
