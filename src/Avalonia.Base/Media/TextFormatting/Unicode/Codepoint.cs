﻿using System.Runtime.CompilerServices;
using Avalonia.Utilities;

namespace Avalonia.Media.TextFormatting.Unicode
{
    public readonly struct Codepoint
    {
        /// <summary>
        /// The replacement codepoint that is used for non supported values.
        /// </summary>
        public static readonly Codepoint ReplacementCodepoint = new Codepoint('\uFFFD');

        public Codepoint(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Get the codepoint's value.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Gets the <see cref="Unicode.GeneralCategory"/>.
        /// </summary>
        public GeneralCategory GeneralCategory => UnicodeData.GetGeneralCategory(Value);

        /// <summary>
        /// Gets the <see cref="Unicode.Script"/>.
        /// </summary>
        public Script Script => UnicodeData.GetScript(Value);

        /// <summary>
        /// Gets the <see cref="Unicode.BidiClass"/>.
        /// </summary>
        public BidiClass BiDiClass => UnicodeData.GetBiDiClass(Value);

        /// <summary>
        /// Gets the <see cref="Unicode.BidiPairedBracketType"/>.
        /// </summary>
        public BidiPairedBracketType PairedBracketType => UnicodeData.GetBiDiPairedBracketType(Value);
        
        /// <summary>
        /// Gets the <see cref="Unicode.LineBreakClass"/>.
        /// </summary>
        public LineBreakClass LineBreakClass => UnicodeData.GetLineBreakClass(Value);

        /// <summary>
        /// Gets the <see cref="GraphemeBreakClass"/>.
        /// </summary>
        public GraphemeBreakClass GraphemeBreakClass => UnicodeData.GetGraphemeClusterBreak(Value);

        /// <summary>
        /// Determines whether this <see cref="Codepoint"/> is a break char.
        /// </summary>
        /// <returns>
        /// <c>true</c> if [is break character]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBreakChar
        {
            get
            {
                switch (Value)
                {
                    case '\u000A':
                    case '\u000B':
                    case '\u000C':
                    case '\u000D':
                    case '\u0085':
                    case '\u2028':
                    case '\u2029':
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Determines whether this <see cref="Codepoint"/> is white space.
        /// </summary>
        /// <returns>
        /// <c>true</c> if [is whitespace]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWhiteSpace
        {
            get
            {
                switch (GeneralCategory)
                {
                    case GeneralCategory.Control:
                    case GeneralCategory.NonspacingMark:
                    case GeneralCategory.Format:
                    case GeneralCategory.SpaceSeparator:
                    case GeneralCategory.SpacingMark:
                        return true;
                }

                return false;
            }
        }
        
        /// <summary>
        /// Gets the canonical representation of a given codepoint.
        /// <see href="http://www.unicode.org/L2/L2013/13123-norm-and-bpa.pdf"/>
        /// </summary>
        /// <param name="codePoint">The code point to be mapped.</param>
        /// <returns>The mapped canonical code point, or the passed <paramref name="codePoint"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Codepoint GetCanonicalType(Codepoint codePoint)
        {
            if (codePoint.Value == 0x3008)
            {
                return new Codepoint(0x2329);
            }

            if (codePoint.Value == 0x3009)
            {
                return new Codepoint(0x232A);
            }

            return codePoint;
        }
        
        /// <summary>
        /// Gets the codepoint representing the bracket pairing for this instance.
        /// </summary>
        /// <param name="codepoint">
        /// When this method returns, contains the codepoint representing the bracket pairing for this instance;
        /// otherwise, the default value for the type of the <paramref name="codepoint"/> parameter.
        /// This parameter is passed uninitialized.
        /// .</param>
        /// <returns><see langword="true"/> if this instance has a bracket pairing; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPairedBracket(out Codepoint codepoint)
        {
            if (PairedBracketType == BidiPairedBracketType.None)
            {
                codepoint = default;
                
                return false;
            }

            codepoint = UnicodeData.GetBiDiPairedBracket(Value);

            return true;
        }

        public static implicit operator int(Codepoint codepoint)
        {
            return codepoint.Value;
        }

        public static implicit operator uint(Codepoint codepoint)
        {
            return (uint)codepoint.Value;
        }

        /// <summary>
        /// Reads the <see cref="Codepoint"/> at specified position.
        /// </summary>
        /// <param name="text">The buffer to read from.</param>
        /// <param name="index">The index to read at.</param>
        /// <param name="count">The count of character that were read.</param>
        /// <returns></returns>
        public static Codepoint ReadAt(ReadOnlySlice<char> text, int index, out int count)
        {
            count = 1;

            if (index >= text.Length)
            {
                return ReplacementCodepoint;
            }

            var code = text[index];

            ushort hi, low;

            //# High surrogate
            if (0xD800 <= code && code <= 0xDBFF)
            {
                hi = code;

                if (index + 1 == text.Length)
                {
                    return ReplacementCodepoint;
                }

                low = text[index + 1];

                if (0xDC00 <= low && low <= 0xDFFF)
                {
                    count = 2;
                    return new Codepoint((hi - 0xD800) * 0x400 + (low - 0xDC00) + 0x10000);
                }

                return ReplacementCodepoint;
            }

            //# Low surrogate
            if (0xDC00 <= code && code <= 0xDFFF)
            {
                if (index == 0)
                {
                    return ReplacementCodepoint;
                }

                hi = text[index - 1];

                low = code;

                if (0xD800 <= hi && hi <= 0xDBFF)
                {
                    count = 2;
                    return new Codepoint((hi - 0xD800) * 0x400 + (low - 0xDC00) + 0x10000);
                }

                return ReplacementCodepoint;
            }

            return new Codepoint(code);
        }
    }
}
