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

            var files = System.IO.Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
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
                    var bmp = pdf.SaveAsImage(0);
                    var outputFilename = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(file)}.jpg");
                    Console.WriteLine($"Saving jpg to {outputFilename}");
                    bmp.Save(outputFilename, ImageFormat.Jpeg);
                }
                else if (lowerFile.EndsWith(".png"))
                {
                    Console.WriteLine($"Current file: {file}");
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
                            }
                            else
                            {
                                Console.WriteLine("-- orientation not found!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("-- orientation not found!");
                        }


                        //foreach (var dir in dirs)
                        //{

                        //    {

                        //    }
                        //}

                        //foreach (var tag in dir.Tags)

                        //    Console.WriteLine($"{dir.Name} - {tag.TagName} = {tag.Description}");
                        Console.WriteLine("---");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("UNABLE TO READ");
                    }
                }
            }
        }
    }
}
