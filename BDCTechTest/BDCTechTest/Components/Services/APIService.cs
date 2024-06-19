using BDCTechTest.Components.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BDCTechTest.Components.Services;

public class APIService : IServiceBase
{
    private readonly IConfiguration _config;

    public APIService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<MOTData> GetMOTDataAsync(string regNumber)
    {
        try
        {
            var reg = new string(regNumber.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpper();

            string uri = _config["ApiURLS:MOTURI"]?.ToString() + reg;
            string apiKey = _config["ApiKeys:MOTKey"]?.ToString();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json+v6"));
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                HttpResponseMessage response = await client.GetAsync(uri);

                return await ProcessMOTCheckResponse(response);
            }

        }
        catch (Exception ex)
        { 
            return new MOTData() { Message = ex.Message };
        }

    }

    public async Task<MOTData> ProcessMOTCheckResponse(HttpResponseMessage response)
    {
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
}
