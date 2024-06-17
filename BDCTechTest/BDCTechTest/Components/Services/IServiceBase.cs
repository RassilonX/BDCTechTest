namespace BDCTechTest.Components.Services;

public interface IServiceBase
{
    public Task GetMOTDataAsync(string regNumber) =>
        Task.FromResult(new { Message = "Service not implemented yet." });
}
