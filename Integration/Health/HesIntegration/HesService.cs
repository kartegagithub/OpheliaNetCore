using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ophelia.Integration.Health
{
    public class HesService
    {
        public HesService(string serviceURL, string userName, string password)
        {
            this.ServiceURL = serviceURL;
            this.UserName = userName;
            this.Password = password;
        }

        //Live: https://hesservis.turkiye.gov.tr/services/g2g/test/saglik/hes/
        //Test: https://hesservis.turkiye.gov.tr/services/g2g/saglik/hes/
        public string ServiceURL { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        private string oAuthenticationToken;

        private string AuthenticationToken
        {
            get
            {
                if (string.IsNullOrEmpty(this.oAuthenticationToken))
                {
                    this.oAuthenticationToken = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", UserName, Password)));
                }
                return this.oAuthenticationToken;
            }
        }

        /// <summary>
        /// Kurum/İşyeri ziyaretçilerinin HES kodlarının sorgulanması gerektiği durumlarda kullanılacak olan servis metodudur.
        /// </summary>
        /// <param name="city">zorunlu / sorgulama yapılan il bilgisi</param>
        /// <param name="district">zorunlu / sorgulama yapılan ilçe bilgisi</param>
        /// <param name="address">zorunlu / sorgulama yapılan adres bilgisi</param>
        /// <param name="hesCode">zorunlu / sorgulama yapılan hes kodu bilgisi</param>
        /// <param name="latitude">sorgulama yapılan enlem</param>
        /// <param name="longitude">sorgulama yapılan boylam</param>
        /// <returns>Dönülen tarih UTC+0 zaman dilimindedir. Tarih kullanılmadan localtime'a çevrilmelidir.</returns>
        public ServiceObjectResult<HesCodeResult> CheckVisitorHesCode(string city, string district, string address, string hesCode, string latitude, string longitude)
        {
            var result = new ServiceObjectResult<HesCodeResult>();
            try
            {
                var tempHesCode = "";
                #region controls
                if (!string.IsNullOrEmpty(city))
                {
                    result.Fail("CityCanNotBeBlank");
                    return result;
                }
                if (!string.IsNullOrEmpty(district))
                {
                    result.Fail("DistrictCanNotBeBlank");
                    return result;
                }
                if (!string.IsNullOrEmpty(address))
                {
                    result.Fail("AddressCanNotBeBlank");
                    return result;
                }
                if (!string.IsNullOrEmpty(hesCode))
                {
                    result.Fail("HesCodeCanNotBeBlank");
                    return result;
                }
                else
                {
                    tempHesCode = hesCode.Replace("-", "");
                    if (tempHesCode.Length != 10)
                    {
                        result.Fail("HesCodeIsNotValid");
                        return result;
                    }
                }
                #endregion
                var URL = ServiceURL + "/check-visitor-hes-code";
                var parameters = new
                {
                    city = city,
                    district = district,
                    explicit_address = address,
                    hes_code = tempHesCode,
                    location = new
                    {
                        latitude = latitude.ToDecimal(),
                        longitude = longitude.ToDecimal()
                    }
                };

                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Basic " + this.AuthenticationToken }
                };

                result.SetData(URL.PostURL<HesCodeResult>(parameters, headers, contentType: "application/json"));
                if (result.Data != null)
                {
                    result.Data.HesCode = hesCode;
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// Kurum/İşyeri personellerinin HES kodlarının sorgulanması gerektiği durumlarda kullanılacak olan servis metodudur.
        /// </summary>
        /// <param name="hesCode">zorunlu / sorgulama yapılan hes kodu bilgisi</param>
        /// <returns>Dönülen tarih UTC+0 zaman dilimindedir. Tarih kullanılmadan localtime'a çevrilmelidir.</returns>
        public ServiceObjectResult<HesCodeResult> CheckEmployeeHesCode(string hesCode)
        {
            var result = new ServiceObjectResult<HesCodeResult>();
            try
            {
                var tempHesCode = "";
                #region controls
                if (!string.IsNullOrEmpty(hesCode))
                {
                    result.Fail("HesCodeCanNotBeBlank");
                    return result;
                }
                else
                {
                    tempHesCode = hesCode.Replace("-", "");
                    if (tempHesCode.Length != 10)
                    {
                        result.Fail("HesCodeIsNotValid");
                        return result;
                    }
                }
                #endregion
                var URL = ServiceURL + "/check-employee-hes-code";
                var parameters = new
                {
                    hes_code = tempHesCode,
                };

                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Basic " + this.AuthenticationToken }
                };

                result.SetData(URL.PostURL<HesCodeResult>(parameters, headers, contentType: "application/json"));
                if (result.Data != null)
                {
                    result.Data.HesCode = hesCode;
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// Kurum/İşyeri personellerinin HES kodlarının toplu olarak sorgulanması gerektiği durumlarda kullanılacak olan servis metodudur.
        /// </summary>
        /// <param name="hesCode">zorunlu / sorgulama yapılan hes kodu bilgisi</param>
        /// <returns>Dönülen tarih UTC+0 zaman dilimindedir. Tarih kullanılmadan localtime'a çevrilmelidir.</returns>
        public ServiceObjectResult<AllHesCodeResult> CheckHesCodes(string hesCode)
        {
            var result = new ServiceObjectResult<AllHesCodeResult>();
            try
            {
                #region controls
                if (!string.IsNullOrEmpty(hesCode))
                {
                    result.Fail("HesCodeCanNotBeBlank");
                    return result;
                }
                #endregion
                var URL = ServiceURL + "/check-hes-codes";

                var list = new List<object>();

                var hesCodes = hesCode.Split(';', ',');

                foreach (var item in hesCodes)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var tempHesCode = item.Replace("-", "");
                        if (tempHesCode.Length != 10)
                            continue;
                        list.Add(new
                        {
                            hes_code = tempHesCode
                        });
                    }
                }

                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Basic " + this.AuthenticationToken }
                };

                result.SetData(URL.PostURL<AllHesCodeResult>(list, headers, contentType: "application/json"));
                if (result.Data != null)
                {
                    if (result.Data.SuccessMap?.Count > 0)
                    {
                        foreach (var item in result.Data.SuccessMap)
                        {
                            item.Value.HesCode = item.Key;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// Kurum/İşyeri ziyaretçi ve personellerine ait HES kodlarının sorgulanması gerektiği durumlarda kullanılacak olan servis metodudur. Servis sonucunda sorgulanan HES kodu sahibine ait aşı, bağışıklık ve negatif test sonuç bilgileri dönmektedir.
        /// </summary>
        /// <param name="hesCode">zorunlu / sorgulama yapılan hes kodu bilgisi</param>
        /// <returns>Dönülen tarih UTC+0 zaman dilimindedir. Tarih kullanılmadan localtime'a çevrilmelidir.</returns>
        public ServiceObjectResult<HesCodeDetailResult> CheckHesCodePlus(string hesCode)
        {
            var result = new ServiceObjectResult<HesCodeDetailResult>();
            try
            {
                var tempHesCode = "";
                #region controls
                if (!string.IsNullOrEmpty(hesCode))
                {
                    result.Fail("HesCodeCanNotBeBlank");
                    return result;
                }
                else
                {
                    tempHesCode = hesCode.Replace("-", "");
                    if (tempHesCode.Length != 10)
                    {
                        result.Fail("HesCodeIsNotValid");
                        return result;
                    }
                }
                #endregion
                var URL = ServiceURL + "/check-hes-code-plus";
                var parameters = new
                {
                    hes_code = tempHesCode
                };

                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Basic " + this.AuthenticationToken }
                };

                result.SetData(URL.PostURL<HesCodeDetailResult>(parameters, headers, contentType: "application/json"));
                if (result.Data != null)
                {
                    result.Data.HesCode = hesCode;
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// Kurum/İşyeri ziyaretçi ve personellerine ait HES kodlarının çoklu sorgulanması gerektiği durumlarda kullanılacak olan servis metodudur. Servis sonucunda sorgulanan HES kodu sahibine ait aşı, bağışıklık ve negatif test sonuç bilgileri dönmektedir. Max 500 HES kodu tek request ile sorgulanabilmektedir.
        /// </summary>
        /// <param name="hesCode">zorunlu / sorgulama yapılan hes kodu bilgisi</param>
        /// <returns>Dönülen tarih UTC+0 zaman dilimindedir. Tarih kullanılmadan localtime'a çevrilmelidir.</returns>
        public ServiceObjectResult<AllHesCodeDetailResult> CheckHesCodesPlus(string hesCode)
        {
            var result = new ServiceObjectResult<AllHesCodeDetailResult>();
            try
            {
                #region controls
                if (!string.IsNullOrEmpty(hesCode))
                {
                    result.Fail("HesCodeCanNotBeBlank");
                    return result;
                }

                #endregion
                var URL = ServiceURL + "/check-hes-codes-plus";

                var list = new List<object>();

                var hesCodes = hesCode.Split(';', ',');

                foreach (var item in hesCodes)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var tempHesCode = item.Replace("-", "");
                        if (tempHesCode.Length != 10)
                            continue;
                        list.Add(new
                        {
                            hes_code = tempHesCode
                        });
                    }
                }

                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Basic " + this.AuthenticationToken }
                };

                result.SetData(URL.PostURL<AllHesCodeDetailResult>(list, headers, contentType: "application/json"));
                if (result.Data != null)
                {
                    if (result.Data.SuccessMap?.Count > 0)
                    {
                        foreach (var item in result.Data.SuccessMap)
                        {
                            item.Value.HesCode = item.Key;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
    }
}