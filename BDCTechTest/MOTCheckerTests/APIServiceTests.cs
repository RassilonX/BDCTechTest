using BDCTechTest.Components.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using System.Text;

namespace MOTCheckerTests;

public class APIServiceTests
{
    private readonly APIService _service;

    public APIServiceTests()
    {
        var mockConfig = new Mock<IConfiguration>();

        mockConfig.SetupGet(c => c["ApiURLS:MOTURI"]).Returns("https://api.example.com/mot/");
        mockConfig.SetupGet(c => c["ApiKeys:MOTKey"]).Returns("my-api-key");

        _service = new APIService(mockConfig.Object);
    }

    #region Happy Path Tests
    [Fact]
    public async Task ProcessMOTCheckResponse_SuccessStatusCode_ReturnsSuccessObject()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(
            @"[
                {
                    ""primaryColour"": ""Blue"",
                    ""model"": ""Ford Focus"",
                    ""make"": ""Ford"",
                    ""motTests"": [
                        {
                            ""odometerValue"": 50000,
                            ""expiryDate"": ""2025.06.30""
                        }
                    ]
                }
            ]",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var result = await _service.ProcessMOTCheckResponse(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Blue", result.Colour);
        Assert.Equal("Ford Focus", result.Model);
        Assert.Equal("Ford", result.Make);
        Assert.Equal(50000, result.Mileage);
        Assert.Equal(new DateTime(2025, 6, 30), result.MOTExpiry);
        Assert.Equal(string.Empty, result.Message);
    }
    #endregion

    #region Unhappy Path Tests
    [Fact]
    public async Task ProcessMOTCheckResponse_UnsuccessfulStatusCode_ReturnsDataWithErrorMessage()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        // Act - Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.ProcessMOTCheckResponse(response));
        Assert.Equal("No vehicle matching that registration was found", ex.Message);
    }
    #endregion
}