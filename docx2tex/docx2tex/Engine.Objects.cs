﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace docx2tex
{
    partial class Engine
    {
        /// <summary>
        /// Add an image
        /// </summary>
        /// <param name="xmlNode"></param>
        private void ProcessDrawing(XmlNode xmlNode)
        {
            XmlNode blipNode = GetNode(xmlNode, @"./wp:inline/a:graphic/a:graphicData/pic:pic/pic:blipFill/a:blip");

            // put as figure
            _tex.AddTextNL(@"\begin{figure}[h]");
            _tex.AddTextNL(@"\centering");

            // apply width and height
            XmlNode extentNode = GetNode(blipNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode, "./wp:extent");
            string widthHeightStr = _imagingFn.GetWidthAndHeightFromStyle(GetInt(extentNode, "@cx"), GetInt(extentNode, "@cy"));
            _tex.AddText(@"\includegraphics[" + widthHeightStr + "]{");
            // convert and resolve new image path
            _tex.AddTextNL(_imagingFn.ResolveImage(GetString(blipNode, "@r:embed")) + "}");

            XmlNode captionP = blipNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.NextSibling;
            // add caption
            ImageCaption(captionP);
            _tex.AddTextNL(@"\end{figure}");
        }

        /// <summary>
        /// Add an image
        /// </summary>
        /// <param name="xmlNode"></param>
        private void ProcessObject(XmlNode xmlNode)
        {
            XmlNode imageData = GetNode(xmlNode, "./v:shape/v:imagedata");

            if (imageData != null)
            {
                // put as figure
                _tex.AddTextNL(@"\begin{figure}[h]");
                _tex.AddTextNL(@"\centering");

                // apply width and height
                string widthHeightStr = _imagingFn.GetWidthAndHeightFromStyle(GetString(imageData.ParentNode, "@style"));
                _tex.AddText(@"\includegraphics[" + widthHeightStr + "]{");
                // convert and resolve new image path
                _tex.AddTextNL(_imagingFn.ResolveImage(GetString(imageData, "@r:id")) + "}");

                XmlNode captionP = imageData.ParentNode.ParentNode.ParentNode.ParentNode.NextSibling;
                // add caption
                ImageCaption(captionP);
                _tex.AddTextNL(@"\end{figure}");
            }
        }

        /// <summary>
        /// Process textboxes
        /// </summary>
        /// <param name="xmlNode"></param>
        private void ProcessPict(XmlNode xmlNode)
        {
            // loop through all textbox contents and process them as normal content

            // if or if not grouped
            foreach (XmlNode txbxs in GetNodes(xmlNode, ".//v:shape/v:textbox/w:txbxContent"))
            {
                BulkMainProcessor(txbxs);
            }
        }

        /// <summary>
        /// process a set of paragraphs
        /// </summary>
        /// <param name="par"></param>
        private void BulkMainProcessor(XmlNode par)
        {
            foreach (XmlNode paragraphNode in GetNodes(par, "./*"))
            {
                MainProcessor(paragraphNode);
            }
        }
    }
}
