using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using ImagingEncoder = System.Drawing.Imaging.Encoder;
using DrawingSize = System.Drawing.Size;

namespace Pet_Shop_Project.Services
{
    public class UploadImageService
    {
        private readonly string _cloudName = ConfigurationManager.ConnectionStrings["cloudinaryName"].ConnectionString;
        private readonly string _apiKey = ConfigurationManager.ConnectionStrings["cloudinaryAPIKey"].ConnectionString;
        private readonly string _apiSecret = ConfigurationManager.ConnectionStrings["cloudinaryAPISecret"].ConnectionString;
        public UploadImageService(Cloudinary cloudinary) => _cloudinary = cloudinary;
        private readonly Cloudinary _cloudinary;

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

        private string CompressIfNeeded(string inputPath, int maxKb = 200, int maxWidth = 1200, int maxHeight = 1200)
        {
            var fi = new FileInfo(inputPath);
            if (fi.Length <= maxKb * 1024) return inputPath;

            using (var src = Image.FromFile(inputPath))
            {
                double scale = Math.Min(1.0, Math.Min((double)maxWidth / src.Width, (double)maxHeight / src.Height));
                int w = Math.Max(1, (int)(src.Width * scale));
                int h = Math.Max(1, (int)(src.Height * scale));

                using (var bmp = new Bitmap(src, new DrawingSize(w, h)))
                {
                    var jpgEncoder = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    var encParams = new EncoderParameters(1);

                    string tempPath = Path.GetTempFileName(); 
                    long quality = 85;
                    while (quality >= 50)
                    {
                        encParams.Param[0] = new EncoderParameter(ImagingEncoder.Quality, (long)quality);
                        bmp.Save(tempPath, jpgEncoder, encParams);
                        if (new FileInfo(tempPath).Length <= maxKb * 1024)
                            return tempPath;
                        quality -= 5;
                    }
                    return tempPath; 
                }
            }
        }

        public string UploadImageGetUrl(string filePath)
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

        public async Task<(string secureUrl, string publicId)> UploadAsync(string filePath, string folder = "products")
        {
            var pathForUpload = CompressIfNeeded(filePath);
            using (var stream = File.OpenRead(pathForUpload))          
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(Path.GetFileName(pathForUpload), stream),
                    Folder = folder,
                    UseFilename = true,    
                    UniqueFilename = true, 
                    Overwrite = false
                };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.StatusCode != HttpStatusCode.OK || result.Error != null)
                throw new InvalidOperationException($"Upload failed: {result.Error?.Message}");

            return (result.SecureUrl?.ToString() ?? string.Empty, result.PublicId);
            }
        }

        public string BuildUrl(string publicId, int? version = null)
        {
            var url = _cloudinary.Api.UrlImgUp.Secure(true);
            if (version.HasValue) 
                url = url.Version(version.Value.ToString());
            return url.BuildUrl(publicId);
        }
    }
}
