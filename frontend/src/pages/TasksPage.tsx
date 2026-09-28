import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { api } from '../api/client';
import {
  PRIORITY_LABELS,
  STATUS_LABELS,
  TASK_STATUSES,
  type PagedResult,
  type TaskListItem,
  type TaskStatus,
} from '../api/types';
import { StatusSelect } from '../components/StatusSelect';
import { formatDate, formatDateTime } from '../format';

const PAGE_SIZE = 10;
const SEARCH_DEBOUNCE_MS = 300;

export function TasksPage() {
  const [params, setParams] = useSearchParams();
  const page = Math.max(1, Number(params.get('page')) || 1);
  const search = params.get('search') ?? '';
  const status = (params.get('status') as TaskStatus | null) ?? undefined;

  const [searchInput, setSearchInput] = useState(search);
  const [result, setResult] = useState<PagedResult<TaskListItem> | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const updateParams = (changes: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params);
    Object.entries(changes).forEach(([key, value]) => (value ? next.set(key, value) : next.delete(key)));
    setParams(next, { replace: true });
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchInput.trim() !== search) updateParams({ search: searchInput.trim() || undefined, page: undefined });
    }, SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(timer);
  }, [searchInput, search]);

  useEffect(() => {
    let cancelled = false;
    api
      .listTasks({ search: search || undefined, status, page, pageSize: PAGE_SIZE })
      .then((data) => {
        if (cancelled) return;
        setResult(data);
        setError(null);
      })
      .catch((err: Error) => {
        if (!cancelled) setError(err.message);
      });
    return () => {
      cancelled = true;
    };
  }, [search, status, page, reloadKey]);

  const handleStatusChange = async (task: TaskListItem, next: TaskStatus) => {
    try {
      await api.changeStatus(task.id, next);
      setReloadKey((k) => k + 1);
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const handleDelete = async (task: TaskListItem) => {
    if (!window.confirm(`Удалить задачу «${task.title}»?`)) return;
    try {
      await api.deleteTask(task.id);
      if (result && result.items.length === 1 && page > 1) updateParams({ page: String(page - 1) });
      else setReloadKey((k) => k + 1);
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <section>
      <div className="toolbar">
        <h1>Мои задачи</h1>
        <Link to="/tasks/new" className="button">
          Новая задача
        </Link>
      </div>

      <div className="filters">
        <input
          type="search"
          placeholder="Поиск по названию"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
        />
        <select
          value={status ?? ''}
          onChange={(e) => updateParams({ status: e.target.value || undefined, page: undefined })}
        >
          <option value="">Все статусы</option>
          {TASK_STATUSES.map((s) => (
            <option key={s} value={s}>
              {STATUS_LABELS[s]}
            </option>
          ))}
        </select>
      </div>

      {error && <p className="error">{error}</p>}

      {result && result.items.length === 0 && <p className="muted">Задач не найдено.</p>}

      {result && result.items.length > 0 && (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Название</th>
                <th>Статус</th>
                <th>Приоритет</th>
                <th>Срок</th>
                <th>Создана</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {result.items.map((task) => (
                <tr key={task.id}>
                  <td>
                    <Link to={`/tasks/${task.id}`}>{task.title}</Link>
                  </td>
                  <td>
                    <StatusSelect value={task.status} onChange={(s) => handleStatusChange(task, s)} />
                  </td>
                  <td>
                    <span className={`priority priority-${task.priority}`}>{PRIORITY_LABELS[task.priority]}</span>
                  </td>
                  <td>{formatDate(task.dueDate)}</td>
                  <td>{formatDateTime(task.createdAt)}</td>
                  <td>
                    <button className="button danger small" onClick={() => handleDelete(task)}>
                      Удалить
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {result && result.totalPages > 1 && (
        <div className="pagination">
          <button
            className="button secondary"
            disabled={page <= 1}
            onClick={() => updateParams({ page: String(page - 1) })}
          >
            ←
          </button>
          <span>
            Страница {result.page} из {result.totalPages} · всего {result.totalCount}
          </span>
          <button
            className="button secondary"
            disabled={page >= result.totalPages}
            onClick={() => updateParams({ page: String(page + 1) })}
          >
            →
          </button>
        </div>
      )}
    </section>
  );
}
