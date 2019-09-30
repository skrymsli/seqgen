using System.Collections.Generic;
using System.IO;
using System.Text;

namespace seqgen
{
    internal class Generator
    {
        private const string IndexVariable = "%i";
        private int StartIndex => _zeroBased ? 0 : 1;

        private bool _zeroBased;
        public Generator WithZeroBased(bool zeroBased = true)
        {
            _zeroBased = zeroBased;
            return this;
        }

        private static string _separator = ",";
        public Generator WithSeparator(string sep)
        {
            if (sep != null)
            {
                _separator = sep;
            }

            return this;
        }

        private static int? _firstColumnWithTemplate;
        public void Generate(string[] templates, TextWriter output, int itemCount)
        {
            var indexList = new List<ColumnInfo>();
            for(int i = 0; i < templates.Length; i++)
            {
                indexList.Add(new ColumnInfo(templates[i], StartIndex));
                if (_firstColumnWithTemplate == null && indexList[i].Include)
                {
                    _firstColumnWithTemplate = i;
                }
            }

            if (_firstColumnWithTemplate == null)
            {
                output.WriteLine(EmitLine(indexList));
            }
            else while (indexList[_firstColumnWithTemplate.Value].Count < (_zeroBased ? itemCount : itemCount + 1))
            {
                output.WriteLine(EmitLine(indexList));
                Increment(itemCount, indexList);
            }
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

        private void Increment(int max, IList<ColumnInfo> indexList)
        {
            var firstColumn = _firstColumnWithTemplate.GetValueOrDefault(0);
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

        private class ColumnInfo
        {
            public ColumnInfo(string template, int startIndex)
            {
                Template = template;
                Count = startIndex;
                Include = Template.Contains(IndexVariable);

            }
            public string Template { get; }
            public int Count {get; set; }
            public bool Include { get; }
        }
    }
}
