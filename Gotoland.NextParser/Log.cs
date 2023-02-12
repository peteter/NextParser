using System;
using System.Diagnostics;
using System.IO;

namespace Gotoland.NextParser
{
    public class Log
    {
        private static string fileName = null;

        static Log()
        {
            fileName = "C:\\code\\temp\\next.log.txt";
        }

        [Conditional("DEBUG")]
        public static void Initialize(string fileName)
        {
            Log.fileName = fileName;
        }

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            Write(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(string message, Elem elem)
        {
            Write(message);
            if (elem != null)
            {
                Write(new PrettyPrinter().PrettyPrint(elem).text);
            }
            else
            {
                Write("<null>");
            }
        }

        private static void Write(string v)
        {
            //Console.WriteLine(v);
            if (fileName != null)
            {
                try
                {
                    File.AppendAllText(fileName, v + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to append to log file.\r\n" + e);
                }
            }
        }
    }
}