import os
import json

from dataclasses import dataclass


class Task:
    name: str

    def start_up(self):
        pass


class BrowserTask(Task):
    def __init__(self, name: str, urls: list[str]):
        super().__init__()

        self.name = name
        self.urls = urls

    def start_up(self):
        for url in self.urls:
            os.system(f"start {url}")


class CommandPromptTask(Task):
    def __init__(self, name: str, commands: list[str]):
        super().__init__()

        self.name = name
        self.commands = commands

    def start_up(self):
        for command in self.commands:
            os.system(f"start cmd /k {command}")


class SystemExecutableTask(Task):
    def __init__(self, name: str, executable_path: str):
        super().__init__()

        self.name = name
        self.executable_path = executable_path

    def start_up(self):
        os.system(f"start {self.executable_path}")


class TaskEncoder(json.JSONEncoder):
    def default(self, o):
        return o.__dict__


@dataclass
class TaskPreset:
    name: str
    tasks: list[Task]
    focus_mode: bool = False
    close_every_task: str = "never"

    def start_up(self):
        """
        Starts up the task preset. This will start up all the tasks in the task preset.
        If the focus mode is enabled, it will turn off desktop notifications.
        """
        for task in self.tasks:
            task.start_up()

        if self.focus_mode:
            self.focus()

    def set_name(self, text: str):
        """Sets the name of the task preset."""
        self.name = text

    def set_focus_mode(self, status: bool):
        """Sets the focus mode of the task preset."""
        self.focus_mode = status

    def set_close_every_task(self, option: str):
        """Sets the close every task option of the task preset."""
        self.close_every_task = option

    def add_task(self, task: Task):
        """Adds a task to the task preset."""
        self.tasks.append(task)

    def remove_task_by_name(self, task_name: str):
        """
        Removes a task from the task preset by its name.
        If the task does not exist, it will raise a TaskManagerError.
        """
        for task in self.tasks:
            if task.name == task_name:
                self.tasks.remove(task)
                return

        raise TaskManagerError(f"Task with name {task_name} does not exist!")

    def focus(self):
        pass

    def encode(self):
        """Encodes the task preset into a dictionary."""
        return self.__dict__

    @staticmethod
    def decode(json_data: dict):
        """Decodes the task preset from a dictionary."""
        return TaskPreset(name=json_data["name"],
                          tasks=[Task(**task) for task in json_data["tasks"]],
                          focus_mode=json_data["focus_mode"],
                          close_every_task=json_data["close_every_task"])


class TaskManagerError(Exception):

    def __init__(self, message: str):
        super().__init__(message)


class TaskManager:
    def __init__(self, file_path: str):
        self.json_file_path = file_path
        self.task_presets: list[TaskPreset] = []
        self.load_task_presets_or_empty()

    def load_task_presets_or_empty(self):
        """
        Loads the task presets from the JSON file.
        If the JSON file does not exist, it will create a new one.
        """
        if os.path.exists(self.json_file_path):
            with open(self.json_file_path, "r") as file:
                self.task_presets = [TaskPreset.decode(task_preset) for task_preset in json.load(file)]
        else:
            self.task_presets = []

    def save_task_presets(self):
        """Saves the task presets to the JSON file."""
        with open(self.json_file_path, "w") as file:
            json.dump([task_preset.__dict__ for task_preset in self.task_presets], file, cls=TaskEncoder)

    def add_task_preset(self, task_preset: TaskPreset):
        """Adds a task preset to the task presets."""
        self.task_presets.append(task_preset)
        self.save_task_presets()

    def remove_task_preset(self, task_preset: TaskPreset):
        """Removes a task preset from the task presets."""
        self.task_presets.remove(task_preset)
        self.save_task_presets()

    def get_task_preset_by_name(self, task_name: str):
        """
        Gets a task preset from the task presets by its name.
        If it does not exist, it will raise a TaskManagerError.
         """
        for task in self.task_presets:
            if task.name == task_name:
                return task

        raise TaskManagerError(f"Task preset with name {task_name} does not exist!")
