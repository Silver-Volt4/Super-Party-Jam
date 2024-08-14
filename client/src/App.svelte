<script lang="ts">
	import client, { ConnectionState } from "./lib/SPJClient.svelte";
	import { getUrl, setUrl } from "./lib/UrlUtil";
	import { onMount } from "svelte";
	import Login from "./Login.svelte";
	import loadModule from "./games/loader";

	let disconnected = $derived(client.isInitialized && client.connectionState === ConnectionState.OFFLINE);

	let isRejoining = false;

	let moduleAwaiter = $state(null);

	onMount(async () => {
		if(client.isInitialized) return;
		let url = getUrl();
		let handoff = url.searchParams.get("handoff");
		if (!handoff) return;
		isRejoining = true;
		try {
			await client.resumeConnection(handoff);
		} catch (e) {
			let url = getUrl();
			url.searchParams.delete("handoff");
			setUrl(url);
			errorRejoining();
		}
	});

	client.onClientEvent("closed", (event: CustomEvent) => {
		let e: CloseEvent = event.detail;
		if (e.code === 4001) {
			let url = getUrl();
			url.searchParams.delete("handoff");
			setUrl(url);
			if (isRejoining) {
				errorRejoining();
			}
		}
	});

	client.onClientEvent("switching", (event: CustomEvent) => {
		switchModule(event.detail);
	});

	function errorRejoining() {
		alert("Error reconnecting you to the game. Returning to main screen.");
		toLoginScreen();
	}

	async function switchModule(module: string) {
		moduleAwaiter = loadModule(module);
	}

	$effect(() => {
		if (client.userData.token) {
			let url = getUrl();
			url.searchParams.set("handoff", client.userData.token);
			setUrl(url);
		}
	});
</script>

{#if client.isInitialized}
	<header>
		<div class="header">{client.userData.username}</div>
	</header>
{/if}

<div class="screen">
	{#if !client.isInitialized}
		<Login></Login>
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
			You have been disconnected from the game. Don't close me, I'm trying to reconnect!
		</div>
	</div>
{/if}

<style>
	header {
		background-color: silver;
		padding: 0.5em;
	}

	header div.header {
		font-size: 3em;
		text-align: center;
		font-family: Anton;
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
