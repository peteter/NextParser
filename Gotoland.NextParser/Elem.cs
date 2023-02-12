using System;
using System.Collections.Generic;

namespace Gotoland.NextParser
{
    public class Elem
    {
        /// <summary>
        /// Node in a parsed formula tree.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="type">The type of the element.</param>
        public Elem(string name, ElemType type)
        {
            Name = name;
            Type = type;
        }

        internal string Text { get; set; }
        internal int Start { get; set; }
        internal int Stop { get; set; }
        internal ElemType Type { get; private set; }

        public ValueType ValueType
        {
            get
            {
                if (this.valueType != ValueType.FromParameter)
                    return this.valueType;
                if (this.Left != null)
                    return this.Left.ValueType;
                else if (this.Right != null)
                    return this.Right.ValueType;
                return ValueType.None;
            }
            set => this.valueType = value;
        }

        private Elem parent;
        internal Elem Parent
        {
            get => parent;
            set
            {
                if (value == this) throw new ArgumentException("The parent-to-set is the element itself!");
                if (value?.parent == this) throw new ArgumentException("The parent of the parent-to-set is this element!");

                if (this.Left == value) { this.Left = null; }
                if (this.Right == value) { this.Right = null; }

                parent = value;
            }
        }
        internal Elem Left { get; set; }
        internal Elem Right { get; set; }

        private object value;
        private ValueType valueType;

        public object Value
        {
            get
            {
                if (this.Type == ElemType.Operator)
                {
                    return Name;
                }

                return this.value;
            }
            set => this.value = value;
        }

        internal List<Elem> Children { get; } = new List<Elem>();
        internal string Name { get; }

        public override string ToString()
        {
            return $"({Name}@{Type}; {Value})";
        }

        internal string Print(string indent = "")
        {
            var text = ToString() + Environment.NewLine;
            text += indent + "\tLeft:  " + Left?.Print(indent + "\t\t") + Environment.NewLine;
            text += indent + "\tRight: " + Right?.Print(indent + "\t\t") + Environment.NewLine;
            return text;
        }
    }

    /// <summary>
    /// The type that an <see cref="Elem"/> can have.
    /// </summary>
    public enum ElemType
    {
        None = 0,
        Function = 1,
        Number = 2,
        Text = 3,
        Operator = 4,
        Constant = 5,
        Group = 6,
        Variable = 7,
        CellReference = 8,
        CellRange = 9,
    }

    /// <summary>
    /// The type of input to a function, type of constant, or type or return value.
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        /// No type.
        /// </summary>
        None = 0,
        Any = 1,
        /// <summary>
        /// Decimal type.
        /// </summary>
        Number = 2,
        Text = 3,
        Boolean = 4,
        /// <summary>
        /// Any number of parameters.
        /// </summary>
        AnyNumber = 5,
        Date = 6,
        /// <summary>
        /// Parameter for function or operand will determine.
        /// </summary>
        FromParameter = 7,
        ///// <summary>
        ///// Variables have undetermined value type when parsing, since type is not know until
        ///// substitution during execution.
        ///// </summary>
        //Undetermined = 8,
    }

    public class Cell
    {
        public string Letters { get; set; }
        public int Numbers { get; set; }
    }

    internal class CellRange
    {
        public Cell From { get; set; }
        public Cell To { get; set; }
    }

}