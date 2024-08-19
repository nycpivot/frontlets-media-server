using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;
using System.Net;

namespace Frontlets.Media.Server
{
    public class Worker : BackgroundService
    {
        //private readonly IConfiguration configuration;
        //private readonly MediaStorage mediaStorage;
        private readonly ILogger<Worker> logger;

        private readonly string home;

        private readonly DirectoryInfo homeDirectory;
        private readonly DirectoryInfo playlistDirectory;

        private AmazonS3Client storageClient;

        private readonly string KJV_CHRISTOPHER_OT = "mp4-bible-kjv-chapters-christopher-ot";
        private readonly string KJV_CHRISTOPHER_NT = "mp4-bible-kjv-chapters-christopher-nt";
        //private readonly string KJV_ALEXANDER_SCOURBY = "mp3-bible-kjv-alexander-scourby";
        //private readonly string KJV_AUDIO_TREASURE = "mp3-bible-kjv-audio-treasure";
        //private readonly string DEVOTIONS = "mp3-devotions";
        private readonly string CLASSICAL_1 = "mp4-classical-1";
        private readonly string CLASSICAL_2 = "mp4-classical-2";
        private readonly string CLASSICAL_3 = "mp4-classical-3";
        private readonly string HYMNS_1 = "mp4-hymns-1";
        //private readonly string THEMES = "media-audio-mp3-themes";

        private readonly string KJV_CHRISTOPHER = "kjv-christopher";
        private readonly string CLASSICAL_HYMNS = "classical-hymns";

        Random random = new Random();

        private readonly List<CatalogItem> storage = new List<CatalogItem>();
        private readonly List<CatalogItem> catalog = new List<CatalogItem>();
        private readonly List<CatalogItem> playlist = new List<CatalogItem>();

        public Worker(ILogger<Worker> logger)
        {
            //this.mediaStorage = options.Value;
            this.logger = logger;

            this.home = Environment.GetEnvironmentVariable("HOME") ?? "~";

            this.homeDirectory = new DirectoryInfo(home);
            this.playlistDirectory = homeDirectory.CreateSubdirectory("playlist");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                this.storageClient = new AmazonS3Client();

                while (!stoppingToken.IsCancellationRequested)
                {
                    Run();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("/home/ubuntu/log.txt", ex.ToString());
                //Console.WriteLine(ex);
            }
        }

        void Run()
        {
            GenerateCatalog();
            GeneratePlaylist();
            BeginStreaming();
        }

        void GenerateCatalog()
        {
            var containers = new List<string>()
            {
                KJV_CHRISTOPHER_OT,
                KJV_CHRISTOPHER_NT,
                //KJV_ALEXANDER_SCOURBY,
                //KJV_AUDIO_TREASURE,
                //DEVOTIONS,
                CLASSICAL_1,
                CLASSICAL_2,
                CLASSICAL_3,
                HYMNS_1,
                //THEMES
            };

            storage.Clear();
            playlist.Clear();

            foreach (var container in containers)
            {
                var objectsRequest = new ListObjectsV2Request()
                {
                    BucketName = "frontlets-media",
                    Prefix = container
                };

                var response = storageClient.ListObjectsV2Async(objectsRequest).Result;

                foreach (var obj in response.S3Objects)
                {
                    if(obj.Key.EndsWith("mp3") || obj.Key.EndsWith("mp4"))
                    {
                        var slashIndex = obj.Key.IndexOf("/");
                        var filename = obj.Key.Substring(slashIndex + 1);

                        storage.Add(new CatalogItem() { Type = container, Key = obj.Key, FileName = filename });
                    }
                }
            }

            var kjvChristopherFilesOt = storage.Where(s => s.Type == KJV_CHRISTOPHER_OT)
                .Select(s => new CatalogItem() { Type = KJV_CHRISTOPHER, Key = s.Key, FileName = s.FileName })
                .OrderBy(f => f.FileName)
                .ToList();

            var kjvChristopherFilesNt = storage.Where(s => s.Type == KJV_CHRISTOPHER_NT)
                .Select(s => new CatalogItem() { Type = KJV_CHRISTOPHER, Key = s.Key, FileName = s.FileName })
                .OrderBy(f => f.FileName)
                .ToList();

            var classical1 = storage.Where(s => s.Type == CLASSICAL_1)
                .Select(s => new CatalogItem() { Type = CLASSICAL_HYMNS, Key = s.Key, FileName = s.FileName })
                .ToList();

            var classical2 = storage.Where(s => s.Type == CLASSICAL_2)
                .Select(s => new CatalogItem() { Type = CLASSICAL_HYMNS, Key = s.Key, FileName = s.FileName })
                .ToList();

            var classical3 = storage.Where(s => s.Type == CLASSICAL_3)
                .Select(s => new CatalogItem() { Type = CLASSICAL_HYMNS, Key = s.Key, FileName = s.FileName })
                .ToList();

            var hymns1 = storage.Where(s => s.Type == HYMNS_1)
                .Select(s => new CatalogItem() { Type = CLASSICAL_HYMNS, Key = s.Key, FileName = s.FileName })
                .ToList();

            catalog.AddRange(kjvChristopherFilesOt);
            catalog.AddRange(kjvChristopherFilesNt);
            catalog.AddRange(classical1);
            catalog.AddRange(classical2);
            catalog.AddRange(classical3);
            catalog.AddRange(hymns1);
        }

