using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace docx2tex
{
    partial class Engine
    {
        /// <summary>
        /// Text level styling starts
        /// </summary>
        /// <param name="xmlNode"></param>
        private void TextRunStyleStart(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            foreach (XmlNode sty in GetNodes(xmlNode, "./*"))
            {
                switch (sty.Name)
                {
                    case "w:i":
                        _tex.AddStartStyle(StyleEnum.TextIt);
                        break;
                    case "w:b":
                        _tex.AddStartStyle(StyleEnum.TextBf);
                        break;
                    case "w:u":
                        _tex.AddStartStyle(StyleEnum.Underline);
                        break;
                    case "w:strike":
                        _tex.AddStartStyle(StyleEnum.Sout);
                        break;
                    case "w:dstrike":
                        _tex.AddStartStyle(StyleEnum.Xout);
                        break;
                    case "w:smallCaps":
                        _tex.AddStartStyle(StyleEnum.TextSc);
                        break;
                    case "w:caps":
                        _tex.AddStartStyle(StyleEnum.TextC);
                        break;
                }
            }

            foreach (XmlNode sty in GetNodes(xmlNode, "./*"))
            {
                if (sty.Name == "w:vertAlign" && GetString(sty, "./@w:val") == "superscript")
                {
                    _tex.AddStartStyle(StyleEnum.SuperScript);
                }
                else if (sty.Name == "w:vertAlign" && GetString(sty, "./@w:val") == "subscript")
                {
                    _tex.AddStartStyle(StyleEnum.SubScript);
                }
            }
        }

        /// <summary>
        /// Text level styling ends
        /// </summary>
        /// <param name="xmlNode"></param>
        private void TextRunStyleEnd(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            foreach (XmlNode sty in GetNodes(xmlNode, "./*"))
            {
                if (sty.Name == "w:vertAlign" && GetString(sty, "./@w:val") == "superscript")
                {
                    _tex.AddEndStyle(StyleEnum.SuperScript);
                }
                else if(sty.Name == "w:vertAlign" && GetString(sty, "./@w:val") == "subscript")
                {
                    _tex.AddEndStyle(StyleEnum.SubScript);
                }
            }

            foreach (XmlNode sty in GetNodes(xmlNode, "./*"))
            {
                switch (sty.Name)
                {
                    case "w:i":
                        _tex.AddEndStyle(StyleEnum.TextIt);
                        break;
                    case "w:b":
                        _tex.AddEndStyle(StyleEnum.TextBf);
                        break;
                    case "w:u":
                        _tex.AddEndStyle(StyleEnum.Underline);
                        break;
                    case "w:strike":
                        _tex.AddEndStyle(StyleEnum.Sout);
                        break;
                    case "w:dstrike":
                        _tex.AddEndStyle(StyleEnum.Xout);
                        break;
                    case "w:smallCaps":
                        _tex.AddEndStyle(StyleEnum.TextSc);
                        break;
                    case "w:caps":
                        _tex.AddEndStyle(StyleEnum.TextC);
                        break;
                }
            }
        }

        /// <summary>
        /// Paragraph level styling starts
        /// </summary>
        /// <param name="xmlNode"></param>
        private void TextParaStyleStart(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            foreach (XmlNode chld in GetNodes(xmlNode, "./*"))
            {
                if (chld.Name == "w:jc")
                {
                    string align = GetString(chld, "@w:val");
                    if (align == "right")
                    {
                        _tex.AddText(@"\begin{flushright}");
                    }
                    else if (align == "center")
                    {
                        _tex.AddText(@"\begin{center}");
                    }
                }
            }
        }

        /// <summary>
        /// Paragraph level styling ends
        /// </summary>
        /// <param name="xmlNode"></param>
        private void TextParaStyleEnd(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            foreach (XmlNode chld in GetNodes(xmlNode, "./*"))
            {
                if (chld.Name == "w:jc")
                {
                    string align = GetString(chld, "@w:val");
                    if (align == "right")
                    {
                        _tex.AddText(@"\end{flushright}");
                    }
                    else if (align == "center")
                    {
                        _tex.AddText(@"\end{center}");
                    }
                }
            }
        }
    }
}
