﻿namespace Frontlets.Media.Server
{
    internal class FileHelper
    {
        public static int FileCounter;

        //public static List<PlaylistItem> Playlist = new List<PlaylistItem>();

        public static List<int> Randoms = new List<int>();

        public static Random Rand { get; } = new Random(DateTime.Now.Millisecond);
    }
}
