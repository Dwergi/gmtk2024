using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using System;

namespace GMTK2024.UI;

public class Button : UIElement
{
	private readonly NinePatch m_ninepatch;

	/// <summary>
	/// What to show in the button.
	/// </summary>
	public UIElement Content
	{
		get;
		set;
	}

	public bool AutoSize
	{
		get;
		set;
	} = true;

	public Point Padding { get; set; } = new( 10, 5 );

	public Color BackgroundTint { get; set; } = new Color( 220, 220, 220 );

	public Color HoverTint { get; set; } = new Color( 230, 230, 230 );

	public Color PressedTint { get; set; } = new Color( 240, 240, 240 );

	public bool IsPressed { get; private set; }

	public bool IsHovered { get; private set; }

	public bool WasClicked { get; private set; }

	public event EventHandler OnClick;

	private bool m_gotMouseDown;

	public Button( GraphicsDevice device, NinePatch ninepatch, UIElement content ) : base( device )
	{
		m_ninepatch = ninepatch;
		Content = content;
		Content.Parent = this;

		UpdateBounds();
	}

	public void SetText( string text )
	{
		TextBlock textBlock = (TextBlock) Content;
		textBlock.Text = text;
	}

	public void SetIcon( Texture2DRegion region )
	{
		Icon icon = (Icon) Content;
		icon.Sprite = region;
	}

	public override void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
	{
		Content.Update( keyboard, mouse, delta_t );

		UpdateBounds();

		Rectangle absBounds = GetAbsoluteBounds();
		IsHovered = absBounds.Contains( mouse.Position );

		IsPressed = IsHovered && mouse.IsButtonDown( MouseButton.Left ) && m_gotMouseDown;

		WasClicked = false;

		if( IsHovered && mouse.WasButtonPressed( MouseButton.Left ) )
		{
			m_gotMouseDown = true;
		}

		if( mouse.WasButtonReleased( MouseButton.Left ) )
		{
			if( IsHovered && m_gotMouseDown )
			{
				OnClick?.Invoke( this, EventArgs.Empty );
				WasClicked = true;
			}

			m_gotMouseDown = false;
		}
	}

	public override void Draw( SpriteBatch batch )
	{
		Color bgTint = IsPressed ? PressedTint :
			IsHovered ? HoverTint :
			BackgroundTint;

		Rectangle destRect = GetAbsoluteBounds();
		Rectangle clipRect = GetClippingRect();
		batch.Draw( m_ninepatch, destRect, bgTint, clipRect );

		Content.Draw( batch );
	}

	private void UpdateBounds()
	{
		if( AutoSize )
		{
			Width = Utils.CeilInt( Content.Width + Padding.X * 2 );
			Height = Utils.CeilInt( Content.Height + Padding.Y * 2 );
		}

		int xOffset = Anchor switch
		{
			Anchor.TopLeft or Anchor.CenterLeft or Anchor.BottomLeft => Padding.X,
			Anchor.TopCenter or Anchor.Center or Anchor.BottomCenter => (Bounds.Width / 2) - (Content.Width / 2),
			Anchor.TopRight or Anchor.CenterRight or Anchor.BottomRight => Bounds.Width - (Content.Width + Padding.X),
			_ => 0
		};

		int yOffset = Anchor switch
		{
			Anchor.TopLeft or Anchor.TopCenter or Anchor.TopRight => Padding.Y,
			Anchor.CenterLeft or Anchor.Center or Anchor.CenterRight => (Bounds.Height / 2) - (Content.Height / 2) - 1,
			Anchor.BottomLeft or Anchor.BottomCenter or Anchor.BottomRight => Bounds.Height - (Content.Height + Padding.Y) - 2,
			_ => 0
		};

		Content.X = xOffset;
		Content.Y = yOffset;
	}
}
