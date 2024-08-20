using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input;
using MonoGame.Extended.ViewportAdapters;
using System;
using GMTK2024.UI;
using System.Collections.Generic;

namespace GMTK2024
{
	public static class Globals
	{
		public const int TILE_SIZE = 64;
		public const int TILE_LEFT = -16;
		public const int TILE_RIGHT = 14;
		public const int TILE_TOP = 14;
		public const int TILE_BOTTOM = -3;
		public const int TILE_WIDTH = TILE_RIGHT - TILE_LEFT;
		public const int TILE_HEIGHT = TILE_TOP - TILE_BOTTOM;

		public static readonly Rectangle PIXEL_BOUNDS = new( TILE_LEFT * TILE_SIZE, -TILE_TOP * TILE_SIZE, TILE_WIDTH * TILE_SIZE, TILE_HEIGHT * TILE_SIZE );

		// 1 second = 5 minutes
		public const float TIME_SCALE = 60 * 5;
		public const float ACCELERATED_TIME_SCALE = 60 * 5 * 10;

		public static Vector2 TileToWorld( Vector2 tile )
		{
			return TileToWorld( tile.X, tile.Y );
		}

		public static Vector2 TileToWorld( float x, float y)
		{
			return new Vector2( x * TILE_SIZE, -y * TILE_SIZE );
		}
	}

	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class GMTK2024Game : Game
	{
		public static GMTK2024Game Instance { get; private set; }

		public UIRoot UI { get; private set; }

		public OrthographicCamera Camera { get; private set; }

		public Wall Wall { get; private set; }

		public TimeOnly CurrentTime { get; private set; } = new TimeOnly( 7, 0 );

		public bool IsPaused { get; set; }

		public bool IsNightTime => CurrentTime > new TimeOnly( 21, 0 ) || CurrentTime < new TimeOnly( 7, 0 );

		private readonly GraphicsDeviceManager m_graphics;
		private SpriteBatch m_spriteBatch;
		private Ground m_ground;
		private EditMode m_editMode;
		private Background m_background;

		/// pixels per second
		private const float SCROLL_SPEED = 400;

		/// % per notch
		private const float ZOOM_SPEED = 0.05f;

		/// % per second
		private const float KB_ZOOM_SPEED = 0.75f;

		public GMTK2024Game()
		{
			Instance = this;

			m_graphics = new GraphicsDeviceManager( this );
			m_graphics.PreferredBackBufferWidth = 1920;
			m_graphics.PreferredBackBufferHeight = 1080;
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new( GraphicsDevice );
		}

		protected override void Initialize()
		{
			base.Initialize();

			var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 1920, 1080);
			Camera = new( viewportAdapter )
			{
				Position = new Vector2( -viewportAdapter.ViewportWidth / 2, -viewportAdapter.ViewportHeight / 2 )
			};

			IsMouseVisible = true;
			//Mouse.SetCursor( MouseCursor.Arrow );

			UI = new UIRoot( GraphicsDevice );

			Wall = new Wall( 12, 5 )
			{
				X = -6,
				Color = Color.Wheat
			};
			m_ground = new Ground();
			m_background = new Background();

			m_editMode = new EditMode();
		}

		protected override void Update( GameTime gameTime )
		{
			KeyboardExtended.Update();
			KeyboardStateExtended keyboard = KeyboardExtended.GetState();

			MouseExtended.Update();
			MouseStateExtended mouse = MouseExtended.GetState();

			float delta_t = gameTime.GetElapsedSeconds();

			UpdateCameraInput( keyboard, mouse, delta_t );

			m_background.Update( delta_t );

			UI.Update( keyboard, mouse, delta_t );

			m_editMode.Update( keyboard, mouse, delta_t );

			base.Update( gameTime );
		}

		private static readonly List<Color> DAY_NIGHT_COLORS = new List<Color>()
		{
			new( 3, 21, 72 ), // 0
			new( 0, 28, 64 ), // 1
			new( 0, 44, 87 ), // 2
			new( 0, 42, 83 ), // 3
			new( 0, 51, 88 ), // 4
			new( 0, 53, 91 ), // 5
			new( 1, 72, 106 ),  // 6
			new( 6, 88, 126 ),  // 7
			new( 14, 129, 160 ),  // 8
			new( 77, 181, 189 ),  // 9
			new( 197, 227, 194 ),  // 10
			new( 230, 224, 114 ),  // 11
			new( 245, 205, 100 ),  // 12
			new( 255, 194, 108 ),  // 13
			new( 254, 191, 95 ),  // 14
			new( 254, 187, 94 ),  // 15
			new( 247, 164, 83 ),  // 16
			new( 244, 132, 110 ),  // 17
			new( 218, 108, 131 ),  // 18
			new( 135, 64, 134 ),  // 19
			new( 78, 37, 125 ),  // 20
			new( 52, 26, 114 ),  // 21
			new( 37, 37, 104 ),  // 22
			new( 16, 26, 81 ),  // 23
		};

