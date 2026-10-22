using System.Diagnostics;
using System.Threading;

namespace Threading
{
	public class ThreadedJob
	{
		protected double timeoutMilliseconds = 30000.0;

		protected Stopwatch timeRunning = new Stopwatch();

		private bool isDone;

		private object handle = new object();

		private Thread thread;

		public bool TimedOut { get; private set; }

		private bool Finished
		{
			get
			{
				lock (handle)
				{
					return isDone;
				}
			}
			set
			{
				lock (handle)
				{
					isDone = value;
				}
			}
		}

		public virtual void Start()
		{
			OnStarting();
			timeRunning.Start();
			thread = new Thread(Run);
			thread.Start();
		}

		protected virtual void OnStarting()
		{
		}

		protected virtual void ThreadFunction()
		{
		}

		protected virtual void OnFinished()
		{
		}

		protected virtual void OnTimedOut()
		{
		}

		public virtual bool Update()
		{
			if (Finished)
			{
				timeRunning.Stop();
				OnFinished();
				return true;
			}
			if (timeRunning.Elapsed.TotalMilliseconds >= timeoutMilliseconds)
			{
				TimedOut = true;
				timeRunning.Stop();
				thread.Abort();
				OnTimedOut();
				return true;
			}
			return false;
		}

		private void Run()
		{
			ThreadFunction();
			Finished = true;
			thread = null;
		}
	}
}
