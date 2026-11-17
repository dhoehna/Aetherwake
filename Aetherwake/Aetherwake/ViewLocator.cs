using Aetherwake.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Aetherwake;
using Aetherwake.ViewModels;
using Aetherwake.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aetherwake
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        private readonly IServiceProvider _services;

        public ViewLocator(IServiceProvider services)
        {
            _services = services;
        }

        public Control? Build(object? data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data switch
            {
                MainWindowViewModel => _services.GetRequiredService<MainWindowView>(),
                FindFFXIVViewModel => _services.GetRequiredService<FindFFXIVExeView>(),
                GeneratePresetImagesViewModel => _services.GetRequiredService<GeneratePresetImagesView>(),
                _ => new TextBlock { Text = $"No view for {data.GetType().Name}" }
            };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
