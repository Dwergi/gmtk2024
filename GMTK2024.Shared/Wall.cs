using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMTK2024
{

	public class HoldType
	{
		public string Name;
		public Texture2DRegion Sprite;
	}

	public class HoldSlot
	{
		public Vector2 Position { get; }
		public float Rotation;
		public HoldType Type;
		public Color Color = Color.White;

		public HoldSlot( Vector2 position )
		{
			Position = position;
		}
	}

	public class Wall : IDrawable
	{
		private class Tile
		{
			public Texture2DRegion Region;
		}

		/// <summary>
		/// X offset.
		/// </summary>
		public int X
		{
			get => m_x;
			set => m_x = value;
		}

		/// <summary>
		/// Height in meters.
		/// </summary>
		public int Height
		{
			get => m_height;
			set
			{
				ResizeTiles( Width, value );

				m_height = value;
			}
		}

		/// <summary>
		/// Width in meters.
		/// </summary>
		public int Width
		{
			get => m_width;
			set
			{
				ResizeTiles( value, Height );

				m_width = value;
			}
		}

		public Rectangle WorldBounds => new( WallToWorld( 0, Height ).ToPoint(), new( Width * Globals.TILE_SIZE, Height * Globals.TILE_SIZE ) );

		public Color Color
		{
			get;
			set;
		}

		private const string HOLD_PREFIX = "hold_";

		public IReadOnlyList<HoldType> HoldTypes => m_holdTypes;
		private List<HoldType> m_holdTypes = new();

		public IReadOnlyList<HoldSlot> HoldSlots => m_holdSlots;
		private List<HoldSlot> m_holdSlots = new();

		private const float HOLD_SEPARATION = 0.25f;

		private readonly Texture2DAtlas m_atlas;
		private readonly Texture2DRegion m_defaultRegion;
		private readonly Texture2DRegion m_holeRegion;

		private int m_height;
		private int m_width;
		private int m_x;

		private Tile[] m_tiles;

		public Wall( int width, int height )
		{
			m_atlas = Utils.CreateAtlasFromPacked( "Content/WallTiles.png", GMTK2024Game.Instance.GraphicsDevice );
			m_defaultRegion = m_atlas.GetRegion( "default" );
			m_holeRegion = m_atlas.GetRegion( "hole" );

			foreach( var region in m_atlas.Where( r => r.Name.StartsWith( HOLD_PREFIX, StringComparison.Ordinal ) ) )
			{
				string holdName = region.Name.Substring( HOLD_PREFIX.Length );
				m_holdTypes.Add( new HoldType { Name = holdName, Sprite = region } );
			}

			ResizeTiles( width, height );
			m_width = width;
			m_height = height;

			bool shouldOffset = false;

			for( float y = HOLD_SEPARATION; y < Height; y += HOLD_SEPARATION )
			{
				float offset = shouldOffset ? HOLD_SEPARATION / 2 : 0;
				for( float x = HOLD_SEPARATION + offset; x < Width - HOLD_SEPARATION; x += HOLD_SEPARATION )
				{
					m_holdSlots.Add( new HoldSlot( new Vector2( x, y ) ) );
				} 

				shouldOffset = !shouldOffset;
			}
		}

		public HoldSlot SnapToNearestHole( Vector2 worldPos )
		{
			if( !WorldBounds.Contains( worldPos ) )
			{
				return null;
			}

			Vector2 wallPos = WorldToWall( worldPos );

			HoldSlot closest = null;
			float closestDistance = float.PositiveInfinity;

			foreach( var slot in HoldSlots )
			{
				float distance = (slot.Position - wallPos).LengthSquared();
				if( distance < closestDistance )
				{
					closest = slot;
					closestDistance = distance;
				}
			}

			return closest;
		}

		/// <summary>
		/// Draw the current wall.
		/// </summary>
		public void Draw( SpriteBatch batch, OrthographicCamera camera )
		{
			batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix() );

			bool isNight = GMTK2024Game.Instance.IsNightTime;

			for( int y = 0; y < Height; ++y )
			{
				for( int x = 0; x < Width; ++x )
				{
					Tile tile = m_tiles[ y * Width + x ];
					Vector2 position = WallToWorld( x, y + 1 );

					batch.Draw( tile.Region, position, isNight ? Color * 0.3f : Color );
				}
			}

			foreach( var slot in HoldSlots )
			{
				Vector2 position = WallToWorld( slot.Position );
				if( slot.Type != null )
				{
					Vector2 halfSize = new Vector2( slot.Type.Sprite.Width / 2, slot.Type.Sprite.Height / 2 );
					batch.Draw( slot.Type.Sprite, position - halfSize, isNight ? slot.Color * 0.5f : slot.Color );
				}
				else
				{
					batch.DrawPoint( position, new Color( 50, 50, 50 ) );
				}
			}

			float thickness = 2;
			Vector2[] outlineVerts =
				[
					new( m_x * Globals.TILE_SIZE, 0 ),
					new( m_x * Globals.TILE_SIZE, -Height * Globals.TILE_SIZE + 1 ),
					new( (m_x + Width) * Globals.TILE_SIZE, -Height * Globals.TILE_SIZE + 1 ),
					new( (m_x + Width) * Globals.TILE_SIZE, 0 )
				];

			batch.DrawLine( outlineVerts[ 0 ], outlineVerts[ 1 ], Color.Black, thickness );
			batch.DrawLine( outlineVerts[ 1 ], outlineVerts[ 2 ], Color.Black, thickness );
			batch.DrawLine( outlineVerts[ 2 ], outlineVerts[ 3 ], Color.Black, thickness );

			batch.End();
		}

		private void ResizeTiles( int newWidth, int newHeight )
		{
			if( newWidth < 0 || newHeight < 0 )
			{
				throw new ArgumentException( $"Invalid width ({newWidth}) or height ({newHeight}).");
			}

			Tile[] newTiles = new Tile[ newWidth * newHeight ];
			int minHeight = Math.Min( newHeight, Height );
			int minWidth = Math.Min( newWidth, Width );

			if( m_tiles != null )
			{
				for( int y = 0; y < minHeight; ++y )
				{
					for( int x = 0; x < minWidth; ++x )
					{
						newTiles[ y * newWidth + x ] = m_tiles[ y * Width + x ];
					}
				}
			}

			for( int y = minHeight; y < newHeight; ++y )
			{
				for( int x = minWidth; x < newWidth; ++x )
				{
					newTiles[ y * newWidth + x ] = new Tile { Region = m_defaultRegion };
				}
			}

			m_tiles = newTiles;
		}

		/// <summary>
		/// Convert wall-relative coordinates to world coordinates
		/// </summary>
		public Vector2 WallToWorld( Vector2 pos )
		{
			return WallToWorld( pos.X, pos.Y );
		}

		/// <summary>
		/// Convert wall-relative coordinates to world coordinates
		/// </summary>
		public Vector2 WallToWorld( float x, float y )
		{
			return new( (m_x + x) * Globals.TILE_SIZE, -y * Globals.TILE_SIZE );
		}

		public Vector2 WorldToWall( Vector2 pos )
		{
			return WorldToWall( pos.X, pos.Y );
		}

		public Vector2 WorldToWall( float x, float y )
		{
			return new( (x / Globals.TILE_SIZE) - m_x, -y / Globals.TILE_SIZE );
		}
	}
}
