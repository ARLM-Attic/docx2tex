using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace docx2tex
{
    class TeXing
    {
        #region Fields

        private static Dictionary<string, string> _replacerPairs;

        #endregion

        #region Lifecycle methods

        static TeXing()
        {
            InitReplacesPairs();
        }

        #endregion

        #region Initialization methods

        private static void InitReplacesPairs()
        {
            _replacerPairs = new Dictionary<string, string>();
            _replacerPairs.Add("’", "'");
            _replacerPairs.Add("‘", "'");
            _replacerPairs.Add("…", "...");
            _replacerPairs.Add("$", "!!!DOLLARSIGN!!!");
            _replacerPairs.Add("\\", "$\\backslash$");
            _replacerPairs.Add("#", "\\#");
            _replacerPairs.Add("{", "\\{");
            _replacerPairs.Add("}", "\\}");
            _replacerPairs.Add("[", "$[$");
            _replacerPairs.Add("]", "$]$");
            _replacerPairs.Add("%", "\\%");
            _replacerPairs.Add("&", "\\&");
            _replacerPairs.Add("~", "\\~");
            _replacerPairs.Add("_", "\\_");
            _replacerPairs.Add("^", "\\^{}");
            _replacerPairs.Add("–", "-");
            _replacerPairs.Add("—", "-");
            _replacerPairs.Add("“", "\"\\,");
            _replacerPairs.Add("„", "\"\\,");
            _replacerPairs.Add("”", "\"");
            _replacerPairs.Add("<", "$<$");
            _replacerPairs.Add(">", "$>$");
        }                      
        #endregion

        #region Referencing

        public string ResolveBookmarkRef(string reference)
        {
            try
            {
                Regex refRegEx = new Regex(" REF (?<Ref>.+?) ", RegexOptions.Compiled);
                Match match = refRegEx.Match(reference);
                return match.Groups["Ref"].Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region TeXize

        public string TeXizeText(string original)
        {
            string replaced = original;
            foreach (KeyValuePair<string, string> replacerPair in _replacerPairs)
            {
                replaced = replaced.Replace(replacerPair.Key, replacerPair.Value);
            }
            return replaced;
        }

        public string VerbatimizeText(string original)
        {
            string newText = original.Replace("’", "'").Replace("“", "\"").Replace("”", "\"");
            newText = newText.Replace("…", "...");

            return newText;
        }                    

        #endregion
    }
}
