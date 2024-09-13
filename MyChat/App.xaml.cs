using Microsoft.Extensions.DependencyInjection;
using MyChat.Data;
using MyChat.Service;
using MyChat.Util;
using MyChat.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<NewChatWindow>();

            services.AddTransient<IChatDocumentRepository, ChatDocumentRepository>();
            services.AddTransient<IUserSettingsRepository, UserSettingsRepository>();

            services.AddTransient<IGPTService, GPTService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IDocumentService, DocumentService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddTransient<IDialogUtil, DialogUtil>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<NewChatViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }

}
