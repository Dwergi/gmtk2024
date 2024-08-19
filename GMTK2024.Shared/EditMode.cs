using GMTK2024.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using System.Linq;
using System;

namespace GMTK2024;

public class EditMode : IDrawable, IReceivesInput
{
	private readonly Button m_editButton;
	private readonly Stack m_holdButtonStack;
	private readonly Texture2DRegion m_errorSprite;

	private bool m_editModeEnabled;
	private Vector2 m_mousePos;
	private HoldType m_draggedHold;

	public EditMode()
	{
		var game = GMTK2024Game.Instance;
		var ui = game.UI;

		m_errorSprite = ui.Atlas.GetRegion( "error" );

		m_editButton = ui.CreateTextButton( "Edit", ui.Font32, Color.Black );
		m_editButton.X = -70;
		m_editButton.Y = -70;
		m_editButton.Width = 80;
		m_editButton.Height = 40;
		m_editButton.AutoSize = false;
		m_editButton.OnClick += OnEditClicked;
		ui.AddRootElement( m_editButton );

		m_holdButtonStack = new Stack( ui, true )
		{
			Padding = new( 20, 20 ),
			ItemSpacing = 20,
			X = m_editButton.X,
			Y = ui.GetAbsoluteY( m_editButton.Y ) - 20 - m_editButton.Height,
			Width = 150,
			AutoSize = false,
			GrowDirection = Stack.Direction.Up,
			IsVisible = false,
			Color = new Color( 220, 220, 220 )
		};

		foreach( var hold in game.Wall.HoldTypes.OrderByDescending( kvp => kvp.Name, StringComparer.Ordinal ) )
		{
			string holdName = char.ToUpper( hold.Name[ 0 ] ) + hold.Name[ 1.. ];
			var holdButton = ui.CreateTextButton( holdName, ui.Font32, Color.Black );
			holdButton.OnClick += ( s, e ) => OnAddHold( hold );

			m_holdButtonStack.AddChild( holdButton );
		}

		ui.AddRootElement( m_holdButtonStack );
	}

	public void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t )
	{
		m_mousePos = mouse.Position.ToVector2();
	}

	public void Draw( SpriteBatch batch, OrthographicCamera camera )
	{
		var ui = GMTK2024Game.Instance.UI;

		Vector2 mouseWorldPos = camera.ScreenToWorld( m_mousePos );

		// draw dragging hold
		if( m_draggedHold != null )
		{
			var wall = GMTK2024Game.Instance.Wall;
			HoldSlot nearestSlot = wall.SnapToNearestHole( mouseWorldPos );

			if( nearestSlot != null )
			{
				batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix() );
				
				Vector2 worldPos = wall.WallToWorld( nearestSlot.Position );
				worldPos -= new Vector2( m_draggedHold.Sprite.Width / 2, m_draggedHold.Sprite.Height / 2 );
				batch.Draw( m_draggedHold.Sprite, worldPos, Color.OrangeRed );

				batch.End();
			}
			else
			{
				batch.Begin( samplerState: SamplerState.PointClamp );

				Vector2 errorPos = m_mousePos - new Vector2( m_errorSprite.Width / 2, m_errorSprite.Height / 2 );
				batch.Draw( m_errorSprite, errorPos, Color.Red );

				batch.End();
			}

			/// DEBUG
			const bool SHOW_SNAPPED_POS = true;
			if( SHOW_SNAPPED_POS )
			{
				batch.Begin( samplerState: SamplerState.PointClamp );

				Color color = Color.Red;
				string snappedString = $"Outside";

				if( nearestSlot != null )
				{
					snappedString = $"{nearestSlot.Position.X:F2}, {nearestSlot.Position.Y:F2}";
					color = Color.Black;
				}

				SizeF snappedSize = ui.Font16.MeasureString( snappedString );
				batch.DrawString( ui.Font16, snappedString, new Vector2( 1920 - (snappedSize.Width + 10), 1080 - (snappedSize.Height + 10) ), color );

				batch.End();
			}
			/// DEBUG
		}
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

	private void OnAddHold( HoldType hold )
	{
		m_draggedHold = hold;

		//GMTK2024Game.Instance.IsMouseVisible = false;
	}
}