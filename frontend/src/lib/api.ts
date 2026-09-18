// Typed client for the TodoApp API. All requests go to relative /api/* paths,
// which the Vite dev proxy forwards to the backend (see vite.config.ts).

export interface TodoResponse {
  id: number
  title: string
  isCompleted: boolean
  isArchived: boolean
  createdAt: string
  completedAt: string | null
}

export interface AuthResponse {
  token: string
  email: string
}

const TOKEN_KEY = 'todoapp.token'

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

// Thrown for any non-2xx response, carrying the HTTP status, a summary message,
// and any per-field validation messages from a ProblemDetails `errors` object.
export class ApiError extends Error {
  status: number
  details: string[]

  constructor(status: number, message: string, details: string[] = []) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.details = details
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = tokenStorage.get()
  const res = await fetch(path, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })

  if (!res.ok) {
    let message = res.statusText
    let details: string[] = []
    try {
      const body = await res.json()
      message = body.title ?? body.detail ?? message
      // ValidationProblemDetails puts per-field messages under `errors`, keyed by
      // field/error code. Flatten every message into a flat list for display.
      if (body.errors && typeof body.errors === 'object') {
        details = Object.values(body.errors as Record<string, string[]>).flat()
      }
    } catch {
      // response had no JSON body; keep the status text
    }
    throw new ApiError(res.status, message, details)
  }

  // 204 No Content (e.g. DELETE) has no body to parse.
  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

export const api = {
  // --- auth ---
  register: (email: string, password: string) =>
    request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  login: (email: string, password: string) =>
    request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  forgotPassword: (email: string) =>
    request<{ message: string }>('/api/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify({ email }),
    }),

  resetPassword: (email: string, token: string, newPassword: string) =>
    request<{ message: string }>('/api/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify({ email, token, newPassword }),
    }),

  // --- todos ---
  getTodos: (includeArchived = false) =>
    request<TodoResponse[]>(`/api/todos?includeArchived=${includeArchived}`),

  createTodo: (title: string) =>
    request<TodoResponse>('/api/todos', {
      method: 'POST',
      body: JSON.stringify({ title }),
    }),

  updateTodo: (id: number, title: string) =>
    request<TodoResponse>(`/api/todos/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ title }),
    }),

  completeTodo: (id: number) =>
    request<TodoResponse>(`/api/todos/${id}/complete`, { method: 'POST' }),

  archiveTodo: (id: number) =>
    request<TodoResponse>(`/api/todos/${id}/archive`, { method: 'POST' }),

  deleteTodo: (id: number) =>
    request<void>(`/api/todos/${id}`, { method: 'DELETE' }),
}
