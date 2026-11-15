using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ALTRAFLOAT.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private string _status = "Ready";

    [RelayCommand]
    private void Increment()
    {
        Count++;
        Status = Count == 1
            ? "Tapped 1 time"
            : $"Tapped {Count} times";
    }
}
