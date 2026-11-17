using Aetherwake.ViewModels;
using Aetherwake.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Aetherwake.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Aetherwake;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        collection.AddServicesToCollection();


        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();
        DataTemplates.Add(new ViewLocator(services));

        var vm = services.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindowView
            {
                DataContext = vm
            };

            vm.SetTopLevel(TopLevel.GetTopLevel(desktop.MainWindow));
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindowView
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}