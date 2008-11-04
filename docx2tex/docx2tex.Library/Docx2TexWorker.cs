using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Packaging;

namespace docx2tex.Library
{
    public class Docx2TexWorker
    {
        public void Process(string inputDocxPath, string outputTexPath, IStatusInformation statusInfo)
        {
            string documentPath = Path.GetDirectoryName(outputTexPath);
            if (documentPath == null)
            {
                documentPath = Path.GetPathRoot(outputTexPath);
            }

            EnsureMediaPath(documentPath);

            Package pkg = null;
            try
            {
                pkg = Package.Open(inputDocxPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                // this happens mostly when the user leaves the Word file open
                statusInfo.WriteLine(ex.Message);
                return;
            }

            ZipPackagePart documentPart = (ZipPackagePart)pkg.GetPart(new Uri("/word/document.xml", UriKind.Relative));

            //numbering part may not exist for simple documents
            ZipPackagePart numberingPart = null;
            if (pkg.PartExists(new Uri("/word/numbering.xml", UriKind.Relative)))
            {
                numberingPart = (ZipPackagePart)pkg.GetPart(new Uri("/word/numbering.xml", UriKind.Relative));
            }

            Numbering numbering = new Numbering(numberingPart);
            Imaging imaging = new Imaging(documentPart, inputDocxPath, outputTexPath);

            using (Stream documentXmlStream = documentPart.GetStream())
            {
                Engine engine = new Engine(documentXmlStream, numbering, imaging, statusInfo);
                string outputString = engine.Process();
                string latexSource = ReplaceSomeCharacters(outputString);

                using (StreamWriter outputTexStream = new StreamWriter(outputTexPath, false, Encoding.Default))
                {
                    outputTexStream.Write(latexSource);
                }
            }
            pkg.Close();
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
