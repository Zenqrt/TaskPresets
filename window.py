from time import sleep

from PyQt5 import QtGui
from PyQt5.QtWidgets import *
from PyQt5.QtGui import *
from PyQt5.QtCore import *

import task
from task import *


# TODO: add settings window
# TODO: add menu bar with settings button
def _create_settings_header_panel(selected_task_preset_settings_panel: QWidget):
    header_panel = QWidget(selected_task_preset_settings_panel)
    header_panel_layout = QHBoxLayout(header_panel)

    header_panel_layout.setDirection(QBoxLayout.LeftToRight)

    # create information label
    information_label = QLabel(
        header_panel)
    information_label.setText("Task Preset Information")
    information_label.setAlignment(Qt.AlignLeft)

    header_panel_layout.addWidget(information_label)

    return header_panel


def _toggle_read_only(widget: QWidget):
    new_state = not widget.isReadOnly()
    widget.setReadOnly(new_state)

    if new_state:
        widget.setStyleSheet("background-color: rgb(230, 230, 230);")
    else:
        widget.setStyleSheet("background-color: rgb(255, 255, 255);")


def _create_option_section(panel: QWidget, name: str, value: str, function: callable):
    focus_mode_option = QComboBox(panel)
    focus_mode_option.addItems(["Never", "Always", "Always ask"])
    focus_mode_option.setCurrentIndex(focus_mode_option.findText(value.capitalize()))
    focus_mode_option.setFixedSize(200, 20)
    focus_mode_option.currentTextChanged.connect(lambda: function(focus_mode_option.currentText()))

    focus_mode_setting_section = _create_setting_section(panel, name, [focus_mode_option])

    return focus_mode_setting_section


def _create_setting_section(panel: QWidget, name: str, setting_widgets: list[QWidget]):
    setting_section = QWidget(panel)

    setting_section_layout = QVBoxLayout(panel)
    setting_section_layout.setDirection(QBoxLayout.TopToBottom)

    header_label = QLabel(panel)
    header_label.setText(name)

    header_layout = QHBoxLayout(panel)
    header_layout.setDirection(QBoxLayout.LeftToRight)
    header_layout.addWidget(header_label, alignment=Qt.AlignLeft)
    header_layout.addSpacing(10)

    setting_section_layout.addLayout(header_layout)

    for setting_widget in setting_widgets:
        setting_section_layout.addWidget(setting_widget)

    setting_section.setLayout(setting_section_layout)

    return setting_section


