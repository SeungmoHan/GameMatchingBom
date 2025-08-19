using Microsoft.Extensions.DependencyInjection;
using NewMatchingBom.Services;
using NewMatchingBom.ViewModels;
using NewMatchingBom.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace NewMatchingBom
{
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISpreadSheetService, SpreadSheetService>();
            services.AddSingleton<IMemberService, MemberService>();
            services.AddSingleton<IMatchingService, MatchingService>();
            services.AddSingleton<IChampionService, ChampionService>();

            // ViewModels (MainWindowViewModel을 먼저 등록)
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<MatchingResultViewModel>();
            services.AddTransient<PeerlessViewModel>();
            services.AddTransient<UpdateRecordViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            // 서비스 초기화 (데이터 로딩)
            InitializeServicesAsync();

            var mainWindow = new MainWindow();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void InitializeServicesAsync()
        {
            try
            {
                var loggingService = ServiceProvider?.GetService<ILoggingService>();
                loggingService?.LogInfo("🎆 애플리케이션 초기화 시작...");

                // MemberService 초기화 (멤버 데이터 및 PlayRecord 로드)
                var memberService = ServiceProvider?.GetService<IMemberService>();
                if (memberService != null)
                {
                    memberService.InitializeAsync();
                }

                // ChampionService는 필요할 때 초기화 (피어리스 뷰에서)
                loggingService?.LogInfo("ChampionService는 피어리스 뷰에서 필요할 때 초기화됩니다.");

                loggingService?.LogInfo("✅ 애플리케이션 초기화 완료!");
            }
            catch (Exception ex)
            {
                var loggingService = ServiceProvider?.GetService<ILoggingService>();
                loggingService?.LogError($"❌ 초기화 실패: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
                disposable.Dispose();
            base.OnExit(e);
        }
    }
}