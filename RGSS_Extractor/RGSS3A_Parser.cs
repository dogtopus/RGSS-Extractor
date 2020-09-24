using System.IO;

namespace RGSS_Extractor
{
    internal class RGSS3A_Parser : Parser
    {
        public RGSS3A_Parser(BinaryReader file) : base(file) { }

        public string Read_filename(int len)
        {
            byte[] array = inFile.ReadBytes(len);
            for (int i = 0; i < len; i++)
            {
                byte[] expr_18_cp_0 = array;
                int expr_18_cp_1 = i;
                expr_18_cp_0[expr_18_cp_1] ^= (byte)(magickey >> (8 * (i % 4)));
            }
            return Get_string(array);
        }

        public void Parse_table()
        {
            while (true)
            {
                long num = inFile.ReadInt32();
                num ^= magickey;
                if (num == 0L)
                {
                    break;
                }
                long num2 = inFile.ReadInt32();
                int num3 = inFile.ReadInt32();
                int num4 = inFile.ReadInt32();
                num2 ^= magickey;
                num3 ^= magickey;
                num4 ^= magickey;
                string name = Read_filename(num4);
                Entry entry = new Entry
                {
                    offset = num,
                    name = name,
                    size = num2,
                    datakey = num3
                };
                entries.Add(entry);
            }
        }

        public override void Parse_file()
        {
            magickey = (inFile.ReadInt32() * 9) + 3;
            Parse_table();
        }
    }
}
