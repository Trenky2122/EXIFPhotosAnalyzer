using CsvHelper;
using EXIFPhotosAnalyzer.Lib.Models;
using EXIFPhotosAnalyzer.Lib.Services;
using EXIFPhotosAnalyzer.UI.Hubs;
using EXIFPhotosAnalyzer.UI.Models;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;

namespace EXIFPhotosAnalyzer.UI.Services
{
    /// <summary>
    /// Processes data from logical services and transforms them for uses of UI
    /// </summary>
    public class ExifAnalyzerService: IExifAnalyzerService
    {
        private readonly IExifService _exifService;
        private readonly IHubContext<ProgressHub> _progressHub;
        public ExifAnalyzerService(IExifService exifService, IHubContext<ProgressHub> hubContext)
        {
            _exifService = exifService;
            _progressHub = hubContext;
        }

        /// <summary>
        /// Returns analyzed data and data for chart
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(List<ChartData> ChartData, List<ExifInfo> DetailedData)> AnalyzeFolder(string? path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var progressEnumeration = _exifService.GetExifValuesForDirectory(path);
            List<ExifInfo> analysisResult = new();
            await foreach (var progress in progressEnumeration)
            {
                analysisResult.Add(progress.ExifInfo);
                if (Convert.ToInt32(progress.Progress * 100) % 3 != 0) continue; // we do not want to send too much updates
                await _progressHub.Clients.All.SendAsync("UpdateProgress", progress);
                await Task.Delay(1); // give UI time to update
            }

            var result = analysisResult.GroupBy(exif => exif.FocalLength)
                .Select(group => new ChartData()
                {
                    ItemsCount = group.Count(),
                    Value = group.Key,
                    CameraModelsList = group.GroupBy(photo => photo.CameraModel)
                        .Select(gr => (gr.Key, gr.Count()))
                        .ToList()
                }).ToList();
            return (result, analysisResult);
        }

        /// <summary>
        /// Returns Memory stream with CSV
        /// </summary>
        /// <param name="analyzedFolderData"></param>
        /// <returns></returns>
        public Task<MemoryStream> GetCsvFromAnalyzedFolder(IEnumerable<ExifInfo>? analyzedFolderData)
        {
            return _exifService.GetCsvFromAnalyzedFolder(analyzedFolderData);
        }
    }
}
