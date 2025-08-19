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
    public class HomeViewModel : ViewModelBase
    {
        private readonly IMemberService _memberService;
        private readonly IMatchingService _matchingService;
        private readonly ILoggingService _loggingService;
        private readonly INavigationService _navigationService;
        
        private string _searchText = string.Empty;
        private MemberCount _selectedTeamSize = MemberCount._5명;
        private bool _useLine = false;
        private bool _isLoading = false;

        public ObservableCollection<User> FilteredMembers { get; } = new();
        public ObservableCollection<User> SelectedMembers => _memberService.SelectedMembers;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterMembers();
                }
            }
        }

        public MemberCount SelectedTeamSize
        {
            get => _selectedTeamSize;
            set => SetProperty(ref _selectedTeamSize, value);
        }

        public bool UseLine
        {
            get => _useLine;
            set => SetProperty(ref _useLine, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadFromGoogleSheetsCommand { get; }
        public ICommand AddMemberCommand { get; }
        public ICommand RemoveMemberCommand { get; }
        public ICommand AddToSelectedCommand { get; }
        public ICommand RemoveFromSelectedCommand { get; }
        public ICommand ClearSelectedCommand { get; }
        public ICommand StartMatchingCommand { get; }

        public HomeViewModel(
            IMemberService memberService,
            IMatchingService matchingService,
            ILoggingService loggingService,
            INavigationService navigationService)
        {
            _memberService = memberService;
            _matchingService = matchingService;
            _loggingService = loggingService;
            _navigationService = navigationService;

            LoadFromGoogleSheetsCommand = new RelayCommand(async () => await LoadFromGoogleSheetsAsync());
            AddMemberCommand = new RelayCommand<User>(AddMember);
            RemoveMemberCommand = new RelayCommand<User>(RemoveMember);
            AddToSelectedCommand = new RelayCommand<User>(AddToSelected);
            RemoveFromSelectedCommand = new RelayCommand<User>(RemoveFromSelected);
            ClearSelectedCommand = new RelayCommand(() => {
                _memberService.ClearSelected();
                FilterMembers(); // 매칭 대상을 모두 제거한 후 전체 멤버 리스트를 다시 표시
            });
            StartMatchingCommand = new RelayCommand(StartMatching, CanStartMatching);

            // 초기 필터링 수행
            FilterMembers();
            
            // 멤버 데이터 변경 감지를 위한 이벤트 구독
            _memberService.AllMembers.CollectionChanged += (s, e) =>
            {
                // UI 스레드에서 필터링 수행
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    FilterMembers();
                    _loggingService.LogInfo($"멤버 데이터 변경 감지 - 필터링 갱신 완료: {_memberService.AllMembers.Count}명");
                });
            };
            
            _loggingService.LogInfo("HomeViewModel 초기화 완료");
        }

        private async Task LoadFromGoogleSheetsAsync()
        {
            IsLoading = true;
            try
            {
                // 1. Google Sheet로부터 멤버정보 로드 및 갱신
                _loggingService.LogInfo("구글 시트에서 멤버 정보 새로고침 중...");
                await _memberService.LoadFromGoogleSheetsAsync();
                FilterMembers();
                _loggingService.LogInfo("멤버 정보 새로고침 및 UI 갱신 완료");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"멤버 정보 새로고침 실패: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddMember(User? user)
        {
            if (user != null)
            {
                // AddMemberDialog 같은 기능이 필요하다면 별도 구현
                _loggingService.LogInfo("멤버 추가 기능 준비 중...");
            }
        }

        private void RemoveMember(User? user)
        {
            if (user != null)
            {
                _memberService.RemoveMember(user);
                FilterMembers();
            }
        }

        private void AddToSelected(User? user)
        {
            if (user != null)
            {
                _memberService.AddToSelected(user);
                FilterMembers(); // 멤버를 매칭 대상에 추가한 후 필터링을 다시 수행하여 전체 멤버 리스트에서 숨김
            }
        }

        private void RemoveFromSelected(User? user)
        {
            if (user != null)
            {
                _memberService.RemoveFromSelected(user);
                FilterMembers(); // 멤버를 매칭 대상에서 제거한 후 필터링을 다시 수행하여 전체 멤버 리스트에 다시 표시
            }
        }

        private void FilterMembers()
        {
            FilteredMembers.Clear();
            
            var filtered = _memberService.AllMembers
                .Where(m => string.IsNullOrEmpty(SearchText) || 
                           m.Name.Contains(SearchText) || 
                           m.NickName.Contains(SearchText))
                .Where(m => !_memberService.SelectedMembers.Contains(m)) // 매칭 대상에 있는 멤버는 전체 멤버 리스트에서 제외
                .ToList();

            foreach (var member in filtered)
            {
                FilteredMembers.Add(member);
            }
        }

        private bool CanStartMatching()
        {
            return SelectedMembers.Count >= (int)SelectedTeamSize * 2;
        }

        private void StartMatching()
        {
            if (!CanStartMatching())
            {
                _loggingService.LogError($"매칭을 위해 최소 {(int)SelectedTeamSize * 2}명이 필요합니다.");
                return;
            }

            var result = _matchingService.CreateMatches(
                SelectedMembers.ToList(), 
                (int)SelectedTeamSize, 
                UseLine);

            if (result.Matches.Count > 0)
            {
                _loggingService.LogInfo($"매칭 완료! {result.Matches.Count}개 게임 생성");
                
                // 매칭 완료 후 자동으로 매칭 결과 화면으로 이동
                _navigationService.RequestNavigateToMatchingResult();
            }
            else
            {
                _loggingService.LogError("매칭 실패");
            }
        }
    }
}