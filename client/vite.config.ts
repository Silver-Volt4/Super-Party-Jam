import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import godotImportImagePlugin from "./gdimport.js"

export default defineConfig({
  plugins: [svelte(), godotImportImagePlugin()],
  build: {
    outDir: "../game/assets/controller",
    emptyOutDir: true,
  }
})