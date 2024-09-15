import type { SvelteComponent } from "svelte";

let loadModule: (moduleName: string) => Promise<SvelteComponent>;

// I don't like this one bit but I've spent too much time here to make it pretty
if (window) {
    loadModule = async function (moduleName: string) {
        console.log("fetching it");
        let a = await import(/* @vite-ignore */"./games/" + moduleName + ".js");
        console.log("got it!")
        return a.default
    }
} else {
    loadModule = async function (moduleName: string) {
        return (await import(`./${moduleName}/game.svelte`)).default
    };
}

export default loadModule;