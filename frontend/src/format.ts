const dateFormat = new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium' });
const dateTimeFormat = new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium', timeStyle: 'short' });

export const formatDate = (value: string | null) => (value ? dateFormat.format(new Date(`${value}T00:00:00`)) : '—');

export const formatDateTime = (value: string) => dateTimeFormat.format(new Date(value));
