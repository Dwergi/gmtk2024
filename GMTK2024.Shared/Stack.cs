using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;

namespace GMTK2024.UI;

public class Stack : UIElement
{
	public enum Direction
	{
		Down,
		Up, 
		Right,
		Left
	}

	public Direction GrowDirection
	{
		get;
		set;
	}

	public bool AutoSize
	{
		get;
		set;
	} = true;

	public Size Padding
	{
		get;
		set;
	} = new( 10, 10 );

	public int ItemSpacing
	{
		get;
		set;
	} = 10;

	private readonly List<UIElement> m_children = new();
	private readonly NinePatch m_background;

	public Stack( GraphicsDevice device, NinePatch background = null ) : base( device )
	{
		m_background = background;
	}

	public void AddChild( UIElement child )
	{
		m_children.Add( child );
		child.Parent = this;
	}

	public void RemoveChild( UIElement child )
	{
		m_children.Remove( child );
		child.Parent = null;
	}

	public override void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
	{
		// top/left padding
		int maxChildWidth = 0;
		int maxChildHeight = 0;

		int currentOffset = GrowDirection switch
		{
			Direction.Down => Padding.Height,
			Direction.Up => -Padding.Height,
			Direction.Right => Padding.Width,
			Direction.Left => -Padding.Width,
			_ => throw new ArgumentException( "Invalid Direction!" )
		};

		foreach( UIElement child in m_children )
		{
			maxChildWidth = Math.Max( maxChildWidth, child.Width );
			maxChildHeight = Math.Max( maxChildHeight, child.Height );

			if( GrowDirection == Direction.Down )
			{
				child.Y = currentOffset;
				currentOffset += child.Height + ItemSpacing;
			}
			else if( GrowDirection == Direction.Up )
			{
				child.Y = currentOffset;
				currentOffset -= child.Height + ItemSpacing;
			}
			else if( GrowDirection == Direction.Right )
			{
				child.X = currentOffset;
				currentOffset += child.Height + ItemSpacing;
			}
			else if( GrowDirection == Direction.Left )
			{
				child.X = currentOffset;
				currentOffset -= child.Height + ItemSpacing;
			}
		}

		if( GrowDirection == Direction.Down )
		{
			Height = currentOffset + Padding.Height;
		}
		else if( GrowDirection == Direction.Up )
		{
			Height = currentOffset - Padding.Height;
		}
		else if( GrowDirection == Direction.Right )
		{
			Width = currentOffset + Padding.Width;
		}
		else if( GrowDirection == Direction.Left )
		{
			Width = currentOffset - Padding.Width;
		}

		if( AutoSize )
		{
			if( GrowDirection == Direction.Down || GrowDirection == Direction.Up )
			{
				Width = maxChildWidth + Padding.Width * 2;
			}
			else
			{
				Height = maxChildHeight + Padding.Height * 2;
			}
		}
	}

	public override void Draw( SpriteBatch batch )
	{
		if( m_background != null )
		{
			var destRect = Bounds;

			if( Width < 0 )
			{
				destRect.X = X + Width;
				destRect.Width = -Height;

			}
			if( Height < 0 )
			{
				destRect.Y = Y + Height;
				destRect.Height = -Height;
			}

			batch.Draw( m_background, destRect, Color, GetClippingRect() );
		}

		foreach( var child in m_children )
		{
			if( child.IsVisible )
			{
				child.Draw( batch );
			}
		}
	}
}