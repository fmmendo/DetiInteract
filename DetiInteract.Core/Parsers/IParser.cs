using System.ComponentModel;

namespace DetiInteract.DSDBroker.Parsers
{
	/// <summary>
	/// Interface to be implemented by any Parser to ensure consistency 
	/// between them.
	/// </summary>
    public interface IParser
    {
        /// <summary>
        /// Changed event, used to sent data to the Presentation Layer.
        /// </summary>
        event ProgressChangedEventHandler Changed;

        /// <summary>
        /// Activate the Changed event, use Args to send data.
        /// </summary>
        /// <param name="e">Use ProgressChangedEventArgs to send data.</param>
        void SetChanged(ProgressChangedEventArgs e);

        /// <summary>
        /// Begin work.
        /// </summary>
        void Start();

        /// <summary>
        /// Pause work.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop work.
        /// </summary>
        void Stop();
    }
}
