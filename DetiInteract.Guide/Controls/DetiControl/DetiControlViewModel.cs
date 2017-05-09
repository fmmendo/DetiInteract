using System.Windows;

namespace DetiInteract.Guide.Controls
{
	public class DetiControlViewModel : DependencyObject
	{
		#region Dependency Properties
		/// <summary>
		/// Control that will be hosted in the DetiControl
		/// </summary>
		public IDetiInteractControl Control
		{
			get { return (IDetiInteractControl)GetValue(ControlProperty); }
			set { SetValue(ControlProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Control.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ControlProperty =
			DependencyProperty.Register("Control", typeof(IDetiInteractControl), typeof(DetiControlViewModel), new UIPropertyMetadata(null));

		/// <summary>
		/// Holds the text in the Expander's header.
		/// </summary>
		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(DetiControlViewModel), new UIPropertyMetadata(""));

		/// <summary>
		/// Sets the expand state for this expander.
		/// </summary>
		public bool Expanded
		{
			get { return (bool)GetValue(ExpandedProperty); }
			set { SetValue(ExpandedProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Expanded.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ExpandedProperty =
			DependencyProperty.Register("Expanded", typeof(bool), typeof(DetiControlViewModel), new UIPropertyMetadata(false));



		#endregion

		/// <summary>
		/// Default Constructor
		/// </summary>
		public DetiControlViewModel()
		{
		}
	}
}
