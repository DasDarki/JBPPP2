import { defineStore } from 'pinia'
import type {Game} from "@/@types/models";
import {DataController} from "@/controllers/data-controller";

export const useStore = defineStore('main', {
  state: () => ({
    games: [] as Game[],
  }),
  actions: {
    async loadGames() {
      this.games = await DataController.getGames();
    }
  }
})
