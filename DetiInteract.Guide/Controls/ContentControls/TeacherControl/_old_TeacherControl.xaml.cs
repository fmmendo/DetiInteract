//using System;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Threading;
//using DetiInteract.DSDBroker.Parsers;
//using DetiInteract.Guide.Controls.TeacherControl;

//namespace DetiInteract.Guide.Controls
//{
//    /// <summary>
//    /// Interaction logic for TeacherControl.xaml
//    /// </summary>
//    public sealed partial class MyTeacherControl : UserControl, IDetiInteractControl
//    {
//        #region Fields
//        /// <summary>
//        /// TeacherParser instance to get information to fill the listbox 
//        /// </summary>
//        private TeacherParser _parser = new TeacherParser();

//        private ObservableCollection<TeacherItemView> Teachers { get; set; }

//        /// <summary>
//        /// Reference to the listbox's Scrollviewer
//        /// </summary>
//        private ScrollViewer _scrollviewer;

//        /// <summary>
//        /// Mouse position used to allow click+drag scrolling 
//        /// </summary>
//        private Point _ptMousePosition;

//        /// <summary>
//        /// Offset, for animation purposes 
//        /// </summary>
//        private double _offset = 0.0;

//        /// <summary>
//        /// Timer that controls the animation 
//        /// </summary>
//        private DispatcherTimer _animationTimer = new DispatcherTimer();

//        /// <summary>
//        /// Timer used to create a fling animation for the teacher list.
//        /// </summary>
//        private DispatcherTimer _flingTimer = new DispatcherTimer();

//        /// <summary>
//        /// Time value used to calculate scroll amount during a fling.
//        /// </summary>
//        private float _time = 0;

//        /// <summary>
//        /// Sets the scroll speed for the fling gesture.
//        /// </summary>
//        private float _flingSpeed;
		
//        /// <summary>
//        /// Event that triggers the beggining of the animation 
//        /// </summary>
//        private event EventHandler Animate;

//        /// <summary>
//        /// Event triggered by the end of the animation 
//        /// </summary>
//        public event EventHandler AnimationEnd;

//        /// <summary>
//        /// Event that triggers the premature end of the animation 
//        /// </summary>
//        private event EventHandler AbortAnimation;

//        #endregion

//        #region Constructor
//        /// <summary>
//        /// Constructor /// </summary>
//        public MyTeacherControl()
//        {
//            InitializeComponent();

//            // build the listbox's visual tree to extract it's scrollviewer control
//            //lbDocentes.ApplyTemplate();
//            //_scrollviewer = FindVisualChild<ScrollViewer>(lbDocentes);

//            // Mouse move event handler to allow click+drag to scroll the listbox
//            //lbDocentes.MouseMove += new MouseEventHandler(lbDocentes_MouseMove);

//            Teachers = new ObservableCollection<TeacherItemView>();
//            //this.TeacherPanel.DataContext = Teachers;

//            // Configure and start the parser that returns teacher info
//            _parser.Changed += new ProgressChangedEventHandler(Parser_Changed);
//            _parser.Start();

//            Animate += new EventHandler(TeacherControl_Animate);
//            AbortAnimation += new EventHandler(TeacherControl_AbortAnimation);
            
//            // Configure the Animation Timer
//            _animationTimer.Tick += new EventHandler(AnimationTimer_Tick);
//            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 10); // 10 milliseconds

//            _flingTimer.Tick += new EventHandler(FlingTimer_Tick);
//            _flingTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
//        }

//        #endregion

//        /// <summary>
//        /// Triggers start of animation
//        /// </summary>
//        public void StartAnimation()
//        {
//            if (Animate != null) Animate(null, null);
//        }

//        /// <summary>
//        /// Triggers End of Animation
//        /// </summary>
//        public void StopAnimation()
//        {
//            if (AbortAnimation != null) AbortAnimation(null, null);
//        }

//        /// <summary>
//        /// Finds child elements within a given item. 
//        /// </summary>
//        /// <typeparam name="childItem">Type of item to find</typeparam>
//        /// <param name="obj">Parent item</param>
//        /// <returns>Child item of given type</returns>
//        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
//        {
//            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
//            {
//                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

//                if (child != null && child is childItem)
//                {
//                    return (childItem)child;
//                }
//                else
//                {
//                    childItem childOfChild = FindVisualChild<childItem>(child);
//                    if (childOfChild != null)
//                    {
//                        return childOfChild;
//                    }
//                }
//            }
//            return null;
//        }

//        #region Event Handlers

//        /// <summary>
//        /// Answers a TeacherParser's Changed event.
//        /// Uses the TeacherItem in the EventArgs to generate a "teacher card"
//        /// and places it in the Lisbox
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void Parser_Changed(object sender, ProgressChangedEventArgs e)
//        {
//            // Get the TeacherItem from the EventArgs
//            TeacherItem ti = e.UserState as TeacherItem;

//            TeacherItemView item = new TeacherItemView(ti);

//            Teachers.Add(item);
//            //stkName.MouseDown += new MouseButtonEventHandler(lbDocentes_MouseDown);
//            //stkName.MouseUp += new MouseButtonEventHandler(lbDocentes_MouseUp);
//        }

//        /// <summary>
//        /// Handles the AnimationTimer's Tick event.
//        /// Scrolls the listbox a tiny amount, until the end.
//        /// Triggers AnimationEnd when finished.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void AnimationTimer_Tick(object sender, EventArgs e)
//        {
//            //// position the scrollviewer at the beggining
//            //_scrollviewer.ScrollToHome();

