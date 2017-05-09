
namespace DetiInteract.DSDBroker.Parsers
{
    public sealed class TeacherItem
	{
		#region Fields
		/// <summary>
        /// Teacher's Name 
		/// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Path (url) to the Teacher's photo 
		/// </summary>
        public string PhotoPath { get; set; }
        
        /// <summary>
        /// Teacher's Office 
		/// </summary>
        public string Office { get; set; }
        
        /// <summary>
        /// Teacher's Phone Extension 
		/// </summary>
        public string PhoneExt { get; set; }
        
        /// <summary>
        /// Teacher's WebPage
		/// </summary>
        public string WebPage { get; set; }

		#endregion

		#region Constructor
		/// <summary>
		/// Default Constructor
		/// </summary>
		public TeacherItem() : this("","","","","")
		{	
		}

		/// <summary>
		/// Base constructor. Fills all the properties with given data
		/// </summary>
		/// <param name="name">Teacher's Name</param>
		/// <param name="path">Path to teacher's photo</param>
		/// <param name="off">Teacher's office door number</param>
		/// <param name="ext">Teachers's phone extension</param>
		/// <param name="web">Teachers' web page address</param>
        public TeacherItem(string name, string path, string off, string ext, string web)
        {
            Name = name;
            PhotoPath = path;
            Office = off;
            PhoneExt = ext;
            WebPage = web;
		}

		#endregion
	}
}
