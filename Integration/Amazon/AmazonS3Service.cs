﻿using Amazon.S3;
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
        private int ExpireYear = 10;

        private AmazonS3Client oClient;

        private AmazonS3Client Client
        {
            get
            {
                if (this.oClient == null)
                {
                    AmazonS3Config config = new AmazonS3Config();
                    config.ServiceURL = this.ServiceURL;
                    this.oClient = new AmazonS3Client(this.AccessKey, this.SecretKey, config);
                }
                return this.oClient;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="API"></param>
        public AmazonS3Service(string accessKey, string secretKey, string bucket, string serviceURL, int expireYear = 10)
        {
            this.AccessKey = accessKey;
            this.SecretKey = secretKey;
            this.Bucket = bucket;
            this.ServiceURL = serviceURL;
            this.ExpireYear = expireYear;
        }

        public ServiceObjectResult<AmazonResponse> Upload(FileData file)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                string fileName = Guid.NewGuid().ToString() + extension;

                using (var memoryStream = new MemoryStream(file.ByteData))
                {
                    PutObjectRequest request = new PutObjectRequest();
                    request.BucketName = this.Bucket;
                    request.Key = fileName;
                    request.InputStream = memoryStream;
                    var objectResult = this.Client.PutObjectAsync(request).Result;
                    var getResult = this.GetURL(fileName);
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

        public ServiceObjectResult<AmazonResponse> GetURL(string file)
        {
            var result = new ServiceObjectResult<AmazonResponse>();
            try
            {
                var model = new AmazonResponse();
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
                request.BucketName = this.Bucket;
                request.Key = file;
                request.Expires = DateTime.Now.AddYears(this.ExpireYear);
                request.Protocol = Protocol.HTTP;
                string url = this.Client.GetPreSignedURL(request);

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