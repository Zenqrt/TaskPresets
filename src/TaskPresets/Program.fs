module Program

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open System
open Avalonia.Animation
open Avalonia.Input
open Semi.Avalonia
open Tasks

let unimplemented _ =
    printfn "This function is not implemented."

let shutdown exitCode =
    match Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.Shutdown exitCode
    | _ -> ()

let fileMenuItemWithHotKey (header: string) (key: Input.KeyGesture) (func: Interactivity.RoutedEventArgs -> unit) =
    MenuItem.create [
        MenuItem.header header
        MenuItem.hotKey key
        MenuItem.onClick func
    ]

let fileMenuItem (header: string) (func: Interactivity.RoutedEventArgs -> unit) =
    MenuItem.create [
        MenuItem.header header
        MenuItem.onClick func
    ]

let newTaskPresetView () =
    Component(fun _ ->
        DockPanel.create [
            DockPanel.children [
                StackPanel.create [
                    StackPanel.verticalAlignment VerticalAlignment.Center
                    StackPanel.spacing 20

                    StackPanel.children [
                        StackPanel.create [
                            StackPanel.horizontalAlignment HorizontalAlignment.Center
                            StackPanel.verticalAlignment VerticalAlignment.Center

                            StackPanel.children [
                                TextBlock.create [ TextBlock.text "Enter task preset name:" ]
                                TextBox.create [
                                    TextBox.horizontalAlignment HorizontalAlignment.Center
                                    TextBox.width 200
                                ]
                            ]
                        ]
                        StackPanel.create [
                            StackPanel.orientation Orientation.Horizontal
                            StackPanel.horizontalAlignment HorizontalAlignment.Center
                            StackPanel.spacing 10

                            StackPanel.children [
                                Button.create [
                                    Button.width 100
                                    Button.horizontalAlignment HorizontalAlignment.Right
                                    Button.content "OK"
                                ]
                                Button.create [
                                    Button.width 100
                                    Button.horizontalAlignment HorizontalAlignment.Right
                                    Button.content "Cancel"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ])

let newTaskPresetWindow () =
    let window = Window()
    window.Title <- "New Task Preset"
    window.CanResize <- false
    window.Width <- 300
    window.Height <- 150
    window.Content <- newTaskPresetView ()

    match Views.mainWindow with
    | None -> null
    | Some mainWindow -> window.ShowDialog mainWindow

let fileMenu =
    MenuItem.create [
        MenuItem.header "File"

        MenuItem.viewItems [
            fileMenuItemWithHotKey "New Task Preset" (KeyGesture(Key.N, KeyModifiers.Control)) (fun _ ->
                ignore (newTaskPresetWindow ()))
            fileMenuItemWithHotKey "Save Task Preset" (KeyGesture(Key.S, KeyModifiers.Control)) unimplemented
            fileMenuItem "Import Task Presets" unimplemented
            fileMenuItem "Export Task Presets" unimplemented
            fileMenuItem "Exit" (fun _ -> shutdown 0)
        ]
    ]

let menuView =
    Menu.create [
        Menu.dock Dock.Top

        Menu.viewItems [ fileMenu ]
    ]

let view () =
    Component(fun _ ->
        DockPanel.create [
            DockPanel.children [
                menuView
                TaskViews.taskPresetView
            ]
        ])

type MainWindow() =
    inherit HostWindow()

    do
        base.Title <- "Task Presets"
        base.Content <- view ()
        base.MinWidth <- 1200
        base.MinHeight <- 800

        base.Width <- base.MinWidth
        base.Height <- base.MinHeight


type App() =
    inherit Avalonia.Application()

    override this.Initialize() =
        let theme = FluentTheme()

        let darkColorPaletteResources = ColorPaletteResources()
        darkColorPaletteResources.RegionColor <- Media.Color.FromRgb(byte 20, byte 20, byte 20)

        theme.Palettes.Add(
            Collections.Generic.KeyValuePair.Create(Styling.ThemeVariant.Dark, darkColorPaletteResources)
        )

        this.Styles.Add theme
        this.RequestedThemeVariant <- Styling.ThemeVariant.Dark

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

[<EntryPoint>]
let main (args: string[]) =
    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime(args)
