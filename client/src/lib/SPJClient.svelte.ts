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


type SyncMapping = { [key in PacketPhase]?: { [key: string]: SPJState<any> } };

interface SPJState<T> {
    value: T;
    setSilently(val: T): void;
}

enum PacketType {
    Sync = 1,
    Call = 2
}

enum PacketPhase {
    Client = 1,
    Player = 2,
    Module = 3,
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
    private eventHandlers = {
        client: new EventTarget(),
        player: new EventTarget(),
        controller: new EventTarget(),
    };
    private syncMap: SyncMapping = {};

    isInitialized = $state(false);
    connectionState: ConnectionState = $state(ConnectionState.OFFLINE);
    spectatorMode: SpectatorMode = $state(SpectatorMode.IN_GAME);

    readonly username: SPJState<string | null>;
    readonly spectator: SPJState<boolean>;
    readonly force_spectator: SPJState<boolean>;
    token: string | null = null;

    constructor() {
        this.username = this.createSync<string | null>(PacketPhase.Player, "username", null);
        this.spectator = this.createSync<boolean>(PacketPhase.Player, "spectator", false);
        this.force_spectator = this.createSync<boolean>(PacketPhase.Player, "force_spectator", false);

        $effect.root(() => {
            $effect(() => {
                this.setSpectatorMode();
            })
        })
    }

    private reset() {
        this.username.setSilently(null);
        this.token = null;
        this.connectionState = ConnectionState.OFFLINE;
        this.spectatorMode = SpectatorMode.IN_GAME;
        this.eventHandlers.client = new EventTarget();
        this.eventHandlers.player = new EventTarget();
        this.isInitialized = false;
    }

    private async connect() {
        if (this.connectionState === ConnectionState.ONLINE) return;
        this.connectionState = ConnectionState.CONNECTING;
        try {
            this.ws = new WebSocket(await getWebsocketUrl());
            return await new Promise<void>(async (resolve, reject) => {
                this.ws?.addEventListener("open", () => {
                    let data: any = {
                        module: this.module?.name ?? ""
                    };
                    if (this.token) {
                        data.token = this.token;
                    } else {
                        data.username = this.username.value;
                    }
                    this.call(PacketPhase.Client, "register", data);
                }, { once: true })

                this.onCall(PacketPhase.Player,
                    "accepted",
                    (e: SPJEvent) => {
                        let data = e.detail;
                        this.username.value = data.username;
                        this.token = data.token;
                        this.isInitialized = true;
                        this.connectionState = ConnectionState.ONLINE;
                        this.ws?.addEventListener("error", this.onError.bind(this))
                        this.ws?.removeEventListener("error", reject)
                        resolve();
                    },
                    { once: true }
                )
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


    createSync<T>(phase: PacketPhase, name: string, initial: T): SPJState<T> {
        let state = $state(initial);

        let reflect = () => this.sync(phase, name, { value: state });

        let wrapper = {
            get value() { return state },
            set value(val: T) {
                state = val;
                reflect();
            },
            setSilently(val: T) { state = val },
        }

        this.syncMap[phase] ??= {};
        this.syncMap[phase][name] = wrapper;

        return wrapper;
    }

    async newConnection(username: string) {
        this.reset();
        this.username.setSilently(username);
        await this.connect();
    }

    async resumeConnection(token: string | null = null) {
        if (token) {
            this.reset();
            this.token = token;
        }
        await this.connect();
    }

    private getEventHandler(phase: PacketPhase): EventTarget | null {
        if (phase === PacketPhase.Client) {
            return this.eventHandlers.client;
        } else if (phase === PacketPhase.Player) {
            return this.eventHandlers.player
        } else if (phase === PacketPhase.Module) {
            return this.module?.eventHandler ?? null;
        }
        return null;
    }

    onCall(phase: PacketPhase.Client | PacketPhase.Player, call: string, callback: EventListener | null, options?: boolean | AddEventListenerOptions | undefined) {
        this.getEventHandler(phase)?.addEventListener(call, callback, options);
    }

    offCall(phase: PacketPhase.Client | PacketPhase.Player, call: string, callback: EventListener | null, options?: boolean | EventListenerOptions | undefined) {
        this.getEventHandler(phase)?.removeEventListener(call, callback, options);
    }

    onControllerEvent(type: string, callback: EventListener | null, options?: boolean | AddEventListenerOptions | undefined) {
        this.eventHandlers.controller.addEventListener(type, callback, options);
    }

    offControllerEvent(type: string, callback: EventListener | null, options?: boolean | EventListenerOptions | undefined) {
        this.eventHandlers.controller.removeEventListener(type, callback, options);
    }

    send(type: PacketType, phase: PacketPhase, name: string, data: any) {
        this.ws?.send(JSON.stringify({
            type: type,
            phase: phase,
            name: name,
            data: data
        }));
    }

    sync(phase: PacketPhase, name: string, data: any) {
        this.send(PacketType.Sync, phase, name, data)
    }

    call(phase: PacketPhase, name: string, data: any) {
        this.send(PacketType.Call, phase, name, data)
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
        let jsonData: any = JSON.parse(event.data);
        var phase: PacketPhase = jsonData.phase;
        var type: PacketType = jsonData.type;
        var name: string = jsonData.name;
        var data: any = jsonData.data;
        if (type === PacketType.Call) {
            this.getEventHandler(phase)?.dispatchEvent(new CustomEvent(name, { detail: data }))
        } else if (type === PacketType.Sync) {
            var state = this.syncMap[phase]?.[name];
            state?.setSilently(data.value);
        }
    }

    private onClose(e: CloseEvent) {
        this.connectionState = ConnectionState.OFFLINE;
        if (e.code == CloseModule.SWITCH_TO_MODULE) {
            this.eventHandlers.controller.dispatchEvent(new CustomEvent("switching", { detail: e.reason }))
        }
        else if (e.code == CloseModule.EXIT_MODULE) {
            this.eventHandlers.controller.dispatchEvent(new CustomEvent("switching", { detail: null }))
        }
        else if (e.code >= 4000) {
            this.isInitialized = false;
            if (e.code >= 4100) {
                this.username.setSilently(null);
                this.token = null;
            }
            this.eventHandlers.controller.dispatchEvent(new CustomEvent("disconnected", { detail: e.code }))
        } else {
            this.eventHandlers.controller.dispatchEvent(new CustomEvent("offline", { detail: e }))
            this.connect();
        }
    }

    private onError(e: Event) {
        this.connectionState = ConnectionState.OFFLINE;
        this.eventHandlers.controller.dispatchEvent(new CustomEvent("error", { detail: e }))
    }

    private setSpectatorMode() {
        if (this.force_spectator.value) {
            this.spectatorMode = SpectatorMode.SPECTATOR_FORCED;
        } else if (this.spectator.value) {
            this.spectatorMode = SpectatorMode.SPECTATOR;
        } else {
            this.spectatorMode = SpectatorMode.IN_GAME;
        }
    }
}

let client = new SPJClient();
export default client;