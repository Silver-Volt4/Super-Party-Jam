<script lang="ts">
    import client, { ConnectionState } from "./lib/SPJClient.svelte";

    let username: string;

    async function connect() {
        if (!username || client.connectionState !== ConnectionState.OFFLINE) return;
        try {
            await client.newConnection(username);
        } catch {
            alert("Could not connect.");
        }
    }
</script>

<div class="login">
    <img src="logo.png" alt="Sonic Party Jam" />
    <div class="form">
        <input bind:value={username} placeholder="Enter username..." />
        <button on:click={connect}>
            {#if client.connectionState === ConnectionState.OFFLINE}
                Join
            {:else if client.connectionState === ConnectionState.CONNECTING}
                Connecting...
            {/if}
        </button>
    </div>
</div>

<style>
    .login {
        background: linear-gradient(60deg, #c08eff, #5600ff);
        padding: 2em;
        box-sizing: border-box;
        display: flex;
        flex-direction: column;
        height: 100%;
        align-items: center;
    }

    .login img {
        max-width: 400px;
        margin: 4em 0;
    }

    .login .form {
        display: flex;
        flex-direction: column;
        width: 100%;
        max-width: 400px;
        gap: 1em;
    }

    .login .form * {
        font-size: 32px;
        border-radius: 8px;
        border: none;
        text-align: center;
    }

    .login .form input {
        padding: 8px;
        background-color: rgba(255, 255, 255, 0.5);
        
        color: black;
    }
</style>
