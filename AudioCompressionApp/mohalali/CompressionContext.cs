// الملف: mohalali/CompressionContext.cs
using System.IO;

namespace AudioCompressionApp.mohalali
{
    public class CompressionContext
    {
        public string OriginalFilePath { get; set; }
        public long OriginalSizeBytes { get; set; }

        public void Load(string filePath)
        {
            OriginalFilePath = filePath;
            OriginalSizeBytes = new FileInfo(filePath).Length;
        }
    }
}