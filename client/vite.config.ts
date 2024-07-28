import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import godotImportImagePlugin from "./gdimport.js"

export default defineConfig({
	plugins: [sveltekit(), godotImportImagePlugin()],
	build: {
        minify: false
    }
});
