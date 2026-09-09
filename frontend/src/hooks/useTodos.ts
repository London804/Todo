import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

// Fetches the current user's todos. TanStack Query handles caching, loading and
// error states; the queryKey identifies this data in the cache (and includes
// includeArchived so the two variants are cached separately).
export function useTodos(includeArchived = false) {
  return useQuery({
    queryKey: ['todos', { includeArchived }],
    queryFn: () => api.getTodos(includeArchived),
  })
}
