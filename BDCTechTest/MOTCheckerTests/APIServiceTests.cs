using BDCTechTest.Components.Models;
using BDCTechTest.Components.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;

namespace MOTCheckerTests;

public class APIServiceTests
{
    private readonly APIService _service;
    private readonly MockHttpMessageHandler _handler;

    public APIServiceTests()
    {
        var mockConfig = new Mock<IConfiguration>();

        mockConfig.SetupGet(c => c["ApiURLS:MOTURI"]).Returns("https://api.example.com/mot/");
        mockConfig.SetupGet(c => c["ApiKeys:MOTKey"]).Returns("my-api-key");

        _handler = new MockHttpMessageHandler();
        _handler.When("https://api.example.com/mot/ABC123")
           .Respond("application/json",
                @"[
                    {
                        ""primaryColour"": ""Red"",
                        ""model"": ""Ford Focus"",
                        ""make"": ""Ford"",
                        ""motTests"": [
                            {
                                ""odometerValue"": 50000,
                                ""expiryDate"": ""2025-01-01T00:00:00""
                            }
                        ]
                    }
                ]");

        _handler.When("https://api.example.com/mot/INVALID")
           .Respond(HttpStatusCode.NotFound);

        var httpClient = new HttpClient(_handler);

        _service = new APIService(mockConfig.Object, httpClient);
    }

    #region Happy Path Tests
    [Fact]
    public async Task GetMOTDataAsync_ValidLicenseNumber_ReturnsSuccess()
    {
        // Arrange
        var regNumber = "ABC123";

        // Act
        var result = await _service.GetMOTDataAsync(regNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Red", result.Colour);
        Assert.Equal("Ford Focus", result.Model);
        Assert.Equal("Ford", result.Make);
        Assert.Equal(50000, result.Mileage);
        Assert.Equal(new DateTime(2025, 1, 1), result.MOTExpiry);
        Assert.Empty(result.Message);
    }
    #endregion

    #region Unhappy Path Tests
    [Fact]
    public async Task GetMOTDataAsync_InvalidLicenseNumber_ReturnsDataWithErrorMessage()
    {
        // Arrange
        var regNumber = "INVALID";

        // Act
        var result = await _service.GetMOTDataAsync(regNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Colour);
        Assert.Null(result.Model);
        Assert.Null(result.Make);
        Assert.Equal(0, result.Mileage);
        Assert.Equal(new DateTime(), result.MOTExpiry);
        Assert.Equal("No vehicle matching that registration was found", result.Message);
    }
    #endregion
}