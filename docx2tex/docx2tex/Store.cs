using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace docx2tex
{
    class Store
    {
        #region Constants
        
        public static readonly int LINELENGTH = 72;

        #endregion

        #region Fields

        private Styling _stylingFn;
        private List<Run> _runs;

        #endregion

        #region Lifecycle methods

        static Store()
        {
            int ll = 0;
            if (int.TryParse(ConfigurationManager.AppSettings["LineLength"], out ll))
            {
                LINELENGTH = ll;
            }
        }

        public Store(Styling stylingFn)
        {
            _stylingFn = stylingFn;
            _runs = new List<Run>();
        }

        #endregion

        #region Add Runs

        public void AddText(string text)
        {
            _runs.Add(new TextRun(text));
        }

        public void AddVerbatim(string text)
        {
            _runs.Add(new VerbatimRun(text));
        }

        public void AddNL()
        {
            _runs.Add(new NewLineRun());
        }

        public void AddTextNL(string text)
        {
            AddText(text);
            AddNL();
        }

        public void AddStartStyle(StyleEnum styleEnum)
        {
            _runs.Add(new StyleStartRun(styleEnum, _stylingFn));
        }

        public void AddEndStyle(StyleEnum styleEnum)
        {
            _runs.Add(new StyleEndRun(styleEnum, _stylingFn));
        }

        #endregion

        #region Convert to "TeXString"

		public override string ToString()
        {
            var originalRuns = _runs;
            List<Run> simplifiedRuns;
            // simplify the runs as long as they can be simplified
            while (Simplify(out simplifiedRuns, originalRuns))
            {
                originalRuns = simplifiedRuns;
            }

            // split the line lengths
            return CorrectLineLengths(simplifiedRuns);
        }

        #endregion

        #region Helper : Simplify

        private bool Simplify(out List<Run> simplifiedRuns, List<Run> originalRuns)
        {
            simplifiedRuns = new List<Run>();

            bool didSimplify = false;
            StyleEndRun lastStyleEndRun = null;

            var runEnum = originalRuns.GetEnumerator();
            while (runEnum.MoveNext())
            {
                Run run = runEnum.Current;
                // if newline or text
                if (run is NewLineRun || run is TextRun || run is VerbatimRun)
                {
                    // if a style ending run found then flush it
                    if (lastStyleEndRun != null)
                    {
                        simplifiedRuns.Add(lastStyleEndRun);
                        lastStyleEndRun = null;
                    }
                    // add run
                    simplifiedRuns.Add(run);
                }
                else if (run is StyleStartRun) // style start run
                {
                    // if a style ending run found then process it
                    if (lastStyleEndRun != null)
                    {
                        // if the style of the end is not the same and the start
                        if (((StyleStartRun)run).Style != lastStyleEndRun.Style)
                        {
                            // flush style end
                            simplifiedRuns.Add(lastStyleEndRun);
                            // add start run
                            simplifiedRuns.Add(run);
                        }
                        else
                        {
                            didSimplify = true;
                        }
                        lastStyleEndRun = null;
                    }
                    else // no style ending run found
                    {
                        // add run
                        simplifiedRuns.Add(run);
                    }
                }
                else if (run is StyleEndRun)
                {
                    // if an other style end run found
                    if (lastStyleEndRun != null)
                    {
                        // flush it
                        simplifiedRuns.Add(lastStyleEndRun);
                        lastStyleEndRun = null;
                    }
                    // save the style end run
                    lastStyleEndRun = (StyleEndRun)run;
                }
            }
            // if an style end run found
            if (lastStyleEndRun != null)
            {
                // flush it
                simplifiedRuns.Add(lastStyleEndRun);
            }
            return didSimplify;
        }

	    #endregion

        #region Helper : CorrectLineLengths

        private string CorrectLineLengths(List<Run> simplifiedRuns)
        {
            StringBuilder sb = new StringBuilder();
            int lastLineLength = 0;

            foreach (Run r in simplifiedRuns)
            {
                if (r is VerbatimRun)
                {
                    sb.Append(r.TeXText);
                }
                else if (r is TextRun || r is StyleStartRun || r is StyleEndRun)
                {
                    string parts = r.TeXText;
                    lastLineLength = AddRun(sb, r.TeXText, lastLineLength);
                }
                else if (r is NewLineRun)
                {
                    sb.Append(r.TeXText);
                    lastLineLength = 0;
                }
            }
            return sb.ToString();
        }

        private int AddRun(StringBuilder sb, string parts, int lastLineLength)
        {
            StringBuilder broken = new StringBuilder();

            foreach (string part in parts.Split(' '))
            {
                if (lastLineLength + part.Length > LINELENGTH)
                {
                    broken.Append(Environment.NewLine);
                    lastLineLength = 0;
                }
                if (!string.IsNullOrEmpty(part))
                {
                    broken.Append(string.Format("{0} ", part));

                    lastLineLength += part.Length + 1;
                }
            }

            string res = broken.ToString();

            if (!parts.EndsWith(" "))
            {
                res = res.TrimEnd();
            }

            if (parts.StartsWith(" ") && !res.StartsWith(" "))
            {
                res = " " + res;
            }
            sb.Append(res);

            return lastLineLength;
        }

        #endregion
    }

    #region Runs

	abstract class Run
    {
        public abstract string TeXText { get; }
    }

    class TextRun : Run
    {
        public string Text { get; private set ; }

        public TextRun(string text)
        {
            this.Text = text;
        }

        public override string TeXText
        {
            get { return Text; }
        }
    }

    class VerbatimRun : Run
    {
        public string Text { get; private set; }

        public VerbatimRun(string text)
        {
            this.Text = text;
        }

        public override string TeXText
        {
            get { return Text; }
        }
    }

    class NewLineRun : Run
    {
        public override string TeXText
        {
            get { return Environment.NewLine; }
        }
    }

    abstract class StyleRun : Run
    {
        public StyleEnum Style { get; private set; }
        protected Styling _stylingFn;

        public StyleRun(StyleEnum style, Styling stylingFn)
        {
            this.Style = style;
            this._stylingFn = stylingFn;
        }
    }

    class StyleStartRun : StyleRun
    {
        public StyleStartRun(StyleEnum style, Styling stylingFn)
            : base(style, stylingFn)
        {
        }

        public override string TeXText
        {
            get { return _stylingFn.Enum2TextStart(this.Style); }
        }
    }

    class StyleEndRun : StyleRun
    {
        public StyleEndRun(StyleEnum style, Styling stylingFn)
            : base(style, stylingFn)
        {
        }

        public override string TeXText
        {
            get { return _stylingFn.Enum2TextEnd(this.Style); }
        }
    }

	#endregion
}
