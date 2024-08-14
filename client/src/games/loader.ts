let loadModule;

// I don't like this one bit but I've spent too much time here to make it pretty
if (import.meta.env.MODE == "build_games" || import.meta.env.DEV) {
    loadModule = async function (moduleName: string) {
        return (await import(`./${moduleName}/game.svelte`)).default
    };
} else {
    loadModule = async function (moduleName: string) {
        return (await import(/* @vite-ignore */"./games/" + moduleName + ".js")).default
    }
}

export default loadModule;