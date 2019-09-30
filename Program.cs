using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace seqgen
{
    internal class Program
    {
        private static int StartIndex => _zeroBased ? 0 : 1;

        private static bool _zeroBased;

        private const string ZeroFlag = "-zero";
        private const string SeparatorFlag = "-sep";
        private static string _separator = ",";
        private const string IndexVariable = "%i";

        private static int? FirstColumnWithTemplate = null;

        private class ColumnInfo
        {
            public ColumnInfo(string template)
            {
                Template = template;
                Count = StartIndex;
                Include = Template.Contains(IndexVariable);

            }
            public string Template { get; }
            public int Count {get; set; }
            public bool Include { get; }
        }

        private static void Main(string[] args)
        {
            if (args.Any(x => x == ZeroFlag))
            {
                _zeroBased = true;
                args = args.Except(new[] {ZeroFlag}).ToArray();
            }

            if (args.Any(x => x == SeparatorFlag))
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == SeparatorFlag)
                    {
                        if (i + 1 > args.Length)
                        {
                            Usage();
                            return;
                        }

                        _separator = args[i + 1];
                    }
                }
                args = args.Except(new[] {SeparatorFlag, _separator}).ToArray();
            }

            if (args.Length == 0)
            {
                Usage();
                return;
            }
            if (!int.TryParse(args[0], out var itemCount))
            {
                Usage();
                return;
            }

            var arglen = args.Length - 1;
            if (arglen == 0)
            {
                Usage();
                return;
            }

            args = args.Skip(1).ToArray();
            var indexList = new List<ColumnInfo>();
            for(int i = 0; i < args.Length; i++)
            {
                indexList.Add(new ColumnInfo(args[i]));
                if (FirstColumnWithTemplate == null && indexList[i].Include)
                {
                    FirstColumnWithTemplate = i;
                }
            }

            if (FirstColumnWithTemplate == null)
            {
                Console.WriteLine(EmitLine(indexList));
            }
            else while (indexList[FirstColumnWithTemplate.Value].Count < (_zeroBased ? itemCount : itemCount + 1))
            {
                Console.WriteLine(EmitLine(indexList));
                Increment(itemCount, indexList);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("seqgen [-zero] [-sep <separator>] <count> <field1> <field2> ... <fieldN>");
        }

        private static string EmitLine(IReadOnlyList<ColumnInfo> indexList)
        {
            var sb = new StringBuilder();
            for(int i = 0; i < indexList.Count; i++)
            {
                var field = indexList[i].Template;
                if (indexList[i].Include)
                {
                    field = field.Replace(IndexVariable, indexList[i].Count.ToString());
                }
                
                sb.Append(field);
                if(i < indexList.Count - 1) sb.Append(_separator);
            }

            return sb.ToString();
        }

        private static void Increment(int max, IList<ColumnInfo> indexList)
        {
            var firstColumn = FirstColumnWithTemplate.GetValueOrDefault(0);
            for (int i = indexList.Count - 1; i >= firstColumn; i--)
            {
                if (!indexList[i].Include) continue;

                if (i == firstColumn || indexList[i].Count < (_zeroBased ? max - 1 : max))
                {
                    indexList[i].Count += 1;
                    return;
                }
                
                indexList[i].Count = StartIndex;
            }
        }
    }
}
