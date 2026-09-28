import type {
  AuthResponse,
  PagedResult,
  SaveTaskRequest,
  TaskDetails,
  TaskListItem,
  TaskListParams,
  TaskStatus,
  User,
} from './types';

const TOKEN_KEY = 'task-manager.token';

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

let unauthorizedHandler: (() => void) | null = null;

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

export function onUnauthorized(handler: () => void) {
  unauthorizedHandler = handler;
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  const token = tokenStorage.get();
  if (token) headers.set('Authorization', `Bearer ${token}`);
  if (init.body) headers.set('Content-Type', 'application/json');

  const response = await fetch(`/api${path}`, { ...init, headers });

  if (response.status === 401 && token) {
    tokenStorage.clear();
    unauthorizedHandler?.();
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(response.status, problem?.detail ?? problem?.title ?? `Ошибка ${response.status}`);
  }

  return response.status === 204 ? (undefined as T) : ((await response.json()) as T);
}

const json = (body: unknown) => JSON.stringify(body);

export const api = {
  register: (name: string, email: string, password: string) =>
    request<AuthResponse>('/auth/register', { method: 'POST', body: json({ name, email, password }) }),
  login: (email: string, password: string) =>
    request<AuthResponse>('/auth/login', { method: 'POST', body: json({ email, password }) }),
  logout: () => request<void>('/auth/logout', { method: 'POST' }),
  me: () => request<User>('/auth/me'),

  listTasks: ({ search, status, page, pageSize }: TaskListParams) => {
    const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (search) query.set('search', search);
    if (status) query.set('status', status);
    return request<PagedResult<TaskListItem>>(`/tasks?${query}`);
  },
  getTask: (id: string) => request<TaskDetails>(`/tasks/${id}`),
  createTask: (task: SaveTaskRequest) => request<TaskDetails>('/tasks', { method: 'POST', body: json(task) }),
  updateTask: (id: string, task: SaveTaskRequest) =>
    request<TaskDetails>(`/tasks/${id}`, { method: 'PUT', body: json(task) }),
  changeStatus: (id: string, status: TaskStatus) =>
    request<TaskDetails>(`/tasks/${id}/status`, { method: 'PATCH', body: json({ status }) }),
  deleteTask: (id: string) => request<void>(`/tasks/${id}`, { method: 'DELETE' }),
};
