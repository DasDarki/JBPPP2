<script lang="ts" setup></script>

<template>
  <div class="blur">
    <slot></slot>
  </div>
</template>

<style scoped lang="scss">
.blur {
  position: fixed;
  left: 0;
  top: 0;
  width: 100%;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: rgba(0, 0, 0, 0.7);
  overflow-x: hidden;
  overflow-y: auto;
}
</style>