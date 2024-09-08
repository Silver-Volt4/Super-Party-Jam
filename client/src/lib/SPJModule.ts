import type { SPJClient } from "./SPJClient.svelte";

export default abstract class SPJModule {
    client: SPJClient | undefined;
    eventHandler: EventTarget = new EventTarget();
    abstract name: string

    sync(name: string, state: any) {
        this.client?.send({
            event: "sync",
            name: name,
            value: state
        })
    }
};