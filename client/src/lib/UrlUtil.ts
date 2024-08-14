export function setUrl(url: string | URL | null | undefined) {
    history.replaceState(null, "", url);
}

export function getUrl() {
    return new URL(window.location.href);
}