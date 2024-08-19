using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using System;
using System.IO;
using System.Xml.Linq;

namespace GMTK2024
{
	internal static class Utils
	{
		public static float Eerp( float min, float max, float t )
		{
			return Math.Clamp( min * MathF.Exp( t * MathF.Log( max / min ) ), min, max );
		}

		public static int CeilInt( float x )
		{
			return (int) MathF.Ceiling( x );
		}

		public static Texture2DAtlas CreateAtlasFromPacked( string filename, GraphicsDevice graphicsDevice )
		{
			using var pngStream = File.OpenRead( filename );
			var texture = Texture2D.FromStream( graphicsDevice, pngStream );
			Texture2DAtlas atlas = new( texture );

			using( var xmlStream = File.OpenRead( Path.ChangeExtension( filename, ".xml" ) ) )
			{
				var doc = XDocument.Load( xmlStream );

				var root = doc.Element( "TextureAtlas" );
				var subTextures = root.Elements( "SubTexture" );
				foreach( var subTexture in subTextures )
				{
					var x = subTexture.Attribute( "x" );
					var y = subTexture.Attribute( "y" );
					var width = subTexture.Attribute( "width" );
					var height = subTexture.Attribute( "height" );
					Rectangle rect = new( int.Parse(x.Value), int.Parse(y.Value), int.Parse(width.Value), int.Parse(height.Value) );
					var name = subTexture.Attribute( "name" );
					atlas.CreateRegion( rect, name.Value );
				}
			}

			return atlas;
		}

		public static NinePatch CreateNinePatchFromPacked( string filename, int size, int padding, GraphicsDevice graphicsDevice, string name = null )
		{
			using var pngStream = File.OpenRead( filename );
			var texture = Texture2D.FromStream( graphicsDevice, pngStream );

			if( name == null )
			{
				name = Path.GetFileNameWithoutExtension( pngStream.Name );
			}

			return CreateNinePatchFromRegion( new Texture2DRegion( texture ), size, padding, name );
		}

		public static NinePatch CreateNinePatchFromRegion( Texture2DRegion region, int size, int padding, string name = null )
		{
			Texture2DRegion[] patches =
			[
				region.GetSubregion( new Rectangle( 0,  0,  size, size ) ),
				region.GetSubregion( new Rectangle( size + padding, 0,  size, size ) ),
				region.GetSubregion( new Rectangle( size * 2 + padding * 2, 0,  size, size ) ),
				region.GetSubregion( new Rectangle( 0,  size + padding, size, size ) ),
				region.GetSubregion( new Rectangle( size + padding, size + padding, size, size ) ),
				region.GetSubregion( new Rectangle( size * 2 + padding * 2, size + padding, size, size ) ),
				region.GetSubregion( new Rectangle( 0,  size * 2 + padding * 2, size, size ) ),
				region.GetSubregion( new Rectangle( size + padding, size * 2 + padding * 2, size, size ) ),
				region.GetSubregion( new Rectangle( size * 2 + padding * 2, size * 2 + padding * 2, size, size ) ),
			];

			return new NinePatch( patches, name );
		}
	}
}
