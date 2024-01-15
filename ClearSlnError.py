import os
import re
import time
import sys
import configparser
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

last_remove_time = None

def is_project_removed(solution_content, project_name):
    pattern = re.compile(rf'Project\(".*"\) = "{project_name}", "(.*)", "(.*)"\nEndProject')
    return not bool(pattern.search(solution_content))

def remove_projects_from_solution(solution_path, projects_to_remove, guids_to_remove):
    max_attempts = 5
    current_attempt = 1

    while current_attempt <= max_attempts:
        try:
            with open(solution_path, 'r', encoding='utf-8') as solution_file:
                solution_content = solution_file.read()

            # Check if the target projects have already been removed
            if all(is_project_removed(solution_content, project) for project in projects_to_remove):
                print("All target projects have already been removed. Exiting...")
                break

            for project_to_remove in projects_to_remove:
                # Build the regex pattern for matching projects and their associated configurations
                pattern = re.compile(rf'Project\(".*"\) = "{project_to_remove}", "(.*)", "(.*)"\nEndProject')

                # Find and remove matching projects and their associated configurations
                solution_content = pattern.sub('', solution_content)

            for guid_to_remove in guids_to_remove:
                # Remove the entire line containing the specified GUID
                solution_content = re.sub(rf'.*{re.escape(guid_to_remove)}.*\n', '', solution_content)

            with open(solution_path, 'w', encoding='utf-8') as solution_file:
                solution_file.write(solution_content)

            break  # Operation successful, exit the loop
        except PermissionError as e:
            print(f"PermissionError: {e}. Retrying in 2 seconds...")
            time.sleep(2)
            current_attempt += 1
    else:
        print(f"Failed after {max_attempts} attempts. Could not modify {solution_path}.")

def is_guid_removed(solution_content, guid):
    return not bool(re.search(re.escape(guid), solution_content))

class SolutionHandler(FileSystemEventHandler):
    def __init__(self, solution_path, projects_to_remove, guids_to_remove, delay_seconds):
        self.solution_path = solution_path
        self.projects_to_remove = projects_to_remove
        self.guids_to_remove = guids_to_remove
        self.delay_seconds = delay_seconds

    def on_modified(self, event):
        global last_remove_time

        # Check if it's been long enough since the last removal operation
        if last_remove_time is not None and (time.time() - last_remove_time) < self.delay_seconds:
            return

        if event.src_path.endswith('.sln'):
            print(f"Solution file {event.src_path} has been modified. Removing projects and GUIDs in {self.delay_seconds} seconds...")
            
            # Delay for the specified time before performing the removal operation
            time.sleep(self.delay_seconds)

            # Check if the targets have already been removed
            with open(self.solution_path, 'r', encoding='utf-8') as solution_file:
                solution_content = solution_file.read()

            if all(is_project_removed(solution_content, project) for project in self.projects_to_remove) and \
               all(is_guid_removed(solution_content, guid) for guid in self.guids_to_remove):
                print("All target projects and GUIDs have already been removed. Exiting...")
                return

            remove_projects_from_solution(self.solution_path, self.projects_to_remove, self.guids_to_remove)

if __name__ == "__main__":
    # Read configuration from the ini file
    config = configparser.ConfigParser()
    config_path = os.path.join(os.path.dirname(__file__), "CSEconfig.ini").replace("\\", "/")

    if os.path.exists(config_path):
        config.read(config_path)

        if 'Settings' not in config:
            print("Cannot find [Settings] tag in CSEconfig. Exiting...")
            sys.exit(0)

        projects_to_remove = config.get("Settings", "ProjectsToRemove", fallback="").strip().split(',')
        guids_to_remove = config.get("Settings", "GUIDsToRemove", fallback="").strip().split(',')
        delay_seconds = config.getfloat("Settings", "DelaySeconds", fallback=5.0)
        solution_path = config.get("Settings", "SolutionPath", fallback="")

        if not solution_path:
            solution_path = os.path.join(os.path.dirname(__file__), "FIRMICE.sln").replace("\\", "/")
            print("SolutionPath is not set in CSEconfig, using default path:", solution_path)

        if not projects_to_remove or (len(projects_to_remove) == 1 and not projects_to_remove[0]):
            print("Found CSEconfig, but the ProjectsToRemove element is empty. Exiting...")
            sys.exit(0)

        if not guids_to_remove or (len(guids_to_remove) == 1 and not guids_to_remove[0]):
            print("Found CSEconfig, but the GUIDsToRemove element is empty. Exiting...")
            sys.exit(0)

        print("Found CSEconfig, proceeding with the preset")
    else:
        with open(config_path, 'w', encoding='utf-8') as config_file:
            config_file.write("[Settings]\nProjectsToRemove = lilToon.Editor.External,lilToon.Editor\n"
                              "GUIDsToRemove = {006DCC64-98AF-9634-B58A-6691E1416853}\n"
                              "DelaySeconds = 5\n"
                              "SolutionPath = \n")
            print("Cannot find CSEconfig. Exiting...")
            print(f"New config has been generated by ClearSlnError: {config_path}")
        sys.exit(0)

    # Print the configuration
    print(f"Delay: {delay_seconds} seconds")
    print("Projects to Remove:")
    for project in projects_to_remove:
        print(f"- {project}")
    print("GUIDs to Remove:")
    for guid in guids_to_remove:
        print(f"- {guid}")

    # Perform the initial removal operation
    remove_projects_from_solution(solution_path, projects_to_remove, guids_to_remove)

    # Start the observer
    event_handler = SolutionHandler(solution_path, projects_to_remove, guids_to_remove, delay_seconds)
    observer = Observer()
    observer.schedule(event_handler, path=os.path.dirname(solution_path), recursive=False)
    observer.start()

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()

    observer.join()
