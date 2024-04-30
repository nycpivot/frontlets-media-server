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

        private readonly string KJV_CHRISTOPHER = "mp4-bible-kjv-chapters-christopher";
        //private readonly string KJV_ALEXANDER_SCOURBY = "mp3-bible-kjv-alexander-scourby";
        //private readonly string KJV_AUDIO_TREASURE = "mp3-bible-kjv-audio-treasure";
        //private readonly string DEVOTIONS = "mp3-devotions";
        private readonly string CLASSICAL = "mp4-classical";
        //private readonly string MASTERPIECES = "media-audio-mp3-classical-millenium-masterpieces";
        //private readonly string MISCELLANEOUS = "media-audio-mp3-classical-miscellaneous";
        //private readonly string HYMNS = "media-audio-mp3-hymns";
        //private readonly string THEMES = "media-audio-mp3-themes";

        Random random = new Random();

        private readonly IList<CatalogItem> catalog = new List<CatalogItem>();
        private readonly IList<CatalogItem> playlist = new List<CatalogItem>();

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
            GetFiles();
            GeneratePlaylist();
            BeginStreaming();
        }

        void GetFiles()
        {
            var containers = new List<string>()
            {
                KJV_CHRISTOPHER,
                //KJV_ALEXANDER_SCOURBY,
                //KJV_AUDIO_TREASURE,
                //DEVOTIONS,
                CLASSICAL,
                //MASTERPIECES,
                //MISCELLANEOUS,
                //HYMNS,
                //THEMES
            };

            catalog.Clear();
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

                        catalog.Add(new CatalogItem() { Type = container, Key = obj.Key, FileName = filename });
                    }
                }
            }
        }

        void GeneratePlaylist()
        {
            //var kjvChristopher = Path.Combine(mediaDirectory.FullName, KJV_CHRISTOPHER);
            //var kjvChristopherDirectory = new DirectoryInfo(kjvChristopher);
            var kjvChristopherFiles = catalog.Where(c => c.Type == KJV_CHRISTOPHER)
                .OrderBy(f => f.FileName)
                .ToList();

            // MOVE KJV CHRISTOPHER 5 FILES AT A TIME
            for (int ctr = 0; ctr < kjvChristopherFiles.Count; ctr++)
            {
                var catalogItem = kjvChristopherFiles[ctr];
                var counter = ctr + 1;

                //var kjvChristopherFile = new FileInfo(kjvChristopherFiles[kjv1].FullName);

                MoveToPlaylist(catalogItem);

                if (counter % 5 == 0)
                {
                    //AddHymns(3);
                    //AddDevotions(1);

                    AddClassical(2);
                    //AddTreasure(1);
                    //AddHymns(8);
                    //AddDevotions(1);
                    //AddMiscellaneous(1);
                    //AddScourby(1);
                    //AddMasterpiece(1);
                    //AddHymns(2);
                }
            }
        }

        void AddClassical(int count)
        {
            var currentCount = catalog.Count(c => c.Type == CLASSICAL);

            if (currentCount < count) // reload
            {
                //var containerClient = storageClient.GetBlobContainerClient(CLASSICAL);

                //foreach (var blob in containerClient.GetBlobs())
                //{
                //    Console.WriteLine($"{CLASSICAL}/{blob.Name}");

                //    var blobUrl = $"{containerClient.Uri}/{blob.Name}";

                //    catalog.Add(new CatalogItem() { Type = CLASSICAL, Name = blob.Name, Url = blobUrl });
                //}

                currentCount = catalog.Count(c => c.Type == CLASSICAL);
            }

            if (currentCount > 0)
            {
                var rnds = Enumerable.Range(0, currentCount - 1)
                    .OrderBy(r => random.Next(currentCount - 1))
                    .Take(count)
                    .ToList();

                foreach (var rnd in rnds)
                {
                    var catalogItem = catalog.Where(c => c.Type == CLASSICAL).ToList()[rnd];

                    MoveToPlaylist(catalogItem);
                }

                foreach (var rnd in rnds.OrderByDescending(r => r))
                {
                    catalog.Where(c => c.Type == CLASSICAL).ToList().RemoveAt(rnd);
                }
            }
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
