<script setup lang="ts">
import Blur from "@/components/Blur.vue";
import {ref} from "vue";
import {AvailableLocales, getLocale, setLocale} from "@/i18n";
import type {PrioLaunchers} from "@/libs/SettingsManager";
import {SettingsManager} from "@/libs/SettingsManager";

const visible = ref(false);
const opened = ref(false);

const invalidatingCache = ref(false);

const currentLanguage = ref("");
const currentPrioLauncher = ref<PrioLaunchers>("Steam");

function show() {
  currentLanguage.value = getLocale();
  currentPrioLauncher.value = SettingsManager.getPrioLauncher();

  visible.value = true;
  setTimeout(() => {
    opened.value = true;
  }, 700);
}

function close() {
  opened.value = false;
  visible.value = false;
}

function onLanugageChange() {
  setLocale(currentLanguage.value);
}

function onPrioLauncherChange() {
  SettingsManager.setPrioLauncher(currentPrioLauncher.value);
}

async function invalidateCache() {
  if (invalidatingCache.value) {
    return;
  }

  invalidatingCache.value = true;

  setTimeout(async () => {
    try {
      //TODO
    } catch (e) {
      console.error(e);
    } finally {
      invalidatingCache.value = false;
    }
  }, 500);
}

defineExpose({
  show
});
</script>

<template>
  <Blur v-if="visible">
    <div class="settings-card card shadowed" :class="{opening: visible}">
      <div class="container" :class="{opened: opened}">
        <div class="close btn primary rounded bordered" @click="close">
          <i class="fas fa-xmark"></i>
        </div>

        <b class="title" style="margin-bottom: 0.5rem">{{$t('settings.title')}}</b>

        <div class="control">
          <label>{{$t('settings.language')}}:</label>
          <select class="input" v-model="currentLanguage" @change="onLanugageChange">
            <option v-for="(locale, i) in AvailableLocales" :key="i" :value="locale.code">{{locale.name}}</option>
          </select>
        </div>

        <div class="control">
          <label>{{$t('settings.prio-launcher')}}:</label>
          <select class="input" v-model="currentPrioLauncher" @change="onPrioLauncherChange">
            <option value="Steam">Steam</option>
            <option value="Epic Games">Epic Games</option>
          </select>
        </div>

        <span class="divider" style="margin-top: 1rem"/>

        <div class="control" style="justify-content: center; align-items: center">
          <button class="btn bordered warn" :class="{loading: invalidatingCache}" @click="invalidateCache">{{$t('settings.invalidate-cache')}}</button>
          <small class="hint">{{$t('settings.invalidate-cache.hint')}}</small>
        </div>
      </div>
    </div>
  </Blur>
</template>

<style scoped lang="scss">
@import "@/styles/_variables.scss";

.settings-card {
  margin: auto;
  opacity: 0;
  width: 20rem;
  &.opening {
    animation: grow-open 0.7s forwards;
  }
  .container {
    position: relative;
    display: flex;
    flex-direction: column;
    gap: 1rem;
    .close {
      width: 1rem;
      height: 1rem;
      position: absolute;
      top: -1.5rem;
      right: -1.5rem;
    }
    .title {
      font-size: 1.4rem;
      font-weight: bold;
      text-decoration: underline;
    }
    .control {
      display: flex;
      flex-direction: column;
      gap: 0.3rem;
      width: 100%;
      .hint {
        color: $fg-color-secondary;
        font-size: 0.8rem;
        font-style: italic;
      }
      label {
        color: $fg-color-secondary;
        font-weight: bold;
        margin-left: 1.5rem;
      }
    }
    .divider {
      width: 100%;
      height: 1px;
      background-color: rgba(255, 255, 255, 0.2);
    }
  }
}

@keyframes grow-open {
  from {
    opacity: 0;
    transform: scale(0);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style>