//            //// while not at the end, scroll
//            //if (_scrollviewer.ExtentHeight > _offset)
//            //{
//            //    _offset += 0.5;
//            //    _scrollviewer.ScrollToVerticalOffset(_offset);
//            //}
//            //// when finished, stop timer, reset, and trigger AnimationEnd
//            //else
//            //{
//            //    _animationTimer.Stop();
//            //    _offset = 0.0;
//            //    _scrollviewer.ScrollToVerticalOffset(_offset);

//            //    if (AnimationEnd != null) AnimationEnd(this, null);
//            //}
//        }

//        /// <summary>
//        /// Handles the Animate Event.
//        /// Start animation of the listbox.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void TeacherControl_Animate(object sender, EventArgs e)
//        {
//            _offset = 0.0;
//            _animationTimer.Start();
//        }

//        /// <summary>
//        /// Handles the AbortAnimation Event
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        void TeacherControl_AbortAnimation(object sender, EventArgs e)
//        {
//            _animationTimer.Stop();
//        }

//        /// <summary>
//        /// Processes a MouseDown event on the listbox.
//        /// Stores the current mouse position.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="mouseButtonEventArgs"></param>
//        private void lbDocentes_MouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
//        {
//            // Store the mouse position
//            //_ptMousePosition = mouseButtonEventArgs.GetPosition(this.lbDocentes);
//        }

//        /// <summary>
//        /// Processes a MouseUp event on the lisbox.
//        /// Resets the mouse position.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="mouseButtonEventArgs"></param>
//        private void lbDocentes_MouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
//        {
//            // Reset the mouse position
//            _ptMousePosition = new Point(-1.0, -1.0);
//        }

//        /// <summary>
//        /// Processes a MouseMove event on the lisbox.
//        /// Scrolls the listbox to a new position.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="mouseEventArgs"></param>
//        private void lbDocentes_MouseMove(object sender, MouseEventArgs mouseEventArgs)
//        {
//            //if (_ptMousePosition.Y > 0)
//            //{
//            //    // Get mouse position
//            //    double dMouseY = mouseEventArgs.GetPosition(this.lbDocentes).Y;

//            //    // Calculate offset
//            //    double dDeltaY = dMouseY - _ptMousePosition.Y;
//            //    double dCurOffset = _scrollviewer.VerticalOffset;
//            //    double dNew = dCurOffset - dDeltaY;

//            //    // Scroll into position
//            //    _scrollviewer.ScrollToVerticalOffset(dNew);

//            //    // Update mouse value
//            //    _ptMousePosition.Y = dMouseY;

//            //    // if Scrollviwer is close to the beggining, hide the Upper Gradient
//            //    if ( dNew < 5) {
//            //        //UpperRect.Visibility = System.Windows.Visibility.Hidden;
//            //    }
//            //    //else, show it
//            //    else {
//            //        //UpperRect.Visibility = System.Windows.Visibility.Visible;
//            //    }

//            //    // if Scroll viewer is far from the end, show the Lower Gradient
//            //    if (dNew < _scrollviewer.ExtentHeight - 5)
//            //    {
//            //        //LowerRect.Visibility = System.Windows.Visibility.Visible;
//            //    }
//            //    // else, hide it
//            //    else
//            //    {
//            //        //LowerRect.Visibility = System.Windows.Visibility.Hidden;
//            //    }
//            //}
//        }

//        #endregion

//        /// <summary>
//        /// Scrolls the scrollviewer's by the given amount.
//        /// </summary>
//        /// <param name="amount">Amount to scroll</param>
//        public void Scroll(float amount)
//        {
//            //_scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset + amount*2);
//        }

//        /// <summary>
//        /// Scrolls the scrollviewer's content by the given speed. 
//        /// Scrollviewer will deccelerate to a stop.
//        /// </summary>
//        /// <param name="speed">Initial scrolling speed</param>
//        public void Fling(float speed)
//        {
//            _time = 0;
//            _flingSpeed = -speed;

//            _flingTimer.Start();
//        }

//        /// <summary>
//        /// Handles a FlingTimer Tick event.
//        /// Calculates the amount to scroll the scrollviewer's content
//        /// according to its initial speed and time elapsed.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        void FlingTimer_Tick(object sender, EventArgs e)
//        {
//            //float value;
//            //if (_flingSpeed > 0)
//            //{
//            //    value = (_flingSpeed - (5 * _time)) / 100;

//            //    if (value < 0.5)
//            //    {
//            //        _flingTimer.Stop();
//            //    }
//            //}
//            //else
//            //{
//            //    value = (_flingSpeed + (5 * _time)) / 100;


//            //    if (value > -0.5)
//            //    {
//            //        _flingTimer.Stop();
//            //    }
//            //}

//            //Scroll(value);

//            //_time += 10;
//        }


//        public void SetDimensions(int width, int height)
//        {

//        }


//        public void Tap()
//        {
//            throw new NotImplementedException();
//        }

//        public void LongPress()
//        {
//            throw new NotImplementedException();
//        }

//        public void Scroll(float x, float y)
//        {
//            throw new NotImplementedException();
//        }

//        public void Fling(float x, float y)
//        {
//            throw new NotImplementedException();
//        }

//        public void Rotation(float x, float y, float z)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
