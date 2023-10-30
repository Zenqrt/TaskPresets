module Views

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open System
open System.IO
open Avalonia.Animation
open Avalonia.Input

let iconButtonWithLabelView (label: string) (icon: Types.IView) (func: Interactivity.RoutedEventArgs -> unit) =
    StackPanel.create [
        StackPanel.width 75
        StackPanel.height 85

        StackPanel.children [
            Button.create [
                Button.dock Dock.Top
                Button.content (icon)
                Button.width 75
                Button.height 75
                Button.cornerRadius 10
                Button.onClick func
            ]
            TextBlock.create [
                TextBlock.textAlignment Media.TextAlignment.Center
                TextBlock.text (if label.Length > 8 then label.Remove 8 + "..." else label)
            ]
        ]
    ]

let private applicationSelectionView =
    Component(fun _ ->
        ScrollViewer.create [
            ScrollViewer.content (
                WrapPanel.create [
                    WrapPanel.horizontalAlignment HorizontalAlignment.Left
                    WrapPanel.verticalAlignment VerticalAlignment.Top
                    WrapPanel.orientation Orientation.Horizontal
                    WrapPanel.margin (25, 0)

                    WrapPanel.children [
                        iconButtonWithLabelView "Choose" (TextBlock.create [ TextBlock.text "..." ]) (fun _ ->
                            printfn "")

                        for registryKey in WindowsSystem.installedApplications do
                            let displayName = registryKey.GetValue "DisplayName"
                            let installLocation = registryKey.GetValue "InstallLocation"
                            let displayIcon = registryKey.GetValue "DisplayIcon"

                            if (displayName <> null && displayIcon <> null) then
                                let displayIconPath = displayIcon.ToString().Replace("\"", "").Split(",")[0]

                                iconButtonWithLabelView
                                    (displayName.ToString())
                                    (Image.create [
                                        let icon =
                                            try
                                                Drawing.Icon.ExtractAssociatedIcon(displayIconPath)
                                            with _ ->
                                                null

                                        if icon <> null then
                                            let bitmap = icon.ToBitmap()

                                            let tempIconPath = Path.GetTempPath() + Guid.NewGuid.ToString() + ".png"
                                            bitmap.Save(tempIconPath, Drawing.Imaging.ImageFormat.Png)

                                            Image.source (new Media.Imaging.Bitmap(tempIconPath))
                                            Image.width 37
                                            Image.height 37
                                            File.Delete tempIconPath
                                    ])
                                    (fun _ -> printfn "")
                    ]
                ]
            )
        ])

let applicationSelectionWindow =
    let window = Window()
    window.Title <- "Choose an application"
    window.CanResize <- false
    window.Width <- 800
    window.Height <- 500
    window.Content <- applicationSelectionView

    match Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as applicationLifeTime ->
        window.ShowDialog applicationLifeTime.MainWindow
    | _ -> null
