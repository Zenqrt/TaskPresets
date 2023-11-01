module TaskViews

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Animation
open Avalonia.Input
open Avalonia.Threading
open Tasks

let unimplemented _ =
    printfn "This function is not implemented."

let private taskPresetsSidebarView (selectedPreset: IWritable<TaskPreset option>) =
    ListBox.create [
        Grid.column 0
        ListBox.horizontalAlignment HorizontalAlignment.Left
        ListBox.dock Dock.Left
        ListBox.width 300
        ListBox.dataItems taskPresets
        ListBox.contextMenu (ContextMenu.create [ ContextMenu.dataItems [ "New Task Preset" ] ])

        ListBox.itemTemplate (
            DataTemplateView<TaskPreset>.create (fun taskPreset ->
                DockPanel.create [
                    DockPanel.contextMenu (
                        ContextMenu.create [
                            ContextMenu.dataItems [
                                "Export Task Preset"
                                "Rename"
                                "Delete"
                            ]
                        ]
                    )
                    DockPanel.children [ TextBlock.create [ TextBlock.text taskPreset.Name ] ]
                ])
        )

        match selectedPreset.Current with
        | None -> ()
        | Some taskPreset -> ListBox.selectedItem taskPreset

        ListBox.onSelectedItemChanged (fun item ->
            match item with
            | :? TaskPreset as taskPreset ->
                if
                    selectedPreset.Current.IsNone
                    || (selectedPreset.Current.IsSome && selectedPreset.Current.Value <> taskPreset)
                then
                    selectedPreset.Set(Some taskPreset)
            | _ -> ())

    ]

let private updateTaskPreset
    (selectedPresetState: IWritable<TaskPreset option>)
    (newTaskPresetFunc: TaskPreset -> TaskPreset)
    =
    let oldTaskPreset = selectedPresetState.Current.Value
    let newTaskPreset = newTaskPresetFunc oldTaskPreset
    replaceTaskPreset oldTaskPreset newTaskPreset
    selectedPresetState.Set(Some newTaskPreset)

let private settingsView (title: string) (view: Types.IView) =
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

let private generalSettingsView (selectedPresetState: IWritable<TaskPreset option>) =
    settingsView
        "General"
        (StackPanel.create [
            let taskPreset = selectedPresetState.Current.Value
            StackPanel.spacing 5

            StackPanel.children [
                ToggleSwitch.create [
                    ToggleSwitch.content "Close other applications"
                    ToggleSwitch.isChecked taskPreset.CloseOtherApplications
                    ToggleSwitch.onChecked (fun _ ->
                        updateTaskPreset selectedPresetState (fun taskPreset -> {
                            taskPreset with
                                CloseOtherApplications = true
                        }))
                    ToggleSwitch.onUnchecked (fun _ ->
                        updateTaskPreset selectedPresetState (fun taskPreset -> {
                            taskPreset with
                                CloseOtherApplications = false
                        }))

                ]

                ToggleSwitch.create [
                    ToggleSwitch.content "Disable opening of other applications"
                    ToggleSwitch.isChecked taskPreset.DisableOpeningOtherApplications
                    ToggleSwitch.onChecked (fun _ ->
                        updateTaskPreset selectedPresetState (fun taskPreset -> {
                            taskPreset with
                                DisableOpeningOtherApplications = true
                        }))
                    ToggleSwitch.onUnchecked (fun _ ->
                        updateTaskPreset selectedPresetState (fun taskPreset -> {
                            taskPreset with
                                DisableOpeningOtherApplications = false
                        }))
                ]

                Button.create [
                    Button.margin (0, 25)
                    Button.content "Schedule a time to activate"
                    Button.onClick unimplemented
                ]

            ]
        ])

let private applicationListView () =
    WrapPanel.create [
        WrapPanel.horizontalAlignment HorizontalAlignment.Left
        WrapPanel.children [
            Views.iconButtonWithLabelView
                "Add App"
                (TextBlock.create [
                    TextBlock.fontSize 36
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (0, 0, 0, 8)
                    TextBlock.text "+"
                ])
                (fun _ -> ignore (ApplicationViews.applicationSelectionWindow ()))
        ]
    ]

let private exceptionApplicationsView
    (selectedPresetState: IWritable<TaskPreset option>)
    (taskPreset: TaskPreset)
    (hideNotifications: IWritable<bool>)
    =
    StackPanel.create [
        StackPanel.isVisible hideNotifications.Current
        StackPanel.spacing 10

        StackPanel.children [
            ComboBox.create [
                ComboBox.dataItems [
                    Silence
                    Allow
                ]
                ComboBox.itemTemplate (
                    DataTemplateView<NotificationExceptionType>.create (fun notificationExceptionType ->
                        TextBlock.create [
                            TextBlock.text (
                                match notificationExceptionType with
                                | Silence -> "Silence notifications from"
                                | Allow -> "Allow notifications from"
                            )
                        ])
                )
                ComboBox.onSelectedItemChanged (fun object ->
                    match object with
                    | :? NotificationExceptionType as notificationExceptionType ->
                        if notificationExceptionType <> taskPreset.NotificationExceptionType then
                            updateTaskPreset selectedPresetState (fun taskPreset -> {
                                taskPreset with
                                    NotificationExceptionType = notificationExceptionType
                            })
                    | _ -> ())

                ComboBox.selectedItem taskPreset.NotificationExceptionType
            ]

            StackPanel.create [
                StackPanel.spacing 5

                StackPanel.children [
                    TextBlock.create [
                        TextBlock.fontSize 18
                        TextBlock.text "Applications"
                    ]

                    applicationListView ()
                ]
            ]
        ]
    ]

