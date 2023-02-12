using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gotoland.NextParser
{
    public class StringSurface
    {
        private List<StringBuilder> rows = new List<StringBuilder>();
        private int offsetX = 0;
        public void Insert(int x, int y, string text)
        {
            if (x < -offsetX)
            {
                var toAdd = -x - offsetX;
                offsetX = -x;
                foreach (var r in rows)
                {
                    if (r.Length > 0)
                    {
                        r.Insert(0, new string(' ', toAdd));
                    }
                }
            }
            x += offsetX;
            while (rows.Count <= y)
            {
                rows.Add(new StringBuilder());
            }
            var row = rows[y];
            if (row.Length < x - 1)
            {
                row.Append(new string(' ', x - row.Length));
            }
            row.Insert(x, text);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var width = rows.Max(s => s.Length);
            foreach (var row in rows)
            {
                row.Append(' ', width - row.Length);
                row.Insert(0, "|");
                row.Append("|");
                sb.Append(row);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}