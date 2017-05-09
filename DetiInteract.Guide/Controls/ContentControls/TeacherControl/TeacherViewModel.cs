using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using DetiInteract.DSDBroker.Parsers;
using DetiInteract.Guide.Controls.TeacherControl;
using System.Collections.Generic;
using DataVirtualization;
using System.Threading;
using System;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Security.Cryptography;
using System.Drawing.Imaging;
using System.IO;

namespace DetiInteract.Guide.Controls.TeacherControl
{
	/// <summary>
	/// ViewModel for the Teacher Control.
	/// Control Logic is dealt with here.
	/// </summary>
	sealed class TeacherViewModel : DependencyObject
	{
		#region Dependecy Properties
		/// <summary>
		/// TeacherList Dependency property.
		/// Stores a list of teachers. Being an ObservableCollection, changes 
		/// to the collection will reflect in the UI in real time.
		/// </summary>
		public ObservableCollection<TeacherItemView> TeacherList
		{
			get { return (ObservableCollection<TeacherItemView>)GetValue(TeacherListProperty); }
			set { SetValue(TeacherListProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TeacherList.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TeacherListProperty =
			DependencyProperty.Register("TeacherList", typeof(ObservableCollection<TeacherItemView>), typeof(TeacherViewModel), new UIPropertyMetadata(null));

		#endregion

		private TeacherParser _parser = new TeacherParser();

		/// <summary>
		/// Constructor
		/// </summary>
		public TeacherViewModel()
		{
			TeacherList = new ObservableCollection<TeacherItemView>();

			//TeacherProvider teacherProvider = new TeacherProvider();
			_parser.Changed += new ProgressChangedEventHandler(Parser_Changed);
			_parser.Start();
		}

		/// <summary>
		/// Handles a Changed event from the parser.
		/// Uses the TeacherItem in the EventArgs to generate a TeacherItemView
		/// control and places it in the ObservableCollection. The Listbox on 
		/// the TeacherView will update automatically due to binding.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Parser_Changed(object sender, ProgressChangedEventArgs e)
		{
			// Get the list of TeacherItems from the EventArgs
			List<TeacherItem> list = (List<TeacherItem>)e.UserState;

            //BitmapImage ignore = null;
			foreach (TeacherItem ti in list)
			{
                // Instance the TeacherItem View
                TeacherItemView tiv = new TeacherItemView((TeacherItem)ti);

                // Ignore 'no photo' image
                //if (ti.PhotoPath.EndsWith("/337.jpeg"))
                //{
                //    ignore = tiv.GetImage();
                //}

                //if (ignore != null && doImagesMatch(tiv.GetImage(), ignore)) continue;
				
                TeacherList.Add(tiv);
			}
		}

        Bitmap GetBitmap(BitmapImage source)
        {
            MemoryStream ms = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(ms);
            return new Bitmap(ms); 
        }

/// <summary>
/// method for comparing 2 images to see if they are the same. First
/// we convert both images to a byte array, we then get their hash (their
/// hash should match if the images are the same), we then loop through
/// each item in the hash comparing with the 2nd Bitmap
/// </summary>
/// <param name="bmp1"></param>
/// <param name="bmp2"></param>
/// <returns></returns>

        
           public bool ImageCompareString(BitmapImage first, BitmapImage second)    
           {
               Bitmap firstImage = GetBitmap(first);
               Bitmap secondImage = GetBitmap(second);

           MemoryStream ms = new MemoryStream();    
           firstImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);   
           String firstBitmap = Convert.ToBase64String(ms.ToArray());    
           ms.Position = 0;    
           
           secondImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);   
           String secondBitmap = Convert.ToBase64String(ms.ToArray());  
        
            if (firstBitmap.Equals(secondBitmap))  
            {  
                return true;   
            } 
            else 
            {  
                return false;
            }  
        }
        
        public bool doImagesMatch(BitmapImage bmpi1, BitmapImage bmpi2)
{
    Bitmap bmp1 = GetBitmap(bmpi1);
    Bitmap bmp2 = GetBitmap(bmpi2);
    try
    {
        //create instance or System.Drawing.ImageConverter to convert
        //each image to a byte array
        ImageConverter converter = new ImageConverter();
        //create 2 byte arrays, one for each image
        byte[] imgBytes1 = new byte[1];
        byte[] imgBytes2 = new byte[1];
 
        //convert images to byte array
        imgBytes1 = (byte[])converter.ConvertTo(bmp1, imgBytes2.GetType());
        imgBytes2 = (byte[])converter.ConvertTo(bmp2, imgBytes1.GetType());
 
        //now compute a hash for each image from the byte arrays
        SHA256Managed sha = new SHA256Managed();
        byte[] imgHash1 = sha.ComputeHash(imgBytes1);
        byte[] imgHash2 = sha.ComputeHash(imgBytes2);
 
        //now let's compare the hashes
        for (int i = 0; i < imgHash1.Length && i < imgHash2.Length; i++)
        {
            //whoops, found a non-match, exit the loop
            //with a false value
            if (!(imgHash1[i] == imgHash2[i]))
                return false;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message);
        return false;
    }
    //we made it this far so the images must match
    return true;
} 
	}
}