        void GeneratePlaylist()
        {
            for(var bookNumber = 1; bookNumber <= 66; bookNumber++)
            {
                var skip = 0;
                var take = FileHelper.GetChapterCount(bookNumber);

                 var chapters = catalog.Where(
                    b => b.Type == KJV_CHRISTOPHER && FileHelper.GetBookNumber(b.FileName) == bookNumber).ToList();

                AddChapters(chapters, skip, take);
            }

            foreach(var item in playlist)
            {
                Debug.WriteLine(item.FileName);
            }

            //// MOVE KJV CHRISTOPHER 5 FILES AT A TIME
            //for (int ctr = 0; ctr < catalog.Count; ctr++)
            //{
            //    var catalogItem = catalog[ctr];
            //    var bookNumber = FileHelper.GetBookNumber(catalogItem.FileName);




            //    var counter = ctr + 1;

            //    //var kjvChristopherFile = new FileInfo(kjvChristopherFiles[kjv1].FullName);

            //    MoveToPlaylist(catalogItem);

            //    var chapterCount = FileHelper.ChaptersToRead(catalogItem.FileName);

            //    if (counter % chapterCount == 0)
            //    {
            //        //AddHymns(3);
            //        //AddDevotions(1);

            //        AddClassical(8);
            //        //AddTreasure(1);
            //        //AddHymns(8);
            //        //AddDevotions(1);
            //        //AddMiscellaneous(1);
            //        //AddScourby(1);
            //        //AddMasterpiece(1);
            //        //AddHymns(2);
            //    }
            //}
        }

        void AddChapters(List<CatalogItem> chapters, int skip, int take)
        {
            var batch = chapters.Skip(skip).Take(take);
            
            foreach(var chapter in batch)
            {
                MoveToPlaylist(chapter);
            }

            AddMusic(8);

            var total = skip + take;

            if (total == chapters.Count)
            {
                return;
            }
            else
            {
                var remainder = chapters.Count % take;
                if (chapters.Count - total == remainder)
                {
                    AddChapters(chapters, total, remainder);

                    return;
                }
            }

            AddChapters(chapters, total, take);
        }

        void AddMusic(int count)
        {
            var currentCount = catalog.Count(c => c.Type == CLASSICAL_HYMNS);

            if (currentCount < count) // reload
            {
                //var containerClient = storageClient.GetBlobContainerClient(CLASSICAL);

                //foreach (var blob in containerClient.GetBlobs())
                //{
                //    Console.WriteLine($"{CLASSICAL}/{blob.Name}");

                //    var blobUrl = $"{containerClient.Uri}/{blob.Name}";

                //    catalog.Add(new CatalogItem() { Type = CLASSICAL, Name = blob.Name, Url = blobUrl });
                //}

                currentCount = catalog.Count(c => c.Type == CLASSICAL_HYMNS);
            }

            if (currentCount > 0)
            {
                var rnds = Enumerable.Range(0, currentCount - 1)
                    .OrderBy(r => random.Next(currentCount - 1))
                    .Take(count)
                    .ToList();

                foreach (var rnd in rnds)
                {
                    var catalogItem = catalog.Where(c => c.Type == CLASSICAL_HYMNS).ToList()[rnd];

                    MoveToPlaylist(catalogItem);
                }

                //foreach (var rnd in rnds.OrderByDescending(r => r))
                //{
                //    catalog.Where(c => c.Type == CLASSICAL_HYMNS).ToList().RemoveAt(rnd);
                //}
            }
        }

