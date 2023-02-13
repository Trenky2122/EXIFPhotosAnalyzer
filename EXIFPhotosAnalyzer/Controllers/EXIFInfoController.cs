using EXIFPhotosAnalyzer.Lib.Models;
using EXIFPhotosAnalyzer.Lib.Services;
using Microsoft.AspNetCore.Mvc;

namespace EXIFPhotosAnalyzer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EXIFInfoController: ControllerBase
    {
        private readonly IEXIFService _exifService;

        public EXIFInfoController(IEXIFService exifService)
        {
            _exifService = exifService;
        }

        [HttpGet("fromDirectory/{directoryPath}")]
        public IAsyncEnumerable<EXIFInfo> GetExifInfoForDirectory(string directoryPath)
        {
            return _exifService.GetExifValuesForDirectory(directoryPath);
        }
    }
}
