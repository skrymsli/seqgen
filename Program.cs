using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace seqgen
{
    internal class Program
    {
        private const string ZeroFlag = "-zero";
        private static bool _zeroBased;
        private const string SeparatorFlag = "-sep";
        private static string _separator;

        private static void Main(string[] args)
        {
            try
            {
                args = ParseZeroFlag(args);
                args = ParseSeparator(args, out _separator);

                if (args.Length == 0)
                {
                    throw new UsageException("Too few arguments.");
                }

                if (!int.TryParse(args[0], out var itemCount))
                {
                    throw new UsageException("Missing item count.");
                }
                args = args.Skip(1).ToArray();
                if (args.Length == 0)
                {
                    throw new UsageException("Missing fields.");
                }
                

                var generator = new Generator()
                    .WithSeparator(_separator)
                    .WithZeroBased(_zeroBased);
                generator.Generate(args, Console.Out, itemCount);
            }
            catch (UsageException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Usage();
            }

        }

        private static string[] ParseZeroFlag(string[] args)
        {
            if (args.Any(x => x == ZeroFlag))
            {
                _zeroBased = true;
                return args.Except(new[] {ZeroFlag}).ToArray();
            }
            return args;
        }

        private static string[] ParseSeparator(string[] args, out string separator)
        {
            separator = ",";
            if (args.Any(x => x == SeparatorFlag))
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == SeparatorFlag)
                    {
                        if (i + 1 > args.Length)
                        {
                            throw new UsageException("Missing separator.");
                        }

                        separator = args[i + 1];
                    }
                }
                return args.Except(new[] {SeparatorFlag, separator}).ToArray();
            }

            return args;
        }


        private static void Usage()
        {
            Console.WriteLine(UsageText());
        }

        private static string UsageText()
        {
            return @"seqgen: Generate sequential data rows

Usage: seqgen [-zero] [-sep <separator>] <count> <field1> <field2> ... <fieldN>

Required:
  count: The number of times to iterate on each field.
  field: Text to output. At least one field is required.

Optional:
  -zero: Use zero based counters. Default: Use one-based counters.
  -sep: Use alternate field separator. Default: ,

Use %i in field names to generate sequential counters.

Example:

  seqgen 2 Row%i Data%i

  This generates the following data:

   Row1,Data1
   Row1,Data2
   Row2,Data1
   Row2,Data2
";
        }
    }

    internal class UsageException : Exception
    {
        public UsageException(string message) : base(message) {}
    }
}
