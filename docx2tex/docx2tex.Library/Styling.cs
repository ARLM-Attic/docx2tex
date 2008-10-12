using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace docx2tex.Library
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
            if (_paraStylePairs.ContainsKey(styleName.ToLower()))
            {
                return _paraStylePairs[styleName];
            }
            return styleName;
        }

        public string ResolveRunStyle(string styleName)
        {
            if (_runStylePairs.ContainsKey(styleName.ToLower()))
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
            _paraStylePairs.Add("section", "heading1");
            _paraStylePairs.Add("subsection", "heading2");
            _paraStylePairs.Add("subsubsection", "heading3");
            _paraStylePairs.Add("verbatim", "verbatim");

            string paraStylePairFile = Path.ChangeExtension(_inputDocxPath, "paraStylePairs");
            if (File.Exists(paraStylePairFile))
            {
                LoadStylePairFile(paraStylePairFile, _paraStylePairs);
            }

            _runStylePairs = new Dictionary<string, string>();
            _runStylePairs.Add("section", "heading1");
            _runStylePairs.Add("subsection", "heading2");
            _runStylePairs.Add("subsubsection", "heading3");
            _runStylePairs.Add("verbatim", "verbatim");

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
                        string latex = match.Groups["Latex"].Value.ToLower();
                        string custom = match.Groups["Custom"].Value.ToLower();
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
                    return Config.Instance.LaTeXTags.StylePair.Begin.TextIt;
                case StyleEnum.TextBf:
                    return Config.Instance.LaTeXTags.StylePair.Begin.TextBf;
                case StyleEnum.Underline:
                    return Config.Instance.LaTeXTags.StylePair.Begin.Underline;
                case StyleEnum.Sout:
                    return Config.Instance.LaTeXTags.StylePair.Begin.Sout;
                case StyleEnum.Xout:
                    return Config.Instance.LaTeXTags.StylePair.Begin.Xout;
                case StyleEnum.TextSc:
                    return Config.Instance.LaTeXTags.StylePair.Begin.TextSc;
                case StyleEnum.TextC:
                    return Config.Instance.LaTeXTags.StylePair.Begin.TextC;
                case StyleEnum.SuperScript:
                    return Config.Instance.LaTeXTags.StylePair.Begin.SuperScript;
                case StyleEnum.SubScript:
                    return Config.Instance.LaTeXTags.StylePair.Begin.SubScript;
                case StyleEnum.ParaFlushRight:
                    return Config.Instance.LaTeXTags.StylePair.Begin.ParaFlushRight;
                case StyleEnum.ParaCenter:
                    return Config.Instance.LaTeXTags.StylePair.Begin.ParaCenter;
            }
            return string.Empty;
        }


        public string Enum2TextEnd(StyleEnum styleEnum)
        {
            switch (styleEnum)
            {
                case StyleEnum.TextIt:
                    return Config.Instance.LaTeXTags.StylePair.End.TextIt;
                case StyleEnum.TextBf:
                    return Config.Instance.LaTeXTags.StylePair.End.TextBf;
                case StyleEnum.Underline:
                    return Config.Instance.LaTeXTags.StylePair.End.Underline;
                case StyleEnum.Sout:
                    return Config.Instance.LaTeXTags.StylePair.End.Sout;
                case StyleEnum.Xout:
                    return Config.Instance.LaTeXTags.StylePair.End.Xout;
                case StyleEnum.TextSc:
                    return Config.Instance.LaTeXTags.StylePair.End.TextSc;
                case StyleEnum.TextC:
                    return Config.Instance.LaTeXTags.StylePair.End.TextC;
                case StyleEnum.SuperScript:
                    return Config.Instance.LaTeXTags.StylePair.End.SuperScript;
                case StyleEnum.SubScript:
                    return Config.Instance.LaTeXTags.StylePair.End.SubScript;
                case StyleEnum.ParaFlushRight:
                    return Config.Instance.LaTeXTags.StylePair.End.ParaFlushRight;
                case StyleEnum.ParaCenter:
                    return Config.Instance.LaTeXTags.StylePair.End.ParaCenter;
            }
            return string.Empty;
        }
    }

    enum StyleEnum
    {
        // run styles
        TextIt,
        TextBf,
        Underline,
        Sout,
        Xout,
        TextSc,
        TextC,
        SuperScript,
        SubScript,

        // paragraph styles
        ParaFlushRight,
        ParaCenter,
    }
}
