<script lang="ts">
	import client, {
		CloseClient,
		ConnectionState,
        SpectatorMode,
	} from "./lib/SPJClient.svelte";
	import Login from "./Login.svelte";
	import loadModule from "./games/loader";
	import type { SvelteComponent } from "svelte";

	let disconnected = $derived(
		client.isInitialized &&
			client.connectionState === ConnectionState.OFFLINE,
	);

	let moduleAwaiter: Promise<SvelteComponent> | null = $state(null);

	client.onClientEvent("disconnected", (event: CustomEvent) => {
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

	client.onClientEvent("switching", (event: CustomEvent) => {
		switchModule(event.detail);
	});

	$effect(() => {
		if (client.userData.token) {
			localStorage.setItem("spjLastGame", client.userData.token);
		}
	});

	async function switchModule(module: string) {
		moduleAwaiter = loadModule(module);
	}

	function rename() {
		let newName = prompt("Enter your new name:", client.userData.username ?? "");
		if(newName) {
			client.setUsername(newName);
		}
	}
</script>

{#if client.isInitialized}
	<header>
		<div class="header" on:click={rename}>{client.userData.username}</div>
	</header>
{/if}

<div class="screen">
	{#if !client.isInitialized}
		<Login></Login>
	{/if}

	{#if moduleAwaiter === null}
		{#if client.spectatorMode !== SpectatorMode.SPECTATOR_FORCED}
			{#if client.spectatorMode === SpectatorMode.IN_GAME}
				You are in the game.
			{:else}
				You have chosen to spectate.
			{/if}
			<button on:click={() => client.toggleSpectatorMode()}>Toggle</button>
		{:else}
			You have been forced into spectator mode. You cannot change this.
		{/if}
	{/if}

	{#await moduleAwaiter}
		Loading...
	{:then module}
		<svelte:component this={module} {client} />
	{/await}
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
