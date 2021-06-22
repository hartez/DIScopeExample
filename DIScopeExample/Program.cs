using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace DIScopeExample
{
	class Program
	{
		static Task Main(string[] args)
		{
			var app = new App();

			// Get a Window
			var window0 = app.Services.GetRequiredService<IWindow>(); // Could also just be IApp.CreateWindow for clarity
			
			// When AlertServices needs a window, it'll be the same Window
			var alertService = window0.GetRequiredService<IAlertService>();

			alertService.Alert("This alert should be in Window 0");

			// Create a Window and get an alert service from it
			var window1 = app.Services.GetRequiredService<IWindow>();
			var alertServices1 = window1.GetRequiredService<IAlertService>();

			alertServices1.Alert("This alert should be in Window 1");

			return app.RunAsync();
		}

		
	}

	public interface IApplication : IHost
	{
		
	}

	public class App : IApplication
	{
		// This should let folks do whatever it is we already do to let them add things to the services collection
		static IHostBuilder CreateHostBuilder(string[] args) =>
			   Host.CreateDefaultBuilder(args)
				   .ConfigureServices((_, services) => services
					   .AddTransient<IWindow, Window>());

		readonly IHost _host;

		public IServiceProvider Services => _host.Services;

		public Task StartAsync(CancellationToken cancellationToken = default)
		{
			return _host.StartAsync(cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken = default)
		{
			return _host.StopAsync(cancellationToken);
		}

		public void Dispose()
		{
			_host.Dispose();
		}

		public object GetService(Type serviceType)
		{
			return _host.Services.GetService(serviceType);
		}

		public App() 
		{
			_host = CreateHostBuilder(null).ConfigureServices((_, services) =>
					   services.AddSingleton<IApplication>(sp => this))
					   .Build();
		}
	}

	public interface IWindow : IServiceProvider
	{
		void ShowDialog(string title);
	}

	public interface IAlertService
	{
		void Alert(string message);
	}

	public class Window : IWindow
	{
		// This should let folks do whatever it is we already do to let them add things to the services collection
		static IHostBuilder CreateHostBuilder(string[] args) =>
			   Host.CreateDefaultBuilder(args)
				   .ConfigureServices((_, services) => services
				   .AddScoped<IAlertService, AlertService>());

		readonly IHost _host;

		static int s_id;
		int _id;

		public IApplication App { get; }

		public Window(IApplication app)
		{
			App = app;
			_id = s_id;
			s_id += 1;

			_host = CreateHostBuilder(null).ConfigureServices((_, services) =>
					   services.AddSingleton<IWindow>(sp => this))
					   .Build();
		}

		public void ShowDialog(string title)
		{
			Console.WriteLine($">>>>> Window {_id} showing dialog: {title}");
		}

		public object GetService(Type serviceType)
		{
			return _host.Services.GetService(serviceType) ?? App.Services.GetService(serviceType);
		}
	}

	public class AlertService : IAlertService
	{
		private IWindow _window;

		public AlertService(IWindow window)
		{
			_window = window;
		}

		public void Alert(string message)
		{
			_window.ShowDialog(message);
		}
	}
}
