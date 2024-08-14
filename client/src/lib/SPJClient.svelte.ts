import type SPJModule from "./SPJModule";

export enum ConnectionState {
    OFFLINE,
    CONNECTING,
    ONLINE
}

export enum CloseClient {
    INVALID_TOKEN = 4100,
    REMOVED_BY_HOST = 4101,
}
export enum ClosePlayer {
    REPLACED = 4200,
}
export enum CloseModule {
    SWITCH_TO_MODULE = 4500,
    EXIT_MODULE = 4501,
}

export enum SpectatorMode {
    IN_GAME,
    SPECTATOR,
    SPECTATOR_FORCED
}

interface EventListener {
    (evt: CustomEvent): void;
}

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
    connectionState: ConnectionState = $state(ConnectionState.OFFLINE);

    userData: UserData = $state({
        username: null,
        token: null,
    });

    spectatorMode: SpectatorMode = $state(SpectatorMode.IN_GAME);

    private reset() {
        this.userData = {
            username: null,
            token: null,
        }
        this.connectionState = ConnectionState.OFFLINE;
        this.spectatorMode = SpectatorMode.IN_GAME;
        this.gameEventHandler = new EventTarget();
        this.onGameEvent("spectator", (e: CustomEvent) => this.setSpectatorMode(e.detail))
        this.isInitialized = false;
    }

    private async connect() {
        if (this.connectionState === ConnectionState.ONLINE) return;
        this.connectionState = ConnectionState.CONNECTING;
        try {
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
                    this.connectionState = ConnectionState.ONLINE;
                    this.ws?.addEventListener("error", this.onError.bind(this))
                    this.ws?.removeEventListener("error", reject)
                }, { once: true })
                this.ws?.addEventListener("error", reject, { once: true })
                this.ws?.addEventListener("close", this.onClose.bind(this))
                this.ws?.addEventListener("message", this.onMessage.bind(this))
            })
        }
        catch (error) {
            this.connectionState = ConnectionState.OFFLINE;
            throw error;
        }
    }

    async newConnection(username: string) {
        this.reset();
        this.userData.username = username;
        await this.connect();
    }

    async resumeConnection(token: string | null = null) {
        if (token) {
            this.reset();
            this.userData.token = token;
        }
        await this.connect();
    }

    onGameEvent(type: string, callback: EventListener | null, options?: boolean | AddEventListenerOptions | undefined) {
        this.gameEventHandler.addEventListener(type, callback, options);
    }

    offGameEvent(type: string, callback: EventListener | null, options?: boolean | EventListenerOptions | undefined) {
        this.gameEventHandler.removeEventListener(type, callback, options);
    }

    onClientEvent(type: string, callback: EventListener | null, options?: boolean | AddEventListenerOptions | undefined) {
        this.clientEventHandler.addEventListener(type, callback, options);
    }

    offClientEvent(type: string, callback: EventListener | null, options?: boolean | EventListenerOptions | undefined) {
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
        this.resumeConnection();
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
        this.connectionState = ConnectionState.OFFLINE;
        if (e.code == CloseModule.SWITCH_TO_MODULE) {
            this.clientEventHandler.dispatchEvent(new CustomEvent("switching", { detail: e.reason }))
        }
        else if (e.code == CloseModule.EXIT_MODULE) {
            this.clientEventHandler.dispatchEvent(new CustomEvent("switching", { detail: null }))
        }
        else if (e.code >= 4000) {
            this.isInitialized = false;
            if (e.code >= 4100) {
                this.userData.username = null;
                this.userData.token = null;
            }
            this.clientEventHandler.dispatchEvent(new CustomEvent("disconnected", { detail: e.code }))
        } else {
            this.clientEventHandler.dispatchEvent(new CustomEvent("offline", { detail: e }))
            this.connect();
        }
    }

    private onError(e: Event) {
        this.connectionState = ConnectionState.OFFLINE;
        this.clientEventHandler.dispatchEvent(new CustomEvent("error", { detail: e }))
    }

    toggleSpectatorMode() {
        console.log("a")
        this.send({
            "event": "setspectator",
            "spectator": this.spectatorMode === SpectatorMode.IN_GAME 
        })
    }

    private setSpectatorMode(detail: {spectator: boolean, force_spectator: boolean}) {
        if(detail.force_spectator) {
            this.spectatorMode = SpectatorMode.SPECTATOR_FORCED;
        } else if (detail.spectator) {
            this.spectatorMode = SpectatorMode.SPECTATOR;
        } else {
            this.spectatorMode = SpectatorMode.IN_GAME;
        }
    }
}

let client = new SPJClient();
export default client;