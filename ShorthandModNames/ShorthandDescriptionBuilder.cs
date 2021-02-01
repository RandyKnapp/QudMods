using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Core;
using XRL.World;

namespace ShorthandModNames
{
    public class ShorthandDescriptionBuilder : DescriptionBuilder
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly List<string> _adjectives = new List<string>();

        public new void AddAdjective(string desc, int orderAdjust = 0)
        {
            XRLCore.Log($"AddAdjective: {desc}");
            _adjectives.Add(desc);
        }

        public override string ToString()
        {
            var adjectiveShorthand = GenerateAdjectiveShorthand();
            AddClause(adjectiveShorthand);
            return base.ToString();
        }

        private string GenerateAdjectiveShorthand()
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("(");

            foreach (string adjective in _adjectives)
            {
                var baseText = ColorUtility.StripFormatting(adjective);
                var firstLetter = baseText.Length > 0 ? baseText[0] : '?';
                var color = ColorUtility.GetMainForegroundColor(adjective);
                _stringBuilder.Append("{{").Append(color).Append("|").Append(firstLetter).Append("}}");
            }

            _stringBuilder.Append(")");
            return _stringBuilder.ToString();
        }
    }
}