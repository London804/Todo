import type { TodoResponse } from '../lib/api'
import {
  useArchiveTodo,
  useCompleteTodo,
  useDeleteTodo,
} from '../hooks/useTodoMutations'

export function TodoItem({ todo }: { todo: TodoResponse }) {
  const complete = useCompleteTodo()
  const archive = useArchiveTodo()
  const remove = useDeleteTodo()

  // Disable this row's controls while any of its actions are in flight.
  const busy = complete.isPending || archive.isPending || remove.isPending

  const handleDelete = () => {
    if (window.confirm(`Delete "${todo.title}"?`)) remove.mutate(todo.id)
  }

  return (
    <li className="flex items-center gap-3 rounded-lg border border-gray-200 bg-white px-4 py-3">
      {/* The API has no "un-complete", so a completed todo's checkbox is locked. */}
      <input
        type="checkbox"
        checked={todo.isCompleted}
        disabled={todo.isCompleted || busy}
        onChange={() => complete.mutate(todo.id)}
        className="h-4 w-4 rounded border-gray-300"
      />

      <span className={`flex-1 ${todo.isCompleted ? 'text-gray-400 line-through' : 'text-gray-900'}`}>
        {todo.title}
      </span>

      {todo.isArchived && (
        <span className="rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-500">Archived</span>
      )}

      {!todo.isArchived && (
        <button
          onClick={() => archive.mutate(todo.id)}
          disabled={busy}
          className="text-xs text-gray-500 hover:text-gray-900 disabled:opacity-50"
        >
          Archive
        </button>
      )}

      <button
        onClick={handleDelete}
        disabled={busy}
        className="text-xs text-red-500 hover:text-red-700 disabled:opacity-50"
      >
        Delete
      </button>
    </li>
  )
}
