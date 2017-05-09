using System.Windows.Controls;
using System.Windows;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for DetiControlView.xaml
	/// </summary>
	public partial class DetiControlView : UserControl
	{
		/// <summary>
		/// The view model for this view.
		/// </summary>
		private DetiControlViewModel _vm;

		/// <summary>
		/// Event t notify this control's parent of the Expanded event
		/// </summary>
		public event RoutedEventHandler ControlExpanded;

		#region Constructors
		/// <summary>
		/// Default Constructor
		/// </summary>
		public DetiControlView()
		{
			InitializeComponent();

			_vm = new DetiControlViewModel();
			this.DataContext = _vm;
		}

		/// <summary>
		/// Alternate Constructor.
		/// Sets up the control and header for the expander in advance.
		/// </summary>
		/// <param name="control">IDetiInteractControl to be added to the Expander' Content Presenter.</param>
		/// <param name="header">String to be displayed as the Expander's Header.</param>
		public DetiControlView(IDetiInteractControl control, string header)
			: this()
		{
			_vm.Control = control;
			_vm.Header = header;
		}
		#endregion

		#region Expander Manipulation
		/// <summary>
		/// Expands the Expander
		/// </summary>
		public void Expand()
		{
			_vm.Expanded = true;
		}

		/// <summary>
		/// Collapses the Expander
		/// </summary>
		public void Collapse()
		{
			_vm.Expanded = false;
		}
		#endregion

		public IDetiInteractControl GetControl()
		{
			return _vm.Control;
		}

		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			if (ControlExpanded != null) ControlExpanded(this, e);
		}
	}
}
