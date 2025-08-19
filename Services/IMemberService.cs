using NewMatchingBom.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NewMatchingBom.Services
{
    public interface IMemberService
    {
        ObservableCollection<User> AllMembers { get; }
        ObservableCollection<User> SelectedMembers { get; }
        
        void InitializeAsync();
        Task LoadMembersAsync();
        void AddMember(User user);
        void RemoveMember(User user);
        void AddToSelected(User user);
        void RemoveFromSelected(User user);
        void ClearSelected();
        
        Task SaveMembersAsync();
        Task LoadFromGoogleSheetsAsync();
        
        Task<List<PlayRecord>> LoadPlayRecords();
        void SavePlayRecord(PlayRecord record);
        List<RecordViewData> GetRankingData();
    }
}