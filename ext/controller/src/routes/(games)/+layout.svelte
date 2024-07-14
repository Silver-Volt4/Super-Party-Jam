<script lang="ts">
    import { getUrl } from "$lib/UrlUtil";
    import { onMount } from "svelte";
	let { children } = $props()

	
	onMount(() => {
		let url = getUrl() 
		let handoff = url.searchParams.get("handoff");
		if (!handoff) alert("fuck!")
	})
</script>

{@render children()}