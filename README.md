# VersionChanger
Small console application to change the version of a C# application

**Content**
<!-- TOC -->

- [What is the purpose?](#what-is-the-purpose)
- [Parameters](#parameters)
    - [Version format](#version-format)
- [Installation](#installation)
    - [Conditions](#conditions)
- [.NET Core version](#net-core-version)

<!-- /TOC -->

## What is the purpose?
If you want to update the assembly version of a C# program you can do it manually by setting the values in the `AssemblyInfo.cs`. But when you have to do it manually the danger is great that you forget it. It's happened to me a lot. I've build my application and shipped it to the customers and then I noticed that I forgot to update the version number. So I had to go back to Visual Studio, update the version number and so on. Because of that I've created this small console application which will update the version number automatically. If installed the *VersionChanger* as a pre-build event to my project to I don't have to worry about the version number anymore.

## Parameters
The tool can start with command line parameters. The following parameters are supported:

| Nr. | Switch    | Description     | Example |
|----:|-----------|-----------------|---------|
| 1.  | ma        | Major number    | `-ma=1` |
| 2.  | mi        | Minor number    | `-mi=2` |
| 3.  | b         | Build number    | `-b=4`  |
| 4.  | r         | Revision number | `-r=12` |
| 5.  | f         | Assembly file path | `-f="C:\Repos\Application\Properties\AssemblyInfo.cs` |
| 6.  | /         | You can ship a complete version number without any switch. | `1.2.4.12` |
| 7.  | vf        | Version format. For more information see: [Version format](#version-format). The default value is `3` when nothing is set| `-vf=3` |

**Example**
1. Assembly file and complete version

    ```
    VersionChanger.exe -f="C:\Repos\Application\Properties\AssemblyInfo.cs" 1.2.4.12
    ```

2. Assembly file and major and minor number

    ```
    VersionChanger.exe -f="C:\Repos\Application\Properties\AssemblyInfo.cs" -ma=1 -mi=2
    ```

3. Major, minor, build and revision number

    ```
    VersionChanger.exe -ma=1 -mi=2 -b=4 -r=12
    ```

### Version format
The following formats are supported:

| Id | Description | Example |
|---:|-------------|---------|
| 1  | Short format with only two places. | `1.2` |
| 2  | Middle format with three places. | `1.2.3` |
| 3  | Long format with four places (default value) | `1.2.3.4` |

> **Note**: If you provide the version number without any switch like this `1.2.4.12` it will be parsed automatically by the [`Version.Parse`-Method](https://docs.microsoft.com/en-us/dotnet/api/system.version.parse?view=netframework-4.8). See the examples of the `Parse`-Method for more information.

## Installation
If you want to install the *VersionChanger* as a pre-build event you have to do the following:
1. Copy the executable of the *VersionChanger* to your desired project (I've copied it into the folder where the solution file is located).
2. *Right click on the project > Properties*
3. Select the tab *Build Events*
4. Add the following: `$(ProjectDir)VersionChanger.exe`

As soon as you compile the project now, the version number will change.

**Build output**
```
1>------ Rebuild All started: Project: Clock, Configuration: Release Any CPU ------
1>  No version number was specified. Number is generated
1>  Version numbers:
1>  	- Current version: 19.37.3.1250
1>  	- New version....: 19.37.4.1252
1>  Version updated.
1>  Clock -> D:\Repo\Clock\bin\Release\Clock.exe
========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
```

### Conditions
If you don't want the the version number is changed with every build, you can add some conditions to the pre-build event. For example, the number should only be changed when you compile the project as release:

```batch
if $(ConfigurationName) == Release (
    $(ProjectDir)VersionChanger.exe
)
```

> **Note** The command will be interpreted line-by-line the same way as a DOS batch file, so it's important to place the opening **"("** in the same line as the **if** statement! <br /><br />
Special thanks to [Laszlo Pinter](http://pinter.org/archives/1348) for his article about pre-/post-build events



But sometimes you want to build your project as release without changing the version number. For this case I have created the configuration *PublishRelease* for me.

A new configuration can be created under *Build > Configuration Manager...*:

1. Create a new configuration

   ![001](images/001.png)

2. Enter the desired name and select *Release* at *Copy settings from*

   ![002](images/002.png)

3. Click OK

Now you can add your configuration name to the pre-build event.

**Example**
- Build with configuration *Release*:

    ```
    1>------ Rebuild All started: Project: Clock, Configuration: Release Any CPU ------
    1>  Clock -> D:\Repo\Clock\bin\Release\Clock.exe
    ========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
    ```

- Build with my custom configuration *PublishRelease*

    ````
    1>------ Rebuild All started: Project: Clock, Configuration: PublishRelease Any CPU ------
    1>  No version number was specified. Number is generated
    1>  Version numbers:
    1>  	- Current version: 19.37.4.1252
    1>  	- New version....: 19.37.5.1273
    1>  Version updated.
    1>  Clock -> D:\Repos\Clock\bin\PublishRelease\Clock.exe
    ========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
    ```

## .NET Core version
The .NET core version is still in development but has the same features as the .NET Framework application