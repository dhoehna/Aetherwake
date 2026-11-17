using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aetherwake.ViewModels;

public partial class FindFFXIVViewModel : ViewModelBase
{
    public event EventHandler? Completed;

    [ObservableProperty]
    public partial bool IsSearchingForExe { get; set; } = false;

    [RelayCommand]
    public async Task FindFFXIVExe(TopLevel topLevel)
    {
        IsSearchingForExe = true;
        var thisLocation = await _topLevel!.StorageProvider.TryGetFolderFromPathAsync(Path.Combine(AppContext.BaseDirectory, "RunReShade"));
        var finalXivExeFolder = await _topLevel!.StorageProvider.TryGetFolderFromPathAsync(@"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\game");
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Find ffxiv_dx11.exe",
            SuggestedStartLocation = finalXivExeFolder,
        });

        if (files.Count == 0)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("No file selected", "No file selected.  Please find ffxiv_dx11.exe",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        if (files.Count > 1)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Multiple files selected", "Please select only ffxiv_dx11.exe",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        var fileName = files[0].Name;
        if (!fileName.Equals("ffxiv_dx11.exe", System.StringComparison.OrdinalIgnoreCase))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Not ffxiv_dx11.exe", "THe name of the file is not ffxiv_dx11.exe.  Please find ffxiv_dx11.exe",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        // file points to FFXIV exe.  check for dxgi and shaders directory.
        var filePath = Path.GetDirectoryName(files[0].Path.LocalPath);
        var realFFXIVLocation = await _topLevel.StorageProvider.TryGetFolderFromPathAsync(filePath);

        var dxgiFile = await realFFXIVLocation!.GetFileAsync("dxgi.dll");
        if (dxgiFile == null)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Could not find dxgi.dll", "dxgi.dll not found.  Please install ReShade.",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        var destinationFile = await thisLocation!.CreateFileAsync("dxgi.dll");
        using (var sourceStream = await dxgiFile.OpenReadAsync())
        using (var destinationStream = await destinationFile!.OpenWriteAsync())
        {
            await sourceStream.CopyToAsync(destinationStream);
            await destinationStream.FlushAsync();
        }


        var reshadeShadersFolderPath = Path.Combine(filePath, "reshade-shaders");
        var reshadeShaders = await _topLevel.StorageProvider.TryGetFolderFromPathAsync(reshadeShadersFolderPath);
        if (reshadeShaders == null)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Could not find reshade-shaders", $"reshade-shaders not found at {filePath}",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        var allTheShadersPath = Path.Combine(reshadeShadersFolderPath, "Shaders");
        var allTheShaders = await _topLevel.StorageProvider.TryGetFolderFromPathAsync(allTheShadersPath);
        if (allTheShaders == null)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Could not find Shaders", $"Shaders folder not found at {allTheShadersPath}",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        // Shaders and dxgi.dll have been found.  Copy them to the exe's location.
        try
        {
            if(Directory.Exists(@"RunReShade\reshade-shaders"))
            {
                foreach(var shader in Directory.EnumerateFiles(@"RunReShade\reshade-shaders", "*.*", SearchOption.AllDirectories))
                {
                    var originalAttributes = File.GetAttributes(shader);
                    if(originalAttributes.HasFlag(FileAttributes.ReadOnly))
                    {
                        File.SetAttributes(shader, originalAttributes & ~FileAttributes.ReadOnly);
                    }
                }
            }
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(reshadeShadersFolderPath, @"RunReShade\reshade-shaders", true);
        }
        catch(Exception e)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Could not copy files", e.ToString(),
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            IsSearchingForExe = false;
            return;
        }

        IsSearchingForExe = false;

        // switch to Generate UI.
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private TopLevel? _topLevel;

    public void SetTopLevel(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }
}
