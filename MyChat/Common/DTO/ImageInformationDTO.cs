using MyChat.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.DTO
{
    public class ImageInformationDTO
    {
        public List<string> ImageFilePaths { get; } = [];
        public Dictionary<string, ImageToolMetadata> ImageMetadata { get; } = [];
    }
}
