// developed by Rui Lopes (ruilopes.com). licensed under GPLv3.

using cur2png;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace png2json
{
    class Program
    {
        private class Datum
        {
            public string Name { get; set; }
            public HotSpot HotSpot { get; set; }
            public byte[] Data { get; set; }
        }

        static void Main(string[] args)
        {
            var data = new List<Datum>();

            var crawler = new FileSystemCrawler();
            crawler.OnEnterDirectory += (directory, depth) => true;
            crawler.OnLeaveDirectory += (directory, depth) => {};
            crawler.OnFoundFile += (directory, file, depth) => data.Add(ConvertToJson(file));
            crawler.Crawl(new DirectoryInfo(args.Length != 0 ? args[0] : "."), "*.png");

            Console.WriteLine(data.Where(d => d != null).ToDictionary(d => d.Name, d => new{d.HotSpot, d.Data}).ToJson());
        }

        private static Datum ConvertToJson(FileInfo file)
        {
            Console.Error.WriteLine(file.FullName);

            var name = Path.GetFileNameWithoutExtension(file.FullName).ToLowerInvariant();

            var cursor = ToCssCursor(name);

            if (cursor == null)
                return null;

            var jsonPath = Path.Combine(file.DirectoryName, name + ".json");

            HotSpot hotSpot = null;

            if (File.Exists(jsonPath))
            {
                hotSpot = JsonConvert.DeserializeObject<HotSpot>(File.ReadAllText(jsonPath));
            }

            return new Datum
                {
                    Name = cursor,
                    HotSpot = hotSpot,
                    Data = File.ReadAllBytes(file.FullName)
                };
        }

        // dictionary key is the cur name and value is the css cursor name.
        //
        // See https://developer.mozilla.org/en-US/docs/Web/CSS/cursor for valid cursor values.
        private readonly static Dictionary<string, string> CssCursorMapping = new Dictionary<string, string>
            {
                {"normal", "default"},
                {"link", "pointer"},
                {"help", "help"},
                {"text", "text"},
                {"busy", "wait"},
                {"working", "progress"},
            };

        private static string ToCssCursor(string name)
        {
            string cursor;

            return CssCursorMapping.TryGetValue(name, out cursor) ? cursor : null;
        }
    }
}
