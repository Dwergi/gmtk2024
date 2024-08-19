using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace GMTK2024.UI;

public class TextBlock : UIElement
{
	public string Text
	{
		get => m_text;
		set
		{
			m_text = value;
			if( m_text == null )
			{
				Width = 0;
				Height = 0;
			}
			else
			{
				var size = m_font.MeasureString( Text );
				Width = Utils.CeilInt( size.Width );
				Height = Utils.CeilInt( size.Height );
			}
		}
	}
	private string m_text;

	private readonly BitmapFont m_font;

	public TextBlock( GraphicsDevice device, BitmapFont font, string text = null ) : base( device )
	{
		m_font = font;
		Text = text;
	}

	public override void Draw( SpriteBatch batch )
	{
		Rectangle dest = GetAbsoluteBounds();
		Rectangle clip = GetClippingRect();

		batch.DrawString( m_font, Text, dest.Location.ToVector2(), Color, 0, clip );
	}
}