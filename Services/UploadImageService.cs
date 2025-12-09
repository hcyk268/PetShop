using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Services
{
    public class UploadImageService
    {
        private readonly string _cloudName = ConfigurationManager.ConnectionStrings["cloudinaryName"].ConnectionString;
        private readonly string _apiKey = ConfigurationManager.ConnectionStrings["cloudinaryAPIKey"].ConnectionString;
        private readonly string _apiSecret = ConfigurationManager.ConnectionStrings["cloudinaryAPISecret"].ConnectionString;

        private static Cloudinary _cloudinary;

        public UploadImageService()
        {
            var account = new Account(
                _cloudName,
                _apiKey,
                _apiSecret
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public static string UploadImageGetUrl(string filePath)
        {
            string publicId = "petpic_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + (new Random().Next(11, 99));

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                PublicId = publicId,
                Folder = "petshop"
            };

            var uploadResult = _cloudinary.Upload(uploadParams);

            return uploadResult.SecureUrl?.ToString();
        }
    }
}
