import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api, tokenStorage, type AuthResponse } from '../lib/api'
import { decodeJwt } from '../lib/jwt'

interface AuthState {
  email: string | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthState | undefined>(undefined)

// The email is derived from the token's claims — the token is the single source
// of truth, so there's nothing separate to keep in sync.
function emailFromToken(token: string | null): string | null {
  return token ? decodeJwt(token)?.email ?? null : null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  // Initialize from localStorage so a page refresh keeps the user logged in.
  const [token, setToken] = useState<string | null>(() => tokenStorage.get())

  const applyAuth = useCallback((res: AuthResponse) => {
    tokenStorage.set(res.token)
    setToken(res.token)
  }, [])

  const login = useCallback(
    async (email: string, password: string) => applyAuth(await api.login(email, password)),
    [applyAuth],
  )

  const register = useCallback(
    async (email: string, password: string) => applyAuth(await api.register(email, password)),
    [applyAuth],
  )

  const logout = useCallback(() => {
    tokenStorage.clear()
    setToken(null)
  }, [])

  const value = useMemo<AuthState>(
    () => ({
      email: emailFromToken(token),
      isAuthenticated: !!token,
      login,
      register,
      logout,
    }),
    [token, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider')
  return ctx
}
