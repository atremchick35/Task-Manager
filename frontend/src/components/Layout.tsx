import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="app">
      <header className="header">
        <Link to="/" className="brand">
          Task Manager
        </Link>
        {user && (
          <div className="header-user">
            <span className="muted">{user.name}</span>
            <button className="button secondary" onClick={handleLogout}>
              Выйти
            </button>
          </div>
        )}
      </header>
      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}
