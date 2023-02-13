using EXIFPhotosAnalyzer.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFPhotosAnalyzer.Lib.Services
{
    public interface IExifService
    {
        IAsyncEnumerable<ExifInfoProgress> GetExifValuesForDirectory(string directoryPath);
        Task<MemoryStream> GetCsvFromAnalyzedFolder(IEnumerable<ExifInfo>? analyzedFolderData);
    }
}
