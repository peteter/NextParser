using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gotoland.NextParser
{
    /*
     * 
   offset
------->+
        offset (from parent center)
       |->
     -     250
   /   \        
 100 | 10
    
     */

    internal class PrettyPrinter
    {
        internal (SubTree tree, string text) PrettyPrint(Elem elem)
        {
            if (elem == null)
            {
                return (null, "<null>");
            }

            var sb = new StringBuilder();

            var tree = MeasureSubTree(elem, true);
            var leftMargin = AdjustOffsets(tree, true);
            if (leftMargin < 0)
            {
                AdjustLeftMargin(tree, -leftMargin);
            }

            var items = new List<SubTree> { tree };
            while (items.Count > 0)
            {
                var children = new List<SubTree>();
                var c = 0;
                foreach (var sub in items)
                {
                    var off = sub.Offset - c;
                    if (off < 0)
                    {
                        off = 1;
                    }
                    sb.Append(new string(' ', off));
                    c += off;
                    sb.Append(sub.Text);
                    c += sub.Text.Length;
                    if (sub.Left != null)
                        children.Add(sub.Left);
                    if (sub.Right != null)
                        children.Add(sub.Right);
                }
                sb.AppendLine();
                items = children;
            }

            return (tree, sb.ToString());
        }

        private void AdjustLeftMargin(SubTree tree, int leftMargin)
        {
            if (tree == null)
            {
                return;
            }
            tree.Center += leftMargin;

            AdjustLeftMargin(tree.Left, leftMargin);
            AdjustLeftMargin(tree.Right, leftMargin);
        }

        private int AdjustOffsets(SubTree tree, bool leftmost)
        {
            int min = 0;
            if (tree == null)
            {
                return min;
            }
            min = tree.Offset;

            if (tree.Right != null)
            {
                tree.Right.Center += tree.Center;
                if (tree.Right.Offset < 0)
                {
                    Console.WriteLine(tree.Right);
                }
                AdjustOffsets(tree.Right, false);
            }
            if (tree.Left != null)
            {
                if (!leftmost)
                {
                    tree.Left.Center += tree.Center;
                    if (tree.Left.Offset < 0)
                    {
                        Console.WriteLine(tree.Left);
                    }
                }
                AdjustOffsets(tree.Left, leftmost);

                if (leftmost)
                {
                    min = Math.Min(min, tree.Left.Offset);
                }
            }
            return min;
        }

        private SubTree MeasureSubTree(Elem elem, bool leftmost)
        {
            if (elem == null) return null;

            var left = MeasureSubTree(elem.Left, leftmost);
            var right = MeasureSubTree(elem.Right, false);

            var elemText = elem.Text ?? elem.Name;
            if (elemText == null)
            {
                if (elem.Type == ElemType.Operator)
                {

                }
            }

            if (left == null && right == null)
            {
                var sub = new SubTree
                {
                    Text = elemText,
                    Width = elemText.Length,
                };
                return sub;
            }

            var leftWidth = left?.Width ?? 0;
            var rightWidth = right?.Width ?? 0;
            var width = Math.Max(leftWidth, elemText.Length);
            var here = new SubTree
            {
                Text = elemText,
                Width = 2 * width,
                Center = leftWidth + rightWidth,
                Left = left,
                Right = right
            };
            if (right != null)
            {
                right.Center = 1 + rightWidth / 2;
            }
            if (left != null && !leftmost)
            {
                left.Center = -1 - leftWidth / 2;
            }
            return here;
        }

        public class SubTree
        {
            public int Width { get; internal set; }
            public SubTree Left { get; internal set; }
            public SubTree Right { get; internal set; }
            public int Offset { get => Center - Text.Length / 2; }
            public string Text { get; internal set; }
            public int Center { get; internal set; }

            public override string ToString()
            {
                if (Offset < 0)
                {
                    global::System.Console.WriteLine(Text);
                }
                return $"{new string('.', Offset)}{Text}";
            }
        }
    }

    class PItem
    {
        public int Offset { get; set; }
        public string Text { get; set; }
        public int Center { get; set; }
        public PItem Parent { get; set; }
        public List<PItem> Children { get; set; }
    }
}