using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace GMTK2024.UI;

public class Icon : UIElement
{
	public Texture2DRegion Sprite
	{
		get;
		set;
	}

	public Icon( GraphicsDevice device, Texture2D texture ) : this( device, new Texture2DRegion( texture ) )
	{
	}

	public Icon( GraphicsDevice device, Texture2DRegion region ) : base( device )
	{
		Sprite = region;
		Width = Sprite.Width;
		Height = Sprite.Height;
	}

	public override void Draw( SpriteBatch batch )
	{
		var destRect = GetAbsoluteBounds();
		batch.Draw( Sprite, destRect.Location.ToVector2(), Color, clippingRectangle: GetClippingRect() );
	}
}