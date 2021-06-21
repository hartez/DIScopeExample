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
			using IHost host = CreateHostBuilder(args).Build();

			// The top level services
			var appServices = host.Services;
			var app = appServices.GetRequiredService<IApplication>();
			
			// Create a scope for the Window
			var windowServices = appServices.CreateScope().ServiceProvider;

			// Get a Window
			var window0 = windowServices.GetRequiredService<IWindow>();
			
			// When AlertServices needs a window, it'll be the same Window
			var alertService = windowServices.GetRequiredService<IAlertService>();

			alertService.Alert("This alert should be in Window 0");

			// Create a second service provider to give to a new Window
			var secondWindowServices = appServices.CreateScope().ServiceProvider;

			var alertServices1 = secondWindowServices.GetRequiredService<IAlertService>();
			var window1 = secondWindowServices.GetRequiredService<IWindow>();

			alertServices1.Alert("This alert should be in Window 1");

			return host.RunAsync();
		}

		static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				.ConfigureServices((_, services) => services
					.AddSingleton<IApplication, App>()
					//.AddScoped<ISer>
					.AddScoped<IWindow, Window>()
					.AddScoped<IAlertService, AlertService>());
	}

	public interface IApplication : IHost
	{
		
	}

	public class App : IApplication, IHost
	{
		IHost _host;

		public IServiceProvider Services => _host.Services;

		static IHostBuilder CreateHostBuilder(string[] args) =>
		   Host.CreateDefaultBuilder(args)
			   .ConfigureServices((_, services) =>
				   services.AddTransient<IWindow, Window>());

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

		public App() 
		{
			_host = CreateHostBuilder(null).Build();
		}
	}

	public interface IWindow
	{
		void ShowDialog(string title);
	}

	public interface IAlertService
	{
		void Alert(string message);
	}

	public class Window : IWindow
	{
		private IApplication _app;
		static int s_id;

		int _id;

		public Window(IApplication app)
		{
			_app = app;
			_id = s_id;
			s_id += 1;
		}

		public void ShowDialog(string title)
		{
			Console.WriteLine($">>>>> Window {_id} showing dialog: {title}");
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
