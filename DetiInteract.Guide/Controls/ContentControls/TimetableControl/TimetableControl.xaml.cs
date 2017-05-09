using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DetiInteract.DSDBroker.Parsers;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Media.Animation;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interaction logic for TimetableControl.xaml
	/// </summary>
	public sealed partial class TimetableControl : UserControl, IDetiInteractControl
	{
		#region Fields
		/// <summary>
		/// TimetableParser instance to get information to fill the listbox 
		/// </summary>
		TimetableParser _parser = new TimetableParser();

		/// <summary>
		/// Array of pre-set colors to colorcode the subjects in the timetable 
		/// </summary>
		private Color[] _color = new Color[] {Colors.Tomato,
                                              Colors.LightGreen,
                                              Colors.Violet,
                                              Colors.LightYellow,
                                              Colors.PaleTurquoise,
                                              Colors.SandyBrown,
                                              Colors.Gray,
                                              Colors.Coral,
                                              Colors.SkyBlue,
                                              Colors.CornflowerBlue};

		/// <summary>
		/// array of time slots to check for overlapping subjects 
		/// </summary>
		private int[,] _timeSlot = new int[5, 30];

		/// <summary>
		/// Event triggered by the end of the animation 
		/// </summary>
		public event EventHandler AnimationEnd;

		/// <summary>
		/// Timer that controls the animation 
		/// </summary>
		Timer AnimationTimer;

		/// <summary>
		/// Course and Year, for animatin purposes 
		/// </summary>
		private int course;
		private int year;

		#endregion

		#region Constructor
		/// <summary>
		/// Constructor /// </summary>
		public TimetableControl()
		{
			InitializeComponent();

			// Configure and start the Timetable Parser
			_parser.Changed += new ProgressChangedEventHandler(Parser_Changed);
			_parser.Start();

			// Configure the Animation Timer
			//AnimationTimer.Tick += new EventHandler(AnimationTimer_Tick);
			//AnimationTimer.Interval = new TimeSpan(0, 0, 1); // 10 seconds
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the AnimationTimer's Tick event.
		/// Alternates between all the timetables. Triggers AnimationEnd 
		/// when finished.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AnimationTimer_Tick(object state)
		{
			switch (year)
			{
				case 1:
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						TimeTable_Right(null, null);
					}));
					
					year++;
					return;

				case 2:
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						TimeTable_Right(null, null);
					}));
					
					year++;
					return;

				case 3:
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						TimeTable_Right(null, null);
					}));
					
					year++;
					return;

				case 4:
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						TimeTable_Right(null, null);
					}));

					year++;

					if (course == 3)
					{
						year = 6;
						course = 0;
						Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
						{
							if (AnimationEnd != null) AnimationEnd(this, new EventArgs());
							AnimationTimer.Dispose();
						}));
						
					}
					return;
				case 5:
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						TimeTable_Right(null, null);
					}));

					year++;
					return;
				default:
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
					{
						TimeTable_Down(null, null);
					}));

					course++;
					year = 1;
					return;
			}
		}

		/// <summary>
		/// Handles the Changed event for the parser.
		/// Draws the timetable item on the grid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Parser_Changed(object sender, ProgressChangedEventArgs e)
		{
			// Parser just started, set up timetable and display header
			if (e.ProgressPercentage == 0)
			{
				// Clear array to check for overlapping classes
				for (int i = 0; i < 5; i++)
					for (int j = 0; j < 30; j++)
						_timeSlot[i, j] = 0;

				//(re)Set the timetable
				SetUpTimeTable();

				// Get the text for the timetable header
				string text = (string)e.UserState;

				// Create the textblock for the header
				Border b = new Border();
				b.BorderBrush = new SolidColorBrush(Colors.Transparent);
				b.BorderThickness = new Thickness(1.0);

				TextBlock titleblock = new TextBlock();
				titleblock.Text = text;
				if (_mainGrid.Width > 800)
				{
					titleblock.FontSize = 26;
				}
				else
				{
					titleblock.FontSize = 20;
				}
				
				titleblock.Padding = new Thickness(0, 0, 0, 15);
				titleblock.Foreground = new SolidColorBrush(Colors.Black);
				titleblock.FontFamily = new FontFamily(new Uri("pack://application:,,,/Fonts/"), "./SegoeWP.ttf#Segoe WP");
				titleblock.TextAlignment = TextAlignment.Center;
				titleblock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				b.Child = titleblock;

				Grid.SetColumn(b, 0);
				Grid.SetRow(b, 0);
				Grid.SetColumnSpan(b, 30);

				// Place textblock on timetable
				grTimeTable.Children.Add(b);
			}
			else
			{
				// Get the Timetable item from the event args
				TimetableItem ti = (TimetableItem)e.UserState;

				// Create a label to place the item on the timetable
				Border b = new Border();
				b.BorderBrush = new SolidColorBrush(Colors.Black);
				b.BorderThickness = new Thickness(1.0);
				b.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
				b.VerticalAlignment = System.Windows.VerticalAlignment.Top;

				TextBlock tb = new TextBlock();
				tb.Text = ti.Subject;
				tb.FontFamily = new FontFamily(new Uri("pack://application:,,,/Fonts/"), "./SegoeWP.ttf#Segoe WP");
				tb.Background = new SolidColorBrush(_color[ti.Color]);
				if (_mainGrid.Width > 800)
				{
					tb.Height = 18;
					tb.FontSize = 11;
				}
				else
				{
					tb.Height = 14;
					tb.FontSize = 9;
				}
				tb.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);

				// Check for overlapping classes
				int val = 0;
				for (int i = ti.StartTime; i < ti.StartTime + ti.Duration; i++)
				{
					if (_timeSlot[ti.Day - 2, i] >= val) 
						val = _timeSlot[ti.Day - 2, i];

					if (_mainGrid.Width > 800)
					{
						b.Margin = new Thickness(0, val * 20, 0, 0);
					}
					else 
					{
						b.Margin = new Thickness(0, val * 15, 0, 0);
					}

					_timeSlot[ti.Day - 2, i] += 1;
				}


				Grid.SetColumn(b, ti.StartTime);
				Grid.SetRow(b, ti.Day);
				Grid.SetColumnSpan(b, ti.Duration);
				b.Child = tb;

				// Position the lable on the timetable
				grTimeTable.Children.Add(b);
				grTimeTable.UpdateLayout();
			}
		}

		/// <summary>
		/// Processes a click on the TimeTable's Right button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TimeTable_Right(object sender, RoutedEventArgs e)
		{
			Storyboard s = (Storyboard)FindResource("_animateLeft");
			s.Begin();

			_parser.TimeTable_Right();
		}

		/// <summary>
		/// Processes a click on the TimeTable's Left button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TimeTable_Left(object sender, RoutedEventArgs e)
		{
			Storyboard s = (Storyboard)FindResource("_animateRight");
			s.Begin();

			_parser.TimeTable_Left();
		}

		/// <summary>
		/// Processes a click on the TimeTable's Down button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TimeTable_Down(object sender, RoutedEventArgs e)
		{
			Storyboard s = (Storyboard)FindResource("_animateUp");
			s.Begin();

			_parser.TimeTable_Down();
		}

		/// <summary>
		/// Processes a click on the TimeTable's Up button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TimeTable_Up(object sender, RoutedEventArgs e)
		{
			Storyboard s = (Storyboard)FindResource("_animateDown");
			s.Begin();

			_parser.TimeTable_Up();
		}

		#endregion

		/// <summary>
		/// Triggers start of animation
		/// </summary>
		public void StartAnimation()
		{
			_parser.ResetState();

			course = 1;
			year = 1;

			AnimationTimer = new Timer(new TimerCallback(AnimationTimer_Tick), null, 500, 7000);
		}

		/// <summary>
		/// Triggers End of Animation
		/// </summary>
		public void StopAnimation()
		{
			if (AnimationTimer != null)
			{
				AnimationTimer.Dispose();
			}
		}

		/// <summary>
		/// Sets up the grid for the timetable
		/// </summary>
		private void SetUpTimeTable()
		{
			// Clear any content currently on the grid
			grTimeTable.Children.Clear();

			//scale button images for differente resolutions.
			if (_mainGrid.Width > 800)
			{
				Image img;
				img = _buttonDown.Content as Image;
				img.Height = 100; 
				img = _buttonUp.Content as Image;
				img.Height = 100; 
				img = _buttonLeft.Content as Image;
				img.Width = 100; 
				img = _buttonRight.Content as Image;
				img.Width = 100;
			}
			else 
			{
				Image img;
				img = _buttonDown.Content as Image;
				img.Height = 75;
				img = _buttonUp.Content as Image;
				img.Height = 75;
				img = _buttonLeft.Content as Image;
				img.Width = 75;
				img = _buttonRight.Content as Image;
				img.Width = 75;
			}

			Border b = new Border();
			TextBlock info = new TextBlock();
			info.Background = new LinearGradientBrush(Colors.DarkGray, Colors.DimGray, 90.0);
			b.BorderBrush = new SolidColorBrush(Colors.Black);
			info.FontFamily = new FontFamily(new Uri("pack://application:,,,/Fonts/"), "./SegoeWP.ttf#Segoe WP");
			b.BorderThickness = new Thickness(1.0);
			info.VerticalAlignment = System.Windows.VerticalAlignment.Top;
			info.TextAlignment = TextAlignment.Center;

			info.Text = "Dia/Hora";
			b.Child = info;
			Grid.SetColumn(b, 0);
			Grid.SetRow(b, 1);
			Grid.SetColumnSpan(b, 2);
			grTimeTable.Children.Add(b);

			for (int i = 2; i < 7; i++)
			{
				Border border = new Border();
				TextBlock weekday = new TextBlock();
				weekday.Background = new LinearGradientBrush(Colors.DarkGray, Colors.DimGray, 90.0);
				border.BorderBrush = new SolidColorBrush(Colors.Black);
				weekday.FontFamily = new FontFamily(new Uri("pack://application:,,,/Fonts/"), "./SegoeWP.ttf#Segoe WP");
				border.BorderThickness = new Thickness(1.0);
				weekday.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
				weekday.TextAlignment = TextAlignment.Center;

				weekday.Text = "" + i + "ª Feira";
				border.Child = weekday;
				Grid.SetColumn(border, 0);
				Grid.SetRow(border,i);
				Grid.SetColumnSpan(border, 2);
				grTimeTable.Children.Add(border);
			}

			for (int i = 1; i <= 14; i++)
			{
				Border border2 = new Border();
				TextBlock time = new TextBlock();
				time.Background = new LinearGradientBrush(Colors.DarkGray, Colors.DimGray, 90.0);
				border2.BorderBrush = new SolidColorBrush(Colors.Black);
				time.FontFamily = new FontFamily(new Uri("pack://application:,,,/Fonts/"), "./SegoeWP.ttf#Segoe WP");
				border2.BorderThickness = new Thickness(1.0);
				time.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				time.TextAlignment = TextAlignment.Left;

				time.Text = "" + (7 + i);
				border2.Child = time;
				Grid.SetColumn(border2, (i * 2));
				Grid.SetRow(border2, 1);
				Grid.SetColumnSpan(border2, 2);
				grTimeTable.Children.Add(border2);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Collapse()
		{
			grTimeTable.Visibility = System.Windows.Visibility.Hidden;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Expand()
		{
			grTimeTable.Visibility = System.Windows.Visibility.Visible;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void SetDimensions(int width, int height)
		{
			_mainGrid.Width = width;
			_mainGrid.Height = height;
		}

		#region Interaction
		/// <summary>
		/// 
		/// </summary>
		public void Tap()
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "TAP on Timetables.");
		}

		/// <summary>
		/// 
		/// </summary>
		public void LongPress()
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "LONGPRESS on Timetables. ");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Scroll(float x, float y)
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "SCROLL on Timetables. ");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Fling(float x, float y)
		{
			DetiInteract.Logger.Log.Instance.Write(this, "USER", "FLING on Timetables. Switching timetable.");

			bool negativeX = false;
			bool negativeY = false;

			bool left = false, right = false, up = false, down = false;

			if (x < 0) negativeX = true;
			if (y < 0) negativeY = true;

			if (Math.Abs(x) > Math.Abs(y))
			{
				if (negativeX) left = true;
				else right = true;
			}
			else
			{
				if (negativeY) up = true;
				else down = true;
			}

			if (right) TimeTable_Left(this, null);
			else if (left) TimeTable_Right(this, null);
			else if (down) TimeTable_Up(this, null);
			else if (up) TimeTable_Down(this, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void Rotation(float x, float y, float z)
		{
		}

		public void Zoom(float scale)
		{
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public void ControlLoaded()
		{
			_parser.ResetState();

			course = 1;
			year = 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool CanSwitchPage()
		{
			return true;
		}
	}
}
