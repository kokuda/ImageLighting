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
- Customize light direction, color, and intensity
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

Run the application with the following command:

```sh
dotnet run -- --image <path-to-image> --normal-map <path-to-normal-map> --output <path-to-output> --light-dir <x,y,z> --light-color <r,g,b> --intensity <value>
```

## Parameters

- **--image:** Path to the input image file (JPG or PNG)
- **--normal-map:** Path to the normal map image file
- **--output:** Path where the output image will be saved
- **--light-dir:** Light direction vector in format x,y,z (e.g., 0,0,1)
- **--light-color:** Light color in RGB format r,g,b (e.g., 255,255,255)
- **--intensity:** (Optional) Light intensity, default is 1.0

## Example

Apply a white light coming from directly above:

```sh
dotnet run -- --image sample.jpg --normal-map sample_normal.png --output output.jpg --light-dir 0,0,1 --light-color 255,255,255 --intensity 1.5
```

Apply a red light from the upper left:

```sh
dotnet run -- --image sample.jpg --normal-map sample_normal.png --output output.jpg --light-dir -1,-1,0.5 --light-color 255,0,0 --intensity 2.0
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
