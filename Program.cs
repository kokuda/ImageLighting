using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using System.Collections.Generic;

namespace ImageLighting
{
    class Program
    {
        // Light class to store all light parameters
        private class Light
        {
            public Vector3 Direction { get; set; }
            public Rgba32 Color { get; set; }
            public float Brightness { get; set; }
            public float Softness { get; set; }

            public Light(Vector3 direction, Rgba32 color, float brightness, float softness)
            {
                Direction = Vector3.Normalize(direction);
                Color = color;
                Brightness = brightness;
                Softness = softness;
            }
        }

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

            // Light 1 - required - with backwards compatibility options
            var lightDir1Option = new Option<string>(
                new[] { "--light-dir", "--light1-dir" },
                "Light 1 direction vector (format: x,y,z)")
            { IsRequired = true };

            var lightColor1Option = new Option<string>(
                new[] { "--light-color", "--light1-color" },
                "Light 1 color (format: r,g,b)")
            { IsRequired = true };

            var brightness1Option = new Option<float>(
                new[] { "--brightness", "--light1-brightness" },
                () => 1.0f,
                "Light 1 brightness (default: 1.0)");

            var softness1Option = new Option<float>(
                new[] { "--softness", "--light1-softness" },
                () => 0.0f,
                "Light 1 softness of lighting transition (default: 0.0)");

            // Light 2 - optional
            var lightDir2Option = new Option<string>(
                "--light2-dir",
                "Light 2 direction vector (format: x,y,z)");

            var lightColor2Option = new Option<string>(
                "--light2-color",
                "Light 2 color (format: r,g,b)");

            var brightness2Option = new Option<float>(
                "--light2-brightness",
                () => 1.0f,
                "Light 2 brightness (default: 1.0)");

            var softness2Option = new Option<float>(
                "--light2-softness",
                () => 0.0f,
                "Light 2 softness of lighting transition (default: 0.0)");

            // Light 3 - optional
            var lightDir3Option = new Option<string>(
                "--light3-dir",
                "Light 3 direction vector (format: x,y,z)");

            var lightColor3Option = new Option<string>(
                "--light3-color",
                "Light 3 color (format: r,g,b)");

            var brightness3Option = new Option<float>(
                "--light3-brightness",
                () => 1.0f,
                "Light 3 brightness (default: 1.0)");

            var softness3Option = new Option<float>(
                "--light3-softness",
                () => 0.0f,
                "Light 3 softness of lighting transition (default: 0.0)");

            // Global settings
            var intensityOption = new Option<float>(
                "--intensity",
                () => 1.0f,
                "Blend factor between original image (0.0) and lighting effect (1.0) (default: 1.0)");

            // Add all options
            rootCommand.AddOption(imageOption);
            rootCommand.AddOption(normalMapOption);
            rootCommand.AddOption(outputOption);

            // Light 1
            rootCommand.AddOption(lightDir1Option);
            rootCommand.AddOption(lightColor1Option);
            rootCommand.AddOption(brightness1Option);
            rootCommand.AddOption(softness1Option);

            // Light 2
            rootCommand.AddOption(lightDir2Option);
            rootCommand.AddOption(lightColor2Option);
            rootCommand.AddOption(brightness2Option);
            rootCommand.AddOption(softness2Option);

            // Light 3
            rootCommand.AddOption(lightDir3Option);
            rootCommand.AddOption(lightColor3Option);
            rootCommand.AddOption(brightness3Option);
            rootCommand.AddOption(softness3Option);

            // Global
            rootCommand.AddOption(intensityOption);

