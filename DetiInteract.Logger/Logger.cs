using System;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace DetiInteract.Logger
{
	/// <summary>
	/// Logger singleon class.
	/// </summary>
	public sealed class Log
	{
		/// <summary>
		/// The Logger singleton instance
		/// </summary>
		private static readonly Log _instance = new Log();

		/// <summary>
		/// file to write to
		/// </summary>
		private String _filename;

		/// <summary>
		/// stream writer to write to the file
		/// </summary>
		private StreamWriter _streamwriter;

		/// <summary>
		/// keeps track of the number of entries to the log file
		/// </summary>
		private double _entryNumber;

		/// <summary>
		/// Flag to deal with race conditions.
		/// </summary>
		private Object flag = new Object();

		/// <summary>
		/// Singleton instance
		/// </summary>
		public static Log Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		private Log()
		{
			CreateFile();
		}

		/// <summary>
		/// Creates and initializes the file
		/// </summary>
		private void CreateFile()
		{
			_filename = "Log_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + Guid.NewGuid() + ".xls";

			_entryNumber = 0;

			Write(this, "INFO", "Logger Started");
		}

		/// <summary>
		/// Writes to the file.
		/// </summary>
		/// <param name="sender">Reference of the class that ordered the write</param>
		/// <param name="type">type of message</param>
		/// <param name="message">Message to write</param>
		public void Write(object sender, string type, string message)
		{
			lock (flag)
			{
				string timestamp = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;

				using (_streamwriter = File.AppendText(_filename))
				{
					_streamwriter.WriteLine(_entryNumber.ToString() + "\t" + timestamp.ToString() + "\t" + sender.ToString() + "\t" + type.ToString() + "\t" + message.ToString());
				}

				_entryNumber++;
			}
		}

		/// <summary>
		/// Closes and releases all resources related to the file
		/// </summary>
		public void CloseFile()
		{
			_streamwriter.Close();
			_streamwriter.Dispose();
		}
	}
}
