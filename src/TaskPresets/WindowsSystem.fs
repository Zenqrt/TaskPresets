module WindowsSystem

open Microsoft.Win32

let installedApplications: RegistryKey list =
    let registryKey =
        Registry.LocalMachine.OpenSubKey @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"

    let nameList = registryKey.GetSubKeyNames()

    [ for name in nameList -> registryKey.OpenSubKey name ]

let iterateInstalledApplications (func: RegistryKey -> unit) =
    let registryKey = Registry.ClassesRoot.OpenSubKey @"Installer\Products"

    let nameList = registryKey.GetSubKeyNames()

    for name in nameList do
        let subKey = registryKey.OpenSubKey name
        func subKey
