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


let fileMenu =
    MenuItem.create [
        MenuItem.header "File"

        MenuItem.viewItems [
            fileMenuItemWithHotKey "New Task Preset" (KeyGesture(Key.N, KeyModifiers.Control)) unimplemented
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
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
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