let private notificationSettingsView (selectedPresetState: IWritable<TaskPreset option>) =
    settingsView
        "Notifications"
        (Component.create (
            "notificationsView",
            fun context ->
                let hideNotificationsState = context.useState false

                StackPanel.create [
                    let taskPreset = selectedPresetState.Current.Value
                    hideNotificationsState.Set taskPreset.HideNotifications

                    StackPanel.spacing 25

                    StackPanel.children [
                        ToggleSwitch.create [
                            ToggleSwitch.content "Hide Notifications"
                            ToggleSwitch.isChecked taskPreset.HideNotifications

                            ToggleSwitch.onChecked (fun _ ->
                                hideNotificationsState.Set true

                                updateTaskPreset selectedPresetState (fun taskPreset -> {
                                    taskPreset with
                                        HideNotifications = true
                                }))
                            ToggleSwitch.onUnchecked (fun _ ->
                                updateTaskPreset selectedPresetState (fun taskPreset -> {
                                    taskPreset with
                                        HideNotifications = false
                                })

                                hideNotificationsState.Set false)
                        ]

                        exceptionApplicationsView selectedPresetState taskPreset hideNotificationsState
                    ]
                ]
        ))

let private taskListView (selectedPresetState: IWritable<TaskPreset option>) =
    settingsView
        "Tasks"
        (StackPanel.create [
            StackPanel.spacing 5

            StackPanel.children [
                StackPanel.create [
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.spacing 15

                    StackPanel.children [
                        Button.create [
                            Button.content "Add Task"
                            Button.flyout (
                                Flyout.create [
                                    Flyout.content (
                                        StackPanel.create [
                                            StackPanel.children [
                                                Popup.create []
                                                for name in
                                                    [
                                                        "Browser"
                                                        "Command Prompt"
                                                        "System Executable"
                                                    ] do
                                                    Button.create [
                                                        Button.width 150
                                                        Button.cornerRadius 0
                                                        Button.content name
                                                        Button.background Media.Brushes.Transparent

                                                        Button.onClick (fun _ ->
                                                            let taskType =
                                                                match name with
                                                                | "Browser" -> Browser {| Urls = [] |}
                                                                | "Command Prompt" -> CommandPrompt {| Commands = [] |}
                                                                | _ -> SystemExecutable {| Path = "" |}

                                                            updateTaskPreset selectedPresetState (fun taskPreset -> {
                                                                taskPreset with
                                                                    Tasks =
                                                                        taskPreset.Tasks
                                                                        @ [
                                                                            {
                                                                                Name = name + " Task"
                                                                                TaskType = taskType
                                                                            }
                                                                        ]
                                                            })


                                                        )
                                                    ]
                                            ]
                                        ]

                                    )
                                ]
                            )
                        ]
                        Button.create [
                            Button.content "Clear all Tasks"
                            Button.onClick unimplemented
                        ]
                    ]
                ]
                ListBox.create [
                    let taskPreset = selectedPresetState.Current.Value
                    ListBox.cornerRadius 10
                    ListBox.dataItems taskPreset.Tasks

                    ListBox.itemTemplate (
                        DataTemplateView<Task>.create (fun task ->
                            DockPanel.create [
                                DockPanel.children [
                                    TextBlock.create [
                                        TextBlock.text task.Name
                                        TextBlock.verticalAlignment VerticalAlignment.Center
                                    ]
                                    Button.create [
                                        Button.content "X"
                                        Button.horizontalAlignment HorizontalAlignment.Right
                                        Button.background Media.Brushes.Transparent
                                    ]
                                ]
                            ])
                    )
                ]
            ]
        ])

let private taskPresetsInformationView (selectedPresetState: IWritable<TaskPreset option>) =
    let taskPreset = selectedPresetState.Current.Value

    DockPanel.create [
        Grid.column 2
        DockPanel.children [
            ScrollViewer.create [
                ScrollViewer.content (
                    StackPanel.create [
                        StackPanel.margin (0, 0, 0, 50)
                        StackPanel.width 550
                        StackPanel.spacing 25

                        StackPanel.children [
                            TextBlock.create [
                                TextBlock.fontSize 32
                                TextBlock.text taskPreset.Name
                                TextBlock.horizontalAlignment HorizontalAlignment.Center
                            ]

                            generalSettingsView selectedPresetState
                            notificationSettingsView selectedPresetState
                            taskListView selectedPresetState
                        ]
                    ]
                )
            ]
        ]
    ]

let private emptyTaskPresetInformationView =
    DockPanel.create [
        Grid.column 2
        DockPanel.children [
            TextBlock.create [
                TextBlock.horizontalAlignment HorizontalAlignment.Center
                TextBlock.verticalAlignment VerticalAlignment.Center
                TextBlock.text "Select a task preset to edit its information."
            ]
        ]
    ]


let taskPresetView (selectedPreset: IWritable<TaskPreset option>) =
    DockPanel.create [
        DockPanel.children [
            taskPresetsSidebarView selectedPreset
            match selectedPreset.Current with
            | None -> emptyTaskPresetInformationView
            | Some _ -> taskPresetsInformationView selectedPreset
        ]
    ]
