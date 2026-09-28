import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api } from '../api/client';
import {
  PRIORITY_LABELS,
  STATUS_LABELS,
  TASK_PRIORITIES,
  TASK_STATUSES,
  type SaveTaskRequest,
  type TaskPriority,
  type TaskStatus,
} from '../api/types';

const EMPTY_TASK: SaveTaskRequest = { title: '', description: '', status: 'Todo', priority: 'Medium', dueDate: null };

export function TaskFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState<SaveTaskRequest>(EMPTY_TASK);
  const [loading, setLoading] = useState(Boolean(id));
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    api
      .getTask(id)
      .then(({ title, description, status, priority, dueDate }) =>
        setForm({ title, description, status, priority, dueDate }),
      )
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false));
  }, [id]);

  const set = <K extends keyof SaveTaskRequest>(key: K, value: SaveTaskRequest[K]) =>
    setForm((prev) => ({ ...prev, [key]: value }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const saved = id ? await api.updateTask(id, form) : await api.createTask(form);
      navigate(`/tasks/${saved.id}`);
    } catch (err) {
      setError((err as Error).message);
      setSubmitting(false);
    }
  };

  if (loading) return <p className="muted">Загрузка…</p>;

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h1>{id ? 'Редактирование задачи' : 'Новая задача'}</h1>
      <label>
        Название
        <input value={form.title} onChange={(e) => set('title', e.target.value)} required maxLength={200} />
      </label>
      <label>
        Описание
        <textarea
          value={form.description}
          onChange={(e) => set('description', e.target.value)}
          rows={6}
          maxLength={4000}
        />
      </label>
      <div className="row">
        <label>
          Статус
          <select value={form.status} onChange={(e) => set('status', e.target.value as TaskStatus)}>
            {TASK_STATUSES.map((s) => (
              <option key={s} value={s}>
                {STATUS_LABELS[s]}
              </option>
            ))}
          </select>
        </label>
        <label>
          Приоритет
          <select value={form.priority} onChange={(e) => set('priority', e.target.value as TaskPriority)}>
            {TASK_PRIORITIES.map((p) => (
              <option key={p} value={p}>
                {PRIORITY_LABELS[p]}
              </option>
            ))}
          </select>
        </label>
        <label>
          Срок выполнения
          <input type="date" value={form.dueDate ?? ''} onChange={(e) => set('dueDate', e.target.value || null)} />
        </label>
      </div>
      {error && <p className="error">{error}</p>}
      <div className="actions">
        <button className="button" type="submit" disabled={submitting}>
          Сохранить
        </button>
        <Link to={id ? `/tasks/${id}` : '/'} className="button secondary">
          Отмена
        </Link>
      </div>
    </form>
  );
}
