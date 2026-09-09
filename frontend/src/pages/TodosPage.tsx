import { useAuth } from '../context/AuthContext'
import { useTodos } from '../hooks/useTodos'

export function TodosPage() {
  const { email, logout } = useAuth()
  const { data: todos, isLoading, isError, error } = useTodos()

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex max-w-2xl items-center justify-between px-4 py-4">
          <h1 className="text-lg font-semibold text-gray-900">My Todos</h1>
          <div className="flex items-center gap-3 text-sm">
            <span className="text-gray-600">{email}</span>
            <button onClick={logout} className="text-gray-500 hover:text-gray-900">
              Log out
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-2xl px-4 py-8">
        {isLoading && <p className="text-gray-500">Loading…</p>}

        {isError && (
          <p className="text-red-600">
            {error instanceof Error ? error.message : 'Failed to load todos.'}
          </p>
        )}

        {todos && todos.length === 0 && (
          <p className="text-gray-500">
            No todos yet. (Creating todos comes in the next step.)
          </p>
        )}

        {todos && todos.length > 0 && (
          <ul className="space-y-2">
            {todos.map((todo) => (
              <li
                key={todo.id}
                className="flex items-center gap-3 rounded-lg border border-gray-200 bg-white px-4 py-3"
              >
                <span className={todo.isCompleted ? 'text-gray-400 line-through' : 'text-gray-900'}>
                  {todo.title}
                </span>
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  )
}
