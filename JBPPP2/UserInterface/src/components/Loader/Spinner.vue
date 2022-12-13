<script setup lang="ts"></script>

<template>
  <div class="spinner">
    <div id="ball-1" class="circle"></div>
    <div id="ball-2" class="circle"></div>
    <div id="ball-3" class="circle"></div>
  </div>
</template>

<style scoped lang="scss">
@import "@/styles/_variables.scss";

.spinner {
  display: flex;
  gap: 0.5rem;
  height: 3rem;
  width: calc(15px * 3 + 0.5rem * 2);
  transform: scale(1.5);
  .circle {
    display: inline-block;
    background-color: $color-primary-active;
    height: 15px;
    width: 15px;
    border-radius: 25px;
  }

  #ball-1 {
    -webkit-animation-name: bounce;
    -webkit-animation-delay: 1s;
    -webkit-animation-duration: 1.2s;
    -webkit-animation-iteration-count: infinite;
  }

  #ball-2 {
    -webkit-animation-name: bounce;
    -webkit-animation-delay: 1.1s;
    -webkit-animation-duration: 1.2s;
    -webkit-animation-iteration-count: infinite;
  }

  #ball-3 {
    -webkit-animation-name: bounce;
    -webkit-animation-delay: 1.2s;
    -webkit-animation-duration: 1.2s;
    -webkit-animation-iteration-count: infinite;
  }

  @-webkit-keyframes bounce {
    0% {
      transform: translateY(0);
    }
    40% {
      transform: translateY(23px);
    }
    60% {
      transform: translateY(-25px);
    }
    80%{
      transform: translateY(0);
    }
  }
}
</style>