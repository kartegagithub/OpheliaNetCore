using Amazon.S3;
using Amazon.S3.Model;
using AngleSharp.Io;
using Ophelia.Integration.Amazon.Model;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Transactions;

namespace Ophelia.Integration.Amazon
{
    public class AmazonS3Service : IDisposable
    {
        private string AccessKey = "";
        private string SecretKey = "";
        private string Bucket = "";
        private string ServiceURL = "";

        private AmazonS3Client oClient;

        public TimeSpan? Expiration { get; set; }
        public bool KeepFilePath { get; set; }
        public Protocol Protocol { get; set; }
        public AmazonS3Config Config { get; set; }
        public bool CanGetFileURLAfterUpload { get; set; }

        private AmazonS3Client Client
        {
            get
            {
                if (this.oClient == null)
                {
                    this.Config ??= new AmazonS3Config();

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
                filePath = filePath.Trim('/');
                var request = new PutObjectRequest
                {
                    BucketName = this.Bucket
                };
                if (this.KeepFilePath)
                    request.Key = filePath;
                else
                    request.Key = System.IO.Path.GetFileName(filePath);
                request.InputStream = fileData;
                var objectResult = this.Client.PutObjectAsync(request).Result;
                if (this.CanGetFileURLAfterUpload)
                {
                    var getResult = this.GetURL(request.Key);
                    if (!getResult.HasFailed && getResult.Data != null)
                        result.SetData(getResult.Data);
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<HttpStatusCode> CreateBucket(string name)
        {
            return this.CreateBucket(new PutBucketRequest() { BucketName = name });
        }

        public ServiceObjectResult<HttpStatusCode> CreateBucket(PutBucketRequest request)
        {
            var result = new ServiceObjectResult<HttpStatusCode>();
            try
            {
                var objectResult = this.Client.PutBucketAsync(request).Result;
                result.SetData(objectResult.HttpStatusCode);
                if (objectResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.Fail($"Could not upload to AWS S3: {objectResult.HttpStatusCode.ToString()}");
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
                var request = new PutObjectRequest
                {
                    BucketName = this.Bucket
                };
                if (this.KeepFilePath)
                    request.Key = filePath;
                else
                    request.Key = System.IO.Path.GetFileName(filePath);
                request.FilePath = filePath;
                var objectResult = this.Client.PutObjectAsync(request).Result;
                if (objectResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (this.CanGetFileURLAfterUpload)
                    {
                        var getResult = this.GetURL(request.Key);
                        if (!getResult.HasFailed && getResult.Data != null)
                            result.SetData(getResult.Data);
                    }
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

        public ServiceObjectResult<DeleteObjectResponse> Delete(string key)
        {
            var result = new ServiceObjectResult<DeleteObjectResponse>();
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = this.Bucket,
                    Key = key
                };

                result.Data = this.Client.DeleteObjectAsync(request).Result;
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<GetObjectMetadataResponse> GetObjectMetadata(string key)
        {
            var result = new ServiceObjectResult<GetObjectMetadataResponse>();
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = this.Bucket,
                    Key = key
                };

                result.Data = this.Client.GetObjectMetadataAsync(request).Result;
            }
            catch (Exception ex)
            {
                result.Fail(ex.ToString());
            }
            return result;
        }

        public ServiceObjectResult<byte[]> GetFile(string key)
        {
            var result = new ServiceObjectResult<byte[]>();
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = this.Bucket,
                    Key = key
                };

                using (GetObjectResponse response = this.Client.GetObjectAsync(request).Result)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        response.ResponseStream.CopyToAsync(memoryStream).Wait();
                        result.Data = memoryStream.ToArray(); // ← işte bu
                    }
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceObjectResult<bool> ObjectExists(string key)
        {
            var result = new ServiceObjectResult<bool>();
            try
            {
                var metaDataResponse = this.GetObjectMetadata(key);
                if (!metaDataResponse.HasFailed)
                    result.SetData(true);
                else if (metaDataResponse.Messages.Any(op => op.Description.Contains("ObjectNotFoundInS3")))
                    result.SetData(false);
                else
                    result.Fail(metaDataResponse);
            }
            catch (Exception ex)
            {
                result.Fail(ex.ToString());
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

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = this.Bucket,
                    Key = file,
                    Expires = Utility.Now.Add(this.Expiration.Value),
                    Protocol = this.Protocol
                };
                string url = this.Client.GetPreSignedURL(request);

                var model = new AmazonResponse
                {
                    ExpireDate = request.Expires,
                    URL = url
                };
                result.SetData(model);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.oClient?.Dispose();
            this.oClient = null;
            this.ServiceURL = "";
            this.AccessKey = "";
            this.SecretKey = "";
        }
    }
}
