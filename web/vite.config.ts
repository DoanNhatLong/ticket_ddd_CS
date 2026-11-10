import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // Forward API calls to TrainBooking.Api (dotnet run) so the browser sees a single origin: no CORS setup needed.
    proxy: {
      '/api': 'http://localhost:7777',
    },
  },
})
