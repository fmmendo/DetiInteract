using System;
using System.Threading;
using System.Windows.Controls;
using DetiInteract.Guide.Controls.Viewer3DControl;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for Viewer3DView.xaml
	/// </summary>
	public partial class Viewer3DView : UserControl, IDetiInteractControl
	{
		#region Fields
		/// <summary>
		/// ViewModel for this View.
		/// </summary>
		private static Viewer3DViewModel _vm;

		/// <summary>
		/// Reference to the scroll viewer, to allow scrolling.
		/// </summary>
		private ScrollViewer _scrollviewer;

		/// <summary>
		/// Holds the mouse position
		/// </summary>
		private System.Windows.Point _mousePosition = new System.Windows.Point(-1, -1);

		/// <summary>
		/// Holds the initial value of the mobile's sensors for calibration
		/// </summary>
		private static float _x, _y, _z;

		/// <summary>
		/// Indicates if sensor's values have gone through calibration
		/// </summary>
		bool calibrated = false;

		/// <summary>
		/// value o PI/180 to convert angle to radian.
		/// </summary>
		private const float pi180 = (float)System.Math.PI / 180;

		/// <summary>
		/// Event that notifies of the end of animation mode.
		/// </summary>
		public event System.EventHandler AnimationEnd;

		/// <summary>
		/// The currently selected line.
		/// </summary>
		private int _selectedLine = 0;

		/// <summary>
		/// The currently selected column.
		/// </summary>
		private int _selectedColumn = 0;

		/// <summary>
		/// Indicates if the user has entered selection mode.
		/// </summary>
		private bool _selecting = false;

		/// <summary>
		/// The number of columns in the Listbox.
		/// </summary>
		private int _columnCount;

		/// <summary>
		/// Number of lines in the listbox.
		/// </summary>
		private int _lineCount;

		/// <summary>
		/// Indicates if the users has entered browsing mode.
		/// </summary>
		private bool _browsing = false;

		/// <summary>
		/// Timer to calculate the fling decceleration.
		/// </summary>
		private Timer _flingTimer;

		/// <summary>
		/// Fling speed used to calculate the fling decceleration.
		/// </summary>
		private float _flingSpeed;

		/// <summary>
		/// Amount to scroll in each iteration of a fling.
		/// </summary>
		private float _scrollValue;

		/// <summary>
		/// Timer used for animation
		/// </summary>
		private Timer _animationTimer;

		/// <summary>
		/// Keeps track of time for animation and fling.
		/// </summary>
		private static float _time = 0;
		#endregion

		#region Initialization
		/// <summary>
		/// Constructor
		/// </summary>
		public Viewer3DView()
		{
			InitializeComponent();

			_vm = new Viewer3DViewModel();
			this.DataContext = _vm;

			_Models.ApplyTemplate();
			_scrollviewer = FindVisualChild<ScrollViewer>(_Models);
		}

		/// <summary>
		/// On loading this control, resets the model to its default position.
		/// </summary>
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

			_Viewer.RendererHeigth = heigth;
			_Viewer.RendererWidth = width;

		}

		/// <summary>
		/// Finds child elements within a given item. 
		/// </summary>
		/// <typeparam name="childItem">Type of item to find</typeparam>
		/// <param name="obj">Parent item</param>
		/// <returns>Child item of given type</returns>
		private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);

				if (child != null && child is childItem)
				{
					return (childItem)child;
				}
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
					{
						return childOfChild;
					}
				}
			}
			return null;
		}

		#endregion

		#region Input Handling
		/// <summary>
		/// Handles a mouse down event. Gets its position on the screen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
		{
			_mousePosition = e.GetPosition(this);

			_Viewer.Visibility = System.Windows.Visibility.Hidden;
			_browsing = false;
		}

		/// <summary>
		/// Handles a Mouse Up event. Resets the saved mouse position.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
		{
			_mousePosition.X = -1;
			_mousePosition.Y = -1;
		}

		/// <summary>
		/// Handles a mouse move event. Allows scrolling the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (_mousePosition.Y > 0)
			{
				// Get mouse position
				double dMouseY = e.GetPosition(this._Models).Y;

				// Calculate offset
				double dDeltaY = dMouseY - _mousePosition.Y;
				double dCurOffset = _scrollviewer.VerticalOffset;
				double dNew = dCurOffset - dDeltaY;

				// Scroll into position
				_scrollviewer.ScrollToVerticalOffset(dNew);

				// Update mouse value
				_mousePosition.Y = dMouseY;
			}
		}

		private void _Models_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			_vm.Game.RotateCamera(0, 0, 0);

			_Viewer.Visibility = System.Windows.Visibility.Visible;
			_Viewer.Focus();

			//load the model.
			_vm.Game.LoadModel(_Models.SelectedIndex);

			_browsing = true;
		}
		#endregion

		#region Animation
		/// <summary>
		/// Begins the animation for this control.
		/// </summary>
		public void StartAnimation()
		{
			_time = 0;
			_x = _y = _z = 0;

			_Viewer.Visibility = System.Windows.Visibility.Visible;

			_animationTimer = new Timer(new TimerCallback(_animationTimer_Tick), null, 0, 25);
		}

		/// <summary>
		/// Aborts animation.
		/// </summary>
		public void StopAnimation()
		{
			if (_animationTimer != null)
			{
				_animationTimer.Dispose();
			}

			_Viewer.Visibility = System.Windows.Visibility.Hidden;
		}

		/// <summary>
		/// Timer callback used to animate the current model.
		/// </summary>
		/// <param name="state"></param>
		private void _animationTimer_Tick(object state)
		{
			_time += 1;

			_vm.Game.RotateCamera(1, 1, 1);

			//
			if (_time == 20 * 25)
			{
				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
				{
					if (AnimationEnd != null) AnimationEnd(this, new EventArgs());
					StopAnimation();
				}));
			}

		}
		#endregion

		#region Interaction
		/// <summary>
		/// 
		/// </summary>
		public void Tap()
		{
			if (_selecting)
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on VIEWER3D. Displaying model.");
				_vm.Game.RotateCamera(0, 0, 0);

				_Viewer.Visibility = System.Windows.Visibility.Visible;
				_Viewer.Focus();

				//load the model.
				_vm.Game.LoadModel(_Models.SelectedIndex);

				_browsing = true;
			}
			else 
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on VIEWER3d. But not in selection mode. ");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void LongPress()
		{
			if (!_selecting)
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on VIEWER3d. Entering selection mode. ");

				//select the item closest to the middle of the screen
				_selectedLine = (_lineCount / 2) + ((int)_scrollviewer.VerticalOffset / 170);
				_selectedColumn = _columnCount / 2;

				//set focus on the listbox.
				_Models.Focus();

				// highlight the selected item
				_Models.SelectedIndex = _selectedLine * _columnCount + _selectedColumn;

				_selecting = true;

				return;
			}
			else
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on VIEWER3d. Leaving selection mode/model display. ");

				_selecting = false;
				_browsing = false;

				_Viewer.Focus();

				// Hide the browser
				_Viewer.Visibility = System.Windows.Visibility.Hidden;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Scroll(float x, float y)
		{
			if (!_selecting)
			{
				_scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset + y * 2);

				DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on VIEWER3d. Scrolling model list. ");
			}
			else
			{
				DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on VIEWER3d. Does Nothing. ");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Fling(float x, float y)
		{
			// if in selection mode
			if (_selecting)
			{
				// focus on the control so the user can see the highlight
				_Models.Focus();

				DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on VIEWER3d. Changing model selection. ");

				// if vertical scrolling
				if (Math.Abs(y) > Math.Abs(x))
				{
					if (y < 0)
					{
						_selectedLine -= 1;
						if (_selectedLine < 0)
						{
							_selectedLine = 0;
						}
					}
					else
					{
						_selectedLine += 1;
						if (_selectedLine >= (_Models.Items.Count / _columnCount))
						{
							_selectedLine = _Models.Items.Count / _columnCount;
						}
					}
				}
				// if horizontal scrolling
				else
				{
					if (x > 0)
					{
						_selectedColumn += 1;
						if (_selectedColumn == _columnCount)
						{
							_selectedColumn = 0;
						}
					}
					else
					{
						_selectedColumn -= 1;
						if (_selectedColumn < 0)
						{
							_selectedColumn = _columnCount - 1;
						}
					}
				}

				// Highlight the selected item.
				_Models.SelectedIndex = _selectedLine * _columnCount + _selectedColumn;
				_Models.ScrollIntoView(_Models.Items[_Models.SelectedIndex]);
			}
			else
			{
				// A vertical fling scrolls the model list with acceleration.
				if (Math.Abs(y) > Math.Abs(x))
				{
					DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on VIEWER3d. Scrolling list. ");

					_selecting = false;

					_time = 0;
					_flingSpeed = y;
					_scrollValue = y;

					_flingTimer = new Timer(new TimerCallback(_flingTimer_Tick), null, 0, 25);

					return;
				}
			}
		}

		/// <summary>
		/// Rotates the object using the passed values for yaw, pitch and roll.
		/// </summary>
		/// <param name="x">Yaw</param>
		/// <param name="y">Pitch</param>
		/// <param name="z">Roll</param>
		public void Rotation(float x, float y, float z)
		{
			_vm.Game.mobileInteracting = true;

			_vm.Game.RotateCamera(x, -y, z);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scale"></param>
		public void Zoom(float scale)
		{

		}
		/// <summary>
		/// Gets the mouse cursor position, to camera interaction wiht mouse/finger.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Viewer3DPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			_mousePosition = e.GetPosition(this);
		}

		/// <summary>
		/// Handles the MouseMove event. Rotates the object given the mouse's
		/// movement.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Viewer3DPanel_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (_mousePosition.X >= 0)
			{
				_vm.Game.mobileInteracting = false;

				System.Windows.Point pos = e.GetPosition(this);
				float x = (float)pos.X - (float)_mousePosition.X;
				float y = (float)pos.Y - (float)_mousePosition.Y;

				_vm.Game.RotateCamera(-x, -y, 0.0f);

				_mousePosition = e.GetPosition(this);
			}
		}

		/// <summary>
		/// Resets the mouse cursor position.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Viewer3DPanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			_mousePosition = new System.Windows.Point(-1, -1);
		}

		/// <summary>
		/// Handles the fling timer callback.
		/// Deccelerates scrolling of the model list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _flingTimer_Tick(object state)
		{
			// keep going while the speed is fast enough.
			if (_scrollValue > 0.5)
			{
				_scrollValue = (_flingSpeed - (5 * _time)) / 100;
			}
			else if (_scrollValue < -0.5)
			{
				_scrollValue = (_flingSpeed + (5 * _time)) / 100;
			}
			//otherwise, stop.
			else
			{
				_flingSpeed = 0;
				_scrollValue = 0;

				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
				{
					_flingTimer.Dispose();
				}));
			}

			_time += 10f;

			// scroll the browser or the listbox.
			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
			{
				Scroll(0, -_scrollValue);
			}));
		}

		#endregion

		public bool CanSwitchPage()
		{
			//if browsing, return false.
			return !_browsing;
		}
	}
}
