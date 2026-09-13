import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'

// After any change we invalidate every ['todos', ...] query so the list refetches
// and reflects the new state. (Partial key match covers both the active and
// include-archived variants.)
function useInvalidateTodos() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: ['todos'] })
}

export function useCreateTodo() {
  const invalidate = useInvalidateTodos()
  return useMutation({
    mutationFn: (title: string) => api.createTodo(title),
    onSuccess: invalidate,
  })
}

export function useCompleteTodo() {
  const invalidate = useInvalidateTodos()
  return useMutation({
    mutationFn: (id: number) => api.completeTodo(id),
    onSuccess: invalidate,
  })
}

export function useArchiveTodo() {
  const invalidate = useInvalidateTodos()
  return useMutation({
    mutationFn: (id: number) => api.archiveTodo(id),
    onSuccess: invalidate,
  })
}

export function useDeleteTodo() {
  const invalidate = useInvalidateTodos()
  return useMutation({
    mutationFn: (id: number) => api.deleteTodo(id),
    onSuccess: invalidate,
  })
}
