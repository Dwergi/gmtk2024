using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.IO;

namespace GMTK2024
{
	public static class Globals
	{
		public const int TILE_SIZE = 64;
		public const float GROUND_LEVEL = 0;
		public static readonly Rectangle TILE_BOUNDS = new( -100, -10, 200, 60 );
		public static readonly Rectangle PIXEL_BOUNDS = new( TILE_BOUNDS.Left * TILE_SIZE, -TILE_BOUNDS.Bottom * TILE_SIZE, TILE_BOUNDS.Width * TILE_SIZE, TILE_BOUNDS.Height * TILE_SIZE );
	}

	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class GMTK2024Game : Game
	{
		private readonly GraphicsDeviceManager m_graphics;
		private SpriteBatch m_spriteBatch;
		private Wall m_wall;
		private Ground m_ground;
		private BitmapFont m_font16;
		private BitmapFont m_font32;
		private NinePatch m_buttonNinePatch;

		private OrthographicCamera m_camera;
		private float ZoomValue = 0.5f;

		/// pixels per second
		private const float SCROLL_SPEED = 400;

		/// % per notch
		private const float ZOOM_SPEED = 0.05f;

		/// % per second
		private const float KB_ZOOM_SPEED = 0.75f;

		private Button m_testButton;

		public GMTK2024Game()
		{
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

			Texture2DAtlas wallAtlas = Utils.CreateAtlasFromPacked( "Content/WallTiles.png", GraphicsDevice );
			m_wall = new Wall( wallAtlas );

			Texture2DAtlas groundAtlas = Utils.CreateAtlasFromPacked( "Content/GroundTiles.png", GraphicsDevice );
			m_ground = new Ground( groundAtlas );

			m_buttonNinePatch = Utils.CreateNinePatchFromPacked( "Content/button.png", GraphicsDevice );

			m_font16 = BitmapFont.FromFile( GraphicsDevice, "Content/KenneyMini16.fnt" );
			m_font32 = BitmapFont.FromFile( GraphicsDevice, "Content/KenneyMini32.fnt" );
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
				MinimumZoom = 0.3f,
				MaximumZoom = 2.0f,
				Position = new Vector2( -viewportAdapter.ViewportWidth / 2, -viewportAdapter.ViewportHeight / 2 )
			};

			IsMouseVisible = true;
			Mouse.SetCursor( MouseCursor.Arrow );

			m_testButton = new Button( m_buttonNinePatch, m_font32 )
			{
				X = 100,
				Y = 100,
				Padding = new( 10, 5 ),
				Text = "Test!",
				TextColor = Color.Black
			};
		}

		private Vector2 m_debugMousePos;

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

			m_debugMousePos = mouse.Position.ToVector2();

			UpdateCameraInput( keyboard, mouse, delta_t );

			m_testButton.Update( mouse );

			if( m_testButton.WasClicked )
			{
				if( m_testButton.Text == "Clicked!" )
				{
					m_testButton.Text = "Test";
				}
				else
				{
					m_testButton.Text = "Clicked!";
				}
			}

			base.Update( gameTime );
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( Color.Black );

			m_spriteBatch.Begin( transformMatrix: m_camera.GetViewMatrix() );

			m_ground.Draw( m_spriteBatch, m_camera );

			m_spriteBatch.End();

			m_spriteBatch.Begin( transformMatrix: m_camera.GetViewMatrix() );

			m_wall.Draw( m_spriteBatch, Color.Wheat );

			m_spriteBatch.End();

			m_spriteBatch.Begin();

			Vector2 screenPos = m_camera.ScreenToWorld( m_debugMousePos );
			string mousePosString = $"{screenPos.X:F1}, {screenPos.Y:F1}";
			SizeF mousePosSize = m_font16.MeasureString( mousePosString );

			m_spriteBatch.DrawString( m_font16, mousePosString, new Vector2( 1920 - (mousePosSize.Width + 10), 10 ), Color.White );

			m_testButton.Draw( m_spriteBatch );

			m_spriteBatch.End();

			base.Draw( gameTime );
		}

		private Point? m_dragStart;
		private bool m_dragging;

		private void UpdateCameraInput( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
		{
			if( mouse.DeltaScrollWheelValue > 0 )
			{
				ZoomValue = Math.Clamp( ZoomValue - ZOOM_SPEED, 0.0f, 1.0f );
			}
			else if( mouse.DeltaScrollWheelValue < 0 )
			{
				ZoomValue = Math.Clamp( ZoomValue + ZOOM_SPEED, 0.0f, 1.0f );
			}

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

			if( keyboard.IsKeyDown( Keys.Q ) )
			{
				ZoomValue = Math.Clamp( ZoomValue - (KB_ZOOM_SPEED * delta_t), 0.0f, 1.0f );
			}
			if( keyboard.IsKeyDown( Keys.E ) )
			{
				ZoomValue = Math.Clamp( ZoomValue + (KB_ZOOM_SPEED * delta_t), 0.0f, 1.0f );
			}

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

			m_camera.Zoom = Utils.Eerp( m_camera.MinimumZoom, m_camera.MaximumZoom, ZoomValue );
		}
	}
}
