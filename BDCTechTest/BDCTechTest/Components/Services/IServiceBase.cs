using BDCTechTest.Components.Models;

namespace BDCTechTest.Components.Services;

public interface IServiceBase
{
    public Task<MOTData> GetMOTDataAsync(string regNumber) =>
        Task.FromResult(new MOTData { Message = "Not implemented yet"  });
}
