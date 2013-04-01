namespace CopyTags
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourcePath = args[0];
            string destinationPath = args[1];

            using (var sourceFile = TagLib.File.Create(sourcePath))
            using (var destinationFile = TagLib.File.Create(destinationPath))
            {
                sourceFile.Tag.CopyTo(destinationFile.Tag, overwrite: true);
                destinationFile.Save();
            }
        }
    }
}
