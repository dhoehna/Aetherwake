using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aetherwake.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly FindFFXIVViewModel _findFFXIV = new();

    private readonly GeneratePresetImagesViewModel _generatePresetImages = new();

    [ObservableProperty]
    public partial ViewModelBase CurrentViewModel { get; set; }

    public MainWindowViewModel()
    {
        CurrentViewModel = _findFFXIV;
        _findFFXIV.Completed += (_, _) =>
        {
            _generatePresetImages.ChangeImage();
            CurrentViewModel = _generatePresetImages;
        };
    }

    public void SetTopLevel(TopLevel topLevel)
    {
        _findFFXIV.SetTopLevel(topLevel);
        _generatePresetImages.SetTopLevel(topLevel);
    }
}
