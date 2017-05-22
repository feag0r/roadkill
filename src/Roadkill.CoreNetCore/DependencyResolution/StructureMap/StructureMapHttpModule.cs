using System;

namespace Roadkill.Core.DependencyResolution.StructureMap
{
	public class StructureMapHttpModule : IHttpModule
	{
		public void Dispose()
		{
		}

		public void Init(HttpApplication context)
		{
			context.BeginRequest += (sender, e) => LocatorStartup.Locator.CreateNestedContainer();
			context.EndRequest += (sender, e) =>
			{
				try
				{
					HttpContextLifecycle.DisposeAndClearAll();
					LocatorStartup.Locator.DisposeNestedContainer();
				}
				catch (InvalidOperationException)
				{
					// Catch HttpContextLifecycle.DisposeAndClearAll(); issues
				}
			};
		}
	}
}