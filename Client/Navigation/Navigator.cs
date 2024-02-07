using CommunityToolkit.Mvvm.ComponentModel;
using de.hsfl.vs.hul.chatApp.client.ViewModel;

namespace de.hsfl.vs.hul.chatApp.client.Navigation;

public partial class Navigator : ObservableObject
{
    public static Navigator Instance { get; } = new();
    
    private ViewModelBase _currentView;
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set
        {
            if (_currentView == value) return;
            _currentView = value;
            OnPropertyChanged();
        }
    }

}