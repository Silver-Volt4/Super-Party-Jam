let websocketUrl: URL

async function fetchWebsocketUrl() {
    let currentUrl = new URL(window.location.href);
    currentUrl.search = "";
    currentUrl.pathname = "/ws";
    if (import.meta.env.DEV) {
        currentUrl.port = "12003";
    }
    let f = await fetch(currentUrl);
    let port = await f.text();
    currentUrl.port = port;
    currentUrl.pathname = "/";
    websocketUrl = currentUrl;
    return currentUrl;
}

async function getWebsocketUrl() {
    return websocketUrl ?? (await fetchWebsocketUrl());
}

interface UserData {
    username: string | null;
    token: string | null;
}

interface SPJEvent extends Event {
    detail: { [key: string]: any }
}

export class SPJClient {
    private ws: WebSocket | undefined
    private module: SPJModule | null = null;
    private gameEventHandler: EventTarget = new EventTarget();
    private clientEventHandler: EventTarget = new EventTarget();

    isInitialized = $state(false);
    isOnline = $state(false);

    userData: UserData = $state({
        username: null,
        token: null,
    });

    private reset() {
        this.userData = {
            username: null,
            token: null,
        }
        this.gameEventHandler = new EventTarget();
        this.isInitialized = false;
        this.isOnline = false;
    }

    private async connect() {
        this.ws = new WebSocket(await getWebsocketUrl());
        return await new Promise(async (resolve, reject) => {
            this.ws?.addEventListener("open", () => {
                let data: any = {
                    event: "register",
                    module: this.module?.name ?? ""
                };
                if (this.userData.token) {
                    data.token = this.userData.token;
                } else {
                    data.username = this.userData.username;
                }
                this.send(data);
            }, { once: true })

            this.onGameEvent("accepted", (e: SPJEvent) => {
                let data = e.detail;
                this.userData.username = data.username;
                this.userData.token = data.token;
                this.isInitialized = true;
                this.isOnline = true;
                this.ws?.addEventListener("error", this.onError.bind(this))
                this.ws?.removeEventListener("error", reject)
            }, { once: true })
            this.ws?.addEventListener("error", reject, { once: true })
            this.ws.addEventListener("close", this.onClose.bind(this))
            this.ws.addEventListener("message", this.onMessage.bind(this))
        })
    }

    async newConnection(username: string) {
        this.reset();
        this.userData.username = username;
        await this.connect();
    }

    async resumeConnection(username: string, token: string) {
        this.reset();
        this.userData.token = token;
        await this.connect();
    }

    onGameEvent(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions | undefined) {
        this.gameEventHandler.addEventListener(type, callback, options);
    }

    offGameEvent(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | EventListenerOptions | undefined) {
        this.gameEventHandler.removeEventListener(type, callback, options);
    }

    onClientEvent(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions | undefined) {
        this.clientEventHandler.addEventListener(type, callback, options);
    }

    offClientEvent(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | EventListenerOptions | undefined) {
        this.clientEventHandler.removeEventListener(type, callback, options);
    }

    setUsername(username: string) {
        this.userData.username = username;
        this.send({
            event: "setusername",
            username: this.userData.username
        });
    }

    send(data: any) {
        this.ws?.send(JSON.stringify(data));
    }

    assignModule(module: SPJModule) {
        this.module = module;
        module.client = this;
    }

    getModule() {
        return this.module;
    }

    private onMessage(event: MessageEvent) {
        let j: any = JSON.parse(event.data);
        if ("event" in j) {
            this.gameEventHandler.dispatchEvent(new CustomEvent(j.event, { detail: j }))
        }
    }

    private onClose(e: CloseEvent) {
        this.isOnline = false;
        if (e.code >= 4000) {
            this.isInitialized = false;
            this.clientEventHandler.dispatchEvent(new CustomEvent("closed", { detail: e }))
        } else {
            this.clientEventHandler.dispatchEvent(new CustomEvent("offline", { detail: e }))
            this.connect();
        }
    }

    private onError(e: Event) {
        this.isOnline = false;
        this.clientEventHandler.dispatchEvent(new CustomEvent("error", { detail: e }))
    }
}

export abstract class SPJModule {
    client: SPJClient | undefined;
    abstract name: string

    sync(name: string, state: any) {
        this.client?.send({
            event: "sync",
            name: name,
            value: state
        })
    }
};

let client = new SPJClient();
export default client;