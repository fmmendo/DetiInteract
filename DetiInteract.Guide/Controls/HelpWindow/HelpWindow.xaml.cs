using System;
using System.Windows;
using System.Windows.Controls;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for ModalWindow.xaml
	/// </summary>
	public sealed partial class HelpWindow : Window, IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		HelpWindowViewModel _vm;

		private bool disposed = false;

		/// <summary>
		/// 
		/// </summary>
		public HelpWindow()
		{
			InitializeComponent();

			_vm = new HelpWindowViewModel();
			DataContext = _vm;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public void AddItem(TextBlock item)
		{
			item.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 1200;
			item.TextWrapping = TextWrapping.Wrap;
			_vm.AddItem(item);
		}

		public void ClearItems()
		{
			_vm.ClearItems();
		}

		#region Disposing
		/// <summary>
		/// Destructor
		/// </summary>
		~HelpWindow()
        {
            Dispose(false);
        }

		/// <summary>
		/// Releases all memory used by this class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes of other resources (if needed)
		/// </summary>
		/// <param name="disposing"></param>
		protected void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					DataContext = this;
					_vm = null;
				}
			}

			disposed = true;
		}
		#endregion
	}
}