class Window(QMainWindow):

    def __init__(self, file_path: str):
        super().__init__()

        self.setWindowTitle("Task Presets")
        self.setFixedSize(800, 500)

        self.task_manager = TaskManager(file_path)
        self.selected_task_preset: TaskPreset = self.task_manager.task_presets[0]

        self.menu_bar = self._create_menu_bar()

        self.operation_box = self._create_operation_box()
        self.create_task_preset_button = self._create_operation_button(function=self._on_click_create_task_preset_button,
                                                                       name="+")
        self.remove_task_preset_button = self._create_operation_button(function=self._on_click_remove_task_preset_button,
                                                                       name="-", horizontal_position=20)
        self.copy_task_preset_button = self._create_operation_button(function=lambda: print("clicked"),
                                                                     name="O", horizontal_position=40)
        self.task_presets_list = self._create_task_presets_list()

        self.selected_task_preset_settings_panel: TaskPresetEditPanel = None
        self.selected_item_list: QListWidgetItem = None
        self.show()

    def closeEvent(self, event: QtGui.QCloseEvent) -> None:
        event.ignore()
        self.task_manager.save_task_presets()
        event.accept()

    def _on_click_create_task_preset_button(self):
        dialog = QInputDialog(self)
        dialog.setWindowTitle("New Task Preset")
        dialog.setLabelText("Enter task preset name:")
        dialog.setFixedSize(300, 100)
        dialog.exec_()

        if dialog.result() == 0:
            return

        task_preset_name = dialog.textValue()

        if task_preset_name != "":
            task_preset = TaskPreset(task_preset_name, [])
            self.task_presets_list.addItem(task_preset_name)
            self.task_manager.task_presets.append(task_preset)
            self.task_manager.save_task_presets()

            self._select_task_preset(self.task_presets_list.itemFromIndex(self.task_presets_list.model().index(self.task_presets_list.count() - 1, 0)))

    def _on_click_remove_task_preset_button(self):
        item = self.task_presets_list.currentItem()
        task_preset = self.task_manager.get_task_preset_by_name(item.text())

        confirm_dialog = self._create_confirmation_dialog("Delete Task Preset", f"Are you sure you want to delete the '{item.text()}' task preset?")

        if confirm_dialog.result() == QMessageBox.Yes:
            self._delete_settings_panel_if_exists()
            self.selected_task_preset_settings_panel = None

            self.task_manager.remove_task_preset(task_preset)
            self.task_presets_list.takeItem(self.task_presets_list.row(item))
            self.task_manager.save_task_presets()

    def _create_confirmation_dialog(self, title: str, text: str) -> QMessageBox:
        confirm_dialog = QMessageBox(self)
        confirm_dialog.setWindowTitle(title)
        confirm_dialog.setText(text)
        confirm_dialog.setStandardButtons(QMessageBox.Yes | QMessageBox.No)
        confirm_dialog.setDefaultButton(QMessageBox.No)
        confirm_dialog.setFixedSize(300, 100)
        confirm_dialog.exec_()

        return confirm_dialog

    def _create_menu_bar(self):
        menu_bar = QMenuBar(self)
        menu_bar.setGeometry(0, 0, 800, 20)

        # create settings menu
        file_menu = menu_bar.addMenu("&File")

        # create settings menu actions
        settings_menu_action = QAction("Settings", self)
        settings_menu_action.triggered.connect(lambda: print("clicked"))
        file_menu.addAction(settings_menu_action)

        return menu_bar

    def _create_operation_button(self, function: callable, name: str = "", icon: QIcon = None,
                                 horizontal_position: int = 0):
        operation_button = QPushButton(name, self)

        if icon is not None:
            operation_button.setIcon(icon)
            operation_button.setIconSize(QSize(20, 20))

        operation_button.setGeometry(horizontal_position, 20, 20, 20)
        operation_button.clicked.connect(function)

        return operation_button

    def _create_operation_box(self):
        operation_box = QGroupBox(self)
        operation_box.setGeometry(0, 20, 200, 20)

        operation_box.setStyleSheet("background-color: rgb(230, 230, 230);")

        return operation_box

    def _create_task_presets_list(self):
        task_presets_list = QListWidget(self)
        task_presets_list.setGeometry(0, 40, 200, 500)
        task_presets_list.itemDoubleClicked.connect(self._select_task_preset)

        task_presets_list.addItems([task_preset.name for task_preset in self.task_manager.task_presets])

        return task_presets_list

    def _select_task_preset(self, item: QListWidgetItem):
        task_preset = self.task_manager.get_task_preset_by_name(item.text())
        self.selected_task_preset = task_preset

        self._show_task_preset_settings_panel()
        self._set_selection_colors(item)

    def _set_selection_colors(self, item: QListWidgetItem):
        if self.selected_item_list is not None:
            self.selected_item_list.setBackground(QColor(255, 255, 255))

        item.setBackground(QColor(200, 200, 200))
        self.selected_item_list = item

    def _show_task_preset_settings_panel(self):
        self._delete_settings_panel_if_exists()
        self.selected_task_preset_settings_panel = self._create_selected_task_preset_settings_panel()

    def _delete_settings_panel_if_exists(self):
        if self.selected_task_preset_settings_panel is not None:
            self.selected_task_preset_settings_panel.deleteLater()

    def _create_selected_task_preset_settings_panel(self):
        if self.selected_task_preset is None:
            return

        widget = TaskPresetEditPanel(self, self.task_manager, self.selected_task_preset)
        widget.show()
        return widget


