using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GMTK2024
{
	public enum MouseButton
	{
		Left,
		Right,
		Middle,
		Button4,
		Button5
	}

	internal class Input
	{
		public KeyboardState PreviousKeyboard { get; private set; }
		public KeyboardState CurrentKeyboard { get; private set; }

		public MouseState PreviousMouse { get; private set; }
		public MouseState CurrentMouse { get; private set; }

		public GamePadState[] PreviousGamePads { get; private set; }
		public GamePadState[] CurrentGamePads { get; private set; }

		public Input()
		{
			PreviousKeyboard = new();
			PreviousMouse = new();

			CurrentKeyboard = new();
			CurrentMouse = new();

			PreviousGamePads = new GamePadState[4];
			CurrentGamePads = new GamePadState[4];
		}

		public void Update()
		{
			PreviousKeyboard = CurrentKeyboard;
			PreviousMouse = CurrentMouse;
			
			CurrentKeyboard = Keyboard.GetState();
			CurrentMouse = Mouse.GetState();

#if !BLAZORGL
			for (int i = 0; i < 4; i++)
			{
				PreviousGamePads[i] = CurrentGamePads[i];
			}

			for (int i = 0; i < 4; i++)
			{
				CurrentGamePads[i] = GamePad.GetState((PlayerIndex) i);
			}
#endif
		}

		public bool IsKeyDown(Keys key)
		{
			return CurrentKeyboard.IsKeyDown(key);
		}

		public bool WasKeyPressed(Keys key)
		{
			return PreviousKeyboard.IsKeyUp(key) && CurrentKeyboard.IsKeyDown(key);
		}

		public Vector2 MouseDelta()
		{
			return new(CurrentMouse.X - PreviousMouse.X, CurrentMouse.Y - PreviousMouse.Y);
		}

		public bool IsMouseButtonDown(MouseButton button)
		{
			return button switch
			{
				MouseButton.Left => CurrentMouse.LeftButton == ButtonState.Pressed,
				MouseButton.Right => CurrentMouse.RightButton == ButtonState.Pressed,
				MouseButton.Middle => CurrentMouse.MiddleButton == ButtonState.Pressed,
				MouseButton.Button4 => CurrentMouse.XButton1 == ButtonState.Pressed,
				MouseButton.Button5 => CurrentMouse.XButton2 == ButtonState.Pressed,
				_ => false
			};
		}

		public bool WasMouseButtonPressed(MouseButton button)
		{
			return button switch
			{
				MouseButton.Left => PreviousMouse.LeftButton == ButtonState.Released && CurrentMouse.LeftButton == ButtonState.Pressed,
				MouseButton.Right => PreviousMouse.RightButton == ButtonState.Released && CurrentMouse.RightButton == ButtonState.Pressed,
				MouseButton.Middle => PreviousMouse.MiddleButton == ButtonState.Released && CurrentMouse.MiddleButton == ButtonState.Pressed,
				MouseButton.Button4 => PreviousMouse.XButton1 == ButtonState.Released && CurrentMouse.XButton1 == ButtonState.Pressed,
				MouseButton.Button5 => PreviousMouse.XButton2 == ButtonState.Released && CurrentMouse.XButton2 == ButtonState.Pressed,
				_ => false
			};
		}
	}
}
