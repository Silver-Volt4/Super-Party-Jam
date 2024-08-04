<script lang="ts">
    import client from "./lib/SPJClient.svelte";

    let username: string;

    async function connect() {
        try {
            await client.newConnection(username);
        } catch {
            alert("Could not connect.")
        }
    }
</script>

{#if !client.isInitialized}
    <div class="login">
        <div><b>Username</b></div>
        <div><input bind:value={username} /></div>
        <div><button on:click={connect}> Connect </button></div>
    </div>
{/if}

{#if client.isInitialized}
    <div><input bind:value={username} /></div>
    {#if username != client.userData.username}
        <button on:click={() => client.setUsername(username)}>
            Change username
        </button>
    {/if}
{/if}

<style>
    .login {
        padding: 2em;
    }
</style>
