<script lang="ts">
    import type { SPJClient } from "../../lib/SPJClient.svelte";
    let { client }: { client: SPJClient } = $props();
    import DiceModule from "./module.svelte";
    const module = new DiceModule(client);
</script>

<input type="number" bind:value={module.dice.value} />
<h1>{module.dice.value}</h1>Wow
<h2>say gex</h2>