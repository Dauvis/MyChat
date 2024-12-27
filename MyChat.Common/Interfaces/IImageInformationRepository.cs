using MyChat.Common.DTO;
using MyChat.Common.Model;

namespace MyChat.Common.Interfaces
{
    public interface IImageInformationRepository
    {
        string AddImage(BinaryData imageData, ImageToolMetadata metadata);
        void Delete(string imagePath);
        ImageInformationDTO Fetch();
        void Move(string imagePath, string destinationPath);
        bool Update(BinaryData imageData, ImageToolMetadata metadata, string imagePath);
        bool UpdateMetadata(ImageInformationDTO imageInformation);
    }
}