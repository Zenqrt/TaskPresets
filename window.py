import traceback
import warnings

from PyQt5 import QtGui
from PyQt5.QtWidgets import *
from PyQt5.QtGui import *
from PyQt5.QtCore import *


def unimplemented():
    warnings.warn("This function is not implemented yet.")


class Window(QMainWindow):
    def __init__(self):
        super().__init__()

        self.setWindowTitle("Task Presets")
        self.setMinimumSize(800, 500)

        self._setup_widgets()

        self.show()

    def _setup_widgets(self):
        self._setup_menu_bar()
        self._setup_main_grid()

    def _setup_menu_bar(self):
        menu_bar = QMenuBar(self)

        file_menu = menu_bar.addMenu("File")

        self._add_menu_action_shortcut(file_menu, "New Task Preset", "Ctrl+N", "Create a new task preset.", unimplemented)
        self._add_menu_action_shortcut(file_menu, "Save Task Preset", "Ctrl+S", "Save the current task preset.", unimplemented)
        self._add_menu_action(file_menu, "Import Task Presets", "Imports a task presets file.", unimplemented)
        self._add_menu_action(file_menu, "Export Task Presets", "Exports a task presets file.", unimplemented)
        self._add_menu_action(file_menu, "Exit", "Exit the application.", self.close)

        self.setMenuBar(menu_bar)

    def _add_menu_action_shortcut(self, menu: QMenu, action_name: str, shortcut: str, status_tip: str, action_function: callable):
        action = QAction(action_name, self)
        action.setShortcut(shortcut)
        action.setStatusTip(status_tip)
        action.triggered.connect(action_function)
        menu.addAction(action)

    def _add_menu_action(self, menu: QMenu, action_name: str, status_tip: str, action_function: callable):
        action = QAction(action_name, self)
        action.setStatusTip(status_tip)
        action.triggered.connect(action_function)
        menu.addAction(action)

    def _setup_main_grid(self):
        layout = QVBoxLayout(self)
        layout.setAlignment(Qt.AlignTop)

        self._setup_task_presets_list(layout)
        self._setup_task_preset_details(layout)

    def _setup_task_presets_list(self, layout: QVBoxLayout):
        self.operations_box = QGroupBox("Operations", self)
        self.operations_box.setMinimumWidth(200)
        self.operations_box_layout = QVBoxLayout(self.operations_box)
        self.operations_box_layout.setAlignment(Qt.AlignTop)
        self.operations_box_layout.setContentsMargins(0, 0, 0, 0)
        self.operations_box.setLayout(self.operations_box_layout)
        layout.addWidget(self.operations_box, alignment=Qt.AlignTop)

        self.task_presets_list = QListWidget(self)
        self.task_presets_list.setMinimumWidth(200)
        self.task_presets_list.itemSelectionChanged.connect(unimplemented)
        layout.addWidget(self.task_presets_list, alignment=Qt.AlignBottom)

    def _setup_task_preset_details(self, layout: QGridLayout):
        pass

