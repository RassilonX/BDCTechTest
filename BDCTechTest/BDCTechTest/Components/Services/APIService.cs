using BDCTechTest.Components.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace BDCTechTest.Components.Services;

public class APIService : IServiceBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _client;

    public APIService(IConfiguration config, HttpClient client)
    {
        _config = config;
        _client = client ?? new HttpClient();
    }

    public async Task<MOTData> GetMOTDataAsync(string regNumber)
    {
        try
        {
            var reg = new string(regNumber.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpper();

            string uri = _config["ApiURLS:MOTURI"]?.ToString() + reg;
            string apiKey = _config["ApiKeys:MOTKey"]?.ToString();

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json+v6"));
            _client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            HttpResponseMessage response = await _client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(responseBody);

                string expiryDate = jsonData[0]["motTests"][0]["expiryDate"];

                var data = new MOTData()
                {
                    Message = string.Empty,
                    Colour = jsonData[0]["primaryColour"],
                    Model = jsonData[0]["model"],
                    Make = jsonData[0]["make"],
                    Mileage = jsonData[0]["motTests"][0]["odometerValue"],
                    MOTExpiry = DateTime.Parse(expiryDate)
                };

                return data;
            }
            else
            {
                throw new Exception("No vehicle matching that registration was found");
            }

        }
        catch (Exception ex)
        { 
            return new MOTData() { Message = ex.Message };
        }

    }
}
