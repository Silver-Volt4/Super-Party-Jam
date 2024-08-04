<script lang="ts">
    import client from "$lib/SPJClient.svelte";
    import DiceModule from "./DiceModule.svelte";
    const module = new DiceModule();
    client.assignModule(module);
</script>

<input type="number" bind:value={module.dice} />
<h1>{module.dice}</h1>