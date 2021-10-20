using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ophelia.Integration.Documentation.BookStack
{
    public class Service
    {
        public Service(string serviceURL, string tokenID, string tokenSecret)
        {
            this.ServiceURL = serviceURL;
            this.TokenID = tokenID;
            this.TokenSecret = tokenSecret;
        }

        public string ServiceURL { get; set; }

        public string TokenID { get; set; }

        public string TokenSecret { get; set; }

        public ServiceObjectResult<Models.PageModel> CreatePage(Models.PageModel model)
        {
            var result = new ServiceObjectResult<Models.PageModel>();
            try
            {
                #region controls
                if (string.IsNullOrEmpty(model.name))
                {
                    result.Fail("NameCanNotBeBlank");
                    return result;
                }
                if (model.book_id <= 0 && model.chapter_id <= 0)
                {
                    result.Fail("BookIDOrChapterIDCanNotBeBothBlank");
                    return result;
                }
                #endregion
                if (string.IsNullOrEmpty(model.slug) && !string.IsNullOrEmpty(model.referenceCode))
                {
                    model.slug = string.Join("-", model.referenceCode.Trim('/').Split("/"));
                }
                var URL = ServiceURL + "/api/pages";
                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Token " + this.TokenID + ":" + this.TokenSecret }
                };

                result.SetData(URL.PostURL<Models.PageModel>(model.ToJson(), "application/json", headers));
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
    }
}