using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace docx2tex.Library
{
    partial class Engine
    {

        /// <summary>
        /// Process a paragraph
        /// </summary>
        /// <param name="paragraphNode"></param>
        /// <param name="prevNode"></param>
        /// <param name="nextNode"></param>
        private void ProcessParagraph(XmlNode paragraphNode, XmlNode prevNode, XmlNode nextNode, bool inTable, bool drawNewLine)
        {
            // list settings of the current paragraph
            int? currentNumId = GetInt(paragraphNode, "./w:pPr/w:numPr/w:numId/@w:val");
            int? currentLevel = GetInt(paragraphNode, "./w:pPr/w:numPr/w:ilvl/@w:val");
            bool isList = currentNumId.HasValue && currentLevel.HasValue;
            ListTypeEnum currentType = _numberingFn.GetNumberingStyle(currentNumId, currentLevel);

            // list settings of the previous paragraph
            int? prevNumId = GetInt(prevNode, "./w:pPr/w:numPr/w:numId/@w:val");
            int? prevLevel = GetInt(prevNode, "./w:pPr/w:numPr/w:ilvl/@w:val");
            ListTypeEnum prevType = _numberingFn.GetNumberingStyle(prevNumId, prevLevel);

            // list settings of the next paragraph
            int? nextNumId = GetInt(nextNode, "./w:pPr/w:numPr/w:numId/@w:val");
            int? nextLevel = GetInt(nextNode, "./w:pPr/w:numPr/w:ilvl/@w:val");
            ListTypeEnum nextType = _numberingFn.GetNumberingStyle(nextNumId, nextLevel);

            // if it is a list
            if (isList)
            {
                ListControl listBegin = _numberingFn.ProcessBeforeListItem(currentNumId.Value, currentLevel.Value, currentType, prevNumId, prevLevel, nextNumId, nextLevel);

                // some numbered
                if (listBegin.ListType == ListTypeEnum.Numbered)
                {
                    switch (listBegin.NumberedCounterType)
                    {
                        // simple numbered begins
                        case NumberedCounterTypeEnum.None:
                            _tex.AddTextNL(@"\begin{enumerate}");
                            break;
                        // a new numbered begins
                        case NumberedCounterTypeEnum.NewCounter:
                            _tex.AddTextNL(@"\newcounter{numberedCnt" + listBegin.Numbering + "}");
                            _tex.AddTextNL(@"\begin{enumerate}");
                            break;
                        // a numbered loaded
                        case NumberedCounterTypeEnum.LoadCounter:
                            _tex.AddTextNL(@"\begin{enumerate}");
                            _tex.AddTextNL(@"\setcounter{enumi}{\thenumberedCnt" + listBegin.Numbering + "}");
                            break;
                    }
                }
                else if (listBegin.ListType == ListTypeEnum.Bulleted)
                {
                    // bulleted list begins
                    _tex.AddTextNL(@"\begin{itemize}");
                }

                //list item
                _tex.AddText(@"\item ");
            }

            // this will process the real content of the paragraph
            ProcessParagraphContent(paragraphNode, prevNode, nextNode, drawNewLine&true, inTable|false, isList);

            // in case of list
            if (isList)
            {
                List<ListControl> listEnd = _numberingFn.ProcessAfterListItem(currentNumId.Value, currentLevel.Value, currentType, prevNumId, prevLevel, nextNumId, nextLevel);

                // rollback the ended lists
                foreach (var token in listEnd)
                {
                    // if a numbered list found
                    if (token.ListType == ListTypeEnum.Numbered)
                    {
                        // save counter of next use
                        if (token.NumberedCounterType == NumberedCounterTypeEnum.SaveCounter)
                        {
                            _tex.AddTextNL("\\setcounter{numberedCnt" + token.Numbering + "}{\\theenumi}");
                        }
                        _tex.AddTextNL(@"\end{enumerate}");
                    }
                    else if (token.ListType == ListTypeEnum.Bulleted)
                    {
                        // bulleted ended
                        _tex.AddTextNL(@"\end{itemize}");
                    }
                }
            }
        }

        /// <summary>
        /// Process the paragraph's real content
        /// </summary>
        /// <param name="paraNode"></param>
        /// <param name="prevNode"></param>
        /// <param name="nextNode"></param>
        /// <param name="drawNewLine"></param>
        /// <param name="inTable"></param>
        /// <param name="isList"></param>
        private void ProcessParagraphContent(XmlNode paraNode, XmlNode prevNode, XmlNode nextNode, bool drawNewLine, bool inTable, bool isList)
        {
            string paraStyle = GetLowerString(paraNode, @"./w:pPr/w:pStyle/@w:val");

            // if a heading found then process it
            if (paraStyle == _stylingFn.ResolveParaStyle("section") ||
                paraStyle == _stylingFn.ResolveParaStyle("subsection") ||
                paraStyle == _stylingFn.ResolveParaStyle("subsubsection"))
            {
                // put sections
                if (paraStyle == _stylingFn.ResolveParaStyle("section"))
                {
                    _tex.AddText(Config.Instance.LaTeXTags.Section + "{");
                }
                else if (paraStyle == _stylingFn.ResolveParaStyle("subsection"))
                {
                    _tex.AddText(Config.Instance.LaTeXTags.SubSection + "{");
                }
                else if (paraStyle == _stylingFn.ResolveParaStyle("subsubsection"))
                {
                    _tex.AddText(Config.Instance.LaTeXTags.SubSubSection + "{");
                }

                // put text
                ParagraphRuns(paraNode, false, false);
                _tex.AddText("}");

                // put the reference name
                if (CountNodes(paraNode, "w:bookmarkStart") > 0)
                {
                    _tex.AddText(@"\label{section:" + GetString(paraNode, "./w:bookmarkStart/@w:name") + "}");
                }
                _tex.AddNL();
            }
            else if (paraStyle == _stylingFn.ResolveParaStyle("verbatim"))
            {
                // if verbatim node found

                string prevParaStyle = GetLowerString(prevNode, "./w:pPr/w:pStyle/@w:val");
                string nextParaStyle = GetLowerString(nextNode, "./w:pPr/w:pStyle/@w:val");

                // the previous was also verbatim
                bool wasVerbatim = prevParaStyle == _stylingFn.ResolveParaStyle("verbatim");
                // the next will be also verbatim
                bool willVerbatim = nextParaStyle == _stylingFn.ResolveParaStyle("verbatim");

                // the first verbatim is begining
                if (!wasVerbatim)
                {
                    _tex.AddText(Config.Instance.LaTeXTags.BeginVerbatim);
                }
                _tex.AddNL();
                // content
                ParagraphRuns(paraNode, false, true);
                // the last verbatim ends
                if (!willVerbatim)
                {
                    _tex.AddNL();
                    _tex.AddTextNL(Config.Instance.LaTeXTags.EndVerbatim);
                }
            }
            else if (CountNodes(paraNode, @"./w:fldSimple[starts-with(@w:instr, ' SEQ ')]") > 0)
            {
                // a caption text here
                ListingCaptionRun(paraNode);
                if (drawNewLine)
                {
                    _tex.AddNL();
                }
            }
            else
            {
                // draw NORMAL paragraph runs
                ParagraphRuns(paraNode, inTable, false);

                if (drawNewLine)
                {
                    _tex.AddNL();
                    if (!isList)
                    {
                        _tex.AddNL();
                    }
                }
            }
        }
    }
}
