using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsr_ffeditc
{
    class Tsr_ffeditc_program
    {
        static void Main(string[] args)
        {
            (string, string) checkedArgs;
            checkedArgs = checkArguments(args);

            
        }

        static (string sourcePath, string destPath) checkArguments(string[] args)
        {
            if (args.Length < 0 || args.Length > 2)
            {
                Console.WriteLine("Incorrect arguments count\r\n");
                WriteHelp();
            }
            else
            {
                
            }
        }

        static void WriteHelp()
        {
            Console.WriteLine("The tsr_ffecditc.exe util converts MSTS SIMIS@ files into unicode 16LE format.");
            Console.WriteLine("If a file is already decompressed, it's being just copied.\r\n");
            Console.WriteLine("The program accepts one or two arguments which must be file name strings:");
            Console.WriteLine("tsr_ffeditc.exe <source_file> [dest_file]\r\n");
            Console.WriteLine("If only one file name is passed, it creates uncompressed file in the same folder with 'tsr_uncomressed_' prefix.");
            Console.WriteLine("If two file names are passed, it creates a file with the name specified in the second argument.");
            Console.WriteLine("If file name doesn't contain path, the file is being searched in the program folder.");
            Console.WriteLine("If destination file already exists, it will be rewritten without any notice.");
        }
    }
}
