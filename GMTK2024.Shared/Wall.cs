using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Shapes;
using System;

namespace GMTK2024
{
	public class Wall
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

		/// <summary>
		/// Separation between holds in the wall in meters.
		/// </summary>
		public float HoldSeparation
		{
			get;
			set;
		} = 0.25f;

		private readonly Texture2DAtlas m_atlas;
		private readonly Texture2DRegion m_defaultRegion;
		private readonly Texture2DRegion m_holeRegion;

		private int m_height;
		private int m_width;
		private int m_x;

		private Tile[] m_tiles;

		public Wall( Texture2DAtlas atlas, int width, int height )
		{
			m_atlas = atlas;
			m_defaultRegion = m_atlas.GetRegion( "default" );
			m_holeRegion = m_atlas.GetRegion( "hole" );

			ResizeTiles( width, height );
			m_width = width;
			m_height = height;
		}

		/// <summary>
		/// Draw the current wall.
		/// </summary>
		public void Draw( SpriteBatch batch, OrthographicCamera camera, Color tint )
		{
			batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix() );

			for( int y = 0; y < Height; ++y )
			{
				for( int x = 0; x < Width; ++x )
				{
					Tile tile = m_tiles[ y * Width + x ];
					Vector2 position = new( (m_x + x) * Globals.TILE_SIZE, -(y + 1) * Globals.TILE_SIZE );

					// some stupid 1 pixel offset bullshit, no idea
					position.X -= 0.5f * x;
					position.Y += 0.5f * y;

					batch.Draw( tile.Region, position, tint );
				}
			}

			for( float y = -1 + HoldSeparation; y < Height - 1; y += HoldSeparation )
			{
				for( float x = HoldSeparation; x < Width; x += HoldSeparation )
				{
					Vector2 position = new( (m_x + x) * Globals.TILE_SIZE, -(y + 1) * Globals.TILE_SIZE );

					batch.Draw( m_holeRegion, position, Color.White );
				}
			}

			float thickness = 2;
			var outlinePoly = new Polygon(
				[
					// feels like this should just be m_x?
					new Vector2( (m_x + Width / 2) * Globals.TILE_SIZE, 0 + thickness ),
					new Vector2( (m_x + Width / 2) * Globals.TILE_SIZE, -Height * Globals.TILE_SIZE + thickness * 2 ),
					new Vector2( (m_x + Width + Width / 2) * Globals.TILE_SIZE - 1, -Height * Globals.TILE_SIZE + thickness * 2 ),
					new Vector2( (m_x + Width + Width / 2) * Globals.TILE_SIZE - 1, 0 + thickness )
				] );


			batch.DrawPolygon( new Vector2( m_x * Globals.TILE_SIZE, 0 ), outlinePoly, Color.Black, thickness );

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
	}
}
