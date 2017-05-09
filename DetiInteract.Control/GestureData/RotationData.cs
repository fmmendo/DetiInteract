using System;
using System.Globalization;

namespace DetiInteract.Control.GestureData
{
    public sealed class RotationData
    {
		/// <summary>
		/// X value
		/// </summary>
        public Single x { get; private set; }

		/// <summary>
		/// Y value
		/// </summary>
		public Single y { get; private set; }

		/// <summary>
		/// Z value
		/// </summary>
		public Single z { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="_x">(string) X</param>
		/// <param name="_y">(string) Y</param>
		/// <param name="_z">(string) Z</param>
		public RotationData(String _x, String _y, String _z)
        {
			// values come preformated using en-US number notation
            CultureInfo ci = new CultureInfo("en-US");

            x = Single.Parse(_x, ci);
            y = Single.Parse(_y, ci);
            z = Single.Parse(_z, ci);
        }
    }
}
