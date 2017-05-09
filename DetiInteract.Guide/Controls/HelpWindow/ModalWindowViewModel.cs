using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DetiInteract.Guide.Controls
{
	sealed class HelpWindowViewModel : DependencyObject
	{
		#region Dependency Properties
		/// <summary>
		/// 
		/// </summary>
		public ObservableCollection<TextBlock> ItemList
		{
			get { return (ObservableCollection<TextBlock>)GetValue(ItemListProperty); }
			set { SetValue(ItemListProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemList.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemListProperty =
			DependencyProperty.Register("ItemList", typeof(ObservableCollection<TextBlock>), typeof(HelpWindowViewModel), new UIPropertyMetadata(null));

		#endregion

		/// <summary>
		/// 
		/// </summary>
		public HelpWindowViewModel()
		{
			ItemList = new ObservableCollection<TextBlock>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public void AddItem(TextBlock item)
		{
			ItemList.Add(item);
		}

		public void ClearItems()
		{
			ItemList.Clear();
		}
	}
}
