# 🖼️ Image Lighting Tool

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [Parameters](#parameters)
- [Example](#example)
- [Normal Maps](#normal-maps)
- [Creating a Standalone Executable](#creating-a-standalone-executable)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Overview

A command-line application for applying directional lighting effects to images using normal maps.

## Features

- Apply directional lighting to any JPG or PNG image
- Use normal maps to achieve realistic lighting effects
- Support for up to three different lights with independent settings
- Customize light direction, color, brightness, blend intensity, and lighting softness
- Simple command-line interface

## Prerequisites

- .NET 6.0 SDK or newer

## Installation

1. **Clone the repository:**

   ```sh
   git clone <repository-url>
   cd ImageLighting
   ```

2. **Build the project:**
   ```sh
   dotnet build
   ```

## Usage

Run the application with a single light:

```sh
dotnet run -- --image <path-to-image> --normal-map <path-to-normal-map> --output <path-to-output> --light-dir <x,y,z> --light-color <r,g,b> --brightness <value> --intensity <value> --softness <value>
```

Run with multiple lights (up to three):

```sh
+dotnet run -- --image <path-to-image> --normal-map <path-to-normal-map> --output <path-to-output> --light-dir <x,y,z> --light-color <r,g,b> --brightness <value> --softness <value> --light2-dir <x,y,z> --light2-color <r,g,b> --light2-brightness <value> --light2-softness <value> --light3-dir <x,y,z> --light3-color <r,g,b> --light3-brightness <value> --light3-softness <value> --intensity <value>
```

## Parameters
 
### Global Parameters
 - **--image:** Path to the input image file (JPG or PNG)
 - **--normal-map:** Path to the normal map image file
 - **--output:** Path where the output image will be saved
 - **--intensity:** (Optional) Blend factor between original image (0.0) and lighting effect (1.0), default is 1.0

### Primary Light Parameters
- **--light-dir:** Light direction vector in format x,y,z (e.g., 0,0,1)
- **--light-color:** Light color in RGB format r,g,b (e.g., 255,255,255)
- **--brightness:** (Optional) Light brightness, default is 1.0
- **--softness:** (Optional) Controls how gradual the lighting transitions are, higher values create softer lighting, default is 0.0

> Note: The primary light can also be specified using `--light1-dir`, `--light1-color`, etc.

### Secondary Light Parameters (Optional)
- **--light2-dir:** Second light direction vector
- **--light2-color:** Second light color
- **--light2-brightness:** (Optional) Second light brightness, default is 1.0
- **--light2-softness:** (Optional) Second light softness, default is 0.0

### Tertiary Light Parameters (Optional)
- **--light3-dir:** Third light direction vector
- **--light3-color:** Third light color
- **--light3-brightness:** (Optional) Third light brightness, default is 1.0
- **--light3-softness:** (Optional) Third light softness, default is 0.0

## Example

Apply a white light coming from directly above:

```sh
dotnet run -- --image sample.jpg --normal-map sample_normal.png --output output.jpg --light-dir 0,0,1 --light-color 255,255,255 --brightness 1.5 --intensity 1.0
```

Apply a red light from the upper left with partial blending and soft transitions:

```sh
dotnet run -- --image sample.jpg --normal-map sample_normal.png --output output.jpg --light-dir -1,-1,0.5 --light-color 255,0,0 --brightness 2.0 --intensity 0.7 --softness 0.3
```

Apply multiple lights - primary blue light from above and secondary orange light from the side:
```sh
dotnet run -- --image sample.jpg --normal-map sample_normal.png --output output.jpg --light-dir 0,0,1 --light-color 100,100,255 --light2-dir 1,0,0.2 --light2-color 255,150,50 --intensity 0.8
```

Apply three lights for complex lighting effects:
```sh
dotnet run -- --image sample.jpg --normal-map sample_normal.png --output output.jpg --light-dir 0,0,1 --light-color 255,255,255 --light2-dir -0.7,-0.7,0 --light2-color 255,50,50 --light2-softness 0.2 --light3-dir 0.7,-0.7,0 --light3-color 50,50,255 --light3-softness 0.2 --intensity 0.9
```

## Normal Maps

Normal maps are special images where:

- The red channel represents the X direction of the surface normal
- The green channel represents the Y direction
- The blue channel represents the Z direction (depth)

Standard normal maps use a blue-purple color as their base, representing a surface pointing straight up.

## Creating a Standalone Executable

To build a standalone executable:

```sh
dotnet publish -c Release -r win-x64 --self-contained
```

The executable will be in the `bin/Release/net6.0/win-x64/publish` directory.

## Troubleshooting

- **"Input image file not found":** Verify that the path to your input image is correct.
- **"Normal map file not found":** Verify that the path to your normal map is correct.
- **"Image and normal map dimensions must match":** Ensure that your input image and normal map have the same dimensions.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
