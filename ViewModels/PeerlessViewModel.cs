using NewMatchingBom.Commands;
using NewMatchingBom.Models;
using NewMatchingBom.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewMatchingBom.ViewModels
{
    public class PeerlessViewModel : ViewModelBase
    {
        private readonly IChampionService _championService;
        private readonly ILoggingService _loggingService;
        
        private string _searchText = string.Empty;
        private string _remainedSearchText = string.Empty;
        private string _selectedSearchText = string.Empty;
        private string _usedSearchText = string.Empty;
        private int _randomPickCount = 5;

        public ObservableCollection<ChampionInfo> FilteredRemainedChampions { get; } = new();
        public ObservableCollection<ChampionInfo> FilteredSelectedChampions { get; } = new();
        public ObservableCollection<ChampionInfo> FilteredUsedChampions { get; } = new();
        public ObservableCollection<ChampionInfo> SelectedChampions => _championService.SelectedChampions;
        public ObservableCollection<ChampionInfo> UsedChampions => _championService.UsedChampions;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterChampions();
                }
            }
        }

        public string RemainedSearchText
        {
            get => _remainedSearchText;
            set
            {
                if (SetProperty(ref _remainedSearchText, value))
                {
                    FilterRemainedChampions();
                }
            }
        }

        public string SelectedSearchText
        {
            get => _selectedSearchText;
            set
            {
                if (SetProperty(ref _selectedSearchText, value))
                {
                    FilterSelectedChampions();
                }
            }
        }

        public string UsedSearchText
        {
            get => _usedSearchText;
            set
            {
                if (SetProperty(ref _usedSearchText, value))
                {
                    FilterUsedChampions();
                }
            }
        }

        public int RandomPickCount
        {
            get => _randomPickCount;
            set => SetProperty(ref _randomPickCount, value);
        }

        public bool IsLoaded => _championService.IsLoaded;
        public int TotalChampionCount => _championService.TotalChampionCount;
        public int CurrentChampionCount => _championService.RemainedChampions.Count;
        public int SelectedCount
        {
            get => _championService.SelectedCount;
            set
            {
                _championService.SelectedCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadChampionsCommand { get; }
        public ICommand AddSelectedCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand RevertSelectedCommand { get; }
        public ICommand CommitSelectedCommand { get; }
        public ICommand PickRandomCommand { get; }
        public ICommand RevertUsedCommand { get; }
        public ICommand RemoveUsedCommand { get; }

        public PeerlessViewModel(IChampionService championService, ILoggingService loggingService)
        {
            _championService = championService;
            _loggingService = loggingService;

            LoadChampionsCommand = new RelayCommand(async () => await LoadChampionsAsync());
            AddSelectedCommand = new RelayCommand<ChampionInfo>(AddSelected);
            RemoveSelectedCommand = new RelayCommand<ChampionInfo>(RemoveSelected);
            RevertSelectedCommand = new RelayCommand(() => _championService.RevertSelectedChampions());
            CommitSelectedCommand = new RelayCommand(() => _championService.CommitSelectedChampions());
            PickRandomCommand = new RelayCommand(PickRandom, CanPickRandom);
            RevertUsedCommand = new RelayCommand(() => _championService.RevertUsedChampions());
            RemoveUsedCommand = new RelayCommand<ChampionInfo>(RemoveUsed);

            // 챔피언 서비스의 컬렉션 변경 감지
            _championService.RemainedChampions.CollectionChanged += (s, e) =>
            {
                FilterRemainedChampions();
                OnPropertyChanged(nameof(CurrentChampionCount));
            };

            _championService.SelectedChampions.CollectionChanged += (s, e) =>
            {
                FilterSelectedChampions();
                OnPropertyChanged(nameof(SelectedChampions));
            };

            _championService.UsedChampions.CollectionChanged += (s, e) =>
            {
                FilterUsedChampions();
                OnPropertyChanged(nameof(UsedChampions));
            };

            // 초기 필터링 (데이터는 이미 App에서 로드됨)
            FilterChampions();
            FilterSelectedChampions();
            FilterUsedChampions();
        }

        private async Task LoadChampionsAsync()
        {
            if (!IsLoaded)
            {
                await _championService.InitializeChampionsAsync();
                FilterChampions();
                FilterSelectedChampions();
                FilterUsedChampions();
                OnPropertyChanged(nameof(IsLoaded));
                OnPropertyChanged(nameof(TotalChampionCount));
            }
        }

        private void AddSelected(ChampionInfo? champion)
        {
            if (champion != null)
            {
                _championService.AddSelectedChampion(champion);
            }
        }

        private void RemoveSelected(ChampionInfo? champion)
        {
            if (champion != null)
            {
                _championService.RemoveSelectedChampion(champion);
            }
        }

        private void RemoveUsed(ChampionInfo? champion)
        {
            if (champion != null)
            {
                _championService.RemoveUsedChampions(new[] { champion }.ToList());
            }
        }

        private bool CanPickRandom()
        {
            return RandomPickCount > 0 && 
                   _championService.RemainedChampions.Count >= RandomPickCount &&
                   _championService.CanSelect();
        }

        private void PickRandom()
        {
            if (CanPickRandom())
            {
                _championService.PickRandomChampions(RandomPickCount);
            }
        }

        private void FilterChampions()
        {
            FilterRemainedChampions();
        }

        private void FilterRemainedChampions()
        {
            FilteredRemainedChampions.Clear();
            
            var filtered = _championService.RemainedChampions
                .Where(c => string.IsNullOrEmpty(RemainedSearchText) || 
                           c.ChampionNameKor.Contains(RemainedSearchText, StringComparison.OrdinalIgnoreCase) || 
                           c.ChampionNameEng.Contains(RemainedSearchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.ChampionNameKor)
                .ToList();

            foreach (var champion in filtered)
            {
                FilteredRemainedChampions.Add(champion);
            }
        }

        private void FilterSelectedChampions()
        {
            FilteredSelectedChampions.Clear();
            
            var filtered = _championService.SelectedChampions
                .Where(c => string.IsNullOrEmpty(SelectedSearchText) || 
                           c.ChampionNameKor.Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase) || 
                           c.ChampionNameEng.Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var champion in filtered)
            {
                FilteredSelectedChampions.Add(champion);
            }
        }

        private void FilterUsedChampions()
        {
            FilteredUsedChampions.Clear();
            
            var filtered = _championService.UsedChampions
                .Where(c => string.IsNullOrEmpty(UsedSearchText) || 
                           c.ChampionNameKor.Contains(UsedSearchText, StringComparison.OrdinalIgnoreCase) || 
                           c.ChampionNameEng.Contains(UsedSearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var champion in filtered)
            {
                FilteredUsedChampions.Add(champion);
            }
        }
    }
}