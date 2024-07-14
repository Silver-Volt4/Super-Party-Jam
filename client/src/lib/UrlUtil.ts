export function setUrl(url: string | URL | null | undefined) {
    history.replaceState(null, "", url);
}

export function switchModule(module: string) {
    let url = new URL(window.location.href);
    url.pathname = "/" + module;
    window.location.replace(url);
}

export function toLoginScreen() {
    switchModule("");
}

export function getUrl() {
    return new URL(window.location.href);
}