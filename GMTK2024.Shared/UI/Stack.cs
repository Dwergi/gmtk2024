using Microsoft.Xna.Framework;
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
	private static NinePatch m_defaultBG;

	public Stack( UIRoot root, bool showBackground ) : base( root )
	{
		if( m_defaultBG == null )
		{
			m_defaultBG = Utils.CreateNinePatchFromPacked( "Content/stack.png", 16, 2, root.GraphicsDevice );
		}

		m_background = m_defaultBG;
	}

	public Stack( UIRoot root, NinePatch background ) : base( root )
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
				child.X = Padding.Width;
				child.Y = currentOffset;
				currentOffset += child.Height + ItemSpacing;
			}
			else if( GrowDirection == Direction.Up )
			{
				child.X = Padding.Width;
				child.Y = currentOffset;
				currentOffset -= child.Height + ItemSpacing;
			}
			else if( GrowDirection == Direction.Right )
			{
				child.X = currentOffset;
				child.Y = Padding.Height;
				currentOffset += child.Height + ItemSpacing;
			}
			else if( GrowDirection == Direction.Left )
			{
				child.X = currentOffset;
				child.Y = Padding.Height;
				currentOffset -= child.Height + ItemSpacing;
			}
		}

		if( GrowDirection == Direction.Down || GrowDirection == Direction.Up )
		{
			Height = currentOffset;
		}
		else if( GrowDirection == Direction.Right || GrowDirection == Direction.Left )
		{
			Width = currentOffset;
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

		foreach( var child in m_children )
		{
			if( child.IsVisible )
			{
				child.Update( keyboard, mouse, delta_t );
			}
		}
	}

	public override void Draw( SpriteBatch batch )
	{
		if( m_background != null )
		{
			Rectangle destRect = GetAbsoluteBounds();
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