# Edgegap Server Nakama Plugin

This plugin has been tested, and supports Unity versions 2021.3.0f1+, including all LTS releases, and Unity 6.

[Simplify integration of Nakama Game Services for deploying and managing Dedicated Servers on Edgegap.](https://github.com/edgegap/nakama-edgegap)

## Install With Git (recommended)

### Benefits

- Avoid Unity Package Manager caching bugs when updating your plugin.

### Caveats

- Requirement: functioning git client installed, for example [git-scm](https://git-scm.com/).

### Instructions

1. Open your Unity project,
2. Select toolbar option **Window** -> **Package Manager**,
3. Click the **+** icon and select **Add package from git URL...**,
4. Input the following URL `https://github.com/edgegap/edgegap-server-nakama-plugin.git`,
5. Click **Add** and wait for the Unity Package Manager to complete the installation.

## Install via ZIP archive

### Benefits

- Slightly easier as no git client is required.

### Caveats

- Installing our plugin this way will require you to manually replace plugin contents and wipe local cache if you wish to update it,
- The newtonsoft package (dependency) version required may not be compatible with your project if you're already using an older version of this package.

### Instructions

1. Select toolbar option **Window** -> **Package Manager**,
2. Click the **+** icon and select **Add package by name...**,
3. Input the name `com.unity.nuget.newtonsoft-json` and wait for the Unity Package Manager to complete the installation.,
4. Back to this github project - make sure you're on the `main` branch,
5. Click **<> Code**, then **Download ZIP**,
6. Paste the contents of the unzipped archive in your `Assets` folder within Unity project root.

## Other Sources

This is the only official distribution channel for this SDK, do not trust unverified sources!

## Plugin Usage

@todo

### Import Simple Example

1. Find this package in Unity Package Manager window.
2. Open the `Samples` tab.
3. Click on **Import** next to **Simple Handler Example**.
4. Locate sample files in your project `Assets/Samples/Edgegap Server Nakama Plugin/{version}/Simple Handler Example`.
5. Create an Empty GameObject in your scene and attach `SimpleHandlerExample.cs` script.
6. optional: configure `Authentication Token` values Inspector UI.

### Usage Requirements

To take full advantage of our hosting service, you will need to [Create an Edgegap Free Tier account](https://app.edgegap.com/auth/register).

### Troubleshooting

> Visual Studio shows `type or namespace name could not be found` for Edgegap namespace.

1. In your Unity Editor, navigate to **Edit / Preferences / External Tools / Generate .csproj files**.
2. Make sure you have enabled **Git packages**.
3. Click **Regenerate project files**.

## For Plugin Developers

This section is only for developers working on this plugin or other plugins interacting / integrating this plugin.

### CSharpier Code Frmatter

This project uses [CSharpier code formatter](https://csharpier.com/) to ensure consistent and readable formatting, configured in `/.config/dotnet-tools.json`.

See [Editor integration](https://csharpier.com/docs/Editors) for Visual Studio extensions, optionally configure `Reformat with CSharpier` on Save under Tools | Options | CSharpier | General. You may also configure [running formatting as a pre-commit git hook](https://csharpier.com/docs/Pre-commit).
