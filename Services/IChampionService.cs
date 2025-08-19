using NewMatchingBom.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NewMatchingBom.Services
{
    public interface IChampionService
    {
        ObservableCollection<ChampionInfo> AllChampions { get; }
        ObservableCollection<ChampionInfo> RemainedChampions { get; }
        ObservableCollection<ChampionInfo> UsedChampions { get; }
        ObservableCollection<ChampionInfo> SelectedChampions { get; }
        
        bool IsLoaded { get; }
        int TotalChampionCount { get; }
        int SelectedCount { get; set; }
        
        Task InitializeChampionsAsync();
        
        bool CanSelect();
        void AddSelectedChampion(ChampionInfo champion);
        void RemoveSelectedChampion(ChampionInfo champion);
        void RevertSelectedChampions();
        void CommitSelectedChampions();
        
        void PickRandomChampions(int count);
        
        void RevertUsedChampions();
        void RemoveUsedChampions(List<ChampionInfo> champions);
    }
}