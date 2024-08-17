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

		public static NinePatch CreateNinePatchFromPacked( string filename, GraphicsDevice graphicsDevice )
		{
			using var pngStream = File.OpenRead( filename );
			var texture = Texture2D.FromStream( graphicsDevice, pngStream );
			Texture2DRegion[] patches =
			[
				new( texture, new Rectangle( 0,	 0,  16, 16 ) ),
				new( texture, new Rectangle( 18, 0,  16, 16 ) ),
				new( texture, new Rectangle( 36, 0,  16, 16 ) ),
				new( texture, new Rectangle( 0,	 18, 16, 16 ) ),
				new( texture, new Rectangle( 18, 18, 16, 16 ) ),
				new( texture, new Rectangle( 36, 18, 16, 16 ) ),
				new( texture, new Rectangle( 0,  36, 16, 16 ) ),
				new( texture, new Rectangle( 18, 36, 16, 16 ) ),
				new( texture, new Rectangle( 36, 36, 16, 16 ) ),
			];

			return new NinePatch( patches );
		}
	}
}
