import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { RequireAuth } from './components/RequireAuth';
import { AuthPage } from './pages/AuthPage';
import { TaskDetailsPage } from './pages/TaskDetailsPage';
import { TaskFormPage } from './pages/TaskFormPage';
import { TasksPage } from './pages/TasksPage';

const secured = (element: JSX.Element) => <RequireAuth>{element}</RequireAuth>;

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/login" element={<AuthPage mode="login" />} />
        <Route path="/register" element={<AuthPage mode="register" />} />
        <Route path="/" element={secured(<TasksPage />)} />
        <Route path="/tasks/new" element={secured(<TaskFormPage />)} />
        <Route path="/tasks/:id" element={secured(<TaskDetailsPage />)} />
        <Route path="/tasks/:id/edit" element={secured(<TaskFormPage />)} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
