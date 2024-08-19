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

	public Icon( UIRoot root, Texture2D texture ) : this( root, new Texture2DRegion( texture ) )
	{
	}

	public Icon( UIRoot root, Texture2DRegion region ) : base( root )
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