using ALTRAFLOAT.ViewModels;

namespace ALTRAFLOAT.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
