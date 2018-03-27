using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace UniversalPlayground.Services.Navigation
{
    public interface INavigableTo
    {
        Task NavigatedTo(NavigationMode navigationMode, object parameter);
    }
}