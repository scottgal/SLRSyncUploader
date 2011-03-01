using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HashEngine;

namespace PicoDeltaClient
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var filePath = args[0];
            var config = new Config();
            var outDict = FileProcessor.GetHashesForFile(filePath, config);
           var sortedDict = outDict.Select(x => x).OrderBy(x => x.Value.Offset).AsParallel();
           foreach (var entry in sortedDict)
            {
                Console.WriteLine("{0}:{1}:{2}:{3}", entry.Value.Offset, entry.Key, Convert.ToBase64String(entry.Value.StrongHash) , entry.Value.Length);
            }

            Console.WriteLine("Returned: {0} hashes", sortedDict.Count());
            Console.ReadKey();
        }
    }
}
