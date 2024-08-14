using System;

namespace GMTK2024
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			using (var game = new GMTK2024Game())
			{
				game.Run();
			}
		}
	}
}
