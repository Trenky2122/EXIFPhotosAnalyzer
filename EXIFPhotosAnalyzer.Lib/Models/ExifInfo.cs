using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFPhotosAnalyzer.Lib.Models
{
    public class ExifInfo
    {
        public string? Path { get; set; }
        public double FocalLength { get; set; }
        public string CameraModel { get; set; }
    }
}
