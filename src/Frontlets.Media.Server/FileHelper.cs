namespace Frontlets.Media.Server
{
    internal class FileHelper
    {
        public static int FileCounter;

        //public static List<PlaylistItem> Playlist = new List<PlaylistItem>();

        public static List<int> Randoms = new List<int>();

        public static Random Rand { get; } = new Random(DateTime.Now.Millisecond);

        public static int ChaptersToRead(string filename)
        {
            var chaptersToRead = 5;

            //var bookNumber = filename.Substring(0, 3);

            //switch(bookNumber)
            //{
            //    case "002": // exodus
            //    case "004": // numbers
            //    case "005": // deuteronomy
            //    case "006": // joshua
            //    case "008": // ruth
            //        chaptersToRead = 4;
            //        break;
            //    case "003": // leviticus
            //    case "007": // judges
            //        chaptersToRead = 3;
            //        break;
            //}

            return chaptersToRead;
        }
    }
}
