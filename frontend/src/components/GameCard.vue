<script setup lang="ts">
import {ref} from "vue";
import type {Game} from "@/@types/models";

defineProps<{
  game: Game;
}>();

const card = ref<InstanceType<typeof HTMLDivElement>>(null!);

const opening = ref(false);
const closing = ref(false);
const opened = ref(false);

function open() {
  opening.value = true;
  let keep = setInterval(() => {
    card.value.scrollIntoView();
  }, 1);
  setTimeout(() => {
    clearInterval(keep);
    opened.value = true;
  }, 1500);
}

function close() {
  opening.value = false;
  opened.value = false;
  closing.value = true;
  setTimeout(() => {
    closing.value = false;
  }, 1500);
}

</script>

<template>
  <div class="game-card card" :class="{opening: opening, closing: closing}" @click="open" ref="card">
    <div class="close btn rounded primary bordered" v-if="opened" @click.stop="close">
      <i class="fas fa-times"></i>
    </div>
    <div class="header">
      <img :src="game.icon"/>
      <div class="meta">
        <div class="info">
          <span>{{game.title}}</span>
        </div>
        <div class="actions" :class="{open: opened}" v-if="opened || (!closing && !opening)">
          <div class="btn bordered rounded" @click.stop="">
            <i class="fas fa-download"></i>
          </div>
          <div class="btn bordered rounded" @click.stop="">
            <i class="fas fa-play"></i>
          </div>
        </div>
      </div>
    </div>
    <div class="content" v-if="opened">

    </div>
  </div>
</template>

<style scoped lang="scss">
@import "@/styles/_variables.scss";

.game-card {
  position: relative;
  width: 16rem;
  height: 5rem;
  display: flex;
  flex-direction: column;
  transition: all .5s;
  &:not(.opened) {
    cursor: pointer;
  }
  &.opening {
    box-shadow: 0 0 17px -2px rgba(0,0,0,0.75);
    -webkit-box-shadow: 0 0 17px -2px rgba(0,0,0,0.75);
    -moz-box-shadow: 0 0 17px -2px rgba(0,0,0,0.75);
    animation: open 2s forwards .5;
  }
  &.closing {
    animation: close 2s forwards;
  }
  .close {
    width: 1rem;
    height: 1rem;
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
  }
  .header {
    display: flex;
    user-select: none;
    gap: 0.5rem;
    img {
      -webkit-user-drag: none;
      width: 5rem;
      height: 5rem;
      flex-shrink: 0;
    }
    .meta {
      display: flex;
      flex-direction: column;
      flex-grow: 1;
      .info {
        display: flex;
        flex-direction: column;
        flex-grow: 1;
      }
      .actions {
        display: flex;
        flex-shrink: 0;
        gap: 0.5rem;
        width: 100%;
        justify-content: flex-end;
        &.open {
          justify-content: flex-start;
        }
        .btn {
          width: 0.5rem;
          height: 0.5rem;
          i {
            font-size: 0.8rem;
          }
        }
      }
    }
  }
}

@keyframes open {
  from {
    width: 15rem;
    height: 5rem;
  }
  to {
    width: 100%;
    height: calc(100% - 3rem);
  }
}

@keyframes close {
  from {
    width: calc(100% - 7rem);
    height: calc(100% - 3rem);
  }
  to {
    width: 15rem;
    height: 5rem;
  }
}
</style>