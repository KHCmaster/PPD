namespace System.IO
{
    public static class ExDirectory
    {
        public static bool Contains(string path, string filename)
        {
            bool found = false;
            var filenames = Directory.GetFiles(path);
            for (int i = 0; i < filenames.Length; i++)
            {
                if (Path.GetFileName(filenames[i]) == filename)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}