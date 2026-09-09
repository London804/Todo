import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    // Dev proxy: the browser only ever talks to the Vite dev server (same origin),
    // and Vite forwards /api/* to the backend. This sidesteps both CORS and the
    // self-signed HTTPS cert during development. `secure: false` accepts the dev cert.
    proxy: {
      '/api': {
        target: 'https://localhost:7243',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
