using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace ImageLighting
{
    class Program
    {
        static int Main(string[] args)
        {
            // Create command line arguments
            var rootCommand = new RootCommand("Apply directional lighting to an image using a normal map");
            
            var imageOption = new Option<FileInfo>(
                "--image", 
                "The input image file (JPG or PNG)");
            
            var normalMapOption = new Option<FileInfo>(
                "--normal-map", 
                "The normal map image file (RGB values map to XYZ normal vector)");
            
            var outputOption = new Option<FileInfo>(
                "--output", 
                "The output image file path");
            
            var lightDirOption = new Option<string>(
                "--light-dir", 
                "Light direction vector (format: x,y,z)") { IsRequired = true };
            
            var lightColorOption = new Option<string>(
                "--light-color", 
                "Light color (format: r,g,b)") { IsRequired = true };
            
            var intensityOption = new Option<float>(
                "--intensity", 
                () => 1.0f, 
                "Light intensity (default: 1.0)");
            
            rootCommand.AddOption(imageOption);
            rootCommand.AddOption(normalMapOption);
            rootCommand.AddOption(outputOption);
            rootCommand.AddOption(lightDirOption);
            rootCommand.AddOption(lightColorOption);
            rootCommand.AddOption(intensityOption);
            
            rootCommand.SetHandler((imageFile, normalMapFile, outputFile, lightDirStr, lightColorStr, intensity) =>
            {
                try 
                {
                    if (imageFile == null || !imageFile.Exists)
                    {
                        Console.WriteLine("Input image file not found.");
                        Environment.Exit(1);
                    }

                    if (normalMapFile == null || !normalMapFile.Exists)
                    {
                        Console.WriteLine("Normal map file not found.");
                        Environment.Exit(1);
                    }

                    if (outputFile == null)
                    {
                        Console.WriteLine("Output file path required.");
                        Environment.Exit(1);
                    }

                    // Parse light direction
                    var lightDir = ParseVector(lightDirStr);
                    // Normalize the light direction
                    lightDir = Vector3.Normalize(lightDir);

                    // Parse light color
                    var lightColor = ParseColor(lightColorStr);

                    Console.WriteLine($"Processing image with light direction: {lightDir}, color: {lightColor}, intensity: {intensity}");
                    
                    // Apply lighting
                    ApplyLighting(imageFile.FullName, normalMapFile.FullName, outputFile.FullName, 
                        lightDir, lightColor, intensity);
                    
                    Console.WriteLine($"Output saved to: {outputFile.FullName}");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Environment.Exit(1);
                }
            }, imageOption, normalMapOption, outputOption, lightDirOption, lightColorOption, intensityOption);
            
            return rootCommand.Invoke(args);
        }

        private static Vector3 ParseVector(string vectorStr)
        {
            var parts = vectorStr.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException("Light direction must be in format: x,y,z");

            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);

            return new Vector3(x, y, z);
        }

        private static Rgba32 ParseColor(string colorStr)
        {
            var parts = colorStr.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException("Light color must be in format: r,g,b");

            byte r = byte.Parse(parts[0]);
            byte g = byte.Parse(parts[1]);
            byte b = byte.Parse(parts[2]);

            return new Rgba32(r, g, b);
        }

        private static void ApplyLighting(string imagePath, string normalMapPath, string outputPath, 
            Vector3 lightDir, Rgba32 lightColor, float intensity)
        {
            // Load the source image and normal map
            using var image = Image.Load<Rgba32>(imagePath);
            using var normalMap = Image.Load<Rgba32>(normalMapPath);

            // Make sure dimensions match
            if (image.Width != normalMap.Width || image.Height != normalMap.Height)
            {
                throw new ArgumentException("Image and normal map dimensions must match");
            }

            // Create a new output image
            using var output = new Image<Rgba32>(image.Width, image.Height);

            // Process each pixel
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    // Get original pixel and normal from normal map
                    var originalPixel = image[x, y];
                    var normalPixel = normalMap[x, y];
                    
                    // Convert normal map RGB to actual vector (assuming standard normal map format)
                    // In normal maps, RGB = XYZ but need to be transformed from 0-255 to -1 to 1
                    var normal = new Vector3(
                        normalPixel.R / 127.5f - 1.0f, 
                        normalPixel.G / 127.5f - 1.0f,
                        normalPixel.B / 127.5f - 1.0f
                    );
                    
                    // Normalize to ensure it's a unit vector
                    normal = Vector3.Normalize(normal);
                    
                    // Calculate light factor (dot product between normal and light direction)
                    float lightFactor = Vector3.Dot(normal, lightDir);
                    
                    // Ensure it's positive and scale by intensity
                    lightFactor = Math.Max(0, lightFactor) * intensity;
                    
                    // Apply lighting to original pixel
                    Rgba32 litPixel = new Rgba32(
                        (byte)Math.Min(255, originalPixel.R * lightFactor * lightColor.R / 255),
                        (byte)Math.Min(255, originalPixel.G * lightFactor * lightColor.G / 255),
                        (byte)Math.Min(255, originalPixel.B * lightFactor * lightColor.B / 255),
                        originalPixel.A // Keep original alpha
                    );
                    
                    output[x, y] = litPixel;
                }
            }

            // Save the result
            output.Save(outputPath);
        }
    }
}