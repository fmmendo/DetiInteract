using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Windows.Forms;


namespace DetiInteract.Control.CommHandlers
{
	sealed class BluetoothCommHandler
	{
		/// <summary>
		/// Message sent by the device is exactly 34 bytes long.
		/// </summary>
		private const int MAX_MESSAGE_SIZE = 34;

		#region Fields

		/// <summary>
		/// Gesture to communicate with the Controller
		/// </summary>
		public event ProgressChangedEventHandler Gesture;

		public bool bListening { get; private set; }

		private BluetoothClient _btClient;
		private BluetoothListener _btListener;

		//private BackgroundWorker _bwSearch;
		//private BackgroundWorker _bwSend;
		private BackgroundWorker _bwListen;

		private Guid _serviceName = Guid.Empty;

		private BluetoothWin32Authentication _pairHandler;

		//private bool _bSearching = false;

		private char[] _charsToTrim = { '[', ']' };
		private string _strReceived;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public BluetoothCommHandler()
		{
			_btClient = new BluetoothClient();

			//ConfigSearchWorker();
			ConfigPairing();
			ConfigListenWorker();
		}

		/// <summary>
		/// Checks if bluetooth is supported by this machine
		/// </summary>
		/// <returns>True of BT is supported, false otherwise</returns>
		public bool IsBluetoothSupported()
		{
			return BluetoothRadio.IsSupported;
		}

		#region Device Pairing

		/// <summary>
		/// Sets up device pairing.
		/// Configures a callback to handle pairing requests from bluetooth devices.
		/// </summary>
		private void ConfigPairing()
		{
			_pairHandler = new BluetoothWin32Authentication(Win32AuthCallbackHandler);
		}

		/// <summary>
		/// Handles pairing requests from bluetooth devies.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void Win32AuthCallbackHandler(object sender, BluetoothWin32AuthenticationEventArgs e)
		{
			e.Pin = "123456";
		}

		#endregion

		#region Listening
		/// <summary>
		/// Configures the and starts background worker that handles Listening
		/// </summary>
		private void ConfigListenWorker()
		{
			_bwListen = new BackgroundWorker();
			_bwListen.WorkerReportsProgress = true;
			_bwListen.WorkerSupportsCancellation = true;
			_bwListen.DoWork += new DoWorkEventHandler(bwListen_DoWork);
			_bwListen.ProgressChanged += new ProgressChangedEventHandler(bwListen_ProgressChanged);
			_bwListen.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwListen_RunWorkerCompleted);

			_btListener = new BluetoothListener(_serviceName);
			_btListener.Start();
		}

		/// <summary>
		/// Sets bListenting to true. The BT adapter starts listening.
		/// Triggers the BackgroundWorker's start.
		/// </summary>
		public void Listen()
		{
			bListening = true;
			_bwListen.RunWorkerAsync();
		}

		/// <summary>
		/// Sets bListenting to false. Stops the BT adapter from listening.
		/// Cancels the BackgroundWorker.
		/// </summary>
		public void AbortListen()
		{
			bListening = false;
		}

		/// <summary>
		/// Background Worker RunWorkerCompleted method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bwListen_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Listen();
		}

		/// <summary>
		/// Background worker ProgressChanged event handler.
		/// Raises a Gesture event when data is read from the stream.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bwListen_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			try
			{

				if (e.ProgressPercentage == 0 || e.ProgressPercentage == 100) return;

				string msg = (string)e.UserState;

				//MessageBox.Show(msg);

				// if message has been corrupted (more than one read) return
				if (!msg.EndsWith("]")) return;

				// trim the message
				msg = msg.Trim(_charsToTrim);

				// if message codes a Disconnect gesture
				if (msg.StartsWith("00"))
				{
					bListening = false;
				}

				// send the gesture via the Gesture event
				if (Gesture != null)
					Gesture(this, new ProgressChangedEventArgs(0, msg));

				return;

			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// Background worker DoWork method.
		/// Listens for communication from devices.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bwListen_DoWork(object sender, DoWorkEventArgs e)
		{
			_bwListen.ReportProgress(0);

			// Wait for connection from client
			using (BluetoothClient client = _btListener.AcceptBluetoothClient())
			{
				client.Authenticate = true;
				client.Encrypt = true;

				_strReceived = String.Format("[Connected:{0}]", client.RemoteMachineName);
				_bwListen.ReportProgress(1, _strReceived);

				// reset message data
				_strReceived = "";

				// Get the stream for the client
				using (BufferedStream stream = new BufferedStream(client.GetStream()))
				{
					byte[] Buffer = new byte[MAX_MESSAGE_SIZE];
					int bytesRead = 0;

					while (bListening)
					{
						// reset buffer
						bytesRead = 0;
						Buffer = new byte[MAX_MESSAGE_SIZE];

						try
						{
							// read data from the stream
							bytesRead = stream.Read(Buffer, 0, MAX_MESSAGE_SIZE);

							// decode the message
							_strReceived = Encoding.UTF8.GetString(Buffer, 0, bytesRead);

							// report to ProgressChanged method
							_bwListen.ReportProgress(1, _strReceived);
						}
						catch (Exception)
						{
						}
					}
				}
			}


			bListening = false;
			return;
		}

		#endregion
	}
}


