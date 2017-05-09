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
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace FittsLawTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Fields

		/// <summary>
		/// Randomly generates circle sizes and colour.
		/// </summary>
		Random _random = new Random();

		/// <summary>
		/// Stores data from the Fitts' Law test.
		/// </summary>
		List<Double[]> data = new List<Double[]>();

		/// <summary>
		/// Amplitude value.
		/// </summary>
		Double FITTS_Amplitude;

		/// <summary>
		/// Width value.
		/// </summary>
		Double FITTS_Width;

		/// <summary>
		/// Simulates mouse input.
		/// </summary>
		WindowsInput.MouseSimulator ms = new WindowsInput.MouseSimulator();

		/// <summary>
		/// Current mouse position, used when controlling the mouse remotely.
		/// </summary>
		Point mousePosition = new Point(-1, -1);

		/// <summary>
		/// Starting position of the cursor.
		/// </summary>
		Point startPosition;

		/// <summary>
		/// Position of the cursor on the last instant.
		/// </summary>
		Point previousPosition;

		/// <summary>
		/// Distance traveled by the mouse, in pixels.
		/// </summary>
		Double mouseTravel;

		/// <summary>
		/// A stopwatch determine the time it took to hit a target.
		/// </summary>
		Stopwatch watch = new Stopwatch();

		/// <summary>
		/// Window dimensions, to avoid having targets rendered outside or clipping the window.
		/// </summary>
		Int32 WindowWidth;
		Int32 WindowHeight;

		bool mouse = false;
		bool touch = false;
		bool accel = false;

		/// <summary>
		/// Array of different allowed sizes.
		/// </summary>
		readonly double[] CircleSize = { 20, 40, 80, 120, 240 };

		/// <summary>
		/// Controller that allows remote interaction with the system.
		/// </summary>
		private DetiInteract.Control.Controller _controller;

		/// <summary>
		/// Using PInvoke to get the SetCursorPos from the Win32 API to allow 
		/// moving the mouse pointer remotely
		/// </summary>
		/// <param name="X">X Coordinate</param>
		/// <param name="Y">Y Coordinate</param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		static extern bool SetCursorPos(int X, int Y);

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			// Set up bluetooth communication
			SetUpController();

			// Set the window dimensions
			WindowWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
			WindowHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
		}

		/// <summary>
		/// Draws a new circle in the canvas.
		/// Also sets up all Fitts' law variables and starts the timer.
		/// </summary>
		private void DrawCircle(double size)
		{
			// Colour for the circle
			SolidColorBrush brush = new SolidColorBrush();
			int r = _random.Next(1, 255);
			int g = _random.Next(1, 255);
			int b = _random.Next(1, 255);
			brush.Color = Color.FromRgb((byte)r, (byte)g, (byte)b);

			// Create the circle
			Ellipse e = new Ellipse();
			e.Fill = brush;
			e.StrokeThickness = 2;
			e.Stroke = Brushes.Black;
			e.Width = size;
			e.Height = size;

			// Set a random position for the circle
			double x = _random.Next(0, WindowWidth - (int)e.Width);
			double y = _random.Next(0, WindowHeight - (int)e.Height);
			e.Margin = new Thickness(x, y, 0, 0);

			// Callback function to handle a click on the circle
			e.MouseDown += new MouseButtonEventHandler(Circle_MouseDown);
			e.Focus();
			// Draw the circle
			_canvas.Children.Add(e);

			// Reset mouse travel variables
			mouseTravel = 0;
			startPosition = Mouse.GetPosition(this);
			previousPosition = startPosition;

			//Fitts law Amplitude and Width
			Point amplitude = Vector.Add(new Vector(x, y), startPosition);
			FITTS_Amplitude = Math.Sqrt(amplitude.X * amplitude.X + amplitude.Y * amplitude.Y);
			FITTS_Width = size;

			// Start the timer
			watch.Start();
		}

		/// <summary>
		/// Handles a mouse down event on a drawn circle.
		/// Adds data to the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Circle_MouseDown(object sender, MouseButtonEventArgs e)
		{

			// Rmove items from the canvas
			_canvas.Children.Clear();

			// Add a new data item to the list.
			data.Add(new Double[] { FITTS_Amplitude, FITTS_Width, watch.ElapsedMilliseconds, mouseTravel });

			// Stop and reset the timer.
			watch.Reset();

			// Notify the user of his progress
			textBox1.Content = data.Count.ToString();

			// While not done, draw more circles
			if (data.Count < 50)
				DrawCircle(CircleSize[_random.Next(0, 5)]);
			// else, output to results to a file
			else
				WriteFile();
		}

		/// <summary>
		/// Handles a mouse move event.
		/// Used to determine the distance covered by the mouse.
		/// NOT WORKING CORRECTLY!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _canvas_MouseMove(object sender, MouseEventArgs e)
		{
			Vector p = (Vector)e.GetPosition(this);
			Vector travel = Vector.Add(p, (Vector)previousPosition);

			mouseTravel += Math.Sqrt(travel.X * travel.X + travel.Y * travel.Y);

			previousPosition = (Point)p;
		}

		/// <summary>
		/// Write the data to an .XLS log file
		/// </summary>
		private void WriteFile()
		{
			String filename = "";
			if (mouse && !touch && !accel) filename = "resultsmouse.xls";
			if (!mouse && touch && !accel) filename = "resultstouch.xls";
			if (!mouse && !touch && accel) filename = "resultsaccel.xls";

			using (StreamWriter sw = File.AppendText(filename))
			{
				// For each value array on the List
				foreach (Double[] item in data)
				{
					// read each value
					for (int i = 0; i < item.Length; i++)
					{
						// Write a value from the array followed by a TAB, this
						// ensures that Excel reads one value per column
						sw.Write(item[i] + "\t");
					}

					// Add a new line
					sw.WriteLine("");
				}
				
				sw.Close();
			} // StreamWriter Disposed

		}

		#region Controller
		/// <summary>
		/// Initializes the controller and configures its callback handlers.
		/// </summary>
		private void SetUpController()
		{
			try
			{
				_controller = new DetiInteract.Control.Controller();
				_controller.Init();

				// Set up the Gesture handlers
				_controller.Tap += new System.ComponentModel.ProgressChangedEventHandler(_controller_Tap);
				_controller.LongPress += new System.ComponentModel.ProgressChangedEventHandler(_controller_LongPress);
				_controller.Rotation += new System.ComponentModel.ProgressChangedEventHandler(_controller_Rotation);
				_controller.Scroll += new System.ComponentModel.ProgressChangedEventHandler(_controller_Scroll);
				_controller.Connected += new System.ComponentModel.ProgressChangedEventHandler(_controller_Connected);
				_controller.Disconnected += new System.ComponentModel.ProgressChangedEventHandler(_controller_Disconnected);
			}
			// If computer has no bluetooth or some error happened
			catch (Exception e)
			{
			}
		}

		/// <summary>
		/// Handles a Connected event.
		/// Stops the inactivity timer and any ongoing animation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _controller_Connected(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			mousePosition = Mouse.GetPosition(this);
		}

		/// <summary>
		/// Handles a Disconnected event.
		/// Starts the inactivity timer.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _controller_Disconnected(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
		}

		/// <summary>
		/// Handles a LongPress gesture.
		/// Exectues the LongPress method on whichever control is visible.
		/// </summary>	
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _controller_LongPress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
		}

		/// <summary>
		/// Handles a Tap gesture.
		/// Exectues the Tap method on whichever control is visible.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _controller_Tap(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			ms.LeftButtonClick();
		}

		/// <summary>
		/// Handles a Rotation gesture.
		/// Expands the control to the left or right, according to device tilt.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _controller_Rotation(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			DetiInteract.Control.GestureData.RotationData rot = e.UserState as DetiInteract.Control.GestureData.RotationData;
			if (rot != null)
			{
				mousePosition = Point.Add(mousePosition, new Vector(-rot.z, -rot.y));

				SetCursorPos((int)mousePosition.X, (int)mousePosition.Y);
			}
		}

		/// <summary>
		/// Handles a Scroll gesture.
		/// Exectues the Scroll method on whichever control is visible.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _controller_Scroll(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			DetiInteract.Control.GestureData.ScrollData scroll = e.UserState as DetiInteract.Control.GestureData.ScrollData;
			if (scroll != null)
			{
				mousePosition = Point.Add(mousePosition, new Vector(-scroll.X, -scroll.Y));

				SetCursorPos((int)mousePosition.X, (int)mousePosition.Y);
			}
		}
		#endregion

		/// <summary>
		/// Button triggers the start of the test.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (radioButton1.IsChecked == true) {mouse = true; touch = false; accel = false;}
			else if (radioButton2.IsChecked == true) {touch = true; mouse = false; accel = false;}
			else if (radioButton3.IsChecked == true) {accel = true; mouse = false; touch = false;}

			//Hide the button
			Button b = sender as Button;
			if (b != null)
				b.Visibility = System.Windows.Visibility.Hidden;

			// draw the first circle
			DrawCircle(CircleSize[_random.Next(0, 5)]);
		}
	}
}
