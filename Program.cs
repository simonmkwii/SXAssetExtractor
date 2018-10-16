using System;
using System.IO;
using System.Text;

namespace SXAssetExtractor
{
    internal class Program
    {
        internal struct FileInfo
        {
            internal byte[] Filename;
            internal int Offset;
            internal int Size;
            internal int TexWidth;
            internal int TexHeight;
        }

        private static string TrimB(byte[] In)
        {
            return Encoding.ASCII.GetString(In).Trim('\0');
        }

        private static void Main(string[] args)
        {
            var FO = File.OpenRead(args[0]);
            var Rd = new BinaryReader(FO);

            var Magic = Rd.ReadBytes(4);
            var NumOfFiles = Rd.ReadInt32();

            FO.Position += 8;

            var Files = new FileInfo[NumOfFiles];

            Directory.CreateDirectory(TrimB(Magic));

            for (int i = 0; i < NumOfFiles; i++)
            {
                Files[i] = new FileInfo
                {
                    Filename = Rd.ReadBytes(8),
                    Offset = Rd.ReadInt32(),
                    Size = Rd.ReadInt32(),
                    TexWidth = Rd.ReadInt32(),
                    TexHeight = Rd.ReadInt32()
                };

                FO.Position += 8;
                var CurPos = FO.Position;

                var Name = TrimB(Files[i].Filename);
                var Resolution = $"{Files[i].TexWidth}x{Files[i].TexHeight}";

                Console.WriteLine($"Extracting {Name}.BIN ({Resolution})...");

                var Out = File.OpenWrite($"GFX/{Name}_{Resolution}.BIN");
                var Wrt = new BinaryWriter(Out);

                FO.Position = Files[i].Offset;
                Wrt.Write(Rd.ReadBytes(Files[i].Size));

                Wrt.Close();
                Out.Close();

                FO.Position = CurPos;
            }

            Console.WriteLine("\nDone!");

            Rd.Close();
            FO.Close();
        }
    }
}