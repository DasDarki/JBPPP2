<script lang="ts" setup>
import Spinner from "@/components/Loader/Spinner.vue";

defineProps<{
  label: string;
}>();
</script>

<template>
  <div class="loader">
    <Spinner/>
    <label class="text">{{label}}</label>
  </div>
</template>

<style scoped lang="scss">
@import "@/styles/_variables.scss";

.loader {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;

  .text {
    margin-top: 2rem;
    color: $fg-color-secondary;
    font-style: italic;
    font-size: 1.1rem;
  }
}
</style>