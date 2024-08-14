using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GMTK2024
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class GMTK2024Game : Game
	{
		GraphicsDeviceManager m_graphics;
		SpriteBatch m_spriteBatch;
		Input m_input;

		public GMTK2024Game()
		{
			m_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

#if (ANDROID || iOS)
			m_graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
			m_graphics.IsFullScreen = true;
#endif

			m_input = new();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();

		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: Use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			m_input.Update();

			if (m_input.IsKeyDown(Keys.Escape) || m_input.IsKeyDown(Keys.Back))
			{
				try { Exit(); }
				catch (PlatformNotSupportedException) { /* ignore */ }
			}

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here

			m_spriteBatch.Begin();

			m_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
