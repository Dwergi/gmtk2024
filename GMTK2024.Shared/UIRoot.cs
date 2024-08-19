using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;

namespace GMTK2024.UI;

public class UIRoot
{
	private readonly BitmapFont m_font16;
	private readonly BitmapFont m_font32;
	private readonly NinePatch m_buttonBG;
	private readonly NinePatch m_stackBG;

	private Vector2 m_debugMousePos;

	private bool m_editModeEnabled;

	private readonly GraphicsDevice m_graphicsDevice;
	private readonly List<UIElement> m_elements = new();

	public UIRoot( GraphicsDevice graphicsDevice )
	{
		m_graphicsDevice = graphicsDevice;

		m_buttonBG = Utils.CreateNinePatchFromPacked( "Content/button.png", 16, 2, m_graphicsDevice );
		m_stackBG = Utils.CreateNinePatchFromPacked( "Content/stack.png", 16, 2, m_graphicsDevice );

		m_font16 = BitmapFont.FromFile( m_graphicsDevice, "Content/Font16.fnt" );
		m_font32 = BitmapFont.FromFile( m_graphicsDevice, "Content/Font32.fnt" );
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
		var button = new Button( m_graphicsDevice, m_buttonBG, content );
		return button;
	}

	public Button CreateTextButton( string text, BitmapFont font, Color color )
	{
		var textBlock = new TextBlock( m_graphicsDevice, font, text )
		{
			Color = color
		};
		return CreateButton( textBlock );
	}

	public Button CreateIconButton( Texture2DRegion region, Color color )
	{
		var icon = new Icon( m_graphicsDevice, region )
		{
			Color = color
		};
		return CreateButton( icon );
	}

	// TODO: Move out of here
	private Stack m_holdButtonStack;
	private Button m_editButton;

	public void Initialize()
	{
		m_editButton = CreateTextButton( "Edit", m_font32, Color.Black );
		m_editButton.X = -100;
		m_editButton.Y = -80;
		m_editButton.OnClick += OnEditClicked;
		AddRootElement( m_editButton );

		var holds = GMTK2024Game.Instance.Holds;

		int yOffset = m_editButton.Height + 20;

		m_holdButtonStack = new Stack( m_graphicsDevice, m_stackBG );
		m_holdButtonStack.Padding = new( 20, 20 );
		m_holdButtonStack.ItemSpacing = 20;
		m_holdButtonStack.X = GetAbsoluteX( m_editButton.X - m_holdButtonStack.Padding.Width - 3 ); // hack: -3
		m_holdButtonStack.Y = GetAbsoluteY( m_editButton.Y - m_editButton.Height - 20 );
		m_holdButtonStack.GrowDirection = Stack.Direction.Up;
		m_holdButtonStack.IsVisible = false;

		foreach( var hold in holds )
		{
			string holdName = hold.Key;
			holdName = char.ToUpper( holdName[ 0 ] ) + holdName[ 1.. ];
			var holdButton = CreateTextButton( holdName, m_font32, Color.Black );
			holdButton.OnClick += ( s, e ) => OnAddHold( hold.Value );

			m_holdButtonStack.AddChild( holdButton );

			yOffset += holdButton.Height + 20;
		}

		AddRootElement( m_holdButtonStack );
	}

	public void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
	{
		m_debugMousePos = mouse.Position.ToVector2();

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

		Vector2 screenPos = camera.ScreenToWorld( m_debugMousePos );
		string mousePosString = $"{screenPos.X:F1}, {screenPos.Y:F1}";
		SizeF mousePosSize = m_font16.MeasureString( mousePosString );

		batch.DrawString( m_font16, mousePosString, new Vector2( 1920 - (mousePosSize.Width + 10), 10 ), Color.White );

		foreach( var element in m_elements )
		{
			if( !element.IsVisible || element.Parent != null )
			{
				continue;
			}

			element.Draw( batch );
		}

		batch.End();
	}

	public int GetAbsoluteX( int x )
	{
		if( x < 0 )
		{
			return m_graphicsDevice.Viewport.Width + x;
		}
		return x;
	}

	public int GetAbsoluteY( int y )
	{
		if( y < 0 )
		{
			return m_graphicsDevice.Viewport.Height + y;
		}
		return y;
	}

	private void OnEditClicked( object sender, EventArgs e )
	{
		if( !m_editModeEnabled )
		{
			m_editModeEnabled = true;
			m_editButton.SetText( "Hide" );

			m_holdButtonStack.IsVisible = true;
		}
		else
		{
			m_editModeEnabled = false;
			m_editButton.SetText( "Edit" );

			m_holdButtonStack.IsVisible = false;
		}
	}

	private Texture2DRegion m_draggingHold;

	private void OnAddHold( Texture2DRegion region )
	{
		m_draggingHold = region;
	}
}