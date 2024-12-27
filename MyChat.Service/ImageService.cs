using MyChat.Common.DTO;
using MyChat.Common.Interfaces;
using OpenAI.Images;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyChat.Service
{
    public class ImageService : IImageService
    {
        private readonly IGPTService _gptService;
        private readonly IImageInformationRepository _imageInfoRepository;

        public ImageService(IGPTService gptService, IImageInformationRepository imageInfoRepository)
        {
            _gptService = gptService;
            _imageInfoRepository = imageInfoRepository;
        }

        public ImageInformationDTO GetImageInformation()
        {
            return _imageInfoRepository.Fetch();
        }

        public async Task<string> GenerateAsync(string prompt, string quality, string size, string style, string imagePath = "")
        {
            var imageData = await _gptService.GenerateImageAsync(prompt, quality, size, style);

            if (imageData is null)
            {
                return "";
            }

            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return _imageInfoRepository.AddImage(imageData, new(prompt, quality, size, style));
                }
                else
                {
                    return _imageInfoRepository.Update(imageData, new(prompt, quality, size, style), imagePath) ? imagePath : "";
                }
            }
            catch
            {
                return "";
            }
        }

        public bool OpenImageInPreferredEditor(string imagePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = imagePath,
                    UseShellExecute = true
                });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void DeleteImage(string imagePath)
        {
            _imageInfoRepository.Delete(imagePath);
        }

        public void MoveImage(string imagePath, string destinationPath)
        {
            _imageInfoRepository.Move(imagePath, destinationPath);
        }
    }
}
