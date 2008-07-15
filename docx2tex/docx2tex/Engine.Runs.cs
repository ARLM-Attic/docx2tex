using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace docx2tex
{
    partial class Engine
    {
        /// <summary>
        /// Process normal paragraph runs
        /// </summary>
        /// <param name="paraNode"></param>
        /// <param name="inTable"></param>
        /// <param name="inVerbatim"></param>
        private void ParagraphRuns(XmlNode paraNode, bool inTable, bool inVerbatim)
        {
            string lastFieldCommand = "none";

            // apply paragraph level styling for standard paragraphs
            if (!inTable && !inVerbatim)
            {
                TextParaStyleStart(GetNode(paraNode, "./w:pPr"));
            }

            // process all runs
            foreach (XmlNode run in GetNodes(paraNode, "./w:r|./m:oMathPara|./m:oMath|./w:smartTag|./w:hyperlink"))
            {
                // normal runs
                if (run.Name == "w:r")
                {
                    ProcessSingleRun(inVerbatim, ref lastFieldCommand, run);
                }
                else if(run.Name == "w:smartTag" || run.Name == "w:hyperlink")
                {   // when runs are under smartTags or hyperlinks
                    foreach (XmlNode stRun in GetNodes(run, ".//w:r"))
                    {
                        ProcessSingleRun(inVerbatim, ref lastFieldCommand, stRun);
                    }
                }
                // math paragraph
                else if (run.Name == "m:oMathPara")
                {
                    //math content
                    ProcessMath(GetNode(run, "./m:oMath"));
                    _tex.AddTextNL(@"\\");
                }
                // math content
                else if (run.Name == "m:oMath")
                {
                    ProcessMath(run);
                }
            }
            // apply style end for standard paragraphs
            if (!inTable && !inVerbatim)
            {
                TextParaStyleEnd(GetNode(paraNode, "./w:pPr"));
            }
        }

        private void ProcessSingleRun(bool inVerbatim, ref string lastFieldCommand, XmlNode run)
        {
            // if it is not verbatim then process breaks and styles
            if (!inVerbatim)
            {
                if (CountNodes(run, "./w:br") > 0)
                {
                    // page break
                    if (GetString(run, @"./w:br/@w:type") == "page")
                    {
                        _tex.AddTextNL(@"\newpage");
                    }
                    else
                    {
                        // line break
                        _tex.AddTextNL(@"\\");
                    }
                }
                // tab
                if (CountNodes(run, "./w:tab") > 0)
                {
                    _tex.AddText(@"\ \ \ \ ");
                }
                // apply run level style
                TextRunStyleStart(GetNode(run, "./w:rPr"));
            }
            else
            {
                // for verbatims put a simple newline
                if (CountNodes(run, "./w:br") > 0)
                {
                    _tex.AddNL();
                }
            }

            string tmp = GetString(run, @"./w:fldChar[@w:fldCharType='begin' or @w:fldCharType='end']/@w:fldCharType");
            // store the last crossref field command (begin or end)
            if (!string.IsNullOrEmpty(tmp))
            {
                lastFieldCommand = tmp;
            }

            // if we are in a crossref
            bool areWeInCrossReferenceField = lastFieldCommand == "begin";
            // if we are in some embedded content
            bool areWeInEmbeddedContent = GetNode(run, "./w:drawing") != null ||
                GetNode(run, "./w:object") != null ||
                GetNode(run, "./w:pict") != null;
            string currentBookmarkName = GetString(run, "./w:instrText");

            // process as normal run
            if (!areWeInCrossReferenceField && !areWeInEmbeddedContent)
            {
                TextRun(GetString(run, "./w:t"), inVerbatim);
            }
            else if (!areWeInCrossReferenceField && areWeInEmbeddedContent)
            {
                // if it is an embedded content

                if (GetNode(run, "./w:drawing") != null)
                {
                    // image
                    ProcessDrawing(GetNode(run, "./w:drawing"));
                }
                else if (GetNode(run, "./w:object") != null)
                {
                    // image
                    ProcessObject(GetNode(run, "./w:object"));
                }
                else if (GetNode(run, "./w:pict") != null)
                {
                    // textbox
                    ProcessPict(GetNode(run, "./w:pict"));
                }
            }
            else if (areWeInCrossReferenceField && !string.IsNullOrEmpty(currentBookmarkName))
            {
                // process cross references
                ProcessReference(currentBookmarkName);
            }

            // apply styles if not verbatim
            if (!inVerbatim)
            {
                TextRunStyleEnd(GetNode(run, "./w:rPr"));
            }
        }

        /// <summary>
        /// Process text run
        /// </summary>
        /// <param name="t"></param>
        /// <param name="inVerbatim"></param>
        private void TextRun(string t, bool inVerbatim)
        {
            if (t == null)
                return;

            // normal
            if (!inVerbatim)
            {
                _tex.AddText(_texingFn.TeXizeText(t));
            }
            else
            {
                // verbatim
                _tex.AddVerbatim(_texingFn.VerbatimizeText(t));
            }
        }
    }
}
