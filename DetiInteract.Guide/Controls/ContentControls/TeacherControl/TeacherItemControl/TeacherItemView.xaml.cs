using System.Windows.Controls;
using DetiInteract.DSDBroker.Parsers;
using DetiInteract.Guide.Controls.TeacherControl.TeacherItemViewControl;
using System;
using System.Windows.Media.Imaging;

namespace DetiInteract.Guide.Controls.TeacherControl
{
	/// <summary>
	/// Interaction logic for TeacherItemView.xaml
	/// </summary>
	public sealed partial class TeacherItemView : UserControl
	{
		/// <summary>
		/// The view model for this view
		/// </summary>
		private TeacherItemViewModel _viewmodel;

		/// <summary>
		/// Constructor.
		/// Takes a TeacherItem instance and passes it to the viewmodel.
		/// Interaction with the view is done via the viewmodel so the DataContext
		/// is set to it.
		/// </summary>
		/// <param name="item">TeacherItem to be displayed in the control</param>
		public TeacherItemView(TeacherItem item)
		{
			InitializeComponent();

			// set up the view model and switch the data context to it.
			_viewmodel = new TeacherItemViewModel(item);
			DataContext = _viewmodel;
		}

		public string GetWebpage()
		{
			return _viewmodel.WebPage;
		}

        public BitmapImage GetImage()
        {
            return _viewmodel.Photo;
        }
	}
}
