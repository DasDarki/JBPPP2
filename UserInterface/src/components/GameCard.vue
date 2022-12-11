<script setup lang="ts">
import {computed, onMounted, ref} from "vue";
import type {Game} from "@/@types/models";
import {LauncherController} from "@/controllers/launcher-controller";
import {InstallController} from "@/controllers/install-controller";

const props = defineProps<{
  game: Game;
}>();

const card = ref<InstanceType<typeof HTMLDivElement>>(null!);

const opening = ref(false);
const closing = ref(false);
const opened = ref(false);

const installed = ref(false);
const loading = ref(true);

const progress = ref(0);
const progressText = ref("");

const normalizedProgress = computed(() => {
  return Math.round(progress.value);
});

async function install() {
  if (loading.value) return;

  loading.value = true;
  progressText.value = "install.preparing";

  const gamePath = await LauncherController.findGamePath(props.game);
  if (!gamePath) {
    progressText.value = "";
    progress.value = 0;
    loading.value = false;

    alert("Game path not found!");
    return;
  }

  if (!await InstallController.checkVersion(gamePath, props.game)) {
    progressText.value = "";
    progress.value = 0;
    loading.value = false;

    alert("Version check failed!");
    return;
  }

  const downloadLink = InstallController.findDownloadLink(props.game);
  if (!downloadLink) {
    progressText.value = "";
    progress.value = 0;
    loading.value = false;

    alert("Download link not found!");
    return;
  }

  progressText.value = "install.downloading";
  const tempPath = await InstallController.download(downloadLink, percent => {
    progress.value = 49 * percent;
  });
  progress.value = 49;

  progressText.value = "install.extracting";
  await InstallController.install(tempPath, gamePath, (action, count, total) => {
    if (action === "EXTRACT") {
      progress.value = 49 + (21 * count / total);
    } else if (action === "MOVE") {
      progress.value = 70 + (29 * count / total);
    }
  });
  progress.value = 99;

  installed.value = true;
  progressText.value = "";
  progress.value = 0;
  loading.value = false;
}

async function uninstall() {
  if (loading.value) return;

  loading.value = true;
  progressText.value = "uninstalling";

  const gamePath = await LauncherController.findGamePath(props.game);
  if (!gamePath) {
    progressText.value = "";
    progress.value = 0;
    loading.value = false;

    alert("Game path not found!");
    return;
  }

  await InstallController.uninstall(gamePath, (action, count, total) => {
    if (action === "MOVE") {
      progress.value = Math.floor(100 * (count / total));
    } else if (action === "ERR01") {
      progress.value = 0;
      loading.value = false;

      alert("Patch not installed!");
    } else if (action === "ERR02") {
      progress.value = 0;
      loading.value = false;

      alert("Patch Confirmation File corrupted!");
    }
  });

  installed.value = false;

  progressText.value = "";
  progress.value = 0;
  loading.value = false;
}

function launchGame() {
  if (loading.value) return;

  LauncherController.launchGame(props.game).then(success => {
    if (!success) {
      alert("Failed to launch game! Maybe not installed? Or not for Steam?");
    }
  });
}

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

onMounted(async () => {
  const dir = await LauncherController.findGamePath(props.game);
  if (dir) {
    installed.value = await InstallController.isPatchInstalled(dir);
  } else {
    installed.value = false;
  }

  loading.value = false;
});
</script>

<template>
  <div class="game-card card" :class="{opening: opening, closing: closing}" ref="card">
    <div class="close btn rounded primary bordered" v-if="opened" @click.stop="close">
      <i class="fas fa-times"></i>
    </div>
    <div class="header">
      <img :src="game.icon"/>
      <div class="meta">
        <div class="info">
          <span>{{game.title}}</span>
        </div>
        <small v-if="loading && progressText" style="font-style: italic; opacity: 0.5; font-size: 0.9rem">{{$t('progress.' + progressText)}} ({{normalizedProgress}}%)</small>
        <div class="actions" :class="{open: opened}" v-if="opened || (!closing && !opening)">
          <div v-if="!installed" class="btn bordered rounded" :class="{'custom-loading': loading}" @click.stop="install">
            <i class="fas" :class="loading ? 'fa-spinner' : 'fa-download'"></i>
          </div>
          <div v-else class="btn error bordered rounded" :class="{'custom-loading': loading}" @click.stop="uninstall">
            <i class="fas" :class="loading ? 'fa-spinner' : 'fa-trash'"></i>
          </div>

          <div class="btn bordered rounded" :class="{'custom-loading': loading}" @click.stop="launchGame">
            <i class="fas" :class="loading ? 'fa-spinner' : 'fa-play'"></i>
          </div>
        </div>
      </div>

      <div class="progress" v-if="loading" :style="{width: progress + '%'}">

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
    position: relative;
    display: flex;
    user-select: none;
    gap: 0.5rem;
    .progress {
      position: absolute;
      bottom: -1rem;
      left: 0;
      height: 0.25rem;
      border-radius: 5rem;
      background: linear-gradient(to right, #005fdb, #0032b0);
      transition: all .5s;
    }
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
          &.custom-loading {
            opacity: 0.5;
            cursor: not-allowed;
            i {
              animation: spin 1s linear infinite;
            }
          }
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

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
