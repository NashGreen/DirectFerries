# DirectFerries

I have used Session Authentication for simplicity, I could also have used a MVC architecture with similar results.
If I was creating a larger app which involved user data from a database or federation I would have used MS Identity as the authentication method as this automatically deals with cookies, claims, roles etc and have the added bonus of tried and tested built in authentication routines using the UserManager, hashed passwords etc, alternatively I could have implemented cookie authentication.
I have added comments to some of the code, especially the Program.cs, as examples of this.

# .NET 8 Razor Pages Application - DummyJSON API Integration

This project is a sample Razor Pages application built with .NET 8 that integrates with the DummyJSON test API. It demonstrates how to consume external JSON APIs using an API service class and display the data in a Razor Page.

## Features

- Built with ASP.NET Core Razor Pages (.NET 8)
- Uses HttpClient to call DummyJSON API
- Displays product data from https://dummyjson.com/products
- Configured to run under HTTPS profile in Visual Studio 2022

## Prerequisites

- Visual Studio 2022 (latest version with .NET 8 SDK installed)
- Internet connection to access DummyJSON API

## Setup Instructions

1. **Clone or Download the Project**
   - Open Visual Studio 2022
   - Select "Open a project or solution" and choose the downloaded folder

2. Run under profile "https"
