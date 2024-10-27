using MyChat.Common.DTO;

namespace MyChat.Common.Interfaces
{
    public interface IImageService
    {
        void DeleteImage(string imagePath);
        Task<string> GenerateAsync(string prompt, string quality, string size, string style, string imagePath = "");
        ImageInformationDTO GetImageInformation();
        void OpenImageInPreferredEditor(string imagePath);
        void MoveImage(string imagePath, string destinationPath);
    }
}