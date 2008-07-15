using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace docx2tex
{
    partial class Engine
    {
        Dictionary<string, string> mathTable = new Dictionary<string, string>();

        private void InitMathTables()
        {
            mathTable.Add("±", @"\pm ");
            mathTable.Add("∞", @"\infty ");
            mathTable.Add("=", "=");
            mathTable.Add("≠", @"\ne ");
            mathTable.Add("~", @"\sim ");
            mathTable.Add("×", @"\times ");
            mathTable.Add("÷", @"\div ");
            mathTable.Add("!", "!");
            mathTable.Add("<", "<");
            mathTable.Add("≪", @"\ll ");
            mathTable.Add(">", ">");
            mathTable.Add("≫", @"\gg ");
            mathTable.Add("≤", @"\le ");
            mathTable.Add("≥", @"\ge ");
            mathTable.Add("∓", @"\mp ");
            mathTable.Add("≅", @"\cong ");
            mathTable.Add("≈", @"\approx ");
            mathTable.Add("≡", @"\equiv ");
            mathTable.Add("∀", "for all");
            mathTable.Add("∁", @"complement ");
            mathTable.Add("∂", "partialdiff");
            mathTable.Add("∪", "union");
            mathTable.Add("∩", "ints");
            mathTable.Add("∅", @"\emptyset ");
            mathTable.Add("%", @"\%");
            mathTable.Add("°", "deg");
            mathTable.Add("℉", "degF");
            mathTable.Add("℃", "degC");
            //mathTable.Add("∆", @"\triangleup ");
            mathTable.Add("∇", @"triangledown ");
            mathTable.Add("∃", @"\exists ");
            mathTable.Add("∄", @"nexists ");
            mathTable.Add("∋", @"\ni ");
            mathTable.Add("←", @"\gets ");
            mathTable.Add("↑", @"\uparrow ");
            mathTable.Add("→", @"\to ");
            mathTable.Add("↓", @"\downarrow ");
            mathTable.Add("↔", @"\leftrightarrow ");

            mathTable.Add("∴", "");
            mathTable.Add("*", "");
            mathTable.Add("∙", @"\cdot ");
            mathTable.Add("⋮", "");
            mathTable.Add("⋯", "");
            mathTable.Add("⋰", "");
            mathTable.Add("⋱", "");
            mathTable.Add("ℵ", "");
            mathTable.Add("ℶ", "");
            mathTable.Add("∎", "");

            mathTable.Add("α", @"\alpha ");
            mathTable.Add("β", @"\beta ");
            mathTable.Add("γ", @"\gamma ");
            mathTable.Add("δ", @"\delta ");
            mathTable.Add("ε", @"\epsilon ");
            mathTable.Add("ϵ", @"E");
            mathTable.Add("θ", @"\theta ");
            mathTable.Add("ϑ", @"\vartheta ");
            mathTable.Add("μ", @"\mu ");
            mathTable.Add("π", @"\pi ");
            mathTable.Add("ρ", @"\rho ");
            mathTable.Add("σ", @"\sigma ");
            mathTable.Add("τ", @"\tau ");
            mathTable.Add("φ", @"\phi ");
            mathTable.Add("ω", @"\omega ");

            mathTable.Add("λ", @"\lambda ");
            mathTable.Add("∆", @"\Delta ");
            mathTable.Add("Γ", @"\Gamma ");
        }

        private void ProcessMath(XmlNode mathNode)
        {
            // begin math
            _tex.AddText("$");

            ProcessMathNodes(GetNodes(mathNode, "./*"));

            // end math
            _tex.AddText("$");
        }

        private void ProcessMathNodes(XmlNodeList xmlNodeList)
        {
            foreach (XmlNode node in xmlNodeList)
            {
                // standard text
                if (node.Name == "m:r")
                {
                    string str = GetString(node, "./m:t");

                    string data = string.Empty;

                    foreach (var c in str)
                    {
                        string cs = c.ToString();
                        // special characters
                        if (mathTable.ContainsKey(cs))
                        {
                            data += mathTable[cs];
                        }
                        else
                        {
                            // normal characters
                            data += cs;
                        }
                    }
                    _tex.AddText(data);
                }
                // ( xxx )
                else if (node.Name == "m:d")
                {
                    string begChar = GetString(node, "./m:dPr/m:begChr/@m:val");
                    string endChar = GetString(node, "./m:dPr/m:endChr/@m:val");

                    if (begChar == "{")
                    {
                        _tex.AddText(@"\lbrace ");
                    }
                    else
                    {
                        _tex.AddText("(");
                    }

                    ProcessMathNodes(GetNodes(node, "./m:e/*"));

                    if (endChar == "}")
                    {
                        _tex.AddText(@"\rbrace ");
                    }
                    else
                    {
                        _tex.AddText(")");
                    }
                }
                // box
                else if (node.Name == "m:box")
                {
                    ProcessMathNodes(GetNodes(node, "./m:e/*"));
                }
                // frac
                else if (node.Name == "m:f")
                {
                    _tex.AddText(@"\frac{");
                    //numerator
                    ProcessMathNodes(GetNodes(node, "./m:num/*"));
                    _tex.AddText("}{");
                    //denominator
                    ProcessMathNodes(GetNodes(node, "./m:den/*"));
                    _tex.AddText("}");
                }
                else if (node.Name == "m:rad")
                {
                    _tex.AddText(@"\sqrt");
                    // if has child nodes
                    if (CountNodes(node, "./m:deg/*") > 0)
                    {
                        _tex.AddText("[");
                        ProcessMathNodes(GetNodes(node, "./m:deg/*"));
                        _tex.AddText("]");
                    }

                    // under deg
                    _tex.AddText("{");
                    ProcessMathNodes(GetNodes(node, "./m:e/*"));
                    _tex.AddText("}");
                }
                // big operators
                else if (node.Name == "m:nary")
                {
                    XmlNode naryPr = GetNode(node, "./m:naryPr");
                    string op = @"\int";
                    if (CountNodes(naryPr, "./m:chr") == 1)
                    {
                        string chr = GetString(naryPr, "./m:chr/@m:val");

                        switch (chr)
                        {
                            case "∑":
                                op = @"\sum";
                                break;
                            case "∏":
                                op = @"\prod";
                                break;
                            default:
                                op = "";
                                break;
                        }
                    }
                    _tex.AddText(op);

                    //subscript
                    if (CountNodes(node, "./m:sub/*") > 0)
                    {
                        _tex.AddText("_{");
                        ProcessMathNodes(GetNodes(node, "./m:sub/*"));
                        _tex.AddText("}");
                    }
                    //superscript
                    if (CountNodes(node, "./m:sup/*") > 0)
                    {
                        _tex.AddText("^{");
                        ProcessMathNodes(GetNodes(node, "./m:sup/*"));
                        _tex.AddText("}");
                    }

                    // main data
                    _tex.AddText("{");
                    ProcessMathNodes(GetNodes(node, "./m:e/*"));
                    _tex.AddText("}");
                }
                //superscript
                else if (node.Name == "m:sSup")
                {
                    // base
                    ProcessMathNodes(GetNodes(node, "./m:e/*"));

                    // sup
                    _tex.AddText("^{");
                    ProcessMathNodes(GetNodes(node, "./m:sup/*"));
                    _tex.AddText("}");
                }
                //subscript
                else if (node.Name == "m:sSub")
                {
                    // base
                    ProcessMathNodes(GetNodes(node, "./m:e/*"));

                    // sub
                    _tex.AddText("_{");
                    ProcessMathNodes(GetNodes(node, "./m:sub/*"));
                    _tex.AddText("}");
                }
                //superscript
                else if (node.Name == "m:sSubSup")
                {
                    // base
                    ProcessMathNodes(GetNodes(node, "./m:e/*"));

                    // sub
                    _tex.AddText("_{");
                    ProcessMathNodes(GetNodes(node, "./m:sub/*"));
                    _tex.AddText("}");
                    // sup
                    _tex.AddText("^{");
                    ProcessMathNodes(GetNodes(node, "./m:sup/*"));
                    _tex.AddText("}");
                }
            }
        }
    }
}
