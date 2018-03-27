using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Microsoft.Toolkit.Uwp.Helpers;

namespace UniversalPlayground.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IFrameAdapter _frameAdapter;
        private readonly IComponentContext _iocResolver;
        private readonly IDictionary<Type, NavigatedToViewModelDelegate> _pageViewModels;
        private bool _isNavigating;

        private delegate Task NavigatedToViewModelDelegate(object page, 
            object parameter, 
            NavigationEventArgs navigationArgs);

        public NavigationService(IFrameAdapter frameAdapter, IComponentContext iocResolver)
        {
            _frameAdapter = frameAdapter;
            _iocResolver = iocResolver;

            _pageViewModels = new Dictionary<Type, NavigatedToViewModelDelegate>();

            _frameAdapter.Navigated += FrameAdapterOnNavigated;
        }

        private void FrameAdapterOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            IsNavigating = false;
            if (_pageViewModels.ContainsKey(navigationEventArgs.SourcePageType))
            {
                var loadViewModelDelegate = _pageViewModels[navigationEventArgs.SourcePageType];
                var ignoredTask = loadViewModelDelegate(navigationEventArgs.Content, 
                    navigationEventArgs.Parameter, navigationEventArgs);
            }
        }

        private Task NavigateToPage<TPage>()
        {
            return NavigateToPage<TPage>(parameter: null);
        }

        private async Task NavigateToPage<TPage>(object parameter)
        {
            // Early out if already in the middle of a Navigation
            if (_isNavigating)
            {
                return;
            }

            _isNavigating = true;

            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                _frameAdapter.Navigate(typeof(TPage), parameter: parameter);
            });
        }

        public void RegisterPage<TPage, TViewModel>()
            where TViewModel : class
        {
            async Task NavigatedTo(object page, object parameter, NavigationEventArgs navArgs)
            {
                if (page is IPageWithViewModel<TViewModel> pageWithViewModel)
                {
                    pageWithViewModel.ViewModel = _iocResolver.Resolve<TViewModel>();

                    if (pageWithViewModel.ViewModel is INavigableTo navigableViewModel)
                    {
                        await navigableViewModel.NavigatedTo(navArgs.NavigationMode, parameter);
                    }

                    // Async loading
                    pageWithViewModel.UpdateBindings();
                }
            }

            _pageViewModels[typeof(TPage)] = NavigatedTo;
        }

        public async Task NavigateToPageAsync<TPage>(object parameter)
        {
            await NavigateToPage<TPage>(parameter);
        }

        public async Task GoBackAsync()
        {
            if (_frameAdapter.CanGoBack)
            {
                _isNavigating = true;

                Page navigatedPage = await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    _frameAdapter.GoBack();
                    return _frameAdapter.Content as Page;
                });

            }
        }

        public event EventHandler<bool> IsNavigatingChanged;

        public event EventHandler Navigated;

        public bool CanGoBack { get; }

        public bool IsNavigating
        {
            get => _isNavigating;

            set
            {
                if (value != _isNavigating)
                {
                    _isNavigating = value;
                    IsNavigatingChanged?.Invoke(this, _isNavigating);

                    // Check that navigation just finished
                    if (!_isNavigating)
                    {
                        // Navigation finished
                        Navigated?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}