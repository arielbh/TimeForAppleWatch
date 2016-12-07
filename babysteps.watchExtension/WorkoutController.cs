using Foundation;
using System;
using WatchKit;
using HealthKit;
using System.Collections.Generic;
using CoreFoundation;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace babysteps.watchExtension
{
    public partial class WorkoutController : WKInterfaceController, IHKWorkoutSessionDelegate
    {
		private Subject<double> _dangerousHeartRate = new Subject<double>();

        public WorkoutController (IntPtr handle) : base (handle)
        {
        }

		public override void WillActivate()
		{
			base.WillActivate();
			ResetUI();
			if (!HKHealthStore.IsHealthDataAvailable)
			{
				return;
			}
			// We need to be able to write workouts, so they display as a standalone workout in the Activity app on iPhone.
			// We also need to be able to write Active Energy Burned to write samples to HealthKit to later associating with our app.

			var typesToShare = new NSSet(HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate), HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned), HKObjectType.GetWorkoutType());
			var typesToRead = new NSSet(HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate), HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned));

			HealthStore.RequestAuthorizationToShare(typesToShare, typesToRead, (bool success, NSError error) =>
			{
				if (error != null && !success)
					Console.WriteLine("You didn't allow HealthKit to access these read/write data types. " +
									  "In your app, try to handle this error gracefully when a user decides not to provide access. " +
									  $"The error was: {error.LocalizedDescription}. If you're using a simulator, try it on a device.");
			});
			_dangerousHeartRate.Where(v => v > 30.0).Subscribe(
				v => SessionManager.SharedManager.UpdateApplicationContext(
					new Dictionary<string, object>() { { "HeartRate", v.ToString() } })); 
		}

		private void ResetUI()
		{
			ToggleWorkoutButton.SetTitle("Crawl Baby Crawl");
			EnergyLabel.SetText("Energy: 0.0");
			HeartRateLabel.SetText("Heart Rate:N/A");
		}

		partial void OnToggleWorkout()
		{

			if (!IsWorkoutRunning && CurrentWorkoutSession == null)
			{
				// Begin workoutt
				IsWorkoutRunning = true;
				ToggleWorkoutButton.SetTitle("Rest little Baby");;

				// Clear the local Active Energy Burned quantity when beginning a workout session
				CurrentActiveEnergyQuantity = HKQuantity.FromQuantity(HKUnit.Kilocalorie, 0.0);
				CurrentHeartRate = HKQuantity.FromQuantity(HKUnit.FromString("count/min"), 0.0);

				CurrentQuery = null;
				HeartRateQuery = null;
				ActiveEnergySamples = new List<HKSample>();
				HeartRateSamples = new List<HKSample>();

				// An indoor walk workout session. There are other activity and location types available to you.

				// Create a workout configuratio
				var configuration = new HKWorkoutConfiguration
				{
					ActivityType = HKWorkoutActivityType.Walking, // Why not crawling? :
					LocationType = HKWorkoutSessionLocationType.Indoor
				};

				NSError error = null;
				CurrentWorkoutSession = new HKWorkoutSession(configuration, out error)
				{
					Delegate = this
				};

				HealthStore.StartWorkoutSession(CurrentWorkoutSession);
			}
			else
			{

				HealthStore.EndWorkoutSession(CurrentWorkoutSession);
				IsWorkoutRunning = false;
				ResetUI(); 
			}

		}


		public void DidChangeToState(HKWorkoutSession workoutSession, HKWorkoutSessionState toState, HKWorkoutSessionState fromState, NSDate date)
		{
			DispatchQueue.MainQueue.DispatchAsync(delegate
			{
				// Take action based on the change in state
				switch (toState)
				{
					case HKWorkoutSessionState.Running:
						BeginWorkout((DateTime)date);
						break;
					case HKWorkoutSessionState.Ended:
						EndWorkout((DateTime)date);
						break;
					default:
						Console.WriteLine($"Unexpected workout session: {toState}.");
						break;
				}
			});
		}

		public void BeginWorkout(DateTime beginDate)
		{
			// Obtain the `HKObjectType` for active energy burned and the `HKUnit` for kilocalories.
			var activeEnergyType = HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned);
			if (activeEnergyType == null)
				return;
			// Obtain the `HKObjectType` for HeartRate.
			var heartRateType = HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate);
			if (heartRateType == null) return;


			var energyUnit = HKUnit.Kilocalorie;
			var heartRateUnit = HKUnit.FromString("count/min");
			// Update properties.
			WorkoutBeginDate = beginDate;

			// Set up a predicate to obtain only samples from the local device starting from `beginDate`.

			var datePredicate = HKQuery.GetPredicateForSamples((NSDate)beginDate, null, HKQueryOptions.None);

			var devices = new NSSet<HKDevice>(new HKDevice[] { HKDevice.LocalDevice });
			var devicePredicate = HKQuery.GetPredicateForObjectsFromDevices(devices);
			var predicate = NSCompoundPredicate.CreateAndPredicate(new NSPredicate[] { datePredicate, devicePredicate });

			//Create a results handler to recreate the samples generated by a query of active energy samples so that they can be associated with this app in the move graph.It should be noted that if your app has different heuristics for active energy burned you can generate your own quantities rather than rely on those from the watch.The sum of your sample's quantity values should equal the energy burned value provided for the workout
			Action<List<HKSample>> sampleHandler;
			sampleHandler = (List<HKSample> samples) =>
			{
				DispatchQueue.MainQueue.DispatchAsync(delegate
				{
					var accumulatedSamples = new List<HKQuantitySample>();

					var initialActivityEnergy = CurrentActiveEnergyQuantity.GetDoubleValue(energyUnit);
					double accumulatedValue = initialActivityEnergy;
					foreach (HKQuantitySample sample in samples)
					{
						accumulatedValue = accumulatedValue + sample.Quantity.GetDoubleValue(energyUnit);
						var ourSample = HKQuantitySample.FromType(activeEnergyType, sample.Quantity, sample.StartDate, sample.EndDate);
						accumulatedSamples.Add(ourSample);
					}

					// Update the UI.
					CurrentActiveEnergyQuantity = HKQuantity.FromQuantity(energyUnit, accumulatedValue);
					EnergyLabel.SetText($"Energy: {accumulatedValue.ToString("F")}");

					// Update our samples.
					ActiveEnergySamples.AddRange(accumulatedSamples);
				});
			};

			// Create a query to report new Active Energy Burned samples to our app.
			var activeEnergyQuery = new HKAnchoredObjectQuery(activeEnergyType, predicate, null, HKSampleQuery.NoLimit, (query, addedObjects, deletedObjects, newAnchor, error) =>
			{
				if (error == null)
				{
					// NOTE: `deletedObjects` are not considered in the handler as there is no way to delete samples from the watch during a workout
					ActiveEnergySamples = new List<HKSample>(addedObjects);
					sampleHandler(ActiveEnergySamples);

				}
				else
				{
					Console.WriteLine($"An error occured executing the query. In your app, try to handle this gracefully. The error was: {error}.");
				}
			});

			// Assign the same handler to process future samples generated while the query is still active.
			activeEnergyQuery.UpdateHandler = (query, addedObjects, deletedObjects, newAnchor, error) =>
			{
				if (error == null)
				{
					ActiveEnergySamples = new List<HKSample>(addedObjects);
					sampleHandler(ActiveEnergySamples);
				}
				else
				{
					Console.WriteLine($"An error occured executing the query. In your app, try to handle this gracefully. The error was: {error}.");
				}
			};

			// Start Query
			CurrentQuery = activeEnergyQuery;
			HealthStore.ExecuteQuery(activeEnergyQuery);

			Action<List<HKSample>> heartRateSampler = (List<HKSample> samples) =>
			{
				foreach (HKQuantitySample sample in samples)
				{
					_dangerousHeartRate.OnNext(sample.Quantity.GetDoubleValue(heartRateUnit));
				}
				DispatchQueue.MainQueue.DispatchAsync(delegate
				{
					var sample = samples.LastOrDefault() as HKQuantitySample;
					if (sample != null)
					{
						HeartRateLabel.SetText($"Heart Rate: {sample.Quantity.GetDoubleValue(heartRateUnit).ToString()}");
					}
				});
			};

			HeartRateQuery = new HKAnchoredObjectQuery(heartRateType, predicate, null, HKSampleQuery.NoLimit, (query, addedObjects, deletedObjects, newAnchor, error) =>
			{
				if (error == null)
				{
					// NOTE: `deletedObjects` are not considered in the handler as there is no way to delete samples from the watch during a workout
					sampleHandler(new List<HKSample>(addedObjects));

				}
				else
				{
					Console.WriteLine($"An error occured executing the query. In your app, try to handle this gracefully. The error was: {error}.");
				}
			});
			// Assign the same handler to process future samples generated while the query is still active.
			(HeartRateQuery as HKAnchoredObjectQuery).UpdateHandler = (query, addedObjects, deletedObjects, newAnchor, error) =>
			{
				if (error == null)
				{
					heartRateSampler(new List<HKSample>(addedObjects));

				}
				else
				{
					Console.WriteLine($"An error occured executing the query. In your app, try to handle this gracefully. The error was: {error}.");
				}
			};
			HealthStore.ExecuteQuery(HeartRateQuery);
		}

		public void EndWorkout(DateTime endDate)
		{
			WorkoutEndDate = endDate;
			ResetUI();
			if (CurrentQuery != null)
			{
				var query = CurrentQuery;
				HealthStore.StopQuery(query);
			}
				if (HeartRateQuery != null)
			{
				HealthStore.StopQuery(HeartRateQuery);
			}
			SaveWorkout();
		}

		public void DidFail(HKWorkoutSession workoutSession, NSError error)
		{
			Console.WriteLine($"An error occured with the workout session. In your app, try to handle this gracefully. The error was: {error}.");
		}

		public void SaveWorkout()
		{
			// Obtain the `HKObjectType` for active energy burned.
			var activeEnergyType = HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned);
			if (activeEnergyType == null) return;

			// Obtain the `HKObjectType` for HeartRate.
			var heartRateType = HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate);
			if (heartRateType == null) return;

			var beginDate = WorkoutBeginDate;
			var endDate = WorkoutEndDate;

			var timeDifference = endDate.Subtract(beginDate);
			double duration = timeDifference.TotalSeconds;
			NSDictionary metadata = null;

			var workout = HKWorkout.Create(HKWorkoutActivityType.Walking,
										   (NSDate)beginDate,
										   (NSDate)endDate,
										   duration,
										   CurrentActiveEnergyQuantity,
			                               HKQuantity.FromQuantity(HKUnit.Meter, 0.0),
										   metadata);

			var finalActiveEnergySamples = ActiveEnergySamples;
			var finalHeartRateSamples = HeartRateSamples;

			if (HealthStore.GetAuthorizationStatus(activeEnergyType) != HKAuthorizationStatus.SharingAuthorized ||
				HealthStore.GetAuthorizationStatus(heartRateType) != HKAuthorizationStatus.SharingAuthorized ||
				HealthStore.GetAuthorizationStatus(HKObjectType.GetWorkoutType()) != HKAuthorizationStatus.SharingAuthorized)
				return;

			HealthStore.SaveObject(workout, (success, error) =>
			{
				if (!success)
				{
					Console.WriteLine($"An error occured saving the workout. In your app, try to handle this gracefully. The error was: {error}.");
					return;
				}

				if (finalActiveEnergySamples.Count > 0)
				{
					HealthStore.AddSamples(finalActiveEnergySamples.ToArray(), workout, (addSuccess, addError) =>
					{
						// Handle any errors
						if (addError != null)
							Console.WriteLine($"An error occurred adding the samples. In your app, try to handle this gracefully. The error was: {error.ToString()}.");
					});
				}
				if (finalHeartRateSamples.Count > 0)
				{
					HealthStore.AddSamples(finalHeartRateSamples.ToArray(), workout, (addSuccess, addError) =>
					{
						// Handle any errors
						if (addError != null)
							Console.WriteLine($"An error occurred adding the samples. In your app, try to handle this gracefully. The error was: {error.ToString()}.");
					});
				}
			});
		}
	

		public HKQuantity CurrentActiveEnergyQuantity { get; set; } = HKQuantity.FromQuantity(HKUnit.Kilocalorie, 0.0);
		public HKQuantity CurrentHeartRate { get; set; } = HKQuantity.FromQuantity(HKUnit.FromString("count/min"), 0.0);
		public List<HKSample> ActiveEnergySamples { get; set; } = new List<HKSample>();
		public List<HKSample> HeartRateSamples { get; set; } = new List<HKSample>();
		public HKQuery CurrentQuery { get; set; }
		public HKQuery HeartRateQuery { get; set; }
		public bool IsWorkoutRunning { get; set; } = false;
		public DateTime WorkoutBeginDate { get; set; }
		public DateTime WorkoutEndDate { get; set; }
		public HKHealthStore HealthStore { get; set; } = new HKHealthStore();
		public HKWorkoutSession CurrentWorkoutSession { get; set; }

	}
}