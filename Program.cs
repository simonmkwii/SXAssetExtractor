using System;
using System.IO;
using System.Text;

namespace SXAssetExtractor
{
    internal class Program
    {
        internal struct Header
        {
            internal byte[] Magic;
            internal int NumOfFiles;
            internal long Padding;
        }

        internal struct FileInfo
        {
            internal byte[] Filename;
            internal int Offset;
            internal int Size;
            internal int TexWidth;
            internal int TexHeight;
            internal bool HasAlpha;
            internal byte[] Padding;
        }

        private static string TrimB(byte[] In)
        {
            return Encoding.ASCII.GetString(In).Trim('\0');
        }

        private static void Main(string[] args)
        {
            var FO = File.OpenRead(args[0]);
            var Rd = new BinaryReader(FO);

            var Hdr = new Header
            {
                Magic      = Rd.ReadBytes(4),
                NumOfFiles = Rd.ReadInt32(),
                Padding    = Rd.ReadInt64()
            };

            var FolNm = TrimB(Hdr.Magic);

            var Files = new FileInfo[Hdr.NumOfFiles];

            Directory.CreateDirectory(FolNm);

            for (int i = 0; i < Hdr.NumOfFiles; i++)
            {
                Files[i] = new FileInfo
                {
                    Filename  = Rd.ReadBytes(8),
                    Offset    = Rd.ReadInt32(),
                    Size      = Rd.ReadInt32(),
                    TexWidth  = Rd.ReadInt32(),
                    TexHeight = Rd.ReadInt32(),
                    HasAlpha  = Rd.ReadBoolean(),
                    Padding   = Rd.ReadBytes(7)
                };

                var CurPos = FO.Position;

                var Name = TrimB(Files[i].Filename);
                var Resolution = $"{Files[i].TexWidth}x{Files[i].TexHeight}";

                Console.WriteLine
                (
                    $"Extracting {Name}.BIN\t" +
                    $"(Size: {Resolution}) " +
                    $"(Alpha: {(Files[i].HasAlpha ? "Yes" : "No")})"
                );

                var Out = File.OpenWrite($"{FolNm}/{Name}_{Resolution}.BIN");
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
