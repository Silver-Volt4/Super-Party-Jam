import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import godotImportImagePlugin from "./gdimport.js"

const GAMES_REGEXP = /src\/games\/(.+)\/game\.svelte$/

export default defineConfig({
  plugins: [svelte(), godotImportImagePlugin()],
  build: {
    outDir: "../game/assets/controller",
    emptyOutDir: true,
    minify: true,
    rollupOptions: {
      // external: ["svelte"],
      output: { // TODO: maybe use a function to make it smart
        entryFileNames: `[name].js`,
        chunkFileNames: (chunkInfo) => {
          let match = chunkInfo.facadeModuleId?.match(GAMES_REGEXP);
          if (match && match.length == 2) {
            return `games/${match[1]}.js`
          }
          return `[name].js`
        },
        assetFileNames: `[name].[ext]`
      }
    }
  },
});