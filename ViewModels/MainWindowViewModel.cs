using NewMatchingBom.Commands;
using NewMatchingBom.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewMatchingBom.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILoggingService _loggingService;
        private readonly INavigationService _navigationService;
        private ViewModelBase? _currentViewModel;

        public HomeViewModel HomeViewModel { get; }
        public MatchingResultViewModel MatchingResultViewModel { get; }
        public PeerlessViewModel PeerlessViewModel { get; }
        public UpdateRecordViewModel UpdateRecordViewModel { get; }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ObservableCollection<string> LogEntries => _loggingService.LogEntries;

        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToMatchingResultCommand { get; }
        public ICommand NavigateToPeerlessCommand { get; }
        public ICommand NavigateToUpdateRecordCommand { get; }
        public ICommand ClearLogCommand { get; }

        public MainWindowViewModel(
            ILoggingService loggingService,
            INavigationService navigationService,
            HomeViewModel homeViewModel,
            MatchingResultViewModel matchingResultViewModel,
            PeerlessViewModel peerlessViewModel,
            UpdateRecordViewModel updateRecordViewModel)
        {
            _loggingService = loggingService;
            _navigationService = navigationService;
            HomeViewModel = homeViewModel;
            MatchingResultViewModel = matchingResultViewModel;
            PeerlessViewModel = peerlessViewModel;
            UpdateRecordViewModel = updateRecordViewModel;

            NavigateToHomeCommand = new RelayCommand(() => CurrentViewModel = HomeViewModel);
            NavigateToMatchingResultCommand = new RelayCommand(() => NavigateToMatchingResult());
            NavigateToPeerlessCommand = new RelayCommand(() => CurrentViewModel = PeerlessViewModel);
            NavigateToUpdateRecordCommand = new RelayCommand(async () => await NavigateToUpdateRecordAsync());
            ClearLogCommand = new RelayCommand(() => _loggingService.Clear());

            // 네비게이션 이벤트 구독
            _navigationService.NavigateToMatchingResult += NavigateToMatchingResult;

            // 초기 뷰 설정
            CurrentViewModel = HomeViewModel;
            
            _loggingService.LogInfo("팀뽑기툴 시작!");
        }

        private void NavigateToMatchingResult()
        {
            MatchingResultViewModel.OnNavigatedTo();
            CurrentViewModel = MatchingResultViewModel;
        }

        private async Task NavigateToUpdateRecordAsync()
        {
            CurrentViewModel = UpdateRecordViewModel;
            await UpdateRecordViewModel.OnNavigatedToAsync();
        }
    }
}