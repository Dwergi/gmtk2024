using MonoGame.Extended.Input;

namespace GMTK2024;

public interface IReceivesInput
{
	void Update( KeyboardStateExtended keyboard, MouseStateExtended mouse, float delta_t );
}