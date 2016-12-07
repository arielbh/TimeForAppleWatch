using System;
using Foundation;
using HealthKit;
using UIKit;

namespace babysteps
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
			SessionManager.SharedManager.StartSession();

			return true;
		}
		HKHealthStore _healthStore = new HKHealthStore();

		public override void ShouldRequestHealthAuthorization(UIApplication application)
		{
			_healthStore.HandleAuthorizationForExtension((bool success, NSError error) =>
			{
				if (error != null && !success)
					Console.WriteLine($"You didn't allow HealthKit to access these read/write data types. In your app, try to handle this error gracefully when a user decides not to provide access. The error was: {error.LocalizedDescription}. If you're using a simulator, try it on a device.");
			});		
		}

		public override void OnResignActivation(UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		
		        ValidateAuthorization ();

		}

		private void ValidateAuthorization()
		{
			var typesToShare = new NSSet(HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate), HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned), HKObjectType.GetWorkoutType());
			var typesToRead = new NSSet(HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate), HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned));

			_healthStore.RequestAuthorizationToShare(
					typesToShare,
					typesToRead,
					ReactToHealthCarePermissions);
		}

		void ReactToHealthCarePermissions(bool success, NSError error)
		{
			//var access = _healthStore.GetAuthorizationStatus(HKObjectType.GetQuantityType(HKQuantityTypeIdentifierKey.HeartRate));
			//if (access.HasFlag(HKAuthorizationStatus.SharingAuthorized))
			//{
			//	HeartRateModel.Instance.Enabled = true;
			//}
			//else
			//{
			//	HeartRateModel.Instance.Enabled = false;
			//}
		}

		public override void WillTerminate(UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}
	}
}

