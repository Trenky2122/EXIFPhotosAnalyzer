using EXIFPhotosAnalyzer.Lib.Models;
using EXIFPhotosAnalyzer.UI.Models;

namespace EXIFPhotosAnalyzer.UI.Services
{
    public interface IExifAnalyzerService
    {
        Task<(List<ChartData> ChartData, List<ExifInfo> DetailedData)> AnalyzeFolder(string? path);
        Task<MemoryStream> GetCsvFromAnalyzedFolder(IEnumerable<ExifInfo>? analyzedFolderData);
    }
}
