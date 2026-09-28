export const TASK_STATUSES = ['Todo', 'InProgress', 'Done'] as const;
export const TASK_PRIORITIES = ['Low', 'Medium', 'High'] as const;

export type TaskStatus = (typeof TASK_STATUSES)[number];
export type TaskPriority = (typeof TASK_PRIORITIES)[number];

export const STATUS_LABELS: Record<TaskStatus, string> = {
  Todo: 'К выполнению',
  InProgress: 'В работе',
  Done: 'Готово',
};

export const PRIORITY_LABELS: Record<TaskPriority, string> = {
  Low: 'Низкий',
  Medium: 'Средний',
  High: 'Высокий',
};

export interface User {
  id: string;
  name: string;
  email: string;
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: User;
}

export interface TaskListItem {
  id: string;
  title: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  createdAt: string;
}

export interface TaskDetails extends TaskListItem {
  description: string;
  updatedAt: string;
}

export interface SaveTaskRequest {
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TaskListParams {
  search?: string;
  status?: TaskStatus;
  page: number;
  pageSize: number;
}
