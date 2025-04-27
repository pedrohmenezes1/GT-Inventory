// frontend/src/pages/DashboardPage.tsx
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 py-3 flex justify-between items-center">
          <h1 className="text-xl font-bold text-gray-800">Dashboard</h1>
          <div className="flex items-center gap-4">
            {user && (
              <div className="flex items-center gap-2">
                <span className="text-gray-600">{user.name}</span>
                <span className="text-gray-400">({user.email})</span>
              </div>
            )}
            <button
              onClick={handleLogout}
              className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition"
            >
              Sair
            </button>
          </div>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-2xl font-semibold mb-4">Bem-vindo ao Sistema</h2>
          <div className="space-y-4">
            <p className="text-gray-600">
              Esta é uma página de dashboard simples para testes de autenticação.
            </p>
            <div className="bg-blue-50 p-4 rounded-md">
              <h3 className="font-medium text-blue-800 mb-2">Informações do Usuário:</h3>
              <ul className="list-disc list-inside text-blue-700">
                <li>Nome: {user?.name || 'N/A'}</li>
                <li>Email: {user?.email || 'N/A'}</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}