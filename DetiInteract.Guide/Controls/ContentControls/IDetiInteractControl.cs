using System;

namespace DetiInteract.Guide.Controls
{
	/// <summary>
	/// Interface to be implemented by any control that requires animation.
	/// </summary>
    public interface IDetiInteractControl
    {
		/// <summary>
		/// Event used to notify when the animation has finished.
		/// </summary>
		event EventHandler AnimationEnd;

		/// <summary>
		/// Triggers the start of the control animation.
		/// </summary>
		void StartAnimation();

		/// <summary>
		/// Aborts control animation.
		/// </summary>
		void StopAnimation();

		/// <summary>
		/// Informs the control that it has been loaded into view.
		/// </summary>
		void ControlLoaded();

		/// <summary>
		/// Configures the control width attribute
		/// </summary>
		/// <param name="width"></param>
		void SetDimensions(int width, int heigth);

		/// <summary>
		/// Performs an action associated with a Tap gesture
		/// </summary>
		void Tap();

		/// <summary>
		/// Performs an action associated with a Long Press gesture
		/// </summary>
		void LongPress();

		/// <summary>
		/// Performs an action associated with a Scroll gesture
		/// </summary>
		void Scroll(float x, float y);

		/// <summary>
		/// Performs an action associated with a Fling gesture
		/// </summary>
		void Fling(float x, float y);

		/// <summary>
		/// Performs an action associated with a Rotation gesture
		/// </summary>
		void Rotation(float x, float y, float z);

		/// <summary>
		/// Performs an action associated with a Zoom gesture
		/// </summary>
		/// <param name="scale"></param>
		void Zoom(float scale);

		/// <summary>
		/// Notifies the Main View Model that device rotation should
		/// not be used to switch pages.
		/// </summary>
		/// <returns></returns>
		bool CanSwitchPage();
    }
}
