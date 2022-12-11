<script setup lang="ts">
import Loader from "@/components/Loader/Loader.vue";
import GameCard from "@/components/GameCard.vue";
import Settings from "@/components/Settings/Settings.vue";
import {LoadingState} from "@/@types/enums";
import {useStore} from "@/stores/main";
import {computed, onMounted, ref} from "vue";
import {LauncherController} from "@/controllers/launcher-controller";
import type {Game, LocationList, StatusList} from "@/@types/models";
import {DataController} from "@/controllers/data-controller";
import {getLocation} from "@/libs/settings-manager";

const store = useStore();

const dataLoading = ref<LoadingState>(LoadingState.Initializing);
const settings = ref<InstanceType<typeof Settings>>(null!);

const games = ref<Game[]>([]);
const statusList = ref<StatusList>(null!);
const locationList = ref<LocationList>(null!);

onMounted(async () => {
  try {
    await LauncherController.getLaunchers();

    dataLoading.value = LoadingState.LoadGames;
    games.value = await DataController.getGames();

    dataLoading.value = LoadingState.LoadStatus;
    statusList.value = await fetch("https://raw.githubusercontent.com/DerErizzle/Jackbox-German-Status/main/status.json").then(res => res.json());

    dataLoading.value = LoadingState.LoadLocations;
    locationList.value = await fetch("https://raw.githubusercontent.com/DerErizzle/JBPPP2/data/locations.json").then(res => res.json());
    store.setLocations(locationList.value);
  } finally {
    dataLoading.value = LoadingState.Done;
  }
});
</script>

<template>
  <div class="main-wrapper wrapper" :class="{centered: dataLoading !== LoadingState.Done}">
    <Loader v-if="dataLoading !== LoadingState.Done" :label="$t('main.loading.' + dataLoading)"/>

    <div class="main-container" v-if="dataLoading === LoadingState.Done">
      <div class="settings-button btn primary bordered rounded" @click="settings.show()">
        <i class="fas fa-cog"></i>
      </div>

      <div class="games">
        <GameCard v-for="game in games" :key="game.appid" :game="game"/>
      </div>

      <Settings ref="settings" :location="locationList"/>
    </div>
  </div>
</template>

<style scoped lang="scss">
.main-wrapper {
  width: 100%;
  height: 100vh;

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
