using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace ChangeImageSizeSerivice
{
    public partial class ImageSizeService : ServiceBase
    {
        private FileSystemWatcher watcher;
        private string watchFolder;
        private string outputFolder;
        private int imageWidth;
        private int imageHeight;

        FileSystemWatcher fileSystemWatcher;
        public ImageSizeService()
        {
            InitializeComponent();
           
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                watchFolder = ConfigurationManager.AppSettings["WatchFolder"];
                outputFolder = ConfigurationManager.AppSettings["OutputFolder"];
                imageWidth = int.Parse(ConfigurationManager.AppSettings["ImageWidth"]);
                imageHeight = int.Parse(ConfigurationManager.AppSettings["ImageHeight"]);

                // Ensure output folder exists
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                if (!Directory.Exists(watchFolder))
                    Directory.CreateDirectory(watchFolder);

                watcher = new FileSystemWatcher
                {
                    Path = watchFolder,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };
                watcher.Created += OnNewFile;
            }
            catch(Exception ex)
            {
                File.AppendAllText(Path.Combine(outputFolder, "ErrorLog.txt"), $"{DateTime.Now}: {ex.Message}\n");
            }
        }

        private void OnNewFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Thread.Sleep(500);

                using (var originalImage = Image.FromFile(e.FullPath))
                {
                    // Detect pixel format from the original image
                    //Checks the PixelFormat — this tells us if it has transparency, how many colors ...
                    PixelFormat format = originalImage.PixelFormat;

                    // If it's indexed (e.g., GIF), we should move to 32bppArgb for safer drawing

                    /*Some images (like GIF) store colors in a color table (indexed mode).
                    We switch them to 32bppArgb so we can draw / resize them easily.*/

                    if ((format & PixelFormat.Indexed) != 0)
                        format = PixelFormat.Format32bppArgb;


                    //Makes a new empty image with the width/height you want from App.config
                    using (var resized = new Bitmap(imageWidth, imageHeight, format))
                    {
                        using (var graphics = Graphics.FromImage(resized))
                        {
                            // If image supports alpha channel, clear with transparent
                            //If the image has transparency -> start with a transparent background.
                            if ((format & PixelFormat.Alpha) != 0)
                                graphics.Clear(Color.Transparent);

                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            graphics.DrawImage(originalImage, 0, 0, imageWidth, imageHeight);
                        }

                        // Keep same extension & format as original
                        string extension = Path.GetExtension(e.FullPath).ToLower();
                        string outputFile = Path.Combine(outputFolder, Path.GetFileName(e.FullPath));

                        if (extension == ".png")
                            resized.Save(outputFile, ImageFormat.Png);
                        else if (extension == ".gif")
                            resized.Save(outputFile, ImageFormat.Gif);
                        else if (extension == ".bmp")
                            resized.Save(outputFile, ImageFormat.Bmp);
                        else
                            resized.Save(outputFile, ImageFormat.Jpeg); // For jpg/jpeg
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(outputFolder, "ErrorLog.txt"), $"{DateTime.Now}: {ex.Message}\n");
            }
        }
        protected override void OnStop()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        public void StartInConsole()
        {
            OnStart(null);
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine();

            OnStop();

        }
    }
}
