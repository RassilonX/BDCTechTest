using BDCTechTest.Components.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace BDCTechTest.Components.Services;

public class APIService : IServiceBase
{
    public async Task<MOTData> GetMOTDataAsync(string regNumber)
    {
        var reg = regNumber.Replace(" ", string.Empty).ToUpper();
        var uri = "https://beta.check-mot.service.gov.uk/trade/vehicles/mot-tests?registration=" + reg;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json+v6"));
        client.DefaultRequestHeaders.Add("x-api-key", "fZi8YcjrZN1cGkQeZP7Uaa4rTxua8HovaswPuIno");

        HttpResponseMessage response = await client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic jsonData = JsonConvert.DeserializeObject(responseBody);

            string expiryDate = jsonData[0]["motTests"][0]["expiryDate"];

            var data = new MOTData()
            {
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
            throw new Exception("Not implemented yet");
        }
    }
}
