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

			bool isNight = GMTK2024Game.Instance.IsNightTime;
			Color tint = isNight ? new Color( 50, 50, 50 ) : new Color( 150, 150, 150 );

			for( int x = Globals.TILE_LEFT + 1; x < Globals.TILE_RIGHT - 1; ++x )
			{
				batch.Draw( m_normalSurface, Globals.TileToWorld( x, 0 ), tint );
			}

			for( int y = -1; y > Globals.TILE_BOTTOM; --y )
			{
				for( int x = Globals.TILE_LEFT + 1; x < Globals.TILE_RIGHT - 1; ++x )
				{
					batch.Draw( m_normalUnder, Globals.TileToWorld( x, y ), tint );
				}
			}

			Wall wall = GMTK2024Game.Instance.Wall;
			const int EXTRA_PADDING = Globals.TILE_SIZE / 4;
			Rectangle padRectangle = new( wall.X * Globals.TILE_SIZE - EXTRA_PADDING, 0, wall.Width * Globals.TILE_SIZE + EXTRA_PADDING * 2, PAD_HEIGHT * Globals.TILE_SIZE );
			batch.Draw( m_padPatch, padRectangle, isNight ? tint : Color.White );		

			batch.End();
		}
	}
}
