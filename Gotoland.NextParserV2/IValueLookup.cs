using System;
using System.Collections.Generic;

namespace Gotoland.NextParserV2
{
    public interface ICellValueLookup
    {
        object this[string sheet, int row, string column] { get; }
        object this[int row, string column] { get; }

        IEnumerable<object> GetRange((int row, string column) from, (int row, string column) to);
    }

    public class CellValues : ICellValueLookup
    {
        public CellValues(Dictionary<string, object> values)
        {
            Values = values;
        }

        public object this[int row, string column] { get => Values[column + row]; }
        public object this[string sheet, int row, string column] { get => Values[sheet + column + row]; }

        public Dictionary<string, object> Values { get; }

        public IEnumerable<object> GetRange((int row, string column) from, (int row, string column) to)
        {
            // TODO handle stepping of letter values, i.e. column.
            for (int y = from.row; y <= to.row; y++)
            {
                yield return this[y, from.column];
            }
        }
    }
}