import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// The API listens on localhost only (ADR 0001 §2.3); the dev server proxies /api to it.
const apiOrigin = 'http://localhost:5080'

export default defineConfig({
  plugins: [react()],
  server: {
    host: 'localhost',
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': apiOrigin,
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    restoreMocks: true,
  },
})
