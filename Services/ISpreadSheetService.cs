using NewMatchingBom.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewMatchingBom.Services
{
    public interface ISpreadSheetService
    {
        Task<List<User>> LoadUsersFromSpreadSheetAsync();
        Task<List<PlayRecord>> LoadPlayRecordsFromSpreadSheetAsync();
        Task SavePlayRecordToSpreadSheetAsync(PlayRecord record);
        Task ExportDataAsync(string data, string fileName);
    }
}