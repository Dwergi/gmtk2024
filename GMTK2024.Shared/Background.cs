using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMTK2024;

public class Background : IDrawable
{
	private readonly Texture2DAtlas m_atlas;
	private readonly Texture2DRegion m_wall;
	private readonly Texture2DRegion m_door;
	private readonly Texture2DRegion m_exitSign;
	private readonly Texture2DRegion m_plant;
	private readonly Texture2DRegion m_counter;

	private readonly List<Texture2DRegion> m_cashierFrames;
	private int m_currentCashierFrame;
	private float m_timeToSwitch = 1.0f;
	private Random m_random = new();

	private const string CASHIER_PREFIX = "cashier_";

	public Background()
	{
		m_atlas = Utils.CreateAtlasFromPacked( "Content/Background.png", GMTK2024Game.Instance.GraphicsDevice );

		m_wall = m_atlas.GetRegion( "wall" );
		m_door = m_atlas.GetRegion( "door" );
		m_exitSign = m_atlas.GetRegion( "exit" );
		m_plant = m_atlas.GetRegion( "plant" );
		m_counter = m_atlas.GetRegion( "counter" );
		
		m_cashierFrames = m_atlas.Where( r => r.Name.StartsWith(CASHIER_PREFIX  ) )
			.OrderBy( r => int.Parse( r.Name[ CASHIER_PREFIX.Length.. ] ) )
			.ToList();
	}

	private static readonly Vector2 ORIGIN = new( -15, 0 );
	private static readonly Vector2 DOOR_POSITION = new( 1, 2 );
	private static readonly Rectangle DOOR_BOUNDS = new( DOOR_POSITION.ToPoint(), new( 1, 2 ) );
	private static readonly Vector2 EXIT_POSITION = new( 1, 2.5f );
	private static readonly Vector2 PLANT_POSITION = new( 0, 1 );
	private static readonly Vector2 COUNTER_POSITION = new( 2.2f, 1.8f );
	private static readonly Vector2 CASHIER_POSITION = new( 2.5f, 1.8f );

	public void Draw( SpriteBatch batch, OrthographicCamera camera )
	{
		batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix() );


		bool isNight = GMTK2024Game.Instance.IsNightTime;


		for( int y = 0; y < Globals.TILE_TOP - 1; ++y )
		{
			for( int x = Globals.TILE_LEFT + 1; x < Globals.TILE_RIGHT - 1; ++x )
			{
				if( x == (ORIGIN + DOOR_POSITION).X && y == (ORIGIN + DOOR_POSITION).Y )
				{
					continue;
				}

				batch.Draw( m_wall, Globals.TileToWorld( x, y ), isNight ? new( 50, 50, 50 ) : Color.White );
			}
		}

		Color tint = isNight ? new( 50, 50, 50 ) : new( 180, 180, 180 );

		batch.Draw( m_door, Globals.TileToWorld( ORIGIN + DOOR_POSITION ), tint );
		batch.Draw( m_exitSign, Globals.TileToWorld( ORIGIN + EXIT_POSITION ) + new Vector2( m_exitSign.Width / 2, 4 ), Color.White );
		batch.Draw( m_plant, Globals.TileToWorld( ORIGIN + PLANT_POSITION ), tint );

		if( !isNight )
		{
			batch.Draw( m_cashierFrames[ m_currentCashierFrame ], Globals.TileToWorld( ORIGIN + CASHIER_POSITION ), Color.White );
		}

		batch.Draw( m_counter, Globals.TileToWorld( ORIGIN + COUNTER_POSITION ), tint );

		batch.End();
	}

	public void Update( float delta_t )
	{
		m_timeToSwitch -= delta_t;

		if( m_timeToSwitch < 0 )
		{
			m_currentCashierFrame = m_random.Next( 0, m_cashierFrames.Count );
			m_timeToSwitch = 3.0f + (float) m_random.NextDouble() * 4.0f;
		}
	}
}