            // SetHandler with grouped parameters to stay under the 8 parameter limit
            rootCommand.SetHandler((context) =>
            {
                // Get main parameters
                var imageFile = context.ParseResult.GetValueForOption(imageOption);
                var normalMapFile = context.ParseResult.GetValueForOption(normalMapOption);
                var outputFile = context.ParseResult.GetValueForOption(outputOption);
                var light1Dir = context.ParseResult.GetValueForOption(lightDir1Option);
                var light1Color = context.ParseResult.GetValueForOption(lightColor1Option);
                var light1Brightness = context.ParseResult.GetValueForOption(brightness1Option);
                var light1Softness = context.ParseResult.GetValueForOption(softness1Option);
                var intensity = context.ParseResult.GetValueForOption(intensityOption);

                // Get optional light parameters
                var light2Dir = context.ParseResult.GetValueForOption(lightDir2Option);
                var light2Color = context.ParseResult.GetValueForOption(lightColor2Option);
                var light2Brightness = context.ParseResult.GetValueForOption(brightness2Option);
                var light2Softness = context.ParseResult.GetValueForOption(softness2Option);

                var light3Dir = context.ParseResult.GetValueForOption(lightDir3Option);
                var light3Color = context.ParseResult.GetValueForOption(lightColor3Option);
                var light3Brightness = context.ParseResult.GetValueForOption(brightness3Option);
                var light3Softness = context.ParseResult.GetValueForOption(softness3Option);

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

                    if (string.IsNullOrEmpty(light1Dir))
                    {
                        Console.WriteLine("Light direction is required.");
                        Environment.Exit(1);
                    }

                    if (string.IsNullOrEmpty(light1Color))
                    {
                        Console.WriteLine("Light color is required.");
                        Environment.Exit(1);
                    }

                    // Clamp intensity between 0 and 1
                    intensity = Math.Clamp(intensity, 0.0f, 1.0f);

                    // Create lights list
                    var lights = new List<Light>();

                    // Add Light 1 (required)
                    var lightDir1 = ParseVector(light1Dir);
                    var lightColor1 = ParseColor(light1Color);
                    light1Softness = Math.Max(0.0f, light1Softness);
                    lights.Add(new Light(lightDir1, lightColor1, light1Brightness, light1Softness));
                    Console.WriteLine($"Light 1: direction: {lightDir1}, color: {lightColor1}, brightness: {light1Brightness}, softness: {light1Softness}");

                    // Add Light 2 if specified
                    if (!string.IsNullOrEmpty(light2Dir) && !string.IsNullOrEmpty(light2Color))
                    {
                        var lightDir2 = ParseVector(light2Dir);
                        var lightColor2 = ParseColor(light2Color);
                        light2Softness = Math.Max(0.0f, light2Softness);
                        lights.Add(new Light(lightDir2, lightColor2, light2Brightness, light2Softness));
                        Console.WriteLine($"Light 2: direction: {lightDir2}, color: {lightColor2}, brightness: {light2Brightness}, softness: {light2Softness}");
                    }

                    // Add Light 3 if specified
                    if (!string.IsNullOrEmpty(light3Dir) && !string.IsNullOrEmpty(light3Color))
                    {
                        var lightDir3 = ParseVector(light3Dir);
                        var lightColor3 = ParseColor(light3Color);
                        light3Softness = Math.Max(0.0f, light3Softness);
                        lights.Add(new Light(lightDir3, lightColor3, light3Brightness, light3Softness));
                        Console.WriteLine($"Light 3: direction: {lightDir3}, color: {lightColor3}, brightness: {light3Brightness}, softness: {light3Softness}");
                    }

                    Console.WriteLine($"Global intensity: {intensity}");
                    Console.WriteLine($"Processing image with {lights.Count} light(s)...");

                    // Apply lighting
                    ApplyLighting(imageFile.FullName, normalMapFile.FullName, outputFile.FullName,
                        lights, intensity);

                    Console.WriteLine($"Output saved to: {outputFile.FullName}");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Environment.Exit(1);
                }
            });

            return rootCommand.Invoke(args);
        }

        private static Vector3 ParseVector(string vectorStr)
        {
            if (string.IsNullOrEmpty(vectorStr))
                throw new ArgumentException("Light direction cannot be null or empty");

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
            if (string.IsNullOrEmpty(colorStr))
                throw new ArgumentException("Light color cannot be null or empty");

            var parts = colorStr.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException("Light color must be in format: r,g,b");

            byte r = byte.Parse(parts[0]);
            byte g = byte.Parse(parts[1]);
            byte b = byte.Parse(parts[2]);

            return new Rgba32(r, g, b);
        }

        private static void ApplyLighting(string imagePath, string normalMapPath, string outputPath,
            List<Light> lights, float intensity)
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

                    // Initialize accumulators for each color channel
                    float redAccumulator = 0;
                    float greenAccumulator = 0;
                    float blueAccumulator = 0;

                    // Calculate contribution from each light
                    foreach (var light in lights)
                    {
                        // Calculate light factor (dot product between normal and light direction)
                        float dotProduct = Vector3.Dot(normal, light.Direction);

                        // Apply softness factor
                        float lightFactor = (dotProduct + light.Softness) / (1.0f + light.Softness);

                        // Ensure it's positive and scale by brightness
                        lightFactor = Math.Max(0, lightFactor) * light.Brightness;

                        // Add this light's contribution
                        redAccumulator += lightFactor * light.Color.R / 255.0f;
                        greenAccumulator += lightFactor * light.Color.G / 255.0f;
                        blueAccumulator += lightFactor * light.Color.B / 255.0f;
                    }

                    // Apply lighting to original pixel, capping at 255
                    Rgba32 litPixel = new Rgba32(
                        (byte)Math.Min(255, originalPixel.R * redAccumulator),
                        (byte)Math.Min(255, originalPixel.G * greenAccumulator),
                        (byte)Math.Min(255, originalPixel.B * blueAccumulator),
                        originalPixel.A // Keep original alpha
                    );

                    // Blend between original and lit pixel based on intensity
                    Rgba32 finalPixel = new Rgba32(
                        (byte)Math.Round(originalPixel.R * (1 - intensity) + litPixel.R * intensity),
                        (byte)Math.Round(originalPixel.G * (1 - intensity) + litPixel.G * intensity),
                        (byte)Math.Round(originalPixel.B * (1 - intensity) + litPixel.B * intensity),
                        originalPixel.A // Keep original alpha
                    );

                    output[x, y] = finalPixel;
                }
            }

            // Save the result
            output.Save(outputPath);
        }
    }
}
