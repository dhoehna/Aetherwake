using Aetherwake.Models;
using Aetherwake.ViewModels;
using Aetherwake;
using Aetherwake.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherwake
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServicesToCollection(this IServiceCollection collection)
        {
            AddViewModels(collection);
            AddViews(collection);
            AddModels(collection);
        }
        private static void AddViewModels(IServiceCollection collection)
        {
            collection.AddSingleton<MainWindowViewModel>();
            collection.AddSingleton<FindFFXIVViewModel>();
            collection.AddSingleton<GeneratePresetImagesViewModel>();
        }

        private static void AddViews(this IServiceCollection collection)
        {
            collection.AddSingleton<MainWindowView>();
            collection.AddSingleton<FindFFXIVExeView>();
            collection.AddSingleton<GeneratePresetImagesView>();
        }

        private static void AddModels(this IServiceCollection collection)
        {
            collection.AddSingleton<Configuration>();
        }
    }

}
