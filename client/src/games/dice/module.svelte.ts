import SPJModule from "../../lib/SPJModule";

export default class DiceModule extends SPJModule {
    name: string = "dice";
    
    dice = $state(0)

    constructor() {
        super();
        $effect(() => this.sync("dice", this.dice))
    }
} 