class TaskPresetEditPanel(QWidget):

    def __init__(self, parent: QWidget, task_manager: TaskManager, task_preset: TaskPreset):
        super().__init__(parent)

        self.task_manager = task_manager
        self.task_preset = task_preset
        self.original_name = task_preset.name
        self.name_changed = False
        self.needs_saving = False

        self.setGeometry(200, 20, 600, 500)
        self._init_ui()

    def _init_ui(self):
        self._create_settings_buttons()
        scroll_area = self._create_scroll_area()
        settings_panel = self._create_settings_panel()

        scroll_area.setWidget(settings_panel)

    def _create_scroll_area(self):
        scroll_area = QScrollArea(self)
        scroll_area.setFixedSize(600, 440)

        return scroll_area

    def _create_settings_buttons(self):
        buttons_widget = QWidget(self)
        buttons_widget.setGeometry(0, 440, 600, 40)

        buttons_layout = QHBoxLayout(buttons_widget)
        buttons_layout.setDirection(QBoxLayout.LeftToRight)
        buttons_layout.addSpacing(400)

        run_button = QPushButton("Run", buttons_widget)
        run_button.setFixedSize(80, 30)
        run_button.clicked.connect(lambda: self.task_preset.start_up())

        self.save_button = QPushButton("Save", buttons_widget)
        self.save_button.setFixedSize(80, 30)
        self.save_button.setEnabled(False)
        self.save_button.clicked.connect(self._on_click_save_button)

        buttons_layout.addWidget(self.save_button)
        buttons_layout.addWidget(run_button)

        buttons_widget.setLayout(buttons_layout)

        return buttons_widget

    def _set_needs_saving(self, status: bool):
        self.needs_saving = status
        self.save_button.setEnabled(status)

    def _on_click_save_button(self):
        self.task_manager.save_task_presets()
        self._set_needs_saving(False)

    def _create_settings_panel(self):
        settings_panel = QWidget(self)

        settings_panel.setLayout(self._create_settings_panel_layout(settings_panel))

        return settings_panel

    def _create_settings_panel_layout(self, parent: QWidget):
        layout = QVBoxLayout(parent)

        layout.addWidget(_create_settings_header_panel(parent))
        layout.addSpacing(20)

        layout.addWidget(self._create_name_setting_section())
        layout.addWidget(_create_option_section(parent, "Exit every other program?", self.task_preset.close_every_task, self._on_choose_close_every_task_option))
        layout.addWidget(self._create_focus_mode_setting_section())

        layout.addSpacing(20)
        layout.addWidget(self._create_tasks_setting_section())

        layout.addSpacing(40)

        return layout

    def _create_name_setting_section(self):
        name_input = QLineEdit(self)
        name_input.setText(self.task_preset.name)
        name_input.setFixedSize(200, 20)
        name_input.textChanged.connect(lambda: self._on_text_changed(name_input.text()))

        name_setting_section = _create_setting_section(self, "Name", [name_input])

        return name_setting_section

    def _on_choose_close_every_task_option(self, option):
        self._set_needs_saving(True)
        self.task_preset.set_close_every_task(option)

    def _on_text_changed(self, text: str):
        self._set_needs_saving(True)
        self.task_preset.set_name(text)
        self.name_changed = True

    def _create_focus_mode_setting_section(self):
        focus_mode_checkbox = QCheckBox(self)
        focus_mode_checkbox.setChecked(self.task_preset.focus_mode)
        focus_mode_checkbox.stateChanged.connect(lambda: self._on_click_focus_mode_checkbox(focus_mode_checkbox.isChecked()))

        focus_mode_setting_section = _create_setting_section(self, "Focus mode", [focus_mode_checkbox])

        return focus_mode_setting_section

    def _on_click_focus_mode_checkbox(self, checked: bool):
        self._set_needs_saving(True)
        self.task_preset.set_focus_mode(checked)

    def _create_tasks_setting_section(self):
        add_new_task_button = QPushButton(self)
        add_new_task_button.setText("Add new task")
        add_new_task_button.setFixedSize(200, 20)
        add_new_task_button.clicked.connect(lambda: print("clicked"))

        setting_widgets = [self._create_task_setting(task) for task in self.task_preset.tasks]
        setting_widgets.append(add_new_task_button)

        tasks_setting_section = _create_setting_section(self, "Tasks", setting_widgets)

        return tasks_setting_section

    def _create_task_setting(self, task: Task):
        task_setting = QWidget(self)

        execution_file_input = QLineEdit(self)

        remove_button = QPushButton(self)
        remove_button.setText("-")
        remove_button.setFixedSize(20, 20)
        remove_button.clicked.connect(lambda: self._on_click_remove_task_setting(task.name, task_setting))

        task_setting_layout = QHBoxLayout(self)
        task_setting_layout.setDirection(QBoxLayout.LeftToRight)
        task_setting_layout.addWidget(task_choice)
        task_setting_layout.addWidget(remove_button)
        task_setting.setLayout(task_setting_layout)

        return task_setting

    def _on_click_remove_task_setting(self, task_name: str, task_setting: QWidget):
        self._set_needs_saving(True)
        self.task_preset.remove_task_by_name(task_name)
        task_setting.deleteLater()
