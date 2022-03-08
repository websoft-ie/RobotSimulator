using System.IO;

namespace MonkeyMotionControl.Simulator
{
    public class Texture
    {
        public static MemoryStream LoadFileToMemory(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                var memory = new MemoryStream();
                file.CopyTo(memory);
                return memory;
            }
        }
    }
}
