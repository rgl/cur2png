// developed by Rui Lopes (ruilopes.com). licensed under GPLv3.
//
// this converts Windows cursor (*.cur) files into PNG files. It also creates a JSON file with the (x, y) cursor hotspot.
//
// you should optimize the generated files with https://bitbucket.org/rgl/imageoptiz/downloads

using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace cur2png
{
    class Program
    {
        static void Main(string[] args)
        {
            var crawler = new FileSystemCrawler();
            crawler.OnEnterDirectory += (directory, depth) => true;
            crawler.OnLeaveDirectory += (directory, depth) => {};
            crawler.OnFoundFile += (directory, file, depth) => ConvertToPng(file);
            crawler.Crawl(new DirectoryInfo(args.Length != 0 ? args[0] : "."), "*.cur");
        }

        private static void ConvertToPng(FileInfo file)
        {
            Console.WriteLine(file.FullName);

            var pngPath = Path.Combine(file.DirectoryName, Path.GetFileNameWithoutExtension(file.FullName) + ".png");
            var jsonPath = Path.Combine(file.DirectoryName, Path.GetFileNameWithoutExtension(file.FullName) + ".json");

            var hotSpots = GetCursorHotSpots(file.FullName);

            if (hotSpots.Length != 1)
            {
                throw new Exception("More than one cursor hot spot! We only expect one!");
            }

            var hotSpot = hotSpots[0];

            using (var image = new MagickImage(file.FullName))
            {
                hotSpot.X -= image.BoundingBox.X;
                hotSpot.Y -= image.BoundingBox.Y;

                image.Crop(image.BoundingBox);

                // +repage to remove the PNG layer offset.
                image.SetAttribute("page", "0x0+0+0");

                image.Format = MagickFormat.Png;

                image.Write(pngPath);

                //Console.WriteLine("{0} -- {1} {2}x{3} BoundingBox={4}x{5} HotSpot={6}x{7}", pngPath, image.Format, image.Width, image.Height, image.BoundingBox.Width, image.BoundingBox.Height, hotSpot.X, hotSpot.Y);

                File.WriteAllText(jsonPath, hotSpot.ToJson());
            }
        }

        public static HotSpot[] GetCursorHotSpots(string path)
        {
            // See http://en.wikipedia.org/wiki/ICO_(file_format)

            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                var reserved = reader.ReadInt16();

                if (reserved != 0)
                    throw new Exception("Not a CUR file");

                var type = reader.ReadInt16();

                if (type != 2)
                    throw new Exception("Not a CUR file");

                var count = reader.ReadInt16();

                var hotSpots = new List<HotSpot>();

                for (var i = 0; i < count; ++i)
                {
                    var width = reader.ReadSByte();
                    var height = reader.ReadSByte();
                    var colorCount = reader.ReadSByte();
                    var bReserved = reader.ReadSByte();

                    if (bReserved != 0)
                    {
                        throw new Exception("Not a CUR file");
                    }

                    var hotSpotX = reader.ReadInt16();
                    var hotSpotY = reader.ReadInt16();
                    var bytesInRes = reader.ReadInt32();
                    var imageOffset = reader.ReadInt32();

                    hotSpots.Add(new HotSpot { X = hotSpotX, Y = hotSpotY });
                }

                return hotSpots.ToArray();
            }
        }
    }
}
