module WindowsSystem

open System.Drawing
open Microsoft.Win32
open Tasks

let applicationFromRegistryKey (registryKey: RegistryKey) =
    let displayName = registryKey.GetValue "DisplayName"
    let displayIcon = registryKey.GetValue "DisplayIcon"

    if displayName <> null && displayIcon <> null then
        let displayNameString = displayName.ToString()
        let displayIconPath = displayIcon.ToString().Replace("\"", "").Split(",")[0]

        let iconBitmap =
            try
                Some(Icon.ExtractAssociatedIcon(displayIconPath).ToBitmap())
            with _ ->
                None

        Some {
            Name = displayNameString
            InstallationPath = ""
            IconBitmap = iconBitmap
        }
    else
        None


let installedApplications: Application list =
    let registryKey =
        Registry.LocalMachine.OpenSubKey @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"

    registryKey.GetSubKeyNames()
    |> Seq.map registryKey.OpenSubKey
    |> Seq.choose applicationFromRegistryKey
    |> Seq.toList
