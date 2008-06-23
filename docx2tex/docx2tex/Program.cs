using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO.Packaging;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace docx2tex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("docx2tex was created by Pocza, Krisztian in 2007, 2008 under the terms of BSD licence");
            Console.WriteLine("info: kpocza@kpocza.net");
            if (args.Length < 2)
            {
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("docx2tex.exe source.docx dest.tex");
                return;
            }
            
            string inputDocxPath = ResolveFullPath(args[0]);
            string outputTexPath = ResolveFullPath(args[1]);
            
            string documentPath = Path.GetDirectoryName(outputTexPath);

            EnsureMediaPath(documentPath);

            using (Package pkg = Package.Open(inputDocxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ZipPackagePart documentPart = (ZipPackagePart)pkg.GetPart(new Uri("/word/document.xml", UriKind.Relative));
                ZipPackagePart numberingPart = (ZipPackagePart)pkg.GetPart(new Uri("/word/numbering.xml", UriKind.Relative));

                Numbering numbering = new Numbering(numberingPart);
                Styling styling = new Styling(inputDocxPath);
                Imaging imaging = new Imaging(documentPart, inputDocxPath, outputTexPath);
                TeXing texing = new TeXing();

                using (Stream documentXmlStream = documentPart.GetStream())
                {
                    Engine engine = new Engine(documentXmlStream, numbering, styling, imaging, texing);
                    string outputString = engine.Process();
                    string latexSource = ReplaceSomeCharacters(outputString);

                    using (StreamWriter outputTexStream = new StreamWriter(outputTexPath, false, Encoding.Default))
                    {
                        outputTexStream.Write(latexSource);
                    }
                }
            }

            using (StreamReader sr = new StreamReader(outputTexPath))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }

        private static string ResolveFullPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            return Path.Combine(Environment.CurrentDirectory, path);
        }

        private static string ReplaceSomeCharacters(string latexSource)
        {
            latexSource = latexSource.Replace("!!!DOLLARSIGN!!!", "\\$");
            return latexSource;
        }

        private static void EnsureMediaPath(string documentPath)
        {
            string mediaPath = Path.Combine(documentPath, "media");
            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }
        }
    }
}
