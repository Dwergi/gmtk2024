using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using System;

namespace GMTK2024
{
	public enum Anchor
	{
		TopLeft,
		TopCenter,
		TopRight,
		CenterLeft,
		Center,
		CenterRight,
		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	internal class Button
	{
		private readonly NinePatch m_ninepatch;
		private readonly BitmapFont m_font;

		public int X { get; set; }

		public int Y { get; set; }

		/// <summary>
		/// Null means auto.
		/// </summary>
		public int? Width
		{
			get => m_width ?? Utils.CeilInt( m_textSize.Width + Padding.X * 2 );
			set => m_width = value;
		}
		private int? m_width;

		/// <summary>
		/// Null means auto.
		/// </summary>
		public int? Height
		{
			get => m_height ?? Utils.CeilInt( m_textSize.Height + Padding.Y * 2 );
			set => m_height = value;
		}
		private int? m_height;

		public Rectangle Bounds => new( X, Y, Width.Value, Height.Value );

		public string Text
		{
			get => m_text;
			set
			{
				m_text = value;
				m_textSize = m_font.MeasureString( Text );
			}
		}
		private string m_text;
		private SizeF m_textSize;

		public Color TextColor { get; set; } = Color.White;

		public Anchor Anchor { get; set; } = Anchor.Center;

		public Vector2 Padding { get; set; } = new( 1 );

		public Color BackgroundTint { get; set; } = new Color( 220, 220, 220 );

		public Color HoverTint { get; set; } = new Color( 230, 230, 230 );

		public Color PressedTint { get; set; } = new Color( 240, 240, 240 );

		public bool IsPressed { get; private set; }

		public bool IsHovered { get; private set; }

		public bool WasClicked { get; private set; }

		public event EventHandler OnClick;

		private bool m_wasClicked;

		public Button( NinePatch ninepatch, BitmapFont font )
		{
			m_ninepatch = ninepatch;
			m_font = font;
		}

		public void Update( MouseStateExtended mouse )
		{
			WasClicked = false;
			IsHovered = Bounds.Contains( mouse.Position );
			
			if( IsHovered && mouse.IsButtonDown( MouseButton.Left ) )
			{
				IsPressed = true;
			}
			else
			{
				IsPressed = false;
			}

			if( IsHovered && mouse.WasButtonPressed( MouseButton.Left ) )
			{
				m_wasClicked = true;
			}

			if( mouse.WasButtonReleased( MouseButton.Left ) )
			{
				if( IsHovered )
				{
					OnClick?.Invoke( this, EventArgs.Empty );
					WasClicked = true;
				}

				m_wasClicked = false;
			}
		}

		public void Draw( SpriteBatch spriteBatch )
		{
			Rectangle buttonBounds = Bounds;

			Color bgTint = IsPressed ? PressedTint :
				IsHovered ? HoverTint :
				BackgroundTint;

			spriteBatch.Draw( m_ninepatch, buttonBounds, bgTint );

			float xOffset = Anchor switch
			{
				Anchor.TopLeft or Anchor.CenterLeft or Anchor.BottomLeft => Padding.X,
				Anchor.TopCenter or Anchor.Center or Anchor.BottomCenter => (buttonBounds.Width / 2) - (m_textSize.Width / 2),
				Anchor.TopRight or Anchor.CenterRight or Anchor.BottomRight => buttonBounds.Width - (m_textSize.Width + Padding.X),
				_ => 0
			};

			float yOffset = Anchor switch
			{
				Anchor.TopLeft or Anchor.TopCenter or Anchor.TopRight => Padding.Y,
				Anchor.CenterLeft or Anchor.Center or Anchor.CenterRight => (buttonBounds.Height / 2) - (m_textSize.Height / 2),
				Anchor.BottomLeft or Anchor.BottomCenter or Anchor.BottomRight => buttonBounds.Height - (m_textSize.Height + Padding.Y),
				_ => 0
			};

			spriteBatch.DrawString( m_font, Text, new Vector2( buttonBounds.Left + xOffset, buttonBounds.Top + yOffset ), TextColor, 0, buttonBounds );
		}
	}
}
