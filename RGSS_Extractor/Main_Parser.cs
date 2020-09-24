using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RGSS_Extractor
{
    public class Main_Parser
    {
        private Parser parser;

        private Parser Get_parser(int version, BinaryReader inFile)
        {
            switch (version)
            {
                case 1:
                    return new RGSSAD_Parser(inFile);

                case 3:
                    return new RGSS3A_Parser(inFile);

                case 107:
                    return new RGSS3A_Fux2Pack_Parser(inFile);

                default:
                    return null;
            }
        }

        public List<Entry> Parse_file(string path)
        {
            MemoryStream fms = new MemoryStream(File.ReadAllBytes(path));
            BinaryReader binaryReader = new BinaryReader(fms);
            string fileHead = Encoding.UTF8.GetString(binaryReader.ReadBytes(6));
            if (!fileHead.Contains("RGSSAD") && !fileHead.Contains("Fux2Pa"))
            { return null; }
            binaryReader.ReadByte();
            int version = binaryReader.ReadByte();
            parser = Get_parser(version, binaryReader);
            if (parser == null)
            { return null; }
            parser.Parse_file();
            return parser.entries;
        }

        public byte[] Get_filedata(Entry e)
        {
            return parser.Read_data(e.offset, e.size, e.datakey);
        }

        public void Export_file(Entry e, string saveDir)
        {
            parser.Write_file(e, saveDir);
        }

        public void Export_archive(string saveDir)
        {
            if (parser == null) { return; }
            parser.Write_entries(saveDir);
        }

        public void Close_file()
        {
            parser.Close_file();
        }
    }
}
