import { PacketPhase, type SPJClient, type SPJState } from "../../lib/SPJClient.svelte";
import SPJModule from "../../lib/SPJModule";

export default class DiceModule extends SPJModule {
    name: string = "dice";
    dice: SPJState<number>;

    constructor(client: SPJClient) {
        super();
        this.client = client;
        client.assignModule(this);
        this.dice = this.client.createSync<number>(PacketPhase.Module, "dice", 0);
    }
} 