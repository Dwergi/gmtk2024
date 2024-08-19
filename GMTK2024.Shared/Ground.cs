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
		private readonly NinePatch m_padPatch;

		private const int PAD_HEIGHT = 1;

		public Ground()
		{
			m_atlas = Utils.CreateAtlasFromPacked( "Content/GroundTiles.png", GMTK2024Game.Instance.GraphicsDevice );

			m_normalSurface = m_atlas.GetRegion( "ground_surface" );
			m_normalUnder = m_atlas.GetRegion( "ground_under" );

			m_padPatch = Utils.CreateNinePatchFromRegion( m_atlas.GetRegion( "pad" ), 16, 2 );
		}

		public void Draw( SpriteBatch batch, OrthographicCamera camera )
		{
			batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix() );

			for( int x = Globals.TILE_BOUNDS.Left; x <= Globals.TILE_BOUNDS.Right; ++x )
			{
				Vector2 position = new( x * Globals.TILE_SIZE, 0 );
				batch.Draw( m_normalSurface, position, new Color( 150, 150, 150 ) );
			}

			for( int y = -1; y > Globals.TILE_BOUNDS.Top; --y )
			{
				for( int x = Globals.TILE_BOUNDS.Left; x <= Globals.TILE_BOUNDS.Right; ++x )
				{
					Vector2 position = new( x * Globals.TILE_SIZE, -y * Globals.TILE_SIZE );
					batch.Draw( m_normalUnder, position, new Color( 150, 150, 150 ) );
				}
			}

			Wall wall = GMTK2024Game.Instance.Wall;
			const int EXTRA_PADDING = Globals.TILE_SIZE / 4;
			Rectangle padRectangle = new( wall.X * Globals.TILE_SIZE - EXTRA_PADDING, 0, wall.Width * Globals.TILE_SIZE + EXTRA_PADDING * 2, PAD_HEIGHT * Globals.TILE_SIZE );
			batch.Draw( m_padPatch, padRectangle, Color.White );		

			batch.End();
		}
	}
}
