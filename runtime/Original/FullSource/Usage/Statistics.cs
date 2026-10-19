using Crisis.Runtime.FullSource;
using UnityEngine;

namespace Usage
{
	public class Statistics : MonoBehaviour
	{
		public UnityEventBool receivedFinalSessionDataCallback = new UnityEventBool();

		public SerializableClientData client = new SerializableClientData();

		public SerializableSessionData session = new SerializableSessionData();

		private float durationTimestamp;

		public bool RecordUsage
		{
			get
			{
				return client.recordData;
			}
		}

		public bool VersionChecked { get; set; }

		public int ScoreSum { get; private set; }

		public int ScoreSamples { get; private set; }

		public int AverageScore
		{
			get
			{
				return (ScoreSamples != 0) ? (ScoreSum / ScoreSamples) : 0;
			}
		}

		private void Start()
		{
			client.Load();
			VerifyPreviousSession();
			PersistGUID();
		}

		private void VerifyPreviousSession()
		{
			if (!client.previousSessionClosed && RecordUsage)
			{
				Global.Context.Connection.PushRequest(new PostClose(client.previousSessionId));
			}
			client.previousSessionClosed = false;
			client.Save();
		}

		public void PersistSessionData(bool full = true)
		{
			if (RecordUsage)
			{
				RecalculateDerivedSessionValues();
				Global.Context.Connection.PushRequest(new PostSession(full, SystemInfo.deviceUniqueIdentifier, session, PersistSessionCallback));
				PersistStaticInformation();
			}
			QueryLatestVersion();
		}

		private void PersistSessionCallback(HTTPRequest request)
		{
			if (!request.StatusOK && !request.EmptyOrError && uint.TryParse(request.responseData, out session.id))
			{
				client.previousSessionId = session.id;
				client.Save();
			}
		}

		private void PersistGUID()
		{
			Global.Context.Connection.PushRequest(new PostClient(SystemInfo.deviceUniqueIdentifier, PersistGUIDCallback));
		}

		private void PersistGUIDCallback(HTTPRequest request)
		{
			if (request.ErrorCode == "R0")
			{
				Debug.LogError("Oops! Dying with code R0.");
				Application.Quit();
			}
			else if (request.EmptyOrError)
			{
				return;
			}
			PersistSessionData(false);
		}

		public void PersistSessionClose()
		{
			if (RecordUsage)
			{
				Global.Context.Connection.PushRequest(new PostClose(session.id, PersisSessionCloseCallback));
			}
		}

		private void PersisSessionCloseCallback(HTTPRequest request)
		{
			if (request.StatusOK)
			{
				client.previousSessionClosed = true;
				client.Save();
			}
			if (receivedFinalSessionDataCallback != null)
			{
				receivedFinalSessionDataCallback.Invoke(request.StatusOK);
			}
		}

		private void QueryLatestVersion()
		{
			if (!VersionChecked)
			{
				Global.Context.Connection.PushRequest(new GetLatestVersion(QueryLatestVersionCallback));
			}
		}

		private void QueryLatestVersionCallback(HTTPRequest request)
		{
			GlobalContext context = Global.Context;
			if (request.responseArray.Length != 2)
			{
				Debug.LogWarning("Received version string is in wrong format.");
				return;
			}
			string text = request.responseArray[0];
			string downloadURL = request.responseArray[1];
			if (Global.VersionIsNewer(text))
			{
				context.downloadURL = downloadURL;
				if (context.MainMenu != null)
				{
					context.MainMenu.newVersionNotice.Display(text);
				}
			}
			context.Usage.VersionChecked = true;
		}

		private void PersistStaticInformation()
		{
			if (!client.reportedHardware)
			{
				Global.Context.Connection.PushRequest(new PostStaticInformation(SystemInfo.deviceUniqueIdentifier, Application.systemLanguage.ToString(), PersistStaticInformationCallback));
			}
		}

		private void PersistStaticInformationCallback(HTTPRequest request)
		{
			if (request.StatusOK)
			{
				client.reportedHardware = true;
				client.Save();
			}
		}

		public void RecordMissionFramerate()
		{
			PerformanceCounter instance = PerformanceCounter.Instance;
			instance.StopMeasurement();
			int num = instance.MeasurementAverage();
			session.lowFrameRateAverage = ((session.lowFrameRateAverage != 0) ? Mathf.Min(session.lowFrameRateAverage, num) : num);
			session.highFrameRateAverage = ((session.highFrameRateAverage != 0) ? Mathf.Max(session.highFrameRateAverage, num) : num);
			Debug.LogFormat("Average frame rate for this mission: {0}", num);
		}

		public void RecordMissionScore(int score)
		{
			ScoreSum += score;
			ScoreSamples++;
		}

		private void RecalculateDerivedSessionValues()
		{
			session.duration = Mathf.FloorToInt(Time.realtimeSinceStartup);
			durationTimestamp = Time.realtimeSinceStartup;
			session.averageScore = AverageScore;
		}
	}
}
