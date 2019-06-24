using System;
using System.Diagnostics;
using NUnit.Framework;
using Xamarin.Forms.Controls;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract class BaseTestFixture
	{
		// TODO: Landscape tests

		public static IApp App { get; set; }

		public string PlatformViewType { get; protected set; }

		public static AppRect ScreenBounds { get; set; }

		static int s_testsrun;
		const int ConsecutiveTestLimit = 10;

		// Until we get more of our memory leak issues worked out, restart the app 
		// after a specified number of tests so we don't get bogged down in GC 
		// (or booted by jetsam)
		public void EnsureMemory()
		{
			s_testsrun += 1;

			if (s_testsrun >= ConsecutiveTestLimit)
			{
				s_testsrun = 0;

				LaunchApp();

				FixtureSetup();
			}
		}


		[SetUp]
		protected virtual void TestSetup()
		{
			EnsureMemory();
		}

		[TearDown]
		protected virtual void TestTearDown()
		{
		}

		protected abstract void NavigateToGallery();

		protected virtual void FixtureTeardown()
		{
		}

		protected virtual void FixtureSetup()
		{
			ResetApp();

			int maxAttempts = 2;
			int attempts = 0;

			while (attempts < maxAttempts)
			{
				attempts += 1;
				try
				{
					NavigateToGallery();
					return;
				}
				catch (Exception ex)
				{
					var debugMessage = $"NavigateToGallery failed: {ex}";

					Debug.WriteLine(debugMessage);
					Console.WriteLine(debugMessage);

					if (attempts < maxAttempts)
					{
						// Something has failed and we're stuck in a place where we can't navigate
						// to the test. Usually this is because we're getting network/HTTP errors 
						// communicating with the server on the device. So we'll try restarting the app.
						LaunchApp();
					}
					else
					{
						// But if it's still not working after [maxAttempts], we've got assume this is a legit
						// problem that restarting won't fix
						throw;
					}
				}
			}
		}

		public static void LaunchApp()
		{
			App = null;
			App = AppSetup.Setup();

			App.SetOrientationPortrait();
			ScreenBounds = BaseTestFixture.App.RootViewRect();
		}

		protected void ResetApp()
		{
#if __IOS__
			App.Invoke("reset:", string.Empty);
#endif
#if __ANDROID__
			App.Invoke("Reset");
#endif
#if __WINDOWS__
			WindowsTestBase.Reset();
#endif
		}
	}
}