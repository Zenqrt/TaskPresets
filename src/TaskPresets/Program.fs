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

let taskPresetsSidebarView =
    ListBox.create [
        Grid.column 0
        ListBox.horizontalAlignment HorizontalAlignment.Left
        ListBox.dock Dock.Left
        ListBox.width 300
        ListBox.onPointerReleased (fun _ -> printfn "Tap")
    ]

let settingsView (title: string) (view: Types.IView) =
    StackPanel.create [
        StackPanel.spacing 10

        StackPanel.children [
            TextBlock.create [
                TextBlock.fontSize 24
                TextBlock.text title
            ]

            view
        ]
    ]

let generalSettingsView =
    settingsView
        "General"
        (ToggleSwitch.create [
            ToggleSwitch.content "Close other applications"
            ToggleSwitch.onChecked unimplemented
            ToggleSwitch.onUnchecked unimplemented
        ])

let appView (icon: Types.IView) (func: Interactivity.RoutedEventArgs -> unit) =
    Button.create [
        Button.dock Dock.Top
        Button.content (icon)
        Button.width 75
        Button.height 75
        Button.cornerRadius 10
        Button.onClick func
    ]

let notificationSettingsView =
    settingsView
        "Notifications"
        (Component.create (
            "notificationsView",
            fun context ->
                let hideNotificationsState = context.useState false

                StackPanel.create [
                    StackPanel.spacing 25
                    StackPanel.children [
                        ToggleSwitch.create [
                            ToggleSwitch.content "Hide Notifications"
                            ToggleSwitch.onChecked (fun _ -> hideNotificationsState.Set true)
                            ToggleSwitch.onUnchecked (fun _ -> hideNotificationsState.Set false)
                        ]

                        StackPanel.create [
                            StackPanel.isVisible hideNotificationsState.Current
                            StackPanel.spacing 10

                            StackPanel.children [
                                ComboBox.create [
                                    ComboBox.dataItems [
                                        "Silence Notifications From"
                                        "Allow Notifications From"
                                    ]

                                    ComboBox.selectedIndex 1
                                ]

                                StackPanel.create [
                                    StackPanel.children [
                                        TextBlock.create [
                                            TextBlock.fontSize 18
                                            TextBlock.text "Applications"
                                        ]

                                        DockPanel.create [
                                            DockPanel.horizontalAlignment HorizontalAlignment.Left
                                            DockPanel.children [
                                                Views.iconButtonWithLabelView
                                                    "Add App"
                                                    (TextBlock.create [
                                                        TextBlock.fontSize 36
                                                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                        TextBlock.verticalAlignment VerticalAlignment.Center
                                                        TextBlock.margin (0, 0, 0, 8)
                                                        TextBlock.text "+"
                                                    ])
                                                    (fun _ -> ignore Views.applicationSelectionWindow)
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
        ))


let taskPresetsInformationView =
    ScrollViewer.create [
        ScrollViewer.width 550
        ScrollViewer.horizontalAlignment HorizontalAlignment.Center
        Grid.column 2
        ScrollViewer.content (
            StackPanel.create [
                StackPanel.spacing 25

                StackPanel.children [
                    TextBlock.create [
                        TextBlock.fontSize 32
                        TextBlock.text "Title"
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]

                    generalSettingsView
                    notificationSettingsView

                ]
            ]
        )
    ]

let taskView =
    DockPanel.create [
        DockPanel.children [
            taskPresetsSidebarView
            taskPresetsInformationView
        ]
    ]

let view () =
    Component(fun _ ->
        DockPanel.create [
            DockPanel.children [
                menuView
                taskView
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
