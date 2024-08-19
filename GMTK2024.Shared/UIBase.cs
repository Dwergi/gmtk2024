using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using System;
using System.Diagnostics;

namespace GMTK2024.UI;

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

public abstract class UIElement
{
	public UIElement Parent { get; set; }

	public Anchor Anchor { get; set; }

	/// <summary>
	/// Pixels from left.
	/// Negative values anchor to right instead.
	/// </summary>
	public int X 
	{
		get;
		set;
	}

	/// <summary>
	/// Pixels from top.
	/// Negative values anchor to bottom instead.
	/// </summary>
	public int Y
	{
		get;
		set;
	}

	public int Width { get; set; }

	public int Height { get; set; }

	public Rectangle Bounds => new( X, Y, Width, Height );

	public Color Color { get; set; } = Color.White;

	public bool IsVisible { get; set; } = true;

	private readonly GraphicsDevice m_graphicsDevice;

	protected UIElement( GraphicsDevice device )
	{
		m_graphicsDevice = device;
	}

	public abstract void Draw( SpriteBatch batch );

	public virtual void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
	{
	}

	public Rectangle GetClippingRect()
	{
		Rectangle parentBounds = Parent?.GetAbsoluteBounds() ?? m_graphicsDevice.Viewport.Bounds;
		return parentBounds;
	}

	public Rectangle GetAbsoluteBounds()
	{
		Rectangle bounds = Bounds;
		Rectangle parentRect = Parent?.GetAbsoluteBounds() ?? m_graphicsDevice.Viewport.Bounds;

		if( bounds.X < 0 && bounds.Width < 0 ||
			bounds.Y < 0 && bounds.Height < 0 )
		{
			// unresolvable
			Debugger.Break();
		}

		// TODO: Fix this anchor stuff, total mess

		// flip negative bounds, ie. growing left/up
		if( bounds.Width < 0 )
		{
			bounds.X += bounds.Width;
			bounds.Width = -bounds.Width;
		}

		if( bounds.Height < 0 )
		{
			bounds.Y += bounds.Height;
			bounds.Height = -bounds.Height;
		}

		// flip relative coordinates
		if( bounds.X < 0 )
		{
			bounds.X = parentRect.Right + bounds.X;
		}
		else
		{
			bounds.X += parentRect.X;
		}

		if( bounds.Y < 0 )
		{
			bounds.Y = parentRect.Bottom + bounds.Y;
		}
		else
		{
			bounds.Y += parentRect.Y;
		}

		if( bounds.X < 0 || bounds.X >= m_graphicsDevice.Viewport.Bounds.Right ||
			bounds.Y < 0 || bounds.Y >= m_graphicsDevice.Viewport.Bounds.Bottom )
		{
			// entirely out of bounds
			Debugger.Break();
		}

		return bounds;
	}
}