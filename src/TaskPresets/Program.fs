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
open Avalonia.Threading

let unimplemented _ =
    printfn "This function is not implemented."

let shutdown exitCode =
    match Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.Shutdown exitCode
    | _ -> ()

let private fileMenuItemWithHotKey
    (header: string)
    (key: KeyGesture)
    (func: Interactivity.RoutedEventArgs -> unit)
    =
    MenuItem.create [
        MenuItem.header header
        MenuItem.hotKey key
        MenuItem.onClick func
    ]

let private fileMenuItem (header: string) (func: Interactivity.RoutedEventArgs -> unit) =
    MenuItem.create [
        MenuItem.header header
        MenuItem.onClick func
    ]

type NewTaskPresetResult =
    | Cancelled
    | Success of {| TaskPreset: TaskPreset |}

let private newTaskPresetView (window: Window) =
    Component(fun context ->
        let taskPresetName = context.useState ""

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
                                    TextBox.onTextChanged taskPresetName.Set
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

                                    Button.onClick (fun _ ->
                                        window.Close(
                                            Success {|
                                                TaskPreset = {
                                                    Name = taskPresetName.Current
                                                    CloseOtherApplications = false
                                                    DisableOpeningOtherApplications = false
                                                    HideNotifications = false
                                                    NotificationExceptionType = Allow
                                                    NotificationExceptionApplications = []
                                                    Tasks = []
                                                }
                                            |}
                                        ))
                                ]
                                Button.create [
                                    Button.width 100
                                    Button.horizontalAlignment HorizontalAlignment.Right
                                    Button.content "Cancel"

                                    Button.onClick (fun _ -> window.Close Cancelled)
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ])

let private newTaskPresetWindow () =
    let window = Window()
    window.Title <- "New Task Preset"
    window.CanResize <- false
    window.Width <- 300
    window.Height <- 150
    window.Content <- newTaskPresetView window

    match Views.mainWindow with
    | None -> null
    | Some mainWindow -> window.ShowDialog<NewTaskPresetResult> mainWindow

let private handleNewTaskPreset () : Async<NewTaskPresetResult> =
    Dispatcher.UIThread.InvokeAsync<NewTaskPresetResult>(
        Func<Threading.Tasks.Task<NewTaskPresetResult>>(fun _ -> task { return! newTaskPresetWindow () })
    )
    |> Async.AwaitTask

let private fileMenu (selectedPreset: IWritable<TaskPreset option>) =
    MenuItem.create [
        MenuItem.header "File"

        MenuItem.viewItems [
            fileMenuItemWithHotKey "New Task Preset" (KeyGesture(Key.N, KeyModifiers.Control)) (fun _ ->
                handleNewTaskPreset ()
                |> Async.map (fun result ->
                    match result with
                    | Cancelled -> ()
                    | Success result ->
                        let taskPreset = result.TaskPreset

                        addPreset taskPreset
                        selectedPreset.Set(Some taskPreset))
                |> Async.Start)
            fileMenuItemWithHotKey "Save Task Preset" (KeyGesture(Key.S, KeyModifiers.Control)) unimplemented
            fileMenuItem "Import Task Presets" unimplemented
            fileMenuItem "Export Task Presets" unimplemented
            fileMenuItem "Exit" (fun _ -> shutdown 0)
        ]
    ]

let private menuView (selectedPreset: IWritable<TaskPreset option>) =
    Menu.create [
        Menu.dock Dock.Top

        Menu.viewItems [ fileMenu selectedPreset ]
    ]

let private view () =
    Component(fun context ->
        let selectedPresetState: IWritable<TaskPreset option> = context.useState None

        DockPanel.create [
            DockPanel.children [
                menuView selectedPresetState
                TaskViews.taskPresetView selectedPresetState
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
