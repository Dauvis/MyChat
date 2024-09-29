using Microsoft.Extensions.DependencyInjection;
using MyChat.Data;
using MyChat.Model;
using MyChat.Service;
using MyChat.Util;
using MyChat.ViewModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9; // Restore window if minimized

#if DEBUG
        private const string _applicationInstanceId = "{E026C9F8-9B5D-44E7-BA40-07C8F39E47F1}";
        private const string _applicationPipeName = "MyChatDebugNamedPipe";
#else
        private const string _applicationInstanceId = "{ABA1841A-9E7D-4524-897F-FA022F9EC4C7}";
        private const string _applicationPipeName = "MyChatApplicationNamedPipe";
#endif

        private static Mutex? _applicationMutex;

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
            services.AddTransient<ImageToolWindow>();

            services.AddTransient<IChatDocumentRepository, ChatDocumentRepository>();
            services.AddTransient<IUserSettingsRepository, UserSettingsRepository>();
            services.AddTransient<IImageInformationRepository, ImageInformationRepository>();

            services.AddTransient<IGPTService, GPTService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddSingleton<IDocumentService, DocumentService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IToolService, ToolService>();
            services.AddTransient<IImageService, ImageService>();

            services.AddTransient<IDialogUtil, DialogUtil>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<NewChatViewModel>();
            services.AddTransient<ImageToolViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Create SingleInstance Mutex
            _applicationMutex = new Mutex(true, _applicationInstanceId);
            var isOnlyInstance = _applicationMutex.WaitOne(TimeSpan.Zero, true);

            if (isOnlyInstance)
            {
                StartNamedPipeServer();

                base.OnStartup(e);
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();

                if (e.Args.Length > 0)
                {
                    foreach (var arg in e.Args)
                    {
                        OpenFile(arg);
                    }
                }

                // Release SingleInstance Mutex
                _applicationMutex.ReleaseMutex();
            }
            else
            {
                if (e.Args.Length > 0)
                {
                    foreach (var arg in e.Args)
                    {
                        SendFileNameToInstance(arg);
                    }
                }

                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_applicationMutex != null)
            {
                _applicationMutex.ReleaseMutex();
                _applicationMutex.Dispose();
                _applicationMutex = null;
            }

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }

        private void OpenFile(string filepath)
        {
            var documentService = _serviceProvider.GetRequiredService<IDocumentService>();
            var openedDocument = documentService.FindDocument(filepath);

            if (openedDocument is not null)
            {
                return;
            }

            documentService.OpenDocument(filepath);
        }

        private void StartNamedPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using var pipeServer = new NamedPipeServerStream(_applicationPipeName, PipeDirection.In);

                        pipeServer.WaitForConnection();
                        using var reader = new StreamReader(pipeServer);
                        string? filename = reader.ReadLine();

                        if (!string.IsNullOrWhiteSpace(filename))
                        {
                            BringExistingInstanceToFront();
                            Application.Current.Dispatcher.Invoke(() => OpenFile(filename));
                        }
                    }
                    catch (IOException)
                    {
                        // TODO: Handle disconnection or other IO exceptions gracefully
                        // If necessary, log or display information about the error
                    }
                    finally
                    {
                        // TODO: The pipe server will be disposed of after the using statement,
                        // allowing the loop to continue and wait for the next connection.
                    }
                }
            });
        }

        private void SendFileNameToInstance(string filename)
        {
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", _applicationPipeName, PipeDirection.Out);

                pipeClient.Connect(1000);

                using var writer = new StreamWriter(pipeClient);
                writer.WriteLine(filename);
                writer.Flush();
            }
            catch (TimeoutException)
            {
                Application.Current.Shutdown();
            }
            catch (IOException)
            {
                Application.Current.Shutdown();
            }
        }

        private void BringExistingInstanceToFront()
        {
            IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
        }
    }

}
