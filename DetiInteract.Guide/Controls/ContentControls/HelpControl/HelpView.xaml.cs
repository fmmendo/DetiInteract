using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for HelpView.xaml
	/// </summary>
	public partial class HelpView : UserControl, IDetiInteractControl
	{
		public HelpView()
		{
			InitializeComponent();
		}

		#region IDetiInteractControl

		public event EventHandler AnimationEnd;

		public void StartAnimation()
		{
		}

		public void StopAnimation()
		{
		}

		public void ControlLoaded()
		{
		}

		/// <summary>
		/// Sets the dimentions of the control.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="heigth"></param>
		public void SetDimensions(int width, int heigth)
		{
			this.Width = width;
			this.Height = heigth;
		}

		public void Tap()
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on HelpView");
		}

		public void LongPress()
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on HelpView.");
		}

		public void Scroll(float x, float y)
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on HelpView.");
		}

		public void Fling(float x, float y)
		{
			DetiInteract.Logger.Log.Instance.Write(this, "FLING", "SCROLL on HelpView.");
		}

		public void Rotation(float x, float y, float z)
		{
		}

		public void Zoom(float scale)
		{
		}
		#endregion


		public bool CanSwitchPage()
		{
			return true;
		}



	}
}
