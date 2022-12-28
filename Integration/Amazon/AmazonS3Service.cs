using Amazon.S3;
using Amazon.S3.Model;
using Ophelia.Integration.Amazon.Model;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ophelia.Integration.Amazon
{
    public class AmazonS3Service
    {
        private string AccessKey = "";
        private string SecretKey = "";
        private string Bucket = "";
        private string ServiceURL = "";

        private AmazonS3Client oClient;

        public TimeSpan Expiration { get; set; }
        public bool KeepFilePath { get; set; }
        public Protocol Protocol { get; set; }
        public AmazonS3Config Config { get; set; }


        private AmazonS3Client Client
        {
            get
            {
                if (this.oClient == null)
                {
                    if (this.Config == null)
                        this.Config = new AmazonS3Config();

                    if (!string.IsNullOrEmpty(this.ServiceURL))
                        this.Config.ServiceURL = this.ServiceURL;
                    this.oClient = new AmazonS3Client(this.AccessKey, this.SecretKey, this.Config);
                }
                return this.oClient;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="API"></param>
        public AmazonS3Service(string accessKey, string secretKey, string bucket, string serviceURL, Protocol protocol = Protocol.HTTPS)
        {
            this.AccessKey = accessKey;
            this.SecretKey = secretKey;
            this.Bucket = bucket;
            this.ServiceURL = serviceURL;
            this.Protocol = protocol;
        }

        public ServiceObjectResult<AmazonResponse> Upload(FileData file)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                return this.Upload(file.ByteData, file.FileName);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<AmazonResponse> Upload(byte[] fileData, string filePath)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                using (var memoryStream = new MemoryStream(fileData))
                {
                    return this.Upload(memoryStream, filePath);
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<AmazonResponse> Upload(Stream fileData, string filePath)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = this.Bucket;
                if (this.KeepFilePath)
                    request.Key = filePath;
                else
                    request.Key = System.IO.Path.GetFileName(filePath);
                request.InputStream = fileData;
                var objectResult = this.Client.PutObjectAsync(request).Result;
                var getResult = this.GetURL(filePath);
                if (!getResult.HasFailed && getResult.Data != null)
                    result.SetData(getResult.Data);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<AmazonResponse> Upload(string filePath)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = this.Bucket;
                if (this.KeepFilePath)
                    request.Key = filePath;
                else
                    request.Key = System.IO.Path.GetFileName(filePath);
                request.FilePath = filePath;
                var objectResult = this.Client.PutObjectAsync(request).Result;
                if (objectResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var getResult = this.GetURL(request.Key);
                    if (!getResult.HasFailed && getResult.Data != null)
                        result.SetData(getResult.Data);
                }
                else
                    result.Fail($"Could not upload to AWS S3: {objectResult.HttpStatusCode.ToString()}");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<AmazonResponse> GetURL(string file)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                if (this.Expiration == null)
                    this.Expiration = TimeSpan.FromDays(1000);

                var request = new GetPreSignedUrlRequest();
                request.BucketName = this.Bucket;
                request.Key = file;
                request.Expires = DateTime.Now.Add(this.Expiration);
                request.Protocol = this.Protocol;
                string url = this.Client.GetPreSignedURL(request);

                var model = new AmazonResponse();
                model.ExpireDate = request.Expires;
                model.URL = url;
                result.SetData(model);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

    }
}
