# Sandwych.DeviceTree

[![Build Status](https://travis-ci.org/{your_username}/{your_repo}.svg?branch=master)](https://travis-ci.org/{your_username}/{your_repo})

Sandwych.DeviceTree is a .NET library for parsing Linux Device Tree Source (DTS) files. It provides a convenient and efficient way to extract information from DTS files and work with the device tree data in a structured manner.

## Status

Current status: **WORKING-IN-PROGRESS**

⚠️ **This project is currently a work in progress and is not yet production-ready.** ⚠️

The library is under active development and should be considered as an early-stage project.
It may have missing features, known issues, or limited compatibility with certain DTS file formats.
Use it at your own risk and be prepared for frequent updates and changes.


## Features

- Parse DTS files and convert them into an easy-to-use object model.
- Traverse and query the device tree data using a familiar and intuitive API.
- Extract information about devices, nodes, properties, and their values.
- Handle includes, references, and overlays for complex device tree structures.
- Support for both 32-bit and 64-bit device tree sources.

## Installation

You can install the Sandwych.DeviceTree library using NuGet. Simply run the following command in the Package Manager Console:

```shell
Install-Package Sandwych.DeviceTree
```


## Usage

To parse a DTS file and extract information from it, follow these steps:

Create an instance of DeviceTreeDocument:

```csharp
var document = new DeviceTreeDocument();
```

Load the DTS file:

```csharp
document.Load("path/to/your/dts/file.dts");
```

Access the device tree nodes and properties:

```csharp
var rootNode = document.RootNode;
foreach (var childNode in rootNode.ChildNodes)
{
    // Process child nodes
}

var property = rootNode.Properties["property_name"];
if (property != null)
{
    // Access the property value
}
```

For more detailed usage examples and API documentation, please refer to the Wiki section.


## Contributing

Contributions to Sandwych.DeviceTree are welcome! If you find a bug, have a feature request, or want to contribute code, please feel free to open an issue or submit a pull request.

Please make sure to follow our Contribution Guidelines and adhere to our Code of Conduct.

## License
This project is licensed under the MIT License.
