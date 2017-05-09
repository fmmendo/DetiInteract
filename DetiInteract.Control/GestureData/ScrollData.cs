using System.Globalization;

namespace DetiInteract.Control.GestureData
{
    public sealed class ScrollData
    {
		/// <summary>
		/// X Value
		/// </summary>
        public float X { get; set; }

		/// <summary>
		/// Y Value
		/// </summary>
        public float Y { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="_x">(string) X</param>
		/// <param name="_y">(string) Y</param>
        public ScrollData(string _x, string _y)
        {
			// values come preformated using en-US number notation
            CultureInfo ci = new CultureInfo("en-US");

            X = float.Parse(_x, ci);
            Y = float.Parse(_y, ci);
        }
    }
}
