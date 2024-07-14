<script lang="ts">
    import { page } from "$app/stores";
	import client from "$lib/SPJClient.svelte";
    import { getUrl, setUrl, toLoginScreen, switchModule } from "$lib/UrlUtil";
    import { onMount } from "svelte";
	let { children } = $props()

	let disconnected = $derived(client.isInitialized && !client.isOnline);

	let isRejoining = false;
	
	onMount(async () => {
		let url = getUrl() 
		let handoff = url.searchParams.get("handoff");
		if (!handoff) return;

		isRejoining = true;
		try {
			await client.resumeConnection(handoff, handoff);
		} catch (e) {
			let url = getUrl() 
			url.searchParams.delete("handoff");
			setUrl(url);
			errorRejoining();
		}
	})

	client.onClientEvent("closed", (event: CustomEvent) => {
		let e: CloseEvent = event.detail;
		if (e.code === 4001) {
			let url = getUrl() 
			url.searchParams.delete("handoff");
			setUrl(url);
			if(isRejoining) {
				errorRejoining();
			}
		} else if (e.code === 4500) {
			switchModule(e.reason);
		}
	});

	function errorRejoining() {
		alert("Error reconnecting you to the game. Returning to main screen.");
		toLoginScreen()
	}

	$effect(() => {
		if(client.userData.token) {
			let url = getUrl() 
			url.searchParams.set("handoff", client.userData.token);
			setUrl(url);
		}
	})
</script>

{#if client.isInitialized}
	<header>
		<div class="header">{client.userData.username}</div>
	</header>
{/if}

<div class="screen">
	{@render children()}
</div>

{#if disconnected}
	<div>You're disconnected</div>
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
</style>
