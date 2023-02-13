namespace EXIFPhotosAnalyzer.UI.Models
{
    public class ChartData
    {
        public int ItemsCount { get; set; }
        public double Value { get; set; }
        public List<(string CameraModel, int Count)>? CameraModelsList { get; set; }
    }
}
