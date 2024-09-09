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
            services.AddSingleton<IChatDocumentRepository, ChatDocumentRepository>();
            services.AddSingleton<IGPTService, GPTService>();
            services.AddSingleton<IChatService, ChatService>();
            services.AddSingleton<IDocumentService, DocumentService>();
            services.AddSingleton<IDialogUtil, DialogUtil>();
            services.AddTransient<MainViewModel>();
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
