using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using System.Collections.Generic;

namespace GMTK2024.UI;

public class UIRoot
{
	public Texture2DAtlas Atlas { get; }

	public BitmapFont Font16 { get; }

	public BitmapFont Font32 { get; }

	public Rectangle Bounds => GraphicsDevice.Viewport.Bounds;

	public GraphicsDevice GraphicsDevice { get; }


	private Vector2 m_mousePos;

	private readonly NinePatch m_buttonBG;

	private readonly List<UIElement> m_elements = new();

	public UIRoot( GraphicsDevice graphicsDevice )
	{
		GraphicsDevice = graphicsDevice;

		Atlas = Utils.CreateAtlasFromPacked( "Content/UI.png", GraphicsDevice );
		Texture2DRegion buttonRegion = Atlas.GetRegion( "button" );
		m_buttonBG = Utils.CreateNinePatchFromRegion( buttonRegion, 16, 2, "button" );

		Font16 = BitmapFont.FromFile( GraphicsDevice, "Content/Font16.fnt" );
		Font32 = BitmapFont.FromFile( GraphicsDevice, "Content/Font32.fnt" );
	}

	public void AddRootElement( UIElement element )
	{
		m_elements.Add( element );
	}

	public bool RemoveRootElement( UIElement element )
	{
		return m_elements.Remove( element );
	}

	public Button CreateButton( UIElement content )
	{
		var button = new Button( this, m_buttonBG, content );
		return button;
	}

	public Stack CreateStack( bool withBackground )
	{
		return new Stack( this, withBackground );
	}

	public Button CreateTextButton( string text, BitmapFont font, Color color )
	{
		var textBlock = new TextBlock( this, font, text )
		{
			Color = color
		};
		return CreateButton( textBlock );
	}

	public Button CreateIconButton( Texture2DRegion region, Color color )
	{
		var icon = new Icon( this, region )
		{
			Color = color
		};
		return CreateButton( icon );
	}

	public void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
	{
		m_mousePos = mouse.Position.ToVector2();

		foreach( var element in m_elements )
		{
			if( element.IsVisible )
			{
				element.Update( keyboard, mouse, delta_t );
			}
		}
	}

	public void Draw( SpriteBatch batch, OrthographicCamera camera )
	{
		batch.Begin( samplerState: SamplerState.PointClamp );


		foreach( var element in m_elements )
		{
			if( !element.IsVisible || element.Parent != null )
			{
				continue;
			}

			element.Draw( batch );
		}

		/// DEBUG
		const bool SHOW_MOUSE_POS = true;
		if( SHOW_MOUSE_POS )
		{
			Vector2 worldPos = camera.ScreenToWorld( m_mousePos );

			string mousePosString = $"{worldPos.X:F1}, {worldPos.Y:F1}";
			SizeF mousePosSize = Font16.MeasureString( mousePosString );
			batch.DrawString( Font16, mousePosString, new Vector2( 1920 - (mousePosSize.Width + 10), 10 ), Color.Black );

			Vector2 tilePos = worldPos / Globals.TILE_SIZE;
			tilePos.Y = -tilePos.Y;
			string tilePosString = $"{tilePos.X:F2}, {tilePos.Y:F2}";
			SizeF tilePosSize = Font16.MeasureString( tilePosString );
			batch.DrawString( Font16, tilePosString, new Vector2( 1920 - (mousePosSize.Width + 10), 10 + mousePosSize.Height + 10 ), Color.Black );
		}
		/// DEBUG

		batch.End();
	}

	public int GetAbsoluteX( int x )
	{
		if( x < 0 )
		{
			return Bounds.Width + x;
		}
		return x;
	}

	public int GetAbsoluteY( int y )
	{
		if( y < 0 )
		{
			return Bounds.Height + y;
		}
		return y;
	}
}