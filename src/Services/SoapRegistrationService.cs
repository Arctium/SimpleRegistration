// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Arctium.SimpleRegistration.Models;
using Arctium.SimpleRegistration.Models.Soap;

namespace Arctium.SimpleRegistration.Services;

class SoapRegistrationService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IRegistrationService
{
    readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    readonly IConfiguration _configuration = configuration;

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegistrationData registrationData, string userNameSuffix)
    {
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Empty);

        httpRequestMessage.Headers.Authorization = new("Basic", _configuration[$"{nameof(SoapRegistrationService)}:HttpClientSettings:Credentials"]);

        using (var soapRequestWriter = new StringWriter())
        {
            var envelope = new SoapEnvelope
            {
                Body = new SoapBody
                {
                    Command = new SoapCommand
                    {
                        CommandText = $"bnetaccount create {registrationData.UserName}{userNameSuffix} {registrationData.Password}"
                    }
                }
            };

            var soapRequestSerializer = new XmlSerializer(typeof(SoapEnvelope));

            soapRequestSerializer.Serialize(soapRequestWriter, envelope);

            httpRequestMessage.Content = new StringContent(soapRequestWriter.ToString()!, Encoding.UTF8, "application/xml");
        }

        using HttpClient httpClient = _httpClientFactory.CreateClient(nameof(SoapRegistrationService));

        var requestTimeout = int.Parse(_configuration["SoapRegistrationService:HttpClientSettings:Timeout"]);

        httpClient.Timeout = TimeSpan.FromMilliseconds(requestTimeout);

        try
        {
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            var soapResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            var soapResponseDocument = XDocument.Parse(soapResponseContent);
            var resultElementTag = httpResponseMessage.IsSuccessStatusCode ? "result" : "faultstring";

            return (httpResponseMessage.IsSuccessStatusCode, soapResponseDocument.Descendants(resultElementTag).FirstOrDefault()?.Value);
        }
        catch (HttpRequestException e)
        {
            return (Success: false, e.HttpRequestError.ToString());
        }
        catch (XmlException)
        {
            return (Success: false, "Malformed answer from the server");
        }
        catch (TaskCanceledException)
        {
            return (Success: false, "Timeout! Please try again later");
        }
    }
}
