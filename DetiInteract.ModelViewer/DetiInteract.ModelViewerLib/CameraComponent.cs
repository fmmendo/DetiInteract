//-----------------------------------------------------------------------------
// Copyright (c) 2007-2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DetiInteract.ModelViewerLib
{
	/// <summary>
	/// A general purpose quaternion based camera component for XNA. This
	/// camera component provides the necessary bindings to the XNA framework
	/// to allow the camera to be manipulated by the keyboard, mouse, and game
	/// pad. This camera component is implemented in terms of the Camera class.
	/// As a result the camera component supports all of the features of the
	/// Camera class. The camera component maps input to a series of actions.
	/// These actions are defined by the Actions enumeration. Methods are
	/// provided to remap the camera components default bindings.
	/// </summary>
	class CameraComponent : GameComponent, ICamera
	{
		#region Fields
		public enum Actions
		{
			FlightYawLeftPrimary,
			FlightYawLeftAlternate,
			FlightYawRightPrimary,
			FlightYawRightAlternate,

			MoveForwardsPrimary,
			MoveForwardsAlternate,
			MoveBackwardsPrimary,
			MoveBackwardsAlternate,

			MoveDownPrimary,
			MoveDownAlternate,
			MoveUpPrimary,
			MoveUpAlternate,

			OrbitRollLeftPrimary,
			OrbitRollLeftAlternate,
			OrbitRollRightPrimary,
			OrbitRollRightAlternate,

			PitchUpPrimary,
			PitchUpAlternate,
			PitchDownPrimary,
			PitchDownAlternate,

			YawLeftPrimary,
			YawLeftAlternate,
			YawRightPrimary,
			YawRightAlternate,

			RollLeftPrimary,
			RollLeftAlternate,
			RollRightPrimary,
			RollRightAlternate,

			StrafeRightPrimary,
			StrafeRightAlternate,
			StrafeLeftPrimary,
			StrafeLeftAlternate
		};

		private const float DEFAULT_ACCELERATION_X = 8.0f;
		private const float DEFAULT_ACCELERATION_Y = 8.0f;
		private const float DEFAULT_ACCELERATION_Z = 8.0f;
		private const float DEFAULT_MOUSE_SMOOTHING_SENSITIVITY = 0.5f;
		private const float DEFAULT_SPEED_FLIGHT_YAW = 100.0f;
		private const float DEFAULT_SPEED_MOUSE_WHEEL = 1.0f;
		private const float DEFAULT_SPEED_ORBIT_ROLL = 100.0f;
		private const float DEFAULT_SPEED_ROTATION = 0.2f;
		private const float DEFAULT_VELOCITY_X = 1.0f;
		private const float DEFAULT_VELOCITY_Y = 1.0f;
		private const float DEFAULT_VELOCITY_Z = 1.0f;

		private const int MOUSE_SMOOTHING_CACHE_SIZE = 10;

		private Camera camera;
		private bool movingAlongPosX;
		private bool movingAlongNegX;
		private bool movingAlongPosY;
		private bool movingAlongNegY;
		private bool movingAlongPosZ;
		private bool movingAlongNegZ;
		private float rotationSpeed;
		private float orbitRollSpeed;
		private float flightYawSpeed;
		private float mouseWheelSpeed;
		private Vector3 acceleration;
		private Vector3 currentVelocity;
		private Vector3 velocity;
		private KeyboardState currentKeyboardState;
		private Dictionary<Actions, Keys> actionKeys;

		#endregion

		#region Public Methods

		/// <summary>
		/// Constructs a new instance of the CameraComponent class. The
		/// camera will have a spectator behavior, and will be initially
		/// positioned at the world origin looking down the world negative
		/// z axis. An initial perspective projection matrix is created
		/// as well as setting up initial key bindings to the actions.
		/// </summary>
		public CameraComponent(Game game)
			: base(game)
		{
			camera = new Camera();
			camera.CurrentBehavior = Camera.Behavior.Spectator;

			movingAlongPosX = false;
			movingAlongNegX = false;
			movingAlongPosY = false;
			movingAlongNegY = false;
			movingAlongPosZ = false;
			movingAlongNegZ = false;

			rotationSpeed = DEFAULT_SPEED_ROTATION;
			orbitRollSpeed = DEFAULT_SPEED_ORBIT_ROLL;
			flightYawSpeed = DEFAULT_SPEED_FLIGHT_YAW;
			mouseWheelSpeed = DEFAULT_SPEED_MOUSE_WHEEL;
			acceleration = new Vector3(DEFAULT_ACCELERATION_X, DEFAULT_ACCELERATION_Y, DEFAULT_ACCELERATION_Z);
			velocity = new Vector3(DEFAULT_VELOCITY_X, DEFAULT_VELOCITY_Y, DEFAULT_VELOCITY_Z);

			Rectangle clientBounds = game.Window.ClientBounds;
			float aspect = (float)clientBounds.Width / (float)clientBounds.Height;

			Perspective(Camera.DEFAULT_FOVX, aspect, Camera.DEFAULT_ZNEAR, Camera.DEFAULT_ZFAR);

			actionKeys = new Dictionary<Actions, Keys>();

			actionKeys.Add(Actions.FlightYawLeftPrimary, Keys.Left);
			actionKeys.Add(Actions.FlightYawLeftAlternate, Keys.A);
			actionKeys.Add(Actions.FlightYawRightPrimary, Keys.Right);
			actionKeys.Add(Actions.FlightYawRightAlternate, Keys.D);
			actionKeys.Add(Actions.MoveForwardsPrimary, Keys.Up);
			actionKeys.Add(Actions.MoveForwardsAlternate, Keys.W);
			actionKeys.Add(Actions.MoveBackwardsPrimary, Keys.Down);
			actionKeys.Add(Actions.MoveBackwardsAlternate, Keys.S);
			actionKeys.Add(Actions.MoveDownPrimary, Keys.Q);
			actionKeys.Add(Actions.MoveDownAlternate, Keys.PageDown);
			actionKeys.Add(Actions.MoveUpPrimary, Keys.E);
			actionKeys.Add(Actions.MoveUpAlternate, Keys.PageUp);
			actionKeys.Add(Actions.OrbitRollLeftPrimary, Keys.Left);
			actionKeys.Add(Actions.OrbitRollLeftAlternate, Keys.A);
			actionKeys.Add(Actions.OrbitRollRightPrimary, Keys.Right);
			actionKeys.Add(Actions.OrbitRollRightAlternate, Keys.D);
			actionKeys.Add(Actions.StrafeRightPrimary, Keys.Right);
			actionKeys.Add(Actions.StrafeRightAlternate, Keys.D);
			actionKeys.Add(Actions.StrafeLeftPrimary, Keys.Left);
			actionKeys.Add(Actions.StrafeLeftAlternate, Keys.A);

			UpdateOrder = 1;
		}

		/// <summary>
		/// Initializes the CameraComponent class. This method repositions the
		/// mouse to the center of the game window.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			Rectangle clientBounds = Game.Window.ClientBounds;
			Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
		}

		/// <summary>
		/// Builds a look at style viewing matrix.
		/// </summary>
		/// <param name="target">The target position to look at.</param>
		public void LookAt(Vector3 target)
		{
			camera.LookAt(target);
		}

		/// <summary>
		/// Builds a look at style viewing matrix.
		/// </summary>
		/// <param name="eye">The camera position.</param>
		/// <param name="target">The target position to look at.</param>
		/// <param name="up">The up direction.</param>
		public void LookAt(Vector3 eye, Vector3 target, Vector3 up)
		{
			camera.LookAt(eye, target, up);
		}

		/// <summary>
		/// Binds an action to a keyboard key.
		/// </summary>
		/// <param name="action">The action to bind.</param>
		/// <param name="key">The key to map the action to.</param>
		public void MapActionToKey(Actions action, Keys key)
		{
			actionKeys[action] = key;
		}

		/// <summary>
		/// Moves the camera by dx world units to the left or right; dy
		/// world units upwards or downwards; and dz world units forwards
		/// or backwards.
		/// </summary>
		/// <param name="dx">Distance to move left or right.</param>
		/// <param name="dy">Distance to move up or down.</param>
		/// <param name="dz">Distance to move forwards or backwards.</param>
		public void Move(float dx, float dy, float dz)
		{
			camera.Move(dx, dy, dz);
		}

		/// <summary>
		/// Moves the camera the specified distance in the specified direction.
		/// </summary>
		/// <param name="direction">Direction to move.</param>
		/// <param name="distance">How far to move.</param>
		public void Move(Vector3 direction, Vector3 distance)
		{
			camera.Move(direction, distance);
		}

		/// <summary>
		/// Builds a perspective projection matrix based on a horizontal field
		/// of view.
		/// </summary>
		/// <param name="fovx">Horizontal field of view in degrees.</param>
		/// <param name="aspect">The viewport's aspect ratio.</param>
		/// <param name="znear">The distance to the near clip plane.</param>
		/// <param name="zfar">The distance to the far clip plane.</param>
		public void Perspective(float fovx, float aspect, float znear, float zfar)
		{
			camera.Perspective(fovx, aspect, znear, zfar);
		}

		/// <summary>
		/// Rotates the camera. Positive angles specify counter clockwise
		/// rotations when looking down the axis of rotation towards the
		/// origin.
		/// </summary>
		/// <param name="headingDegrees">Y axis rotation in degrees.</param>
		/// <param name="pitchDegrees">X axis rotation in degrees.</param>
		/// <param name="rollDegrees">Z axis rotation in degrees.</param>
		public void Rotate(float headingDegrees, float pitchDegrees, float rollDegrees)
		{
			camera.Rotate(headingDegrees, pitchDegrees, rollDegrees);
		}

		/// <summary>
		/// Updates the state of the CameraComponent class.
		/// </summary>
		/// <param name="gameTime">Time elapsed since the last call to Update.</param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			//UpdateInput();
			UpdateCamera(gameTime);
		}

		/// <summary>
		/// Undo any camera rolling by leveling the camera. When the camera is
		/// orbiting this method will cause the camera to become level with the
		/// orbit target.
		/// </summary>
		public void UndoRoll()
		{
			camera.UndoRoll();
		}

		/// <summary>
		/// Zooms the camera. This method functions differently depending on
		/// the camera's current behavior. When the camera is orbiting this
		/// method will move the camera closer to or further away from the
		/// orbit target. For the other camera behaviors this method will
		/// change the camera's horizontal field of view.
		/// </summary>
		///
		/// <param name="zoom">
		/// When orbiting this parameter is how far to move the camera.
		/// For the other behaviors this parameter is the new horizontal
		/// field of view.
		/// </param>
		/// 
		/// <param name="minZoom">
		/// When orbiting this parameter is the min allowed zoom distance to
		/// the orbit target. For the other behaviors this parameter is the
		/// min allowed horizontal field of view.
		/// </param>
		/// 
		/// <param name="maxZoom">
		/// When orbiting this parameter is the max allowed zoom distance to
		/// the orbit target. For the other behaviors this parameter is the max
		/// allowed horizontal field of view.
		/// </param>
		public void Zoom(float zoom, float minZoom, float maxZoom)
		{
			camera.Zoom(zoom, minZoom, maxZoom);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Determines which way to move the camera based on player input.
		/// The returned values are in the range [-1,1].
		/// </summary>
		/// <param name="direction">The movement direction.</param>
		private void GetMovementDirection(out Vector3 direction)
		{
			direction.X = 0.0f;
			direction.Y = 0.0f;
			direction.Z = 0.0f;

			if (currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveForwardsPrimary]) ||
				currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveForwardsAlternate]))
			{
				if (!movingAlongNegZ)
				{
					movingAlongNegZ = true;
					currentVelocity.Z = 0.0f;
				}

				direction.Z += 1.0f;
			}
			else
			{
				movingAlongNegZ = false;
			}

			if (currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveBackwardsPrimary]) ||
				currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveBackwardsAlternate]))
			{
				if (!movingAlongPosZ)
				{
					movingAlongPosZ = true;
					currentVelocity.Z = 0.0f;
				}

				direction.Z -= 1.0f;
			}
			else
			{
				movingAlongPosZ = false;
			}

			if (currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveUpPrimary]) ||
				currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveUpAlternate]))
			{
				if (!movingAlongPosY)
				{
					movingAlongPosY = true;
					currentVelocity.Y = 0.0f;
				}

				direction.Y += 1.0f;
			}
			else
			{
				movingAlongPosY = false;
			}

			if (currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveDownPrimary]) ||
				currentKeyboardState.IsKeyDown(actionKeys[Actions.MoveDownAlternate]))
			{
				if (!movingAlongNegY)
				{
					movingAlongNegY = true;
					currentVelocity.Y = 0.0f;
				}

				direction.Y -= 1.0f;
			}
			else
			{
				movingAlongNegY = false;
			}

			switch (CurrentBehavior)
			{
				case Camera.Behavior.FirstPerson:
				case Camera.Behavior.Spectator:
					if (currentKeyboardState.IsKeyDown(actionKeys[Actions.StrafeRightPrimary]) ||
						currentKeyboardState.IsKeyDown(actionKeys[Actions.StrafeRightAlternate]))
					{
						if (!movingAlongPosX)
						{
							movingAlongPosX = true;
							currentVelocity.X = 0.0f;
						}

						direction.X += 1.0f;
					}
					else
					{
						movingAlongPosX = false;
					}

					if (currentKeyboardState.IsKeyDown(actionKeys[Actions.StrafeLeftPrimary]) ||
						currentKeyboardState.IsKeyDown(actionKeys[Actions.StrafeLeftAlternate]))
					{
						if (!movingAlongNegX)
						{
							movingAlongNegX = true;
							currentVelocity.X = 0.0f;
						}

						direction.X -= 1.0f;
					}
					else
					{
						movingAlongNegX = false;
					}

					break;

				case Camera.Behavior.Flight:
					if (currentKeyboardState.IsKeyDown(actionKeys[Actions.FlightYawLeftPrimary]) ||
						currentKeyboardState.IsKeyDown(actionKeys[Actions.FlightYawLeftAlternate]))
					{
						if (!movingAlongPosX)
						{
							movingAlongPosX = true;
							currentVelocity.X = 0.0f;
						}

						direction.X += 1.0f;
					}
					else
					{
						movingAlongPosX = false;
					}

					if (currentKeyboardState.IsKeyDown(actionKeys[Actions.FlightYawRightPrimary]) ||
						currentKeyboardState.IsKeyDown(actionKeys[Actions.FlightYawRightAlternate]))
					{
						if (!movingAlongNegX)
						{
							movingAlongNegX = true;
							currentVelocity.X = 0.0f;
						}

						direction.X -= 1.0f;
					}
					else
					{
						movingAlongNegX = false;
					}
					break;

				case Camera.Behavior.Orbit:
					if (currentKeyboardState.IsKeyDown(actionKeys[Actions.OrbitRollLeftPrimary]) ||
						currentKeyboardState.IsKeyDown(actionKeys[Actions.OrbitRollLeftAlternate]))
					{
						if (!movingAlongPosX)
						{
							movingAlongPosX = true;
							currentVelocity.X = 0.0f;
						}

						direction.X += 1.0f;
					}
					else
					{
						movingAlongPosX = false;
					}

					if (currentKeyboardState.IsKeyDown(actionKeys[Actions.OrbitRollRightPrimary]) ||
						currentKeyboardState.IsKeyDown(actionKeys[Actions.OrbitRollRightAlternate]))
					{
						if (!movingAlongNegX)
						{
							movingAlongNegX = true;
							currentVelocity.X = 0.0f;
						}

						direction.X -= 1.0f;
					}
					else
					{
						movingAlongNegX = false;
					}
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Dampens the rotation by applying the rotation speed to it.
		/// </summary>
		/// <param name="headingDegrees">Y axis rotation in degrees.</param>
		/// <param name="pitchDegrees">X axis rotation in degrees.</param>
		/// <param name="rollDegrees">Z axis rotation in degrees.</param>
		private void RotateSmoothly(float headingDegrees, float pitchDegrees, float rollDegrees)
		{
			headingDegrees *= rotationSpeed;
			pitchDegrees *= rotationSpeed;
			rollDegrees *= rotationSpeed;

			Rotate(headingDegrees, pitchDegrees, rollDegrees);
		}

		/// <summary>
		/// Updates the camera's velocity based on the supplied movement
		/// direction and the elapsed time (since this method was last
		/// called). The movement direction is the in the range [-1,1].
		/// </summary>
		/// <param name="direction">Direction moved.</param>
		/// <param name="elapsedTimeSec">Elapsed game time.</param>
		private void UpdateVelocity(ref Vector3 direction, float elapsedTimeSec)
		{
			if (direction.X != 0.0f)
			{
				// Camera is moving along the x axis.
				// Linearly accelerate up to the camera's max speed.

				currentVelocity.X += direction.X * acceleration.X * elapsedTimeSec;

				if (currentVelocity.X > velocity.X)
					currentVelocity.X = velocity.X;
				else if (currentVelocity.X < -velocity.X)
					currentVelocity.X = -velocity.X;
			}
			else
			{
				// Camera is no longer moving along the x axis.
				// Linearly decelerate back to stationary state.

				if (currentVelocity.X > 0.0f)
				{
					if ((currentVelocity.X -= acceleration.X * elapsedTimeSec) < 0.0f)
						currentVelocity.X = 0.0f;
				}
				else
				{
					if ((currentVelocity.X += acceleration.X * elapsedTimeSec) > 0.0f)
						currentVelocity.X = 0.0f;
				}
			}

			if (direction.Y != 0.0f)
			{
				// Camera is moving along the y axis.
				// Linearly accelerate up to the camera's max speed.

				currentVelocity.Y += direction.Y * acceleration.Y * elapsedTimeSec;

				if (currentVelocity.Y > velocity.Y)
					currentVelocity.Y = velocity.Y;
				else if (currentVelocity.Y < -velocity.Y)
					currentVelocity.Y = -velocity.Y;
			}
			else
			{
				// Camera is no longer moving along the y axis.
				// Linearly decelerate back to stationary state.

				if (currentVelocity.Y > 0.0f)
				{
					if ((currentVelocity.Y -= acceleration.Y * elapsedTimeSec) < 0.0f)
						currentVelocity.Y = 0.0f;
				}
				else
				{
					if ((currentVelocity.Y += acceleration.Y * elapsedTimeSec) > 0.0f)
						currentVelocity.Y = 0.0f;
				}
			}

			if (direction.Z != 0.0f)
			{
				// Camera is moving along the z axis.
				// Linearly accelerate up to the camera's max speed.

				currentVelocity.Z += direction.Z * acceleration.Z * elapsedTimeSec;

				if (currentVelocity.Z > velocity.Z)
					currentVelocity.Z = velocity.Z;
				else if (currentVelocity.Z < -velocity.Z)
					currentVelocity.Z = -velocity.Z;
			}
			else
			{
				// Camera is no longer moving along the z axis.
				// Linearly decelerate back to stationary state.

				if (currentVelocity.Z > 0.0f)
				{
					if ((currentVelocity.Z -= acceleration.Z * elapsedTimeSec) < 0.0f)
						currentVelocity.Z = 0.0f;
				}
				else
				{
					if ((currentVelocity.Z += acceleration.Z * elapsedTimeSec) > 0.0f)
						currentVelocity.Z = 0.0f;
				}
			}
		}

		/// <summary>
		/// Moves the camera based on player input.
		/// </summary>
		/// <param name="direction">Direction moved.</param>
		/// <param name="elapsedTimeSec">Elapsed game time.</param>
		private void UpdatePosition(ref Vector3 direction, float elapsedTimeSec)
		{
			if (currentVelocity.LengthSquared() != 0.0f)
			{
				// Only move the camera if the velocity vector is not of zero
				// length. Doing this guards against the camera slowly creeping
				// around due to floating point rounding errors.

				Vector3 displacement = (currentVelocity * elapsedTimeSec) +
					(0.5f * acceleration * elapsedTimeSec * elapsedTimeSec);

				// Floating point rounding errors will slowly accumulate and
				// cause the camera to move along each axis. To prevent any
				// unintended movement the displacement vector is clamped to
				// zero for each direction that the camera isn't moving in.
				// Note that the UpdateVelocity() method will slowly decelerate
				// the camera's velocity back to a stationary state when the
				// camera is no longer moving along that direction. To account
				// for this the camera's current velocity is also checked.

				if (direction.X == 0.0f && (float)Math.Abs(currentVelocity.X) < 1e-6f)
					displacement.X = 0.0f;

				if (direction.Y == 0.0f && (float)Math.Abs(currentVelocity.Y) < 1e-6f)
					displacement.Y = 0.0f;

				if (direction.Z == 0.0f && (float)Math.Abs(currentVelocity.Z) < 1e-6f)
					displacement.Z = 0.0f;

				Move(displacement.X, displacement.Y, displacement.Z);
			}

			// Continuously update the camera's velocity vector even if the
			// camera hasn't moved during this call. When the camera is no
			// longer being moved the camera is decelerating back to its
			// stationary state.

			UpdateVelocity(ref direction, elapsedTimeSec);
		}

		/// <summary>
		/// Updates the state of the camera based on player input.
		/// </summary>
		/// <param name="gameTime">Elapsed game time.</param>
		private void UpdateCamera(GameTime gameTime)
		{
			float elapsedTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds;
			Vector3 direction = new Vector3();

			GetMovementDirection(out direction);

			float dx = 0.0f;
			float dy = 0.0f;
			float dz = 0.0f;

			//switch (camera.CurrentBehavior)
			//{
			//case Camera.Behavior.FirstPerson:
			//case Camera.Behavior.Spectator:
			//    dx = smoothedMouseMovement.X;
			//    dy = smoothedMouseMovement.Y;

			//    RotateSmoothly(dx, dy, 0.0f);
			//    UpdatePosition(ref direction, elapsedTimeSec);
			//    break;

			//case Camera.Behavior.Flight:
			//    dy = -smoothedMouseMovement.Y;
			//    dz = smoothedMouseMovement.X;

			//    RotateSmoothly(0.0f, dy, dz);

			//    if ((dx = direction.X * flightYawSpeed * elapsedTimeSec) != 0.0f)
			//        camera.Rotate(dx, 0.0f, 0.0f);

			//    direction.X = 0.0f; // ignore yaw motion when updating camera's velocity
			//    UpdatePosition(ref direction, elapsedTimeSec);
			//    break;

			//case Camera.Behavior.Orbit:
			//    dx = -smoothedMouseMovement.X;
			//    dy = -smoothedMouseMovement.Y;

			//    RotateSmoothly(dx, dy, 0.0f);

			//    if (!camera.PreferTargetYAxisOrbiting)
			//    {
			//        if ((dz = direction.X * orbitRollSpeed * elapsedTimeSec) != 0.0f)
			//            camera.Rotate(0.0f, 0.0f, dz);
			//    }

			//    if ((dz = GetMouseWheelDirection() * mouseWheelSpeed) != 0.0f)
			//        camera.Zoom(dz, camera.OrbitMinZoom, camera.OrbitMaxZoom);

			//    break;

			//default:
			//    break;
			//}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Property to get and set the camera's acceleration.
		/// </summary>
		public Vector3 Acceleration
		{
			get { return acceleration; }
			set { acceleration = value; }
		}

		/// <summary>
		/// Property to get and set the camera's behavior.
		/// </summary>
		public Camera.Behavior CurrentBehavior
		{
			get { return camera.CurrentBehavior; }
			set { camera.CurrentBehavior = value; }
		}

		/// <summary>
		/// Property to get the camera's current velocity.
		/// </summary>
		public Vector3 CurrentVelocity
		{
			get { return currentVelocity; }
		}

		/// <summary>
		/// Property to get and set the flight behavior's yaw speed.
		/// </summary>
		public float FlightYawSpeed
		{
			get { return flightYawSpeed; }
			set { flightYawSpeed = value; }
		}

		///// <summary>
		///// Property to get and set the sensitivity value used to smooth
		///// mouse movement.
		///// </summary>
		//public float MouseSmoothingSensitivity
		//{
		//    get { return mouseSmoothingSensitivity; }
		//    set { mouseSmoothingSensitivity = value; }
		//}

		/// <summary>
		/// Property to get and set the speed of the mouse wheel.
		/// This is used to zoom in and out when the camera is orbiting.
		/// </summary>
		public float MouseWheelSpeed
		{
			get { return mouseWheelSpeed; }
			set { mouseWheelSpeed = value; }
		}

		/// <summary>
		/// Property to get and set the max orbit zoom distance.
		/// </summary>
		public float OrbitMaxZoom
		{
			get { return camera.OrbitMaxZoom; }
			set { camera.OrbitMaxZoom = value; }
		}

		/// <summary>
		/// Property to get and set the min orbit zoom distance.
		/// </summary>
		public float OrbitMinZoom
		{
			get { return camera.OrbitMinZoom; }
			set { camera.OrbitMinZoom = value; }
		}

		/// <summary>
		/// Property to get and set the distance from the target when orbiting.
		/// </summary>
		public float OrbitOffsetDistance
		{
			get { return camera.OrbitOffsetDistance; }
			set { camera.OrbitOffsetDistance = value; }
		}

		/// <summary>
		/// Property to get and set the orbit behavior's rolling speed.
		/// This only applies when PreferTargetYAxisOrbiting is set to false.
		/// Orbiting with PreferTargetYAxisOrbiting set to true will ignore
		/// any camera rolling.
		/// </summary>
		public float OrbitRollSpeed
		{
			get { return orbitRollSpeed; }
			set { orbitRollSpeed = value; }
		}

		/// <summary>
		/// Property to get and set the camera orbit target position.
		/// </summary>
		public Vector3 OrbitTarget
		{
			get { return camera.OrbitTarget; }
			set { camera.OrbitTarget = value; }
		}

		/// <summary>
		/// Property to get and set the camera orientation.
		/// </summary>
		public Quaternion Orientation
		{
			get { return camera.Orientation; }
			set { camera.Orientation = value; }
		}

		/// <summary>
		/// Property to get and set the camera position.
		/// </summary>
		public Vector3 Position
		{
			get { return camera.Position; }
			set { camera.Position = value; }
		}

		/// <summary>
		/// Property to get and set the flag to force the camera
		/// to orbit around the orbit target's Y axis rather than the camera's
		/// local Y axis.
		/// </summary>
		public bool PreferTargetYAxisOrbiting
		{
			get { return camera.PreferTargetYAxisOrbiting; }
			set { camera.PreferTargetYAxisOrbiting = value; }
		}

		/// <summary>
		/// Property to get the perspective projection matrix.
		/// </summary>
		public Matrix ProjectionMatrix
		{
			get { return camera.ProjectionMatrix; }
		}

		/// <summary>
		/// Property to get and set the mouse rotation speed.
		/// </summary>
		public float RotationSpeed
		{
			get { return rotationSpeed; }
			set { rotationSpeed = value; }
		}

		/// <summary>
		/// Property to get and set the camera's velocity.
		/// </summary>
		public Vector3 Velocity
		{
			get { return velocity; }
			set { velocity = value; }
		}

		/// <summary>
		/// Property to get the viewing direction vector.
		/// </summary>
		public Vector3 ViewDirection
		{
			get { return camera.ViewDirection; }
		}

		/// <summary>
		/// Property to get the view matrix.
		/// </summary>
		public Matrix ViewMatrix
		{
			get { return camera.ViewMatrix; }
		}

		/// <summary>
		/// Property to get the concatenated view-projection matrix.
		/// </summary>
		public Matrix ViewProjectionMatrix
		{
			get { return camera.ViewProjectionMatrix; }
		}

		/// <summary>
		/// Property to get the camera's local X axis.
		/// </summary>
		public Vector3 XAxis
		{
			get { return camera.XAxis; }
		}

		/// <summary>
		/// Property to get the camera's local Y axis.
		/// </summary>
		public Vector3 YAxis
		{
			get { return camera.YAxis; }
		}

		/// <summary>
		/// Property to get the camera's local Z axis.
		/// </summary>
		public Vector3 ZAxis
		{
			get { return camera.ZAxis; }
		}

		#endregion
	}
}
