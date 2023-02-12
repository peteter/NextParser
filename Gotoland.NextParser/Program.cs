using System;
using System.Text.Json;

namespace Gotoland.NextParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (true)
            //{
            //    var area = new StringSurface();
            //    area.Insert(4, 4, "4x4");
            //    area.Insert(2, 3, "bajs");
            //    area.Insert(-2, 3, "bajs");
            //    Console.WriteLine(area.ToString());
            //}
            //else
            {
                Console.WriteLine("Enter formula:");
                var input = "";
                while (input != "quit")
                {
                    var k = Console.ReadKey();
                    var (left, top) = Console.GetCursorPosition();
                    if (k.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Bye!");
                        return;
                    }
                    if (k.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write(" ");
                    }
                    else
                    {
                        input += k.KeyChar;
                    }
                    try
                    {
                        var (elem, result) = new Next().EvaluateDebug(input, null);
                        WipeALine(top + 1);
                        Console.WriteLine("result: " + result);

                        var (tree, text) = new PrettyPrinter().PrettyPrint(elem);
                        var json = JsonSerializer.Serialize(tree, new JsonSerializerOptions { WriteIndented = true });
                        Console.WriteLine(text);
                        //Console.WriteLine(json);
                    }
                    catch (Exception e)
                    {
                        WipeALine(top + 1);
                        Console.WriteLine("Error:\r\n" + e);
                    }
                    Console.SetCursorPosition(left, top);
                }
            }
        }

        public static void WipeALine(int line)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(0, line + i);
                Console.WriteLine(new string(' ', Console.BufferWidth));
            }
            Console.SetCursorPosition(0, line);
        }
    }
}