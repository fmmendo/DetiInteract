using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DetiInteract.ModelViewerLib
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ModelViewer : Microsoft.Xna.Framework.Game
	{
		#region Fields
		/// <summary>
		/// The Graphics Device Manager.
		/// </summary>
		private GraphicsDeviceManager _graphics;

		private CameraComponent _camera;

		/// <summary>
		/// Reference to the model to be rendered.
		/// </summary>
		private Model _model;

		private List<String> _ModelList = new List<String>();
		private int _selectedModel = -1;

		/// <summary>
		/// Quaternion to rotate the model
		/// </summary>
		private Quaternion _modelOrientation;

		/// <summary>
		/// Vector to position the model
		/// </summary>
		private Vector3 _modelPosition;

		/// <summary>
		/// Model Bone Transforms
		/// </summary>
		private Matrix[] _modelTransforms;

		public bool mobileInteracting = false;

		/// <summary>
		/// The aspect ratio in which to render the model.
		/// </summary>
        private float _aspectRatio;

		/// <summary>
		/// Vector to position the camera
		/// </summary>
        private Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);

		public bool IsReady { get; private set; }

		/// <summary>
		/// Quaternion to rotate the model
		/// </summary>
		//public Quaternion Rotation { get; set; }

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public ModelViewer()
        {
			IsReady = false;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

			_camera = new CameraComponent(this);
			Components.Add(_camera);
			_camera.CurrentBehavior = Camera.Behavior.Orbit;
        }

		public void RotateCamera(float x, float y, float z)
		{
			if (mobileInteracting) 
			{
				_modelOrientation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(x),
																	  MathHelper.ToRadians(y),
																	  MathHelper.ToRadians(z));
			}
			else
			{
				_camera.Rotate(x, y, z);
			}
		}

		#region XNA Overridden operations
		/// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
			// Reset Rotation
			_modelOrientation = Quaternion.Identity;

			// Set aspect ration for the XNA app
            _aspectRatio = _graphics.GraphicsDevice.Viewport.AspectRatio;

			_camera.Perspective(90.0f, _aspectRatio, 0.1f, 100.0f);
			_camera.Position = new Vector3(0.0f, 0.0f, 5.0f);
			_camera.Acceleration = new Vector3(4.0f, 4.0f, 4.0f);
			_camera.Velocity = new Vector3(1.0f, 1.0f, 1.0f);
			_camera.OrbitMinZoom = 3.0f;
			_camera.OrbitMaxZoom = 20.0f;
			_camera.OrbitOffsetDistance = _camera.OrbitMinZoom;

            base.Initialize();

			IsReady = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load all of your content.
        /// </summary>
        protected override void LoadContent()
        {
			_ModelList.Add("teapot");
			_ModelList.Add("bigship1");

			LoadModel(0);
		}

		public void LoadModel(int index)
		{
			if (index >= 0 && index < _ModelList.Count)
			{
				_model = null;
				_modelTransforms = null;
				_model = Content.Load<Model>(_ModelList[index]);

				_selectedModel = index;
			}
		}

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
			// Set background color
            GraphicsDevice.Clear(Color.Transparent);

			if (_selectedModel >= 0)
			{
				if (_modelTransforms == null)
				{
					_modelTransforms = new Matrix[_model.Bones.Count];
					_model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
				}

				foreach (ModelMesh mesh in _model.Meshes)
				{
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.EnableDefaultLighting();
						if (mobileInteracting)
						{
							effect.World = _modelTransforms[mesh.ParentBone.Index] * Matrix.CreateFromQuaternion(_modelOrientation) * Matrix.CreateTranslation(_modelPosition);
							effect.View = Matrix.CreateLookAt(_cameraPosition, Vector3.Zero, Vector3.Up);
							effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), _aspectRatio, 1.0f, 1000.0f);
						}
						else
						{
							effect.View = _camera.ViewMatrix;
							effect.Projection = _camera.ProjectionMatrix;
							effect.World = _modelTransforms[mesh.ParentBone.Index] * Matrix.CreateFromQuaternion(_modelOrientation);
						}
					}

					mesh.Draw();
				}
			}

            base.Draw(gameTime);
		}
		#endregion
	}
}
