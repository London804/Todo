import type { ReactNode } from 'react'

// The centered card shell used by the auth pages (forgot/reset). Keeps their outer
// layout consistent with the login/register form.
export function AuthCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-sm rounded-xl border border-gray-200 bg-white p-8 shadow-sm">
        <h1 className="mb-6 text-2xl font-semibold text-gray-900">{title}</h1>
        {children}
      </div>
    </div>
  )
}
