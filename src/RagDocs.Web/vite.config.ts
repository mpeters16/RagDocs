import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        // The API redirects HTTP to HTTPS, which the browser can't follow through the proxy.
        target: 'https://localhost:7121',
        changeOrigin: true,
        secure: false, // accept the ASP.NET dev certificate
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})
