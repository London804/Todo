import { useState, type FormEvent, type ReactNode } from 'react'
import { useMutation } from '@tanstack/react-query'
import { ApiError } from '../lib/api'

interface AuthFormProps {
  title: string
  submitLabel: string
  // The auth action to run (login or register) with the entered credentials.
  action: (email: string, password: string) => Promise<void>
  onSuccess: () => void
  footer: ReactNode
}

// Shared UI + submit handling for the login and register pages, which are
// structurally identical (email + password -> an auth action).
export function AuthForm({ title, submitLabel, action, onSuccess, footer }: AuthFormProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  // useMutation gives us pending/error state for the async auth call.
  const mutation = useMutation({
    mutationFn: () => action(email, password),
    onSuccess,
  })

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    mutation.mutate()
  }

  // Prefer the specific per-field validation messages (e.g. password rules); fall
  // back to a single summary message for other errors (e.g. "Invalid email or
  // password"), and a generic line for anything non-API (network failure, etc.).
  const apiError = mutation.error instanceof ApiError ? mutation.error : null
  const details = apiError?.details ?? []
  const summaryMessage =
    details.length > 0
      ? null
      : apiError
        ? apiError.message
        : mutation.error
          ? 'Something went wrong. Please try again.'
          : null

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-sm rounded-xl border border-gray-200 bg-white p-8 shadow-sm">
        <h1 className="mb-6 text-2xl font-semibold text-gray-900">{title}</h1>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Email</label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Password</label>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          {details.length > 0 && (
            <ul className="list-disc space-y-1 pl-5 text-sm text-red-600">
              {details.map((d, i) => (
                <li key={i}>{d}</li>
              ))}
            </ul>
          )}
          {summaryMessage && <p className="text-sm text-red-600">{summaryMessage}</p>}
          <button
            type="submit"
            disabled={mutation.isPending}
            className="w-full rounded-lg bg-blue-600 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {mutation.isPending ? 'Please wait…' : submitLabel}
          </button>
        </form>
        <p className="mt-6 text-center text-sm text-gray-600">{footer}</p>
      </div>
    </div>
  )
}
