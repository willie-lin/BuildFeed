# BuildFeed

BuildFeed is a website for tracking Windows build numbers. The canonical version is hosted at [https://buildfeed.net/](https://buildfeed.net/).

## Using

### Hosting

Current versions of BuildFeed require the following:
* IIS 7.0 or greater
* IIS URL Rewriting module
* .NET Framework 4.7
* MongoDB v2.4 or newer

There are a few configuration files missing from the Git repository. You will need to add these to your local copy.

**smtp.config**

This file is responsible for configuring SMTP. Without these details in place, sending emails, such as registration emails, will fail.

```xml
<?xml version="1.0"?>

<smtp deliveryMethod="Network" from="from@email.com">
   <network host="smtp.email.com" port="465" userName="from@email.com" password="password" enableSsl="true" />
</smtp>
```

**settings.config**

This file contains sensitive configuration data:

* `data:SecretKey` is a string used when generating some hashes for use in URLs. Primarily, it's used to ensure that the hash in an email validation link cannot be predicted without knowledge of this secret key.

* `data:MongoUser` and `data:MongoPass` are the Username and Password for MongoDB. If you are running without authentication, such as in a local testing environment, you should omit these two settings.

* `push:OneSignalApiKey` and `push:AppId` are used by the OneSignal push notification system. These two properties can be obtained from within the OneSignal configuration panel.

```xml
<?xml version="1.0"?>

<appSettings>
   <add key="data:SecretKey" value="[Secret Key]" />

   <add key="data:MongoUser" value="username" />
   <add key="data:MongoPass" value="password" />
   <add key="push:OneSignalApiKey" value="[API Key]" />
   <add key="push:AppId" value="[App ID]" />
</appSettings>
```

### Developing

The following will be required to develop BuildFeed:
* Visual Studio 2017 (Community will be sufficient)
* .NET Framework 4.7 (inc. Targeting Pack via VS 2017 installer)
* MongoDB 2.4 or newer
* Node support within Visual Studio 2017.

You will need to follow the instructions under hosting for restoring the files missing from the repository.

The website is split up into three main projects.

**BuildFeed**

This is the main website, built using ASP.NET MVC.

Styling is provided under `~/res/css/` as SASS files. These are then compiled into CSS using gulp. This recompilation will run automatically on a file change normally, but can also be ran on-demand using the Task Runner Explorer in Visual Studio. There are four SASS files:
* `default.scss` - the styling in this file is common to the entire site.
* `dark.scss` - the styling in this file is applied when the Dark theme is active.
* `light.scss` - the styling in this file is applied when the Light theme is active.
* `rtl.scss` - the styling in this file is applied when a Right-to-Left language is active.

Scripting is provided under `~/res/ts/` as a Typescript file. We don't use Visual Studio's built-in TypeScript compiler, it's disabled in the project file. Instead, we use gulp along with the Node TypeScript compiler directly. Recompilation will run automatically on a file change normally, but can also be ran on-demand using the Task Runner Explorer in Visual Studio. Mostly, this script is just pure ECMAScript and DOM, but two external javascript libraries are referenced:
* `jsrender` - this is used to render the search results in the search dialog after fetching the data via an AJAX request.
* `google.analytics` - used to channel search terms to Site Search in Google Analytics via the use of Virtual Page Views.

Other files of note include:
* `manifest.json` - contains the configuration data for BuildFeed as a Progressive Web Application.
* `gulpfile.js` - contains the gulp tasks and configuration/
* `package.json` - contains the Node packages used within the gulp system.
* `tsconfig.json` - contains the configuration data for TypeScript. Used here to exclude typings from producing syntax warnings.
* `browserconfig.xml` - contains the Internet Explorer web application description.
* `Controllers/apiController.cs` - contains the controller for the BuildFeed API.

**BuildFeed.Local**

This contains the many translations that BuildFeed maintains. `VariantTerms.resx` contains the authoritative, and fallback, English translation. `VariantTerms.qps-ploc.resx` is included for reference, but not used as non-Windows environments do not support the `qps-ploc` locale.

The only other file in this project is `InvariantTerms.cs`, which includes some common terms used throughout the site, but do not change for each language, in a single place for easy adjustment.

**BuildFeed.Model**

This project contains all code that interacts with the data store. Many files in the root folder are simple MongoDB document models. There's a couple of important exceptions:

* `BuildRepository.cs` is the class which allows you to easily interact with the database - it contains all methods that carry out the various CRUD tasks for the Build models. This is also split out into a number of `BuildRepository-{function}.cs` files, where methods relating to specific common functions are grouped together.
* `MongoConfig.cs` is responsible for managing the configuration and connection to MongoDB.

There are two main subfolders that you should take note of:

* `Api` contains the models used exclusively within the BuildFeed API.
* `View` contains the models used exclusively within Views in the MVC website.

### Other

There's also two further projects contained within the solution.

* **Mobile/BuildFeedApp-Westminster** - A Project Westminster UWP application that essentially wraps the website into an application that can be submitted in the Windows store. This is effectively abandoned and will be replaced with a more robust, purpose-developed mobile application in the future.
* **Authentication/MongoAuth** - This is a custom Membership and Role Provider designed to store the Users and Roles for BuildFeed within MongoDB.

## License

Copyright © 2017, Thomas Hounsell.

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.