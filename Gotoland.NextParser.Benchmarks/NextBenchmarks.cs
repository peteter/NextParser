using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NextParser.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    //[MemoryDiagnoser]
    public class NextBenchmarks
    {
        [Benchmark(Baseline = true)]
        public void Parse()
        {
            var next = new Gotoland.NextParser.Next();

            foreach (var formula in formulas)
            {
                try
                {
                    next.Parse(formula);
                }
                catch (Exception)
                {
                    //Console.WriteLine($"{formula}: {ex.Message}");
                }
            }
        }


        [Benchmark]
        public void ParseSpan()
        {
            var next = new Gotoland.NextParserV2.Next();

            foreach (var formula in formulas)
            {
                try
                {
                    next.Parse(formula);
                }
                catch (Exception)
                {
                    //Console.WriteLine($"{formula}: {ex.Message}");
                }
            }
        }

        static List<string> formulas = new List<string> {
                "1"
                ,"-1"
                ,"()"
                ," \t\r\n"
                ,"PI"
                ,"PI()"
                ,"MAX(3, 2"
                ,"MAX(3, 2)"
                ,"MAX(100/2, 100-49)"
                ,"\"hello"
                ,"\"hello\""
                ,"  \"hello\"\t"
                ,"  \"hello, this is my FUN(1, 2) + 8 \""
                ,"1="
                ,"1 + 2"
                ,"1 + 2 + 3"
                ,"8 - 2 + 3"
                ,"1 - 2"
                ,"1 - -2"
                ,"-MAX(3, 2)"
                ,"MAX(MAX(3, 2), 8)"
                ,"MAX(3, 2, 1) + 8"
                ,"MAX(3, 2, 1) + MAX(20, 20)"
                ,"(3 + 1 + 2) + 20"
                ,"true"
                ,"COUNT(\"a\", \"b\""
                ,"IF(true, \"True\", \"False\""
                ,"IF(\"true\", 1, 2)"
                ,"IFS(\"true\", 1, \"false\", 2)"
                ,"IFS(False, 1, False, 2, 3)"
                ,"IFS(False)"
                ,"OR(\"true\", \"false\""
                ,"COUNT(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)"
                ,"2e"
                ,"10 - (5 - 2)"
                ,"10 - MAX(5, 4)"
                ,"10 * 2"
                ,"10 / 2"
                ,"10 - 4 / 2"
                ,"9 / 3 - 1"
                ,"10 * 2 + 1"
                ,"1 + 10 * 2"
                ,"NEG()"
                ,"NEG(\"text\""
                ,"NEG(IF(TRUE, \"text\", 2))"
                ,"NEG(IF(TRUE, 1, 2))"
                ,"!True"
                ," ( MAX(1, 3, 2) )"
                ,"(1)"
                ,"((1))"
                ,"MAX(@one, @fun, 0)"
                ,"ABS(@fun - 20)"
                ,"2 < 3"
                ,"2 <= 3"
                ,"2 > 3"
                ,"2 >= 3"
                ,"2^2"
                ,"50% * (100 / 2)"
                ,"(1+2)"
                ,"SUM(4, 10)"
                ,"SUM(C4,A10)"
                ,"SUM(C4,A10)"
                ,"SUM(C$4,$A$10, $D9)"
                ,"SUM(C4:C100)"
                ,"DATE(2023, 01, 01) - DATE(2022, 12, 31)"
                };
    }
}