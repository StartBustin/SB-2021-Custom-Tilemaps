using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(CustomTilemaps.BuildInfo.Description)]
[assembly: AssemblyDescription(CustomTilemaps.BuildInfo.Description)]
[assembly: AssemblyCompany(CustomTilemaps.BuildInfo.Company)]
[assembly: AssemblyProduct(CustomTilemaps.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + CustomTilemaps.BuildInfo.Author)]
[assembly: AssemblyTrademark(CustomTilemaps.BuildInfo.Company)]
[assembly: AssemblyVersion(CustomTilemaps.BuildInfo.Version)]
[assembly: AssemblyFileVersion(CustomTilemaps.BuildInfo.Version)]
[assembly: MelonInfo(typeof(CustomTilemaps.CustomTilemaps), CustomTilemaps.BuildInfo.Name, CustomTilemaps.BuildInfo.Version, CustomTilemaps.BuildInfo.Author, CustomTilemaps.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]