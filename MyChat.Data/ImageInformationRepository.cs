using MyChat.Common;
using MyChat.Common.DTO;
using MyChat.Common.Interfaces;
using MyChat.Common.Model;
using System.Text.Json;

namespace MyChat.Data
{
    public class ImageInformationRepository : IImageInformationRepository
    {
        private string _userImageMetaPath = "";
        private string _userPicturesFolder = "";

        public ImageInformationDTO Fetch()
        {
            string metadataPath = GetUserMetadataPath();
            string picturesPath = GetUserPicturesFolder();

            ImageInformationDTO imageInformation = new();
            FetchImageList(imageInformation, picturesPath);
            FetchImageMetadata(imageInformation, metadataPath);

            return imageInformation;
        }

        public bool UpdateMetadata(ImageInformationDTO imageInformation)
        {
            string metadataPath = GetUserMetadataPath();
            return SaveImageMetadata(imageInformation, metadataPath);
        }

        public bool Update(BinaryData imageData, ImageToolMetadata metadata, string imagePath)
        {
            string metaDataPath = GetUserMetadataPath();
            bool wasSaved = SaveImage(imageData, imagePath);

            if (!wasSaved)
            {
                return false;
            }

            ImageInformationDTO imageInformation = Fetch();
            ApplyNewImageMetadata(imageInformation, imagePath, metadata.Prompt, metadata.Quality, metadata.Size, metadata.Style);

            if (!SaveImageMetadata(imageInformation, metaDataPath))
            {
                return false;
            }

            return true;
        }

        public string AddImage(BinaryData imageData, ImageToolMetadata metadata)
        {
            string metaDataPath = GetUserMetadataPath();
            string picturesPath = GetUserPicturesFolder();

            string imagePath = Path.Combine(picturesPath, $"DallE-{DateTime.UtcNow:yyMMddHHmmss}.png");
            bool wasSaved = SaveImage(imageData, imagePath);

            if (!wasSaved)
            {
                return "";
            }

            ImageInformationDTO imageInformation = Fetch();
            ApplyNewImageMetadata(imageInformation, imagePath, metadata.Prompt, metadata.Quality, metadata.Size, metadata.Style);

            if (!SaveImageMetadata(imageInformation, metaDataPath))
            {
                return "";
            }

            return imagePath;
        }

        public void Delete(string imagePath)
        {
            try
            {
                File.Delete(imagePath);

                var metadata = Fetch();
                int index = metadata.ImageFilePaths.IndexOf(imagePath);
                metadata.ImageFilePaths.RemoveAt(index);
                metadata.ImageMetadata.Remove(imagePath);
                UpdateMetadata(metadata);
            }
            catch 
            {
                // do nothing
            }
        }

        public void Move(string imagePath, string destinationPath)
        {
            try
            {
                File.Move(imagePath, destinationPath);

                var metadata = Fetch();
                int index = metadata.ImageFilePaths.IndexOf(imagePath);
                metadata.ImageFilePaths.RemoveAt(index);
                metadata.ImageMetadata.Remove(imagePath);
                UpdateMetadata(metadata);
            }
            catch
            {
                // do nothing
            }
        }

        private static void FetchImageList(ImageInformationDTO imageInformation, string picturesPath)
        {
            string[] fileList = Directory.GetFiles(picturesPath, "*.png");

            foreach (string filePath in fileList)
            {
                imageInformation.ImageFilePaths.Add(filePath);
            }
        }

        private static void FetchImageMetadata(ImageInformationDTO imageInformation, string metadataPath)
        {
            imageInformation.ImageMetadata.Clear();

            if (File.Exists(metadataPath))
            {
                string metadataJson = File.ReadAllText(metadataPath);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, ImageToolMetadata>>(metadataJson) ?? [];

                foreach (var metadataItem in metadata)
                {
                    imageInformation.ImageMetadata[metadataItem.Key] = metadataItem.Value;
                }
            }
        }

        private static bool SaveImageMetadata(ImageInformationDTO imageInformation, string metadataPath)
        {
            try
            {
                CleanUpMetadataEntries(imageInformation);
                string metadataJson = JsonSerializer.Serialize(imageInformation.ImageMetadata);
                File.WriteAllText(metadataPath, metadataJson);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool SaveImage(BinaryData imageData, string imagePath)
        {
            try
            {
                using FileStream stream = File.OpenWrite(imagePath);
                imageData.ToStream().CopyTo(stream);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static void ApplyNewImageMetadata(ImageInformationDTO imageInformation, string imagePath, string prompt, string quality, string size, string style)
        {
            imageInformation.ImageFilePaths.Add(imagePath);
            imageInformation.ImageMetadata[imagePath] = new(prompt, quality, size, style);
        }

        private string GetUserMetadataPath()
        {
            if (string.IsNullOrEmpty(_userImageMetaPath))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userAppFolder = Path.Combine(appDataPath, Constants.UserAppFolderName);
                Directory.CreateDirectory(userAppFolder);
                _userImageMetaPath = Path.Combine(userAppFolder, Constants.ImageMetadataFileName);
            }

            return _userImageMetaPath;
        }

        private string GetUserPicturesFolder()
        {
            if (string.IsNullOrEmpty(_userPicturesFolder))
            {
                string picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                _userPicturesFolder = Path.Combine(picturesFolder, Constants.UserAppFolderName);
                Directory.CreateDirectory(_userPicturesFolder);
            }

            return _userPicturesFolder;
        }

        // Since the user can remove images from the designated folder,
        // this logic is needed to purge unneeded records.
        private static void CleanUpMetadataEntries(ImageInformationDTO imageInformation)
        {
            HashSet<string> validPathSet = new(imageInformation.ImageFilePaths);
            List<string> pathsToPurge = [];

            foreach (var entry in imageInformation.ImageMetadata)
            {
                if (!validPathSet.Contains(entry.Key))
                {
                    pathsToPurge.Add(entry.Key);
                }
            }

            foreach (var path in pathsToPurge)
            {
                imageInformation.ImageMetadata.Remove(path);
            }
        }
    }
}
