# Inner Content

[![Build status](https://img.shields.io/appveyor/ci/UMCO/umbraco-inner-content.svg)](https://ci.appveyor.com/project/UMCO/umbraco-inner-content)
[![NuGet release](https://img.shields.io/nuget/v/Our.Umbraco.InnerContent.svg)](https://www.nuget.org/packages/Our.Umbraco.InnerContent)


A helper library for Umbraco Doc Type based property editors providing overlays and conversion helpers.

## Getting Started

### Installation

> *Note:* Inner Content has been developed against **Umbraco v7.4.0** and will support that version and above.

Inner Content can be installed from the NuGet package repositories or build manually from the source-code:

#### NuGet package repository

To [install from NuGet](https://www.nuget.org/packages/Our.Umbraco.InnerContent), you can run the following command from within Visual Studio:

	PM> Install-Package Our.Umbraco.InnerContent

We also have a [MyGet package repository](https://www.myget.org/gallery/umbraco-packages) - for bleeding-edge / development releases.

#### Manual build

If you prefer, you can compile  Inner Content yourself, you'll need:

* [Visual Studio 2017 (or above, including Community Editions)](https://www.visualstudio.com/downloads/)
* Microsoft Build Tools 2015 (aka [MSBuild 15](https://www.microsoft.com/en-us/download/details.aspx?id=48159))

To clone it locally click the "Clone in Windows" button above or run the following git commands.

	git clone https://github.com/umco/umbraco-inner-content.git umbraco-inner-content
	cd umbraco-inner-content
	.\build.cmd

---

## Known Issues

- _[TBC]_

---

## Implementations

Umbraco packages that use Inner Content as a dependency library.

- [Stacked Content](https://github.com/umco/umbraco-stacked-content)
- [Content List](https://github.com/umco/umbraco-content-list)

---

## Contributing to this project

Anyone and everyone is welcome to contribute. Please take a moment to review the [guidelines for contributing](CONTRIBUTING.md).

- [Bug reports](CONTRIBUTING.md#bugs)
- [Feature requests](CONTRIBUTING.md#features)
- [Pull requests](CONTRIBUTING.md#pull-requests)

---

## Contact

Have a question?

- [Raise an issue](https://github.com/umco/umbraco-inner-content/issues) on GitHub

## Dev Team

- [Matt Brailsford](https://github.com/mattbrailsford)
- [Lee Kelleher](https://github.com/leekelleher)

## License

Copyright &copy; 2016 UMCO, Our Umbraco and [other contributors](https://github.com/umco/umbraco-inner-content/graphs/contributors)

Licensed under the [MIT License](LICENSE.md)
