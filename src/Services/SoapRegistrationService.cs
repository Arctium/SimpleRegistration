// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
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
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, string.Empty);

        httpRequestMessage.Headers.Authorization = new("Basic", _configuration[$"{nameof(SoapRegistrationService)}:Credentials"]);

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

            httpRequestMessage.Content = new StringContent(soapRequestSerializer.ToString()!, Encoding.UTF8, "application/xml");
        }

        using var httpClient = _httpClientFactory.CreateClient(nameof(SoapRegistrationService));

        var requestTimeout = int.Parse(_configuration["SoapRegistrationService:HttpClientSettings:Timeout"]);

        httpClient.Timeout = TimeSpan.FromMilliseconds(requestTimeout);

        try
        {
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (!httpResponseMessage.IsSuccessStatusCode)
                return (Success: false, httpResponseMessage.ReasonPhrase);

            var soapResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            var soapResponseDocument = XDocument.Parse(soapResponseContent);

            return (Success: true, soapResponseDocument.Descendants("result").FirstOrDefault()?.Value);
        }
        catch (HttpRequestException e)
        {
            return (Success: false, e.HttpRequestError.ToString());
        }
        catch (TaskCanceledException)
        {
            return (Success: true, "Timeout! Please try again later");
        }
    }
}
