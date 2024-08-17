using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using System;

namespace GMTK2024
{
	internal class Wall
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
		} = 0.3f;

		private readonly Texture2DAtlas m_atlas;
		private readonly Texture2DRegion m_defaultRegion;
		private readonly Texture2DRegion m_holeRegion;

		private int m_height = 10;
		private int m_width = 3;
		private int m_x;

		private Tile[] m_tiles;

		public Wall( Texture2DAtlas atlas )
		{
			m_atlas = atlas;
			m_defaultRegion = m_atlas.GetRegion( "default" );
			m_holeRegion = m_atlas.GetRegion( "hole" );

			m_tiles = new Tile[ m_height * m_width ];
			for( int y = 0; y < Height; ++y )
			{
				for( int x = 0; x < Width; ++x )
				{
					m_tiles[ y * Width + x ] = new Tile { Region = m_defaultRegion };
				}
			}
		}

		/// <summary>
		/// Draw the current wall.
		/// </summary>
		public void Draw( SpriteBatch batch, Color tint )
		{
			for( int y = 0; y < Height; ++y )
			{
				for( int x = 0; x < Width; ++x )
				{
					Tile tile = m_tiles[ y * Width + x ];
					Vector2 position = new( (m_x + x) * Globals.TILE_SIZE, -y * Globals.TILE_SIZE );

					// some stupid 1 pixel offset bullshit, no idea
					position.X -= 0.5f * x;
					position.Y += 0.5f * y;

					batch.Draw( tile.Region, position, tint );
				}
			}

			for( float y = -1 + HoldSeparation; y < Height - (1 + HoldSeparation); y += HoldSeparation )
			{
				for( float x = HoldSeparation; x < (Width - HoldSeparation); x += HoldSeparation )
				{
					Vector2 position = new( (m_x + x) * Globals.TILE_SIZE, -y * Globals.TILE_SIZE );

					batch.Draw( m_holeRegion, position, Color.White );
				}
			}
		}

		private void ResizeTiles( int newWidth, int newHeight )
		{
			if( newWidth < 0 || newHeight < 0 )
			{
				throw new ArgumentException( $"Invalid width ({newWidth}) or height ({newHeight}).");
			}

			var newTiles = new Tile[ newWidth * newHeight ];

			int minHeight = Math.Min( newHeight, Height );
			int minWidth = Math.Min( newWidth, Width );
			for( int y = 0; y < minHeight; ++y )
			{
				for( int x = 0; x < minWidth; ++x )
				{
					newTiles[ y * newWidth + x ] = m_tiles[ y * Width + x ];
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
