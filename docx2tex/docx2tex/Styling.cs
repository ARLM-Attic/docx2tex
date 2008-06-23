using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace docx2tex
{
    class Styling
    {
        #region Fields
        
        private static Dictionary<string, string> _paraStylePairs;
        private static Dictionary<string, string> _runStylePairs;
        private string _inputDocxPath;

        #endregion    
        
        #region Lifecycle methods

        public Styling(string inputDocxPath)
        {
            _inputDocxPath = inputDocxPath;
            InitStylePairs();
        }

        #endregion

        #region Public methods

        public string ResolveParaStyle(string styleName)
        {
            if (_paraStylePairs.ContainsKey(styleName))
            {
                return _paraStylePairs[styleName];
            }
            return styleName;
        }

        public string ResolveRunStyle(string styleName)
        {
            if (_runStylePairs.ContainsKey(styleName))
            {
                return _runStylePairs[styleName];
            }
            return styleName;
        }

        #endregion

        #region Initialization

        private void InitStylePairs()
        {
            _paraStylePairs = new Dictionary<string, string>();
            _paraStylePairs.Add("section", "Heading1");
            _paraStylePairs.Add("subsection", "Heading2");
            _paraStylePairs.Add("subsubsection", "Heading3");
            _paraStylePairs.Add("verbatim", "Verbatim");

            string paraStylePairFile = Path.ChangeExtension(_inputDocxPath, "paraStylePairs");
            if (File.Exists(paraStylePairFile))
            {
                LoadStylePairFile(paraStylePairFile, _paraStylePairs);
            }

            _runStylePairs = new Dictionary<string, string>();
            _runStylePairs.Add("section", "Heading1");
            _runStylePairs.Add("subsection", "Heading2");
            _runStylePairs.Add("subsubsection", "Heading3");
            _runStylePairs.Add("verbatim", "Verbatim");

            string runStylePairFile = Path.ChangeExtension(_inputDocxPath, "runStylePairs");
            if (File.Exists(runStylePairFile))
            {
                LoadStylePairFile(runStylePairFile, _runStylePairs);
            }
        }

        private static void LoadStylePairFile(string stylePairFile, Dictionary<string, string> stylePairs)
        {
            using (StreamReader sr = new StreamReader(stylePairFile))
            {
                Regex stylePairRegex = new Regex("^(?<Latex>.+?)\\=(?<Custom>.+)$", RegexOptions.Compiled);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    Match match = stylePairRegex.Match(line);
                    if (match != null)
                    {
                        string latex = match.Groups["Latex"].Value;
                        string custom = match.Groups["Custom"].Value;
                        stylePairs[latex] = custom;
                    }
                }
            }
        }

        #endregion

        public string Enum2TextStart(StyleEnum styleEnum)
        {
            switch (styleEnum)
            {
                case StyleEnum.TextIt:
                    return @"\textit{";
                case StyleEnum.TextBf:
                    return @"\textbf{";
                case StyleEnum.Underline:
                    return @"\underline{";
                case StyleEnum.Sout:
                    return @"\sout{";
                case StyleEnum.Xout:
                    return @"\xout{";
                case StyleEnum.TextSc:
                    return @"\textsc{";
                case StyleEnum.TextC:
                    return @"\textsc{";
                case StyleEnum.SuperScript:
                    return @"$^{";
                case StyleEnum.SubScript:
                    return @"$_{";
            }
            return string.Empty;
        }


        public string Enum2TextEnd(StyleEnum styleEnum)
        {
            switch (styleEnum)
            {
                case StyleEnum.TextIt:
                    return @"}";
                case StyleEnum.TextBf:
                    return @"}";
                case StyleEnum.Underline:
                    return @"}";
                case StyleEnum.Sout:
                    return @"}";
                case StyleEnum.Xout:
                    return @"}";
                case StyleEnum.TextSc:
                    return @"}";
                case StyleEnum.TextC:
                    return @"}";
                case StyleEnum.SuperScript:
                    return @"}$";
                case StyleEnum.SubScript:
                    return @"}$";
            }
            return string.Empty;
        }
    }

    enum StyleEnum
    {
        TextIt,
        TextBf,
        Underline,
        Sout,
        Xout,
        TextSc,
        TextC,
        SuperScript,
        SubScript
    }
}
