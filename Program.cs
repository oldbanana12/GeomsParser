using System.IO;

namespace GeomsParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                if(Path.GetExtension(args[0]) == ".geoms")
                {
                    Geoms geoms = new Geoms();
                    geoms.read(args[0]);
                }
            }
        }
    }
}
