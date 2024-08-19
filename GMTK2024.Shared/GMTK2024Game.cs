using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;
using GMTK2024.UI;

namespace GMTK2024
{
	public static class Globals
	{
		public const int TILE_SIZE = 64;
		public static readonly Rectangle TILE_BOUNDS = new( -16, -3, 32, 17 );
		public static readonly Rectangle PIXEL_BOUNDS = new( TILE_BOUNDS.Left * TILE_SIZE, -TILE_BOUNDS.Bottom * TILE_SIZE, TILE_BOUNDS.Width * TILE_SIZE, TILE_BOUNDS.Height * TILE_SIZE );
	}

	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class GMTK2024Game : Game
	{
		public static GMTK2024Game Instance { get; private set; }

		public Wall Wall { get; private set; }

		public IReadOnlyDictionary<string, Texture2DRegion> Holds => m_holds;
		Dictionary<string, Texture2DRegion> m_holds = new();

		private readonly GraphicsDeviceManager m_graphics;
		private SpriteBatch m_spriteBatch;
		private Ground m_ground;
		private UIRoot m_ui;

		private Texture2DAtlas m_groundAtlas;
		private Texture2DAtlas m_wallAtlas;

		private OrthographicCamera m_camera;

		/// pixels per second
		private const float SCROLL_SPEED = 400;

		/// % per notch
		private const float ZOOM_SPEED = 0.05f;

		/// % per second
		private const float KB_ZOOM_SPEED = 0.75f;

		private const string HOLD_PREFIX = "hold_";

		public GMTK2024Game()
		{
			Instance = this;

			m_graphics = new GraphicsDeviceManager( this );
			m_graphics.PreferredBackBufferWidth = 1920;
			m_graphics.PreferredBackBufferHeight = 1080;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new( GraphicsDevice );

			m_groundAtlas = Utils.CreateAtlasFromPacked( "Content/GroundTiles.png", GraphicsDevice );
			m_wallAtlas = Utils.CreateAtlasFromPacked( "Content/WallTiles.png", GraphicsDevice );

			foreach( var region in m_wallAtlas )
			{
				if( region.Name.StartsWith( HOLD_PREFIX, StringComparison.Ordinal ) )
				{
					string holdName = region.Name.Substring( HOLD_PREFIX.Length );
					m_holds.Add( holdName, region );
				}
			}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 1920, 1080);
			m_camera = new( viewportAdapter )
			{
				Position = new Vector2( -viewportAdapter.ViewportWidth / 2, -viewportAdapter.ViewportHeight / 2 )
			};

			IsMouseVisible = true;
			Mouse.SetCursor( MouseCursor.Arrow );

			m_ui = new UIRoot( GraphicsDevice );
			m_ui.Initialize();

			Wall = new Wall( m_wallAtlas, 6, 12 )
			{
				X = -3
			};
			m_ground = new Ground( m_groundAtlas );
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update( GameTime gameTime )
		{
			KeyboardExtended.Update();
			KeyboardStateExtended keyboard = KeyboardExtended.GetState();

			MouseExtended.Update();
			MouseStateExtended mouse = MouseExtended.GetState();

			float delta_t = gameTime.GetElapsedSeconds();

			if( keyboard.IsKeyDown( Keys.Escape ) )
			{
				Exit();
			}


			UpdateCameraInput( keyboard, mouse, delta_t );

			m_ui.Update( keyboard, mouse, delta_t );

			base.Update( gameTime );
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( new Color( 180, 180, 180 ) );

			m_ground.Draw( m_spriteBatch, m_camera );

			Wall.Draw( m_spriteBatch, m_camera, Color.Wheat );

			m_ui.Draw( m_spriteBatch, m_camera );

			base.Draw( gameTime );
		}

		private void UpdateCameraInput( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
		{
			UpdateDrag( keyboard, mouse, delta_t );

			//UpdateSmoothZoom( keyboard, mouse, delta_t );
			UpdateDiscreteZoom( keyboard, mouse, delta_t );

			float scrollSpeed = SCROLL_SPEED;
			if( keyboard.IsShiftDown() )
			{
				scrollSpeed *= 2;
			}

			if( keyboard.IsKeyDown( Keys.A ) || keyboard.IsKeyDown( Keys.Left ) )
			{
				m_camera.Move( new Vector2( -scrollSpeed * delta_t, 0 ) );
			}
			if( keyboard.IsKeyDown( Keys.D ) || keyboard.IsKeyDown( Keys.Right ) )
			{
				m_camera.Move( new Vector2( scrollSpeed * delta_t, 0 ) );
			}
			if( keyboard.IsKeyDown( Keys.W ) )
			{
				m_camera.Move( new Vector2( 0, -scrollSpeed * delta_t ) );
			}
			if( keyboard.IsKeyDown( Keys.S ) )
			{
				m_camera.Move( new Vector2( 0, scrollSpeed * delta_t ) );
			}

			Vector2 position = m_camera.Position;
			if( m_camera.BoundingRectangle.Left < Globals.PIXEL_BOUNDS.Left )
			{
				position.X -= m_camera.BoundingRectangle.Left - Globals.PIXEL_BOUNDS.Left;
			}
			else if( m_camera.BoundingRectangle.Right > Globals.PIXEL_BOUNDS.Right )
			{
				position.X -= m_camera.BoundingRectangle.Right - Globals.PIXEL_BOUNDS.Right;
			}

			if( m_camera.BoundingRectangle.Top < Globals.PIXEL_BOUNDS.Top )
			{
				position.Y -= m_camera.BoundingRectangle.Top - Globals.PIXEL_BOUNDS.Top;
			}
			else if( m_camera.BoundingRectangle.Bottom > Globals.PIXEL_BOUNDS.Bottom )
			{
				position.Y -= m_camera.BoundingRectangle.Bottom - Globals.PIXEL_BOUNDS.Bottom;
			}
			m_camera.Position = position;
		}

		private Point? m_dragStart;
		private bool m_dragging;

		private void UpdateDrag( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
		{
			if( mouse.WasButtonPressed( MouseButton.Right ) || mouse.WasButtonPressed( MouseButton.Middle ) )
			{
				m_dragStart = mouse.Position;
				Mouse.SetCursor( MouseCursor.SizeAll );
			}

			if( mouse.WasButtonReleased( MouseButton.Right ) || mouse.WasButtonReleased( MouseButton.Middle ) )
			{
				m_dragStart = null;
				m_dragging = false;
				Mouse.SetCursor( MouseCursor.Arrow );
			}

			if( m_dragStart != null )
			{
				if( (mouse.Position - m_dragStart.Value).ToVector2().LengthSquared() > 25 )
				{
					m_dragging = true;
				}
			}

			if( m_dragging )
			{
				m_camera.Move( mouse.DeltaPosition.ToVector2() / m_camera.Zoom );
			}
		}

		private float m_zoomInterp = 0.5f;

		private void UpdateSmoothZoom( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
		{
			if( mouse.DeltaScrollWheelValue > 0 )
			{
				m_zoomInterp = m_zoomInterp - ZOOM_SPEED;
			}
			else if( mouse.DeltaScrollWheelValue < 0 )
			{
				m_zoomInterp =  m_zoomInterp + ZOOM_SPEED;
			}
			 
			if( keyboard.IsKeyDown( Keys.Q ) )
			{
				m_zoomInterp = m_zoomInterp - (KB_ZOOM_SPEED * delta_t);
			}
			else if( keyboard.IsKeyDown( Keys.E ) )
			{
				m_zoomInterp = Math.Clamp( m_zoomInterp + (KB_ZOOM_SPEED * delta_t), 0.0f, 1.0f );
			}

			m_zoomInterp = Math.Clamp( m_zoomInterp, 0.0f, 1.0f );

			m_camera.Zoom = Utils.Eerp( m_camera.MinimumZoom, m_camera.MaximumZoom, m_zoomInterp );
		}

		private static readonly float[] ZoomLevels = [ 1, 2, 3, 4 ];
		private int m_zoomIndex = Array.IndexOf( ZoomLevels, 1 );

		private void UpdateDiscreteZoom( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
		{
			if( keyboard.WasKeyPressed( Keys.Q ) )
			{
				m_zoomIndex -= 1;
			}
			if( keyboard.WasKeyPressed( Keys.E ) )
			{
				m_zoomIndex += 1;
			}

			if( mouse.DeltaScrollWheelValue > 0 )
			{
				m_zoomIndex -= 1;
			}
			else if( mouse.DeltaScrollWheelValue < 0 )
			{
				m_zoomIndex += 1;
			}

			m_zoomIndex = Math.Clamp( m_zoomIndex, 0, ZoomLevels.Length - 1 );

			m_camera.Zoom = ZoomLevels[ m_zoomIndex ];
		}
	}
}
