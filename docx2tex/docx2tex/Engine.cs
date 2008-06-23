﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace docx2tex
{
    partial class Engine
    {
        private XmlDocument _doc;
        private Store _tex;
        private XmlNamespaceManager _nsmgr;
        private Numbering _numberingFn;
        private Styling _stylingFn;
        private Imaging _imagingFn;
        private TeXing _texingFn;

        /// <summary>
        /// Setup helpers and namespaces
        /// </summary>
        /// <param name="documentXmlStream"></param>
        /// <param name="dotnetFn"></param>
        public Engine(Stream documentXmlStream, Numbering numberingFn, Styling stylingFn, Imaging imagingFn, TeXing texingFn)
        {
            _doc = new XmlDocument();
            _doc.Load(documentXmlStream);
            _tex = new Store(stylingFn);

            _nsmgr = new XmlNamespaceManager(_doc.NameTable);
            _nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            _nsmgr.AddNamespace("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
            _nsmgr.AddNamespace("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
            _nsmgr.AddNamespace("pic", "http://schemas.openxmlformats.org/drawingml/2006/picture");
            _nsmgr.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            _nsmgr.AddNamespace("v", "urn:schemas-microsoft-com:vml");
        
            _numberingFn = numberingFn;
            _stylingFn = stylingFn;
            _imagingFn = imagingFn;
            _texingFn = texingFn;
        }

        /// <summary>
        /// Entry method
        /// </summary>
        /// <returns></returns>
        public string Process()
        {
            Header();

            foreach (XmlNode paragraphNode in GetNodes(_doc, "/w:document/w:body/*"))
            {
                MainProcessor(paragraphNode);
            }

            Footer();

            return _tex.ToString();
        }

        /// <summary>
        /// Processes a paragraph or a table
        /// </summary>
        /// <param name="paragraphNode"></param>
        private void MainProcessor(XmlNode paragraphNode)
        {
            if (paragraphNode.Name == "w:p")
            {
                ProcessParagraph(paragraphNode, paragraphNode.PreviousSibling, paragraphNode.NextSibling);
            }
            else if (paragraphNode.Name == "w:tbl")
            {
                ProcessTable(paragraphNode);
            }
        }

        /// <summary>
        /// Header
        /// </summary>
        private void Header()
        {
            _tex.AddTextNL(@"\documentclass[12pt, a4paper]{article}");
            _tex.AddTextNL(@"\usepackage[latin2]{inputenc}");
            _tex.AddTextNL(@"\usepackage{graphicx}");
            _tex.AddTextNL(@"\usepackage{ulem}");
            _tex.AddTextNL(@"\begin{document}");
        }

        /// <summary>
        /// Footer
        /// </summary>
        private void Footer()
        {
            _tex.AddTextNL(@"\end{document}");
        }
    }
}
