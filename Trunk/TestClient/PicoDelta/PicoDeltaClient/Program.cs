using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PicoDeltaSl;

namespace PicoDeltaClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var fileProcessor = new FileProcessor();
            var filePath = args[0];
            var config = new Config();
            var sortedDict = fileProcessor.GetHashesForFile(filePath, config);
           foreach (var entry in sortedDict)
            {
                Console.WriteLine("{0}:{1}:{2}:{3}", entry.Value.Offset, entry.Key, Convert.ToBase64String(entry.Value.StrongHash) , entry.Value.Length);
            }
            Console.Clear();

            //var comparisonFile = fileProcessor.GetDiffBlocksForFile(sortedDict, args[1], config);

           

            Console.WriteLine("Returned: {0} hashes", sortedDict.Count());

            //Console.WriteLine(comparisonFile.Count);
            Console.ReadKey();
        }
    }
}
