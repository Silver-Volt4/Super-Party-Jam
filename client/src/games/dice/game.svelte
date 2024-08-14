<script lang="ts">
    import type { SPJClient } from "../../lib/SPJClient.svelte";
    let { client }: { client: SPJClient } = $props();
    import DiceModule from "./module.svelte";
    const module = new DiceModule();
    client.assignModule(module);
</script>

<input type="number" bind:value={module.dice} />
<h1>{module.dice}</h1>
<h2>say gex</h2>