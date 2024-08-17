using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace GMTK2024
{
	internal class Ground
	{
		private readonly Texture2DAtlas m_atlas;
		private readonly Texture2DRegion m_normalSurface;
		private readonly Texture2DRegion m_normalUnder;
		private readonly Texture2DRegion m_padSurface;
		private readonly Texture2DRegion m_padUnder;

		public Ground( Texture2DAtlas atlas )
		{
			m_atlas = atlas;

			m_normalSurface = m_atlas.GetRegion( "ground_surface" );
			m_normalUnder = m_atlas.GetRegion( "ground_lower" );
		}

		public void Draw( SpriteBatch batch, OrthographicCamera camera )
		{
			for( int x = Globals.TILE_BOUNDS.Left; x <= Globals.TILE_BOUNDS.Right; ++x )
			{
				Vector2 position = new( x * Globals.TILE_SIZE, Globals.TILE_SIZE );

				// some stupid 1 pixel offset bullshit, no idea
				position.X -= 0.5f * x;
				position.Y += 0.5f;

				batch.Draw( m_normalSurface, position, Color.White );
			}

			for( int y = -2; y > Globals.TILE_BOUNDS.Top; --y )
			{
				for( int x = Globals.TILE_BOUNDS.Left; x <= Globals.TILE_BOUNDS.Right; ++x )
				{
					Vector2 position = new( x * Globals.TILE_SIZE, -y * Globals.TILE_SIZE );

					// some stupid 1 pixel offset bullshit, no idea
					position.X -= 0.5f * x;
					position.Y += 0.5f * y;

					batch.Draw( m_normalUnder, position, Color.White );
				}
			}
		}
	}
}
