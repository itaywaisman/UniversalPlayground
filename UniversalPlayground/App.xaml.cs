using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Autofac;
using UniversalPlayground.Helpers;
using UniversalPlayground.Services;
using UniversalPlayground.Services.Navigation;
using UniversalPlayground.Views;

namespace UniversalPlayground
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        private IContainer _container;
        private BackgroundTaskDeferral appServiceDeferral;
        private NavigationRoot rootPage;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
        }
        public static AppServiceConnection Connection { get; set; }

        public NavigationRoot GetNavigationRoot()
        {
            if (Window.Current.Content is NavigationRoot)
            {
                return Window.Current.Content as NavigationRoot;
            }
            else if (Window.Current.Content is Frame)
            {
                return ((Frame)Window.Current.Content).Content as NavigationRoot;
            }

            throw new Exception("Window content is unknown type");
        }

        public Frame GetFrame()
        {
            var root = GetNavigationRoot();
            return root.AppFrame;
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            //XBOX support
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                bool result = ApplicationViewScaling.TrySetDisableLayoutScaling(true);
            }

            await InitializeAsync();
            InitWindow(skipWindowCreation: args.PrelaunchActivated);

            // Tasks after activation
            await StartupAsync();

            await Window.Current.Dispatcher.RunIdleAsync(async (s) =>
                await BackgroundDownloadHelper.AttachToDownloads());
        }
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            await InitializeAsync();
            InitWindow(skipWindowCreation: false);

            if (args.Kind == ActivationKind.Protocol)
            {
                Window.Current.Activate();

                // Tasks after activation
                await StartupAsync();
            }
        }
        

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            Console.WriteLine(args.Request.Message);
            deferral.Complete();
            appServiceDeferral.Complete();
        }

        private void OnAppServicesCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            appServiceDeferral.Complete();
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            appServiceDeferral.Complete();
        }

        private void InitWindow(bool skipWindowCreation)
        {
            var builder = new ContainerBuilder();

            rootPage = Window.Current.Content as NavigationRoot;
            bool initApp = rootPage == null && !skipWindowCreation;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (initApp)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootPage = new NavigationRoot();

                FrameAdapter adapter = new FrameAdapter(rootPage.AppFrame);

                builder.RegisterInstance(adapter)
                        .AsImplementedInterfaces();

//                builder.RegisterType<HomeViewModel>();

                // The feed details view model needs to be a singleton in order to better accomodate Connected Animation
//                builder.RegisterType<FeedDetailsViewModel>()
//                    .SingleInstance();
//                builder.RegisterType<EpisodeDetailsViewModel>();
//                builder.RegisterType<PlayerViewModel>();
//                builder.RegisterType<InkNoteViewModel>();
//                builder.RegisterType<FavoritesViewModel>();
//                builder.RegisterType<NotesViewModel>();
//                builder.RegisterType<DownloadsViewModel>();
//                builder.RegisterType<SettingsViewModel>();

                builder.RegisterType<NavigationService>()
                        .AsImplementedInterfaces()
                        .SingleInstance();
                
                _container = builder.Build();
                rootPage.InitializeNavigationService(_container.Resolve<INavigationService>());

                adapter.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootPage;

                Window.Current.Activate();
            }
        }

        private async Task InitializeAsync()
        {
            await ThemeSelectorService.InitializeAsync();
            await Task.CompletedTask;
        }

        private async Task StartupAsync()
        {
            ThemeSelectorService.SetRequestedTheme();
            
            await Task.CompletedTask;
        }
        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
