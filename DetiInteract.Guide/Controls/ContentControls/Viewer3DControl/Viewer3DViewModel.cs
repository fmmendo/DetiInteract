using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.IO;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using DetiInteract.Guide.Controls.ContentControls.Viewer3DControl;

namespace DetiInteract.Guide.Controls.Viewer3DControl
{
	/// <summary>
	/// Viewer3D ViewModel
	/// </summary>
	sealed class Viewer3DViewModel : DependencyObject
	{
		#region Dependency Properties

		/// <summary>
		/// ModelList Dependency property.
		/// Stores a list of images of models. Being an ObservableCollection, changes 
		/// to the collection will reflect in the UI in real time.
		/// </summary>
		public ObservableCollection<Image> ModelList
		{
			get { return (ObservableCollection<Image>)GetValue(ModelListProperty); }
			set { SetValue(ModelListProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ModelList.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ModelListProperty =
			DependencyProperty.Register("ModelList", typeof(ObservableCollection<Image>), typeof(Viewer3DViewModel), new UIPropertyMetadata(null));
		#endregion

		public bool IsRunning { get; private set; }
		/// <summary>
		/// ModelViewer Instance
		/// </summary>
		public ModelViewerLib.ModelViewer Game { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public Viewer3DViewModel()
		{
			IsRunning = false;
			Game = new ModelViewerLib.ModelViewer();

			ModelList = new ObservableCollection<Image>();

			AddImage("teapot.png");
			AddImage("bigship.png");		
		}

		/// <summary>
		/// Add an image file to the list
		/// </summary>
		/// <param name="imageName">Filename</param>
		private void AddImage(String imageName)
		{
			BitmapImage bmp = new BitmapImage();
			bmp.BeginInit();
			bmp.UriSource = new Uri(@"pack://application:,,,/Resources/"+imageName, UriKind.RelativeOrAbsolute);
			bmp.EndInit();

			Image i = new Image();
			i.Source = bmp;

			ModelList.Add(i);
		}
	}
}
