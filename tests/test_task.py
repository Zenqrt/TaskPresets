from unittest import TestCase

from task import BrowserTask


class TestBrowserTask(TestCase):
    def test__start_default_browser(self):
        browser_task = BrowserTask("google", "https://www.google.com")
        self.assertEqual(browser_task.execution_command, "start https://www.google.com")
