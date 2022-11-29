<script setup lang="ts">
import Loader from "@/components/Loader/Loader.vue";
import GameCard from "@/components/GameCard.vue";
import Settings from "@/components/Settings/Settings.vue";
import {LoadingState} from "@/@types/enums";
import {useStore} from "@/stores/main";
import {onMounted, ref} from "vue";

const {games, loadGames} = useStore();

const dataLoading = ref(LoadingState.Initializing);

onMounted(async () => {
  try {
    dataLoading.value = LoadingState.LoadGames;
    await loadGames();
  } finally {
    dataLoading.value = LoadingState.Done;
  }
});
</script>

<template>
  <div class="main-wrapper wrapper" :class="{centered: dataLoading !== LoadingState.Done}">
    <Loader v-if="dataLoading !== LoadingState.Done" :label="$t('main.loading.' + dataLoading)"/>

    <div class="main-container" v-else>
      <div class="settings-button btn primary bordered rounded" @click="settings.show()">
        <i class="fas fa-cog"></i>
      </div>

      <div class="games">
        <GameCard v-for="game in games" :key="game.appid" :game="game"/>
      </div>

      <Settings ref="settings"/>
    </div>
  </div>
</template>

<style scoped lang="scss">
.main-wrapper {
  &.centered {
    justify-content: center;
    align-items: center;
  }

  .main-container {
    display: flex;
    padding: 3rem 4rem;
    width: 100%;
    height: calc(100vh - 6rem);
    overflow-y: auto;
    .settings-button {
      position: fixed;
      top: 1rem;
      right: 1rem;
    }

    .games {
      width: 100%;
      display: flex;
      flex-wrap: wrap;
      justify-content: space-evenly;
      align-content: flex-start;
      gap: 2rem;
    }
  }
}
</style>
