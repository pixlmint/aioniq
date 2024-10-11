from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from datetime import time, datetime, date, timedelta, timezone
from pytz import utc


class Tasklist:
    """
    Representing a list of tasks

    Attributes:
        _id (string): Task list identifier
        etag (string): ETag of the resource
        kind (string): Output only. Type of the resource. This is always "tasks#taskList".
        selflink (string): Output only. URL pointing to this task list. Used to retrieve, update, or delete this task list
        title (string): Title of the task list. Maximum length allowed: 1024 characters.
        updated (datetime): Last modification time of the task list
        tasks (list[Task]): list of tasks
    """

    def __init__(self, _id: str, etag: str, kind: str, selflink: str, title: str, updated: datetime):
        self._id = _id
        self.etag = etag
        self.kind = kind
        self.selflink = selflink
        self.title = title
        self.updated = updated
        self.tasks: list['Task'] = []

    def __repr__(self):
        """
        Returns a string representation of the tasklist.
        """
        return f"<Tasklist id={self._id} title={self.title}, num_tasks={len(self.tasks)}>"

    def add_task(self, task: 'Task'):
        self.tasks.append(task)

    @staticmethod
    def from_dict(obj: dict) -> 'Tasklist':
        return Tasklist(
            _id=obj.get('id'),
            etag=obj.get('etag'),
            kind=obj.get('kind'),
            selflink=obj.get('selflink'),
            title=obj.get('title'),
            updated=datetime.fromisoformat(obj.get('updated'))
        )


class Task:
    """
    A class representing a task, with details about the task's origin, status, and other metadata.

    Attributes:
        assignment_info (dict): Information about the source of the task assignment.
        completed (str): The date when the task was completed (RFC 3339 format). Optional.
        deleted (bool): Indicates if the task has been deleted.
        due (str): The due date of the task (RFC 3339 format). Optional.
        etag (str): ETag of the resource.
        hidden (bool): Indicates if the task is hidden. Read-only.
        id (str): Task identifier.
        kind (str): Type of the resource. Always "tasks#task".
        links (list): A list of links related to the task.
        notes (str): Notes describing the task. Optional.
        parent (str): The parent task identifier. Read-only.
        position (str): The position of the task among its siblings. Read-only.
        self_link (str): URL to retrieve, update, or delete this task.
        status (str): Status of the task. Either "needsAction" or "completed".
        title (str): Title of the task. Max length: 1024 characters.
        updated (str): Last modification time of the task (RFC 3339 format). Read-only.
        web_view_link (str): Link to the task in Google Tasks Web UI.
    """

    def __init__(self, assignment_info: dict, completed: bool, deleted: bool,
                 due: datetime | None, etag: str, hidden: bool,
                 task_id: str, kind: str, links: list[dict], notes: str,
                 parent: str | None, position: str, self_link: str,
                 status: str, title: str, updated: datetime, web_view_link: str):
        """
        Initializes a Task object with provided details.
        """
        self.assignment_info = assignment_info or {}
        self.completed = completed
        self.deleted = deleted
        self.due = due
        self.etag = etag
        self.hidden = hidden
        self.id = task_id
        self.kind = kind
        self.links = links or []
        self.notes = notes
        self.parent = parent
        self.position = position
        self.self_link = self_link
        self.status = status
        self.title = title
        self.updated = updated
        self.web_view_link = web_view_link

    def __repr__(self):
        """
        Returns a string representation of the task.
        """
        return f"<Task id={self.id} title={self.title} status={self.status}>"

    @staticmethod
    def from_dict(data):
        """
        Creates a Task object from a dictionary.

        Args:
            data (dict): Dictionary containing task data.

        Returns:
            Task: A Task object created from the dictionary.
        """
        return Task(
            assignment_info=data.get('assignmentInfo', {}),
            completed=data.get('completed'),
            deleted=data.get('deleted', False),
            due=data.get('due'),
            etag=data.get('etag'),
            hidden=data.get('hidden', False),
            task_id=data.get('id'),
            kind=data.get('kind'),
            links=data.get('links', []),
            notes=data.get('notes'),
            parent=data.get('parent'),
            position=data.get('position'),
            self_link=data.get('selfLink'),
            status=data.get('status'),
            title=data.get('title'),
            updated=datetime.fromisoformat(data.get('updated')),
            web_view_link=data.get('webViewLink')
        )


def _fetch_all_items(func, request) -> list[dict]:
    items = []
    iterations = 0
    while request is not None:
        results = request.execute()
        items += results.get('items', [])
        request = func.list_next(previous_request=request, previous_response=results)
        iterations += 1
    return items


def fetch_all_task_lists(client) -> list[Tasklist]:
    request = client.tasklists().list(maxResults=100)
    items = _fetch_all_items(client.tasklists(), request)

    return [Tasklist.from_dict(x) for x in items]


def fetch_tasks_in_range(client, tasklist: Tasklist, due_start: datetime, due_end: datetime):
    request = client.tasks().list(tasklist=tasklist._id,
                                  updatedMin=due_start.isoformat(sep='T'))
    items = _fetch_all_items(client.tasks(), request)

    return [Task.from_dict(x) for x in items]


SCOPES = ['https://www.googleapis.com/auth/tasks.readonly']

# Authenticate user
flow = InstalledAppFlow.from_client_secrets_file('secrets.json', SCOPES)
creds = flow.run_local_server(port=8080)

# Build the service
service = build('tasks', 'v1', credentials=creds)

now = datetime.now(tz=utc)
delta = timedelta(days=30)

print("start: ", (now - delta).isoformat(sep="T"))
print("end: ", (now + delta).isoformat(sep="T"))

lists = fetch_all_task_lists(service)
for list in lists:
    fetch_tasks_in_range(service, list, now - delta, now + delta)
    print(list)
    for task in list.tasks:
        print(task)

