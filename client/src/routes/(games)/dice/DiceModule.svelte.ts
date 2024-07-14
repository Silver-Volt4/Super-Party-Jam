import { SPJModule } from "$lib/SPJClient.svelte";

export default class DiceModule extends SPJModule {
    name: string = "dice";
    
    dice = $state(0)

    constructor() {
        super();
        $effect(() => this.sync("dice", this.dice))
    }
} 