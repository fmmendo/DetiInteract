using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace DetiInteract.Guide.Controls
{
	class DETIVirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
	{
		#region Fields

		private UIElementCollection _children;
		private ScrollViewer _owner;
		private bool _canHScroll = false;
		private bool _canVScroll = false;
		private Size _extent = new Size(0, 0);
		private Size _viewport = new Size(0, 0);
		private Point _offset;
		private TranslateTransform _transform = new TranslateTransform();

		#endregion

		#region Properties

		#endregion

		public DETIVirtualizingWrapPanel()
		{
			this.RenderTransform = _transform;

			_children = base.InternalChildren;
		}

		#region Override

		protected override Size MeasureOverride(Size availableSize)
		{
			Size childSize = new Size(availableSize.Width, (availableSize.Height * 2) / this.InternalChildren.Count);
			Size extent = new Size(availableSize.Width, childSize.Height * this.InternalChildren.Count);

			if (extent != _extent)
			{
				_extent = extent;
				if (_owner != null)
					_owner.InvalidateScrollInfo();
			}

			if (availableSize != _viewport)
			{
				_viewport = availableSize;
				if (_owner != null)
					_owner.InvalidateScrollInfo();
			}

			foreach (UIElement child in this.InternalChildren)
			{
				child.Measure(childSize);
			}

			return availableSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Size childSize = new Size( finalSize.Width, (finalSize.Height * 2) / this.InternalChildren.Count);
			Size extent = new Size( finalSize.Width, childSize.Height * this.InternalChildren.Count);

			if (extent != _extent)
			{
				_extent = extent;
				if (_owner != null)
					_owner.InvalidateScrollInfo();
			}

			if (finalSize != _viewport)
			{
				_viewport = finalSize;
				if (_owner != null)
					_owner.InvalidateScrollInfo();
			}

			for (int i = 0; i < this.InternalChildren.Count; i++)
			{
				this.InternalChildren[i].Arrange(new Rect(0, childSize.Height * i, childSize.Width, childSize.Height));
			}

			return finalSize;
		}

		#endregion

		#region IScrollInfo implementation

		public bool CanHorizontallyScroll
		{
			get { return _canHScroll; }
			set { _canHScroll = value; }
		}

		public bool CanVerticallyScroll
		{
			get { return _canVScroll; }
			set { _canVScroll = value; }
		}

		public double ExtentHeight
		{
			get { return _extent.Height; }
		}

		public double ExtentWidth
		{
			get { return _extent.Width; }
		}

		public double HorizontalOffset
		{
			get { return _offset.X; }
		}

		public double VerticalOffset
		{
			get { return _offset.Y; }
		}

		public double ViewportHeight
		{
			get { return _viewport.Height; }
		}

		public double ViewportWidth
		{
			get { return _viewport.Width; }
		}

		public void LineDown()
		{
			SetVerticalOffset(this.VerticalOffset + 1);
		}

		public void LineLeft()
		{
			throw new NotImplementedException();
		}

		public void LineRight()
		{
			throw new NotImplementedException();
		}

		public void LineUp()
		{
			SetVerticalOffset(this.VerticalOffset - 1);
		}

		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			for (int i = 0; i < this.InternalChildren.Count; i++)
			{
				if ((Visual)this.InternalChildren[i] == visual)
				{
					Size finalSize = this.RenderSize;
					Size childSize = new Size(finalSize.Width, (finalSize.Height * 2) / this.InternalChildren.Count);

					SetVerticalOffset(childSize.Height * i);

					return rectangle;
				}
			}

			return Rect.Empty;
		}

		public void MouseWheelDown()
		{
			SetVerticalOffset(this.VerticalOffset + 10);
		}

		public void MouseWheelLeft()
		{
			throw new NotImplementedException();
		}

		public void MouseWheelRight()
		{
			throw new NotImplementedException();
		}

		public void MouseWheelUp()
		{
			SetVerticalOffset(this.VerticalOffset - 10);
		}

		public void PageDown()
		{
			double childHeight = (_viewport.Height * 2) / this.InternalChildren.Count;

			SetVerticalOffset(this.VerticalOffset + childHeight);
		}

		public void PageLeft()
		{
			throw new NotImplementedException();
		}

		public void PageRight()
		{
			throw new NotImplementedException();
		}

		public void PageUp()
		{
			double childHeight = (_viewport.Height * 2) / this.InternalChildren.Count;

			SetVerticalOffset(this.VerticalOffset - childHeight);
		}

		public ScrollViewer ScrollOwner
		{
			get { return _owner; }
			set { _owner = value; }
		}

		public void SetHorizontalOffset(double offset)
		{
			throw new NotImplementedException();
		}

		public void SetVerticalOffset(double offset)
		{
			if (offset < 0 || _viewport.Height >= _extent.Height)
			{
				offset = 0;
			}
			else
			{
				if (offset + _viewport.Height >= _extent.Height)
				{
					offset = _extent.Height - _viewport.Height;
				}
			}

			_offset.Y = offset;

			if (_owner != null)
				_owner.InvalidateScrollInfo();

			_transform.Y = -offset;
		}

		#endregion
	}
}
