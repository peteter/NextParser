using System;
using System.Runtime.Serialization;

namespace Gotoland.NextParserV2
{
    public class NextException : Exception
    {
        public NextException() { }

        public NextException(string message) : base(message) { }

        public NextException(string message, Exception innerException) : base(message, innerException) { }

        public NextException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class NextParsingException : NextException
    {
        public Elem Elem { get; set; }

        public NextParsingException(string message, Elem elem) : base(message)
        {
            Elem = elem;
        }

        public NextParsingException(string message, Exception innerException, Elem elem) : base(message, innerException)
        {
            Elem = elem;
        }

        public NextParsingException() : base() { }

        public NextParsingException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            return base.ToString() + ErrorHelper.MakeElemStuff(Elem);
        }
    }


    /// <summary>
    /// Exception used when there are errors in the formula or the formula engine.
    /// </summary>
    public class BadFormulaException : NextParsingException
    {
        /// <summary>
        /// Create an exception with given message. The message should say what is wrong.
        /// </summary>
        /// <param name="message">What is wrong in the formula or the formula engine.</param>
        public BadFormulaException(string message, Elem elem = null) : base(message, elem)
        {
        }

        /// <summary>
        /// Create an exception with given message and source exception. The message should say what is wrong.
        /// </summary>
        /// <param name="message">What is wrong in the formula or the formula engine.</param>
        /// <param name="inner">Source exception.</param>
        public BadFormulaException(string message, Exception inner) : base(message, inner, null)
        {
        }
    }

    /// <summary>
    /// Exception thrown when the evaluation specifically (not parsing) of a formula creates an error.
    /// </summary>
    public class FormulaEvaluationException : NextParsingException
    {
        public FormulaEvaluationException(string message, Elem elem = null) : base(message, elem)
        {
        }
    }

    /// <summary>
    /// Exception class for errors with parameters.
    /// </summary>
    [Serializable]
    public class InvalidParameterException : NextParsingException
    {
        public InvalidParameterException() : base()
        {
        }

        public InvalidParameterException(string message) : base(message, null)
        {
        }

        public InvalidParameterException(string message, Exception innerException) : base(message, innerException, null)
        {
        }

        protected InvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class InvalidFormulaException : Exception
    {
        public InvalidFormulaException()
        {
        }

        public InvalidFormulaException(string message) : base(message)
        {
        }

        public InvalidFormulaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidFormulaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class ErrorHelper
    {
        public static string MakeElemStuff(Elem elem)
        {
            if (elem == null)
            {
                return string.Empty;
            }
            return Environment.NewLine
               + $"Element: {{{elem.Start}, {elem.Stop}}}: [{elem.Text}]'";
        }
    }
}