import window
import sys

from PyQt5.QtWidgets import QApplication


file_path = "task_presets.json"

app = QApplication(sys.argv)
window = window.Window()
sys.exit(app.exec_())
