using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DetiInteract.Guide.Controls
{
    /// <summary>
    /// Interaction logic for MarqueeControl.xaml
    /// </summary>
    public partial class MarqueeView : UserControl
    {
        private double ItemSpacing { get; set; }
        private double MoveAmount { get; set; }
        private int Interval { get; set; }

        private int length = 0;

        private LinkedList<UIElement> marqueeItems = new LinkedList<UIElement>();
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MarqueeView() : this(50.0, 2.0, 16)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spacing"></param>
        /// <param name="move"></param>
        /// <param name="interval"></param>
        public MarqueeView(double spacing, double move, int interval)
        {
            InitializeComponent();

            ItemSpacing = spacing;
            Interval = interval;
            MoveAmount = move;

            SetUpMarquee();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(Interval);
            timer.Tick += new EventHandler(timer_Tick);

            timer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetUpMarquee()
        {
            var node = marqueeItems.First;

            while (node != null)
            {
                if (node.Previous != null)
                {
                    double width = marqueeCanvas.Children[marqueeCanvas.Children.IndexOf(node.Previous.Value)].RenderSize.Width;

                    Canvas.SetLeft(node.Value, Canvas.GetLeft(node.Previous.Value) + width + ItemSpacing);
                }
                else
                {
                    Canvas.SetLeft(node.Value, marqueeCanvas.Width + ItemSpacing);
                }

                node = node.Next;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            var node = marqueeItems.First;
            var last = marqueeItems.Last;

            double lastWidth = marqueeCanvas.Children[marqueeCanvas.Children.IndexOf(last.Value)].RenderSize.Width;

            while (node != null)
            {
                double left = Canvas.GetLeft(node.Value) - MoveAmount;
                double nodeWidth = marqueeCanvas.Children[marqueeCanvas.Children.IndexOf(node.Value)].RenderSize.Width;

                if (left < (0 - nodeWidth) + ItemSpacing)
                {
                    marqueeItems.Remove(node);

                    var lastNodePosition = Canvas.GetLeft(last.Value);

                    marqueeItems.AddLast(node);

                    if ((lastNodePosition + lastWidth + ItemSpacing) > marqueeCanvas.Width)
                    {
                        left = lastNodePosition + lastWidth + ItemSpacing;
                    }
                    else
                    {
                        left = marqueeCanvas.Width + ItemSpacing;
                    }
                }

                Canvas.SetLeft(node.Value, left);

                node = node == last ? null : node.Next;
            }
        }
        
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(UIElement item)
        {
            
            marqueeCanvas.Children.Add(item);

            Canvas.SetTop(item, 5);
            Canvas.SetLeft(item, -1000000);

            SetUpMarquee();

            marqueeItems.AddLast(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public bool RemoveItem(UIElement item)
        {
            marqueeCanvas.Children.Remove(item);
            return marqueeItems.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="old_item"></param>
        /// <param name="new_item"></param>
        public bool UpdateItem(UIElement old_item, UIElement new_item)
        {
            if (!marqueeItems.Contains(old_item))
                return false;

            var node = marqueeItems.Find(old_item);

            node.Value = new_item;

            return true;
        }
    }
}
