using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using ExifLib;
using EXIFPhotosAnalyzer.Lib.Models;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;


namespace EXIFPhotosAnalyzer.Lib.Services
{
    /// <summary>
    /// Provides logical services for application
    /// </summary>
    public class ExifService: IExifService
    {
        /// <summary>
        /// Goes through all files in directory and tries to search for their EXIF tags
        /// </summary>
        /// <param name="directoryPath">Path to directory to analyze</param>
        /// <returns></returns>
        public async IAsyncEnumerable<ExifInfoProgress> GetExifValuesForDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new ArgumentException($"Path {directoryPath} does not exist.");
            List<Task<ExifInfoProgress?>> tasks = Directory.GetFiles(directoryPath)
                .Select(pathToPhoto => Task.Run(async() =>
                {
                    try
                    {
                        using var exifReader = new ExifReader(pathToPhoto);
                        exifReader.GetTagValue(ExifTags.FocalLength, out double value);
                        exifReader.GetTagValue(ExifTags.Model, out string cameraModel);

                        return new ExifInfoProgress()
                        {
                            ExifInfo = new ExifInfo
                            {
                                FocalLength = value,
                                Path = pathToPhoto,
                                CameraModel = cameraModel
                            }
                        };
                    }
                    catch (Exception)
                    {
                        // there might be files with no such tags in directory,
                        // also non image files
                        return null;
                    }
                })) 
                .ToList();
            int totalCount = tasks.Count;
            int processedCount = 0;
            while (tasks.Count > 0)
            {
                // we are returning items as soon as they are completed rather than
                // keeping the order and awaiting collection sequentially, because
                // order does not bother us and this way by the time the last photos processing is done,
                // other photos data can already be processed in other methods
                processedCount++;
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                var photoTaskResult = await completedTask;
                if(photoTaskResult is null)
                    continue; // we are only interested in valid data
                photoTaskResult.Progress = processedCount * 1.0 / totalCount;
                yield return photoTaskResult;
            }
        }

        /// <summary>
        /// Transforms list of processed data to memory stream containing csv file with those data
        /// </summary>
        /// <param name="analyzedFolderData">List of processed exif information to be transformed to csv</param>
        /// <returns></returns>
        public async Task<MemoryStream> GetCsvFromAnalyzedFolder(IEnumerable<ExifInfo>? analyzedFolderData)
        {
            var memoryStream = new MemoryStream(); // intentionally not using var to avoid disposal of stream
            if (analyzedFolderData is null)
                return memoryStream;
            await using var writer = new StreamWriter(memoryStream, leaveOpen: true);
            await using var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);
            // cycling through properties to avoid having to adjust the method when another fields are added to ExifInfo
            foreach (var property in typeof(ExifInfo).GetProperties())
            {
                csvWriter.WriteField(property.Name);
            }
            foreach (var photo in analyzedFolderData)
            {
                foreach (var property in typeof(ExifInfo).GetProperties())
                {
                    csvWriter.WriteField(property.GetValue(photo));
                }
                await csvWriter.NextRecordAsync();
            }

            await writer.FlushAsync();
            memoryStream.Position = 0; // set position to 0 so method that actually
                                       // streams it to user can read it from the start
            return memoryStream;
        }
    }
}
