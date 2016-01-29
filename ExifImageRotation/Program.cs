using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Spire.Pdf;
using System.Drawing.Imaging;

namespace ExifImageRotation
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if path was provided
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a path to read for image/PDF files! Exiting..");

                return;
            }

            var path = args[0];
            if (!System.IO.Directory.Exists(path))
            {
                Console.WriteLine("Invalid path, does not exist! Exiting..");

                return;
            }

            var outputPath = Path.Combine(path, "output");
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);

            var files = System.IO.Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(
                    f => f.ToLower().EndsWith(".jpg") ||
                    f.ToLower().EndsWith(".jpeg") ||
                    f.ToLower().EndsWith(".png") ||
                    f.ToLower().EndsWith(".pdf"));

            foreach (var file in files)
            {
                var lowerFile = file.ToLower();

                if (lowerFile.EndsWith(".pdf"))
                {
                    Console.WriteLine($"Current file: {file}");
                    var pdf = new PdfDocument();
                    pdf.LoadFromFile(file);

                    // Save first page of PDF
                    using (var bmp = pdf.SaveAsImage(0))
                    {
                        var outputFilename = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(file)}.jpg");
                        Console.WriteLine($"Saving jpg to {outputFilename}");
                        bmp.Save(outputFilename, ImageFormat.Jpeg);
                    }
                }
                else if (lowerFile.EndsWith(".png"))
                {
                    Console.WriteLine($"Current file: {file}");
                    RotateImageIfNeeded(file, outputPath);
                }
                else if (lowerFile.EndsWith(".jpg") || lowerFile.EndsWith(".jpeg"))
                {
                    Console.WriteLine($"Current file: {file}");
                    try
                    {
                        var dirs = ImageMetadataReader.ReadMetadata(file);
                        if (dirs.Any())
                        {
                            Console.WriteLine("meta data to read");
                        }
                        var dir = dirs.Where(d => d.Name.Equals("Exif IFD0")).SingleOrDefault();
                        if (dir != null)
                        {
                            var orientation = dir.Tags.Where(t => t.TagName.Equals("Orientation")).SingleOrDefault();
                            if (orientation != null)
                            {
                                Console.WriteLine($"-- orientation = {orientation.Description}, dir name = {dir.Name}");
                                ExifRotateImageIfNeeded(file, outputPath);
                            }
                            else
                            {
                                Console.WriteLine("-- orientation not found!");
                                RotateImageIfNeeded(file, outputPath);
                            }
                        }
                        else
                        {
                            Console.WriteLine("-- orientation not found!");
                            RotateImageIfNeeded(file, outputPath);
                        }


                        //foreach (var dir in dirs)
                        //{

                        //    {

                        //    }
                        //}

                        //foreach (var tag in dir.Tags)

                        //    Console.WriteLine($"{dir.Name} - {tag.TagName} = {tag.Description}");
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("UNABLE TO READ");
                        //RotateImageIfNeeded(file, outputPath);
                    }
                    
                }
            }

            
            Console.WriteLine("---");
        }

        static void RotateImageIfNeeded(string file, string outputPath)
        {
            Console.WriteLine("RotateImageIfNeeded");
            bool changes = false;
            var extension = Path.GetExtension(file);
            var outputFilename = Path.Combine(outputPath, $"{Path.GetFileName(file)}");
            using (var image = Bitmap.FromFile(file))

            {

                ImageFormat format = ImageFormat.Jpeg;
                if (extension.ToLower().Equals(".png"))
                    format = ImageFormat.Png;


                if (image.Width > image.Height)
                {
                    Console.WriteLine($"extension = {extension}, format = {format}");

                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    changes = true;
                    image.Save(outputFilename, format);

                }
                else
                {
                    Console.WriteLine($"no rotation needed!! save to {outputFilename}");
                }
            }

            if (!changes)
                File.Copy(file, outputFilename, true);
        }

        static void ExifRotateImageIfNeeded(string file, string outputPath)
        {
            var extension = Path.GetExtension(file);
            var outputFilename = Path.Combine(outputPath, $"{Path.GetFileName(file)}");

            using (var image = Bitmap.FromFile(file))
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];
                Console.WriteLine($"ExifRotateImageIfNeeded: orientation = {orientation}, outfilename = {outputFilename}");
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                image.RemovePropertyItem(274);

                image.Save(outputFilename, ImageFormat.Jpeg);
            }
        }
    }
}
