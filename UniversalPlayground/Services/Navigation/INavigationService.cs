using System;
using System.Threading.Tasks;

namespace UniversalPlayground.Services.Navigation
{
    public interface INavigationService
    {
        event EventHandler<bool> IsNavigatingChanged;

        event EventHandler Navigated;

        bool CanGoBack { get; }
        bool IsNavigating { get; }

        void RegisterPage<TPage, TViewModel>() where TViewModel : class;
        Task NavigateToPageAsync<TPage>(object parameter);
        Task GoBackAsync();
    }
}