using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Aetherwake.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Aetherwake.ViewModels;

public partial class GeneratePresetImagesViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial INIGenerator.ColorFilter ColorFilter { get; set; } = INIGenerator.ColorFilter.Vanilla;

    [ObservableProperty]
    public partial INIGenerator.Foundation Foundation { get; set; } = INIGenerator.Foundation.Vanilla;

    [ObservableProperty]
    public partial INIGenerator.Brightness Brightness { get; set; } = INIGenerator.Brightness.Dim;

    [ObservableProperty]
    public partial INIGenerator.Contrast Contrast { get; set; } = INIGenerator.Contrast.Soft;

    [ObservableProperty]
    public partial INIGenerator.Saturation Saturation { get; set; } = INIGenerator.Saturation.Muted;

    [ObservableProperty]
    public partial INIGenerator.RayTracing RayTracing { get; set; } = INIGenerator.RayTracing.Off;

    // Original Eden ULTIMA has Texture Sharpening as an option.
    // I was unable to get this to work and removed the option from the UI.
    // Keeping this value here for this iteration as the whole app was built with
    // this enum avalible.
    public INIGenerator.TextureSharpening TextureSharpening => INIGenerator.TextureSharpening.Off;

    // Same as Texture Sharpening.
    public INIGenerator.DepthOfField DepthOfField => INIGenerator.DepthOfField.Off;

    [ObservableProperty]
    public partial Bitmap PhotoToDisplay { get; set; }

    [ObservableProperty]
    public partial bool AreImmerseProShadersPresent { get; set; }


    [ObservableProperty]
    public partial bool ShowProgressRing { get; set; }

    private readonly UltimaTechniques _techniques;

    private readonly Permutations _permutations;

    private TopLevel? _topLevel;

    [RelayCommand]
    public async Task SavePreset()
    {
        var suggestedFileName = $"Eden ULTIMA {ColorFilter} - {RayTracing}.ini";

        var file = await _topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Preset",
            SuggestedFileName = suggestedFileName
        });

        if (file is not null)
        {
            // Open writing stream from the file.
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);
            // Write some content to the file.
            await streamWriter.WriteLineAsync(GenerateIniContent());
        }
    }

    [RelayCommand]
    public async Task ChangeImage()
    {
        var imageName = $"GeneratedImages\\{Print()}.png";
        if (!File.Exists(imageName))
        {
            ShowProgressRing = true;

            await Task.Run(() =>
            {
                var presetContent = GenerateIniContent();
                GenerateImageFromSelection(presetContent);
            });

            ShowProgressRing = false;
        }

        SetImage(imageName);
    }

    public GeneratePresetImagesViewModel()
    {
        var serializer = new XmlSerializer(typeof(UltimaTechniques));
        using var stream = File.OpenRead("Techniques.xml");
        _techniques = (UltimaTechniques)serializer.Deserialize(stream)!;

        var permutationSerializer = new XmlSerializer(typeof(Permutations));
        using var permutationsStream = File.OpenRead("Permutations.xml");
        _permutations = (Permutations)permutationSerializer.Deserialize(permutationsStream)!;

        var imageStream = File.OpenRead("RunReShade/BaseImage/Before.png");
        PhotoToDisplay = new Bitmap(imageStream);

        if (!Directory.Exists("GeneratedImages"))
        {
            Directory.CreateDirectory("GeneratedImages");
        }

        var allImageNames = INIGenerator.GetAllStrings();

        AreImmerseProShadersPresent = File.Exists(@"RunReShade\reshade-shaders\Shaders\iMMERSE\MartysMods_RTGI_DIFFUSE.fx");
        ShowProgressRing = false;
    }

    public void SetTopLevel(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    private void SetImage(string imageLocation)
    {
        var imageStream = File.OpenRead(imageLocation);
        PhotoToDisplay = new Bitmap(imageStream);
    }

    private string Print()
    {
        var builder = new StringBuilder();

        builder.Append($"CFI{(int)ColorFilter}_");
        builder.Append($"FON{(int)Foundation}_");
        builder.Append($"BRI{(int)Brightness}_");
        builder.Append($"CON{(int)Contrast}_");
        builder.Append($"SAT{(int)Saturation}_");
        builder.Append($"TSH{(int)TextureSharpening}_");
        builder.Append($"DOF{(int)DepthOfField}_");
        builder.Append($"RTR{(int)RayTracing}");

        return builder.ToString();
    }

    private string GenerateIniContent()
    {
        var theSpecificPermutation = _permutations.PermutationList
            .Where(x => x.Color == ColorFilter)
            .Where(x => x.Foundation == Foundation)
            .Where(x => x.Brightness == Brightness)
            .Where(x => x.Contrast == Contrast)
            .Where(x => x.Saturation == Saturation)
            .Where(x => x.RayTracing == RayTracing)
            .Where(x => x.Sharpening == TextureSharpening)
            .Where(x => x.DepthOfField == DepthOfField)
            .First();

        var preset = new StringBuilder();
        preset.Append("Techniques=");
        preset.AppendLine(string.Join(",", theSpecificPermutation.Techniques));
        preset.Append("TechniqueSorting=");
        preset.AppendLine(theSpecificPermutation.TechniqueSorting);
        preset.AppendLine();

        foreach (var thisTechnique in theSpecificPermutation.AppliedTechniques.Techniques)
        {
            var variant = thisTechnique.Variant;
            var techniqueName = thisTechnique.TechniqueName;
            preset.Append("[");
            preset.Append(techniqueName);
            preset.AppendLine("]");

            var techniqueParameters = _techniques.GetTechnique(techniqueName, variant);
            foreach (var parameter in techniqueParameters)
            {
                preset.Append(parameter.ParameterName);
                preset.Append('=');
                preset.AppendLine(parameter.ParameterValue);
            }

            preset.AppendLine();
        }

        return preset.ToString();
    }

    private void GenerateImageFromSelection(string preset)
    {
        var imageName = $"GeneratedImages\\{Print()}.png";
        File.WriteAllText("GeneratedPreset.ini", preset);

        var runReShadeProcessInfo = new ProcessStartInfo();
        runReShadeProcessInfo.UseShellExecute = false;
        runReShadeProcessInfo.ArgumentList.Add(Path.GetFullPath(imageName));
        runReShadeProcessInfo.ArgumentList.Add(Path.GetFullPath("GeneratedPreset.ini"));
        runReShadeProcessInfo.FileName = @"RunReShade\RunReShade.exe";
        runReShadeProcessInfo.WorkingDirectory = "RunReShade";

        var runReShadeProcess = Process.Start(runReShadeProcessInfo);
        runReShadeProcess!.WaitForExit();
        SetImage(imageName);
    }
}
