<script lang="ts">
	import client, {
		CloseClient,
		ConnectionState,
		SpectatorMode,
	} from "./lib/SPJClient.svelte";
	import Login from "./Login.svelte";
	import loadModule from "./games/loader";
	import type { SvelteComponent } from "svelte";

	console.log(loadModule);

	let disconnected = $derived(
		client.isInitialized &&
			client.connectionState === ConnectionState.OFFLINE,
	);

	let moduleAwaiter: Promise<SvelteComponent> | null = $state(null);

	client.onControllerEvent("disconnected", (event: CustomEvent) => {
		let code: number = event.detail;
		if (code >= 4100) {
			localStorage.removeItem("spjLastGame");
		}
		if (code === CloseClient.INVALID_TOKEN) {
			alert(
				"Your connection to the game has been lost. You will now return to the main screen.",
			);
		}
	});

	client.onControllerEvent("switching", (event: CustomEvent) => {
		switchModule(event.detail);
	});

	$effect(() => {
		if (client.token) {
			localStorage.setItem("spjLastGame", client.token);
		}
	});

	function switchModule(module: string) {
		moduleAwaiter = awaitModule(module);
	}

	async function awaitModule(module: string) {
		console.log("awaiting module")
		let m = await loadModule(module);
		console.log("got module")
		console.log(m)
		return m;
	}

	function rename() {
		let newName = prompt(
			"Enter your new name:",
			client.username.value ?? "",
		);
		if (newName) {
			client.username.value = newName;
		}
	}
</script>

{#if client.isInitialized}
	<header>
		<div class="header" onclick={rename}>{client.username.value}</div>
	</header>
{/if}

<div class="screen">
	{#if !client.isInitialized}
		<Login></Login>
	{:else if moduleAwaiter === null}
		{#if client.spectatorMode !== SpectatorMode.SPECTATOR_FORCED}
			{#if client.spectatorMode === SpectatorMode.IN_GAME}
				You are in the game.
			{:else}
				You have chosen to spectate.
			{/if}
			<button onclick={() => client.spectator.value = !client.spectator.value}>Toggle</button
			>
		{:else}
			You have been forced into spectator mode. You cannot change this.
		{/if}
	{:else}
		{#await moduleAwaiter}
			Loading...
		{:then module}
			<svelte:component this={module} {client} />
		{/await}
	{/if}
</div>

{#if disconnected}
	<div class="offline">
		<div>
			You have been disconnected from the game. Don't close me, I'm trying
			to reconnect!
		</div>
	</div>
{/if}

<style>
	header {
		background: linear-gradient(to right, #c08eff, #5600ff);
		padding: 0.5em;
	}

	header div.header {
		font-size: 3em;
		text-align: center;
		font-family: Anton;
		color: white;
	}

	.screen {
		flex-grow: 1;
	}

	.offline {
		position: fixed;
		bottom: 0;
		width: 100%;
	}

	.offline div {
		margin: 0.5em;
		padding: 0.5em;
		font-size: 1.5em;
		background-color: rgba(255, 100, 100);
		color: white;
	}
</style>
