﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Saturn.Backend.Data.Services;
using MudBlazor.Services;
using Saturn.Backend.Data.Utils;


namespace Saturn.Client
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Title = "Saturn Swapper - " + Constants.UserVersion;
            
            var services = new ServiceCollection();
            services.AddBlazorWebView();

            services.AddScoped<IConfigService, ConfigService>();
            
            // NEEDS TO BE BEFORE FORTNITEAPI
            services.AddScoped<ICloudStorageService, CloudStorageService>();

            services.AddScoped<IDiscordRPCService, DiscordRPCService>();
            services.AddScoped<IFortniteAPIService, FortniteAPIService>();
            services.AddScoped<ISaturnAPIService, SaturnAPIService>(); 
            services.AddScoped<IBenBotAPIService, BenBotAPIService>();
            
            services.AddScoped<ISwapperService, SwapperService>();
            
            services.AddMudServices();

            Resources.Add("services", services.BuildServiceProvider());

            InitializeComponent();
        }
    }

    // Workaround for compiler error "error MC3050: Cannot find the type 'local:Main'"
    // It seems that, although WPF's design-time build can see Razor components, its runtime build cannot.
    public class Main
    {
    }
}