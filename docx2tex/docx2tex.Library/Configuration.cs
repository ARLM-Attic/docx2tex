using System;
using System.Collections.Generic;
using System.Text;

namespace docx2tex.Library
{
    public class Config
    {
        private Config()
        {
        }

        public static readonly Config Instance = new Config();

        public string ImageMagickPath { get; set; }
        public int LineLength { get; set; }
    }
}
