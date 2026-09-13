import { useState } from 'react'
import { useAuth } from '../context/AuthContext'
import { useTodos } from '../hooks/useTodos'
import { CreateTodoForm } from '../components/CreateTodoForm'
import { TodoItem } from '../components/TodoItem'

export function TodosPage() {
  const { email, logout } = useAuth()
  const [showArchived, setShowArchived] = useState(false)
  const { data: todos, isLoading, isError, error } = useTodos(showArchived)

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
        <CreateTodoForm />

        <label className="mb-4 flex items-center gap-2 text-sm text-gray-600">
          <input
            type="checkbox"
            checked={showArchived}
            onChange={(e) => setShowArchived(e.target.checked)}
            className="h-4 w-4 rounded border-gray-300"
          />
          Show archived
        </label>

        {isLoading && <p className="text-gray-500">Loading…</p>}

        {isError && (
          <p className="text-red-600">
            {error instanceof Error ? error.message : 'Failed to load todos.'}
          </p>
        )}

        {todos && todos.length === 0 && (
          <p className="text-gray-500">No todos yet. Add one above.</p>
        )}

        {todos && todos.length > 0 && (
          <ul className="space-y-2">
            {todos.map((todo) => (
              <TodoItem key={todo.id} todo={todo} />
            ))}
          </ul>
        )}
      </main>
    </div>
  )
}
