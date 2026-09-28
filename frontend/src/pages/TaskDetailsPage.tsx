import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api } from '../api/client';
import { PRIORITY_LABELS, type TaskDetails, type TaskStatus } from '../api/types';
import { StatusSelect } from '../components/StatusSelect';
import { formatDate, formatDateTime } from '../format';

export function TaskDetailsPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const [task, setTask] = useState<TaskDetails | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.getTask(id).then(setTask).catch((err: Error) => setError(err.message));
  }, [id]);

  if (error) return <p className="error">{error}</p>;
  if (!task) return <p className="muted">Загрузка…</p>;

  const handleStatusChange = async (status: TaskStatus) => {
    try {
      setTask(await api.changeStatus(task.id, status));
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm(`Удалить задачу «${task.title}»?`)) return;
    try {
      await api.deleteTask(task.id);
      navigate('/');
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <article className="card">
      <Link to="/" className="muted">
        ← К списку
      </Link>
      <h1>{task.title}</h1>
      <dl className="details">
        <dt>Статус</dt>
        <dd>
          <StatusSelect value={task.status} onChange={handleStatusChange} />
        </dd>
        <dt>Приоритет</dt>
        <dd>
          <span className={`priority priority-${task.priority}`}>{PRIORITY_LABELS[task.priority]}</span>
        </dd>
        <dt>Срок выполнения</dt>
        <dd>{formatDate(task.dueDate)}</dd>
        <dt>Создана</dt>
        <dd>{formatDateTime(task.createdAt)}</dd>
        <dt>Обновлена</dt>
        <dd>{formatDateTime(task.updatedAt)}</dd>
      </dl>
      <h2>Описание</h2>
      <p className="description">{task.description || <span className="muted">Нет описания</span>}</p>
      <div className="actions">
        <Link to={`/tasks/${task.id}/edit`} className="button">
          Редактировать
        </Link>
        <button className="button danger" onClick={handleDelete}>
          Удалить
        </button>
      </div>
    </article>
  );
}
