
namespace DetiInteract.DSDBroker.Parsers
{
    /// <summary>
    /// Timetable Item class holds relevant data to create a new item on a
    /// timetable.  /// </summary>
    public sealed class TimetableItem
	{
		#region Fields
		/// <summary>
        /// ID for the subject  /// </summary>
        public int IDSubject { get; set; }

        /// <summary>
        /// Duration for the class  /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Class start time  /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// Day of week for the class  /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Name of subject  /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Color to paint the subject item on the UI  /// </summary>
        public int Color { get; set; }

		#endregion

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="?"></param>
		public TimetableItem() : this(0,0,0,0,"")
		{
		}
		/// <summary>
        /// Construtor
        /// </summary>
        /// <param name="id">Subject ID</param>
        /// <param name="dur">Classs Duration</param>
        /// <param name="time">Class Start Time</param>
        /// <param name="day">Class day of week</param>
        /// <param name="subj">Subject name</param>
        public TimetableItem(int id, int dur, int time, int day, string subj)
        {
            IDSubject = id;
            Duration = dur;
            StartTime = time;
            Day = day;
            Subject = subj;
        }
    }
}