        void AddClassical(int count)
        {
            //var currentCount = catalog.Count(c => c.Type == CLASSICAL);

            //if (currentCount < count) // reload
            //{
            //    //var containerClient = storageClient.GetBlobContainerClient(CLASSICAL);

            //    //foreach (var blob in containerClient.GetBlobs())
            //    //{
            //    //    Console.WriteLine($"{CLASSICAL}/{blob.Name}");

            //    //    var blobUrl = $"{containerClient.Uri}/{blob.Name}";

            //    //    catalog.Add(new CatalogItem() { Type = CLASSICAL, Name = blob.Name, Url = blobUrl });
            //    //}

            //    currentCount = catalog.Count(c => c.Type == CLASSICAL);
            //}

            //if (currentCount > 0)
            //{
            //    var rnds = Enumerable.Range(0, currentCount - 1)
            //        .OrderBy(r => random.Next(currentCount - 1))
            //        .Take(count)
            //        .ToList();

            //    foreach (var rnd in rnds)
            //    {
            //        var catalogItem = catalog.Where(c => c.Type == CLASSICAL).ToList()[rnd];

            //        MoveToPlaylist(catalogItem);
            //    }

            //    foreach (var rnd in rnds.OrderByDescending(r => r))
            //    {
            //        catalog.Where(c => c.Type == CLASSICAL).ToList().RemoveAt(rnd);
            //    }
            //}
        }

        void MoveToPlaylist(CatalogItem catalogItem)
        {
            playlist.Add(catalogItem);

            //var filename = targetFile;

            //targetFile = PrefixFile(targetFile);

            //var target = Path.Combine(targetPath, targetFile);

            //Console.WriteLine(target);

            ////if(FileHelper.Playlist.Count(p => p.Filename == filename) > 0)
            ////{
            ////    Debugger.Break();
            ////}

            //if (!FileHelper.Playlist.Any(p => p.Filename == filename))
            //{
            //    var item = new PlaylistItem
            //    {
            //        Source = source,
            //        Target = target,
            //        Filename = filename,
            //        FullName = targetFile,
            //        IsCopy = false,
            //        Next = 0
            //    };

            //    FileHelper.Playlist.Add(item);
            //}

            //File.Move(source, target);

            FileHelper.FileCounter += 1;
        }

        void BeginStreaming()
        {
            var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.CreateNoWindow = true;

            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = hostEntry.AddressList.First(a => a.ToString().Count(c => c == '.') == 3); // [0];

            //foreach(var address in hostEntry.AddressList)
            //{
            //    Console.WriteLine(address);
            //}

            foreach (var catalogItem in playlist)
            {
                var objectRequest = new GetObjectRequest()
                {
                    BucketName = "frontlets-media",
                    Key = catalogItem.Key
                };

                var file = storageClient.GetObjectAsync(objectRequest).Result;

                if (file.ContentLength > 0)
                {
                    file.WriteResponseStreamToFileAsync(
                        @$"/home/ubuntu/playlist/{catalogItem.FileName}", false, new CancellationToken()).Wait();
                }

                var args = $"-re -i \"/home/ubuntu/playlist/{catalogItem.FileName}\" -vcodec libx264 -preset ultrafast -maxrate 3000k -b:v 2500k -bufsize 600k -pix_fmt yuv420p -g 60 -c:a aac -b:a 160k -ac 2 -ar 44100 -f flv -s 1280x720 rtmp://{ipAddress.ToString()}/live/stream";

                Console.WriteLine(args);

                //process.StartInfo.Arguments = $"-re -i \"{file}\" -c:v copy -c:a aac -ar 44100 -ac 1 -f flv rtmp://localhost/live/stream";
                process.StartInfo.Arguments = args;
                process.Start();

                process.WaitForExit();

                File.Delete(Path.Combine(playlistDirectory.FullName, catalogItem.FileName));

                // maybe check here for existing connections instead of a different thread
                //CheckConnections();
            }
        }
    }
}
