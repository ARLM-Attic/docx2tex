using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Configuration;
using System.Diagnostics;
using docx2tex.Library;

namespace docx2tex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("docx2tex was created by Krisztian Pocza in 2007-2008 under the terms of BSD licence");
            Console.WriteLine("info: kpocza@kpocza.net");
            Console.WriteLine();
            //Console.ReadLine();
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("docx2tex.exe source.docx dest.tex");
                return;
            }

            int ll = 72;
            try
            {
                int.TryParse(ConfigurationManager.AppSettings["LineLength"], out ll);
            }
            finally
            {
                Config.Instance.LineLength = ll;
            }

            Config.Instance.ImageMagickPath = ConfigurationManager.AppSettings["ImageMagick"];

            string inputDocxPath = ResolveFullPath(args[0]);
            string outputTexPath = ResolveFullPath(args[1]);

            new Docx2TexWorker().Process(inputDocxPath, outputTexPath);

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
    }
}
