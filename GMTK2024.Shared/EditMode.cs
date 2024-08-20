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
	private HoldSlot m_hoveredSlot;

	// TODO: Should be a state machine
	private HoldType m_draggedHold;
	private float? m_currentRotation;

	public bool m_ignoreRelease;

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
		if( m_draggedHold != null && !m_ignoreRelease )
		{
			m_mousePos = mouse.Position.ToVector2();
			Vector2 mouseWorldPos = GMTK2024Game.Instance.Camera.ScreenToWorld( m_mousePos );

			m_hoveredSlot = GMTK2024Game.Instance.Wall.SnapToNearestHole( mouseWorldPos );

			if( mouse.WasButtonReleased( MouseButton.Left ) )
			{
				if( m_hoveredSlot != null )
				{
					m_hoveredSlot.Type = m_draggedHold;
					m_hoveredSlot.Color = Color.Yellow;

					StopDrag();
				}
			}
			else if( mouse.WasButtonReleased( MouseButton.Right ) )
			{
				StopDrag();
			}
		}

		m_ignoreRelease = false;
	}

	private float GetRotation( Vector2 mouseWorldPos, HoldSlot slot )
	{
		Vector2 slotWorldPos = GMTK2024Game.Instance.Wall.WallToWorld( m_hoveredSlot.Position );
		return MathHelper.WrapAngle( (float) (Math.Atan2( mouseWorldPos.Y - slotWorldPos.Y, mouseWorldPos.X - slotWorldPos.X ) + Math.PI / 2) );
	}

	public void Draw( SpriteBatch batch, OrthographicCamera camera )
	{
		var ui = GMTK2024Game.Instance.UI;

		// draw dragging hold
		if( m_draggedHold != null )
		{
			if( m_hoveredSlot != null )
			{
				batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix() );

				Vector2 halfSize = new Vector2( m_draggedHold.Sprite.Width / 2, m_draggedHold.Sprite.Height / 2 );
				Vector2 holdWorldPos = GMTK2024Game.Instance.Wall.WallToWorld( m_hoveredSlot.Position );
				holdWorldPos -= halfSize;
				batch.Draw( m_draggedHold.Sprite, holdWorldPos, Color.Yellow );//, m_currentRotation ?? 0.0f, halfSize, Vector2.One, SpriteEffects.None, 0 );

				if( m_currentRotation != null )
				{
					Vector2 mouseWorldPos = camera.ScreenToWorld( m_mousePos );
					batch.DrawLine( holdWorldPos, mouseWorldPos, Color.Black, thickness: 1 );
				}

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
			const bool SHOW_ROTATION = false;
			if( SHOW_ROTATION )
			{
				batch.Begin( samplerState: SamplerState.PointClamp );

				Color color = Color.Red;
				string rotationString = $"None";

				if( m_currentRotation != null )
				{
					rotationString = $"{MathHelper.ToDegrees( m_currentRotation.Value )}";
					color = Color.Black;
				}

				SizeF snappedSize = ui.Font16.MeasureString( rotationString );
				batch.DrawString( ui.Font16, rotationString, new Vector2( 1920 - (snappedSize.Width + 10), 1080 - (snappedSize.Height + 10) ), color );

				batch.End();
			}

			const bool SHOW_SNAPPED_POS = false;
			if( SHOW_SNAPPED_POS )
			{
				batch.Begin( samplerState: SamplerState.PointClamp );

				Color color = Color.Red;
				string snappedString = $"Outside";

				if( m_hoveredSlot != null )
				{
					snappedString = $"{m_hoveredSlot.Position.X:F2}, {m_hoveredSlot.Position.Y:F2}";
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
			GMTK2024Game.Instance.IsPaused = true;

			m_editButton.SetText( "Hide" );

			m_holdButtonStack.IsVisible = true;
		}
		else
		{
			m_editModeEnabled = false;
			GMTK2024Game.Instance.IsPaused = false;

			m_editButton.SetText( "Edit" );

			m_holdButtonStack.IsVisible = false;
		}
	}

	private void StopDrag()
	{
		m_draggedHold = null;
		m_hoveredSlot = null;
		m_currentRotation = null;

		GMTK2024Game.Instance.IsMouseVisible = true;
	}

	private void OnAddHold( HoldType hold )
	{
		m_draggedHold = hold;
		m_ignoreRelease = true;

		//GMTK2024Game.Instance.IsMouseVisible = false;
	}
}