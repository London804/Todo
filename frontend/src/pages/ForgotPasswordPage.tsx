import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { api } from '../lib/api'
import { AuthCard } from '../components/AuthCard'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const mutation = useMutation({ mutationFn: () => api.forgotPassword(email) })

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    mutation.mutate()
  }

  return (
    <AuthCard title="Forgot password">
      {mutation.isSuccess ? (
        // The backend always responds the same way (no account enumeration), so we
        // show the same generic confirmation regardless of whether the email exists.
        <div className="space-y-4">
          <p className="text-sm text-gray-700">
            If that email is registered, a reset link has been sent. Check your inbox.
          </p>
          <p className="text-center text-sm text-gray-600">
            <Link to="/login" className="text-blue-600 hover:underline">
              Back to log in
            </Link>
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <p className="text-sm text-gray-600">
            Enter your email and we&apos;ll send a link to reset your password.
          </p>
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
          <button
            type="submit"
            disabled={mutation.isPending}
            className="w-full rounded-lg bg-blue-600 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {mutation.isPending ? 'Sending…' : 'Send reset link'}
          </button>
          <p className="text-center text-sm text-gray-600">
            <Link to="/login" className="text-blue-600 hover:underline">
              Back to log in
            </Link>
          </p>
        </form>
      )}
    </AuthCard>
  )
}
