import { useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { api, ApiError } from '../lib/api'
import { AuthCard } from '../components/AuthCard'

export function ResetPasswordPage() {
  // The emailed link is /reset-password?email=...&token=...
  const [params] = useSearchParams()
  const email = params.get('email') ?? ''
  const token = params.get('token') ?? ''

  const [newPassword, setNewPassword] = useState('')
  const mutation = useMutation({
    mutationFn: () => api.resetPassword(email, token, newPassword),
  })

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    mutation.mutate()
  }

  // Prefer per-field validation messages (password rules); fall back to a summary.
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

  // Guard against a malformed link (missing email or token).
  if (!email || !token) {
    return (
      <AuthCard title="Reset password">
        <p className="text-sm text-red-600">This reset link is invalid or incomplete.</p>
        <p className="mt-4 text-center text-sm text-gray-600">
          <Link to="/forgot-password" className="text-blue-600 hover:underline">
            Request a new link
          </Link>
        </p>
      </AuthCard>
    )
  }

  return (
    <AuthCard title="Reset password">
      {mutation.isSuccess ? (
        <div className="space-y-4">
          <p className="text-sm text-gray-700">Your password has been reset.</p>
          <p className="text-center text-sm text-gray-600">
            <Link to="/login" className="text-blue-600 hover:underline">
              Log in
            </Link>
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <p className="text-sm text-gray-600">
            Resetting password for <span className="font-medium">{email}</span>.
          </p>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">New password</label>
            <input
              type="password"
              required
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
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
            {mutation.isPending ? 'Resetting…' : 'Reset password'}
          </button>
        </form>
      )}
    </AuthCard>
  )
}
