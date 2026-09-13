import { useState, type FormEvent } from 'react'
import { useCreateTodo } from '../hooks/useTodoMutations'
import { ApiError } from '../lib/api'

export function CreateTodoForm() {
  const [title, setTitle] = useState('')
  const createTodo = useCreateTodo()

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    const trimmed = title.trim()
    if (!trimmed) return // don't submit blank titles
    // Clear the input only once the create actually succeeds.
    createTodo.mutate(trimmed, { onSuccess: () => setTitle('') })
  }

  const errorMessage =
    createTodo.error instanceof ApiError
      ? createTodo.error.message
      : createTodo.error
        ? 'Could not add the todo. Please try again.'
        : null

  return (
    <div className="mb-6">
      <form onSubmit={handleSubmit} className="flex gap-2">
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Add a todo…"
          className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          type="submit"
          disabled={createTodo.isPending}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
        >
          Add
        </button>
      </form>
      {errorMessage && <p className="mt-2 text-sm text-red-600">{errorMessage}</p>}
    </div>
  )
}