		private Color GetDayNightColor()
		{
			Color currentColor = DAY_NIGHT_COLORS[ CurrentTime.Hour ];
			Color nextColor = CurrentTime.Hour == 23 ? DAY_NIGHT_COLORS[ 0 ] : DAY_NIGHT_COLORS[ CurrentTime.Hour + 1 ];

			float t = CurrentTime.Minute / 60.0f;

			return Color.Lerp( currentColor, nextColor, t );
		}

		protected override void Draw( GameTime gameTime )
		{
			float delta_t = gameTime.GetElapsedSeconds();

			float timeScale = IsNightTime ? Globals.ACCELERATED_TIME_SCALE : Globals.TIME_SCALE;
			if( IsPaused )
			{
				timeScale = 0;
			}
			CurrentTime = CurrentTime.Add( TimeSpan.FromSeconds( delta_t * timeScale ) );

			Color dayNightColor = GetDayNightColor();

			GraphicsDevice.Clear( dayNightColor );

			m_background.Draw( m_spriteBatch, Camera );

			m_ground.Draw( m_spriteBatch, Camera );

			Wall.Draw( m_spriteBatch, Camera );

			UI.Draw( m_spriteBatch, Camera );

			m_editMode.Draw( m_spriteBatch, Camera );

			m_spriteBatch.Begin();

			m_spriteBatch.DrawString( UI.Font32, CurrentTime.ToString(), new Vector2( 10, 10 ), Color.White );

			m_spriteBatch.End();

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
				Camera.Move( new Vector2( -scrollSpeed * delta_t, 0 ) );
			}
			if( keyboard.IsKeyDown( Keys.D ) || keyboard.IsKeyDown( Keys.Right ) )
			{
				Camera.Move( new Vector2( scrollSpeed * delta_t, 0 ) );
			}
			if( keyboard.IsKeyDown( Keys.W ) )
			{
				Camera.Move( new Vector2( 0, -scrollSpeed * delta_t ) );
			}
			if( keyboard.IsKeyDown( Keys.S ) )
			{
				Camera.Move( new Vector2( 0, scrollSpeed * delta_t ) );
			}

			Vector2 position = Camera.Position;
			if( Camera.BoundingRectangle.Left < Globals.PIXEL_BOUNDS.Left )
			{
				position.X -= Camera.BoundingRectangle.Left - Globals.PIXEL_BOUNDS.Left;
			}
			else if( Camera.BoundingRectangle.Right > Globals.PIXEL_BOUNDS.Right )
			{
				position.X -= Camera.BoundingRectangle.Right - Globals.PIXEL_BOUNDS.Right;
			}

			if( Camera.BoundingRectangle.Top < Globals.PIXEL_BOUNDS.Top )
			{
				position.Y -= Camera.BoundingRectangle.Top - Globals.PIXEL_BOUNDS.Top;
			}
			else if( Camera.BoundingRectangle.Bottom > Globals.PIXEL_BOUNDS.Bottom )
			{
				position.Y -= Camera.BoundingRectangle.Bottom - Globals.PIXEL_BOUNDS.Bottom;
			}
			Camera.Position = position;
		}

		private Point? m_dragStart;
		private bool m_dragging;

		private void UpdateDrag( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
		{
			if( m_zoomIndex == 0 )
			{
				StopDrag();
				return;
			}

			if( mouse.WasButtonReleased( MouseButton.Right ) || mouse.WasButtonReleased( MouseButton.Middle ) )
			{
				StopDrag();
				return;
			}

			if( mouse.WasButtonPressed( MouseButton.Right ) || mouse.WasButtonPressed( MouseButton.Middle ) )
			{
				m_dragStart = mouse.Position;
				//Mouse.SetCursor( MouseCursor.SizeAll );
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
				Camera.Move( mouse.DeltaPosition.ToVector2() / Camera.Zoom );
			}
		}

		private void StopDrag()
		{
			m_dragStart = null;
			m_dragging = false;
			//Mouse.SetCursor( MouseCursor.Arrow );
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

			Camera.Zoom = Utils.Eerp( Camera.MinimumZoom, Camera.MaximumZoom, m_zoomInterp );
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

			Camera.Zoom = ZoomLevels[ m_zoomIndex ];
		}
	}
}
