import { defineConfig } from 'vite';
export default defineConfig({
  build: {
    outDir: 'wwwroot/js', // Blazor 프로젝트의 정적 파일 경로에 맞게 조정
    rollupOptions: {
      input: '/src/index.js'
    }
  }
});