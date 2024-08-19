namespace Frontlets.Media.Server
{
    internal class FileHelper
    {
        public static int FileCounter;

        //public static List<PlaylistItem> Playlist = new List<PlaylistItem>();

        public static List<int> Randoms = new List<int>();

        public static Random Rand { get; } = new Random(DateTime.Now.Millisecond);

        public static int GetBookNumber(string filename)
        {
            return Convert.ToInt32(filename.Substring(0, 3));
        }

        public static int GetChapterCount(int bookNumber)
        {
            var chapterCount = 0;

            switch (bookNumber)
            {
                case 1: // Genesis
                case 5: // Deuteronomy
                case 11: // I Kings
                case 12: // II Kings
                case 13: // I Chronicles
                case 15: // Ezra
                case 16: // Nehemiah
                case 17: // Esther
                case 19: // Psalms
                case 25: // Lamentations
                case 28: // Hosea
                case 30: // Amos
                case 38: // Zechariah
                case 47: // II Corinthians
                case 52: // I Thessalonians
                case 58: // Hebrews
                case 59: // James
                case 60: // I Peter
                case 62: // I John
                    chapterCount = 5;
                    break;
                case 2: // Exodus
                case 4: // Numbers
                case 6: // Joshua
                case 8: // Ruth
                case 9: // I Samuel
                case 10: // II Samuel
                case 14: // II Chronicles
                case 20: // Proverbs
                case 21: // Ecclesiastes
                case 22: // Songs
                case 24: // Jeremiah
                case 26: // Ezekiel
                case 27: // Daniel
                case 32: // Jonah
                case 33: // Micah
                case 39: // Malachi
                case 40: // Matthew
                case 41: // Mark
                case 42: // Luke
                case 44: // Acts
                case 45: // Romans
                case 46: // I Corinthians
                case 50: // Philippians
                case 51: // Colossians
                case 55: // II Timothy
                case 66: // Revelation
                    chapterCount = 4;
                    break;
                case 3: // Leviticus
                case 7: // Judges
                case 18: // Job
                case 29: // Joel
                case 34: // Nahum
                case 35: // Habakkuk
                case 36: // Zephaniah
                case 43: // John
                case 48: // Galatians
                case 49: // Ephesians
                case 53: // II Thessalonians
                case 54: // I Timothy
                case 56: // Titus
                case 61: // II Peter
                    chapterCount = 3;
                    break;
                case 23: // Isaiah
                    chapterCount = 6;
                    break;
                case 37: // Haggai
                    chapterCount = 2;
                    break;
                case 31: // Obadiah
                case 57: // Philemon
                case 63: // II John
                case 64: // III John
                case 65: // Jude
                    chapterCount = 1;
                    break;
            }

            return chapterCount;
        }
    }
}
