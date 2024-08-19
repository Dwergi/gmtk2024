using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace GMTK2024;

interface IDrawable
{
	void Draw( SpriteBatch batch, OrthographicCamera camera );
}