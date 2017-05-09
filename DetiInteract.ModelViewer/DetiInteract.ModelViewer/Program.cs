using System;

namespace DetiInteract.ModelViewer
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			using (ModelViewerLib.ModelViewer game = new ModelViewerLib.ModelViewer())
            {
                game.Run();
            }
        }
    }
#endif